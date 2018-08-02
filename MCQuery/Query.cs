using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MCQuery
{
	public class Query : Connection
	{
		//Byte[] - Magic Number
		private readonly byte[] _magic = { 0xFE, 0xFD };

		//Byte[] - Connection Type
		private readonly byte[] _handshake = { 0x09 };
		private readonly byte[] _stat = { 0x00 };

		//Int32 - but written in hex format as byte array - SessionIs has
		private readonly byte[] _sessionId = { 0x01, 0x01, 0x01, 0x01 };

		//Byte[] - but written as byte array - Challenge Token
		private byte[] _challengeToken;
        private readonly Timer _challengeTimer = new Timer();

		public Query(string address, int port) : base(address, port)
		{
			//Do the handshake with the server to receive a challenge token.
			Handshake(address, port);
		}

		private void Handshake(string address, int port)
		{
			List<byte> message = new List<byte>();
			message.AddRange(_magic);
			message.AddRange(_handshake);
			message.AddRange(_sessionId);

			Byte[] handshakeMessage = message.ToArray();
			byte[] udpResponse = SendByUdp(address, port, handshakeMessage);

			//If handshake could not be done through UDP then try connecting through TCP.
			if (udpResponse.Length == 0)
			{
				byte[] tcpResponse = SendByTcp(address, port, handshakeMessage);

				if (tcpResponse.Length == 0) throw new NotImplementedException();
			}
			else
			{
                _challengeTimer.Elapsed += RegenerateChallengeToken;
                _challengeTimer.Interval = 30000;
                _challengeTimer.Start();
                _challengeToken = GetChallengeToken(udpResponse);
			}
		}

		private byte[] GetBasicStat(string address, int port)
		{
			List<byte> message = new List<byte>();
			message.AddRange(_magic);
			message.AddRange(_stat);
			message.AddRange(_sessionId);
			message.AddRange(_challengeToken);
			byte[] basicStatMessage = message.ToArray();

			byte[] udpResponse = SendByUdp(address, port, basicStatMessage);

			if (udpResponse.Length == 0)
			{
				byte[] tcpResponse = SendByTcp(address, port, basicStatMessage);

				if (tcpResponse.Length == 0) return new byte[] { };
			    return tcpResponse;
			}
		    return udpResponse;
		}

		private byte[] GetFullStat(string address, int port)
		{
			List<byte> message = new List<byte>();
			message.AddRange(_magic);
			message.AddRange(_stat);
			message.AddRange(_sessionId);
			message.AddRange(_challengeToken);
			message.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 }); //Padding
			byte[] fullStatMessage = message.ToArray();

			byte[] udpResponse = SendByUdp(address, port, fullStatMessage);

			if (udpResponse.Length == 0)
			{
				byte[] tcpResponse = SendByTcp(address, port, fullStatMessage);

				if (tcpResponse.Length == 0) return new byte[] { };
			    return tcpResponse;
			}
		    return udpResponse;
		}

		public Server GetBasicServerInfo()
		{
			byte[] responseData = GetBasicStat(_address, _port);

			if (responseData.Length != 0)
			{
				responseData = responseData.Skip(5).ToArray();

				string stringData = Encoding.ASCII.GetString(responseData);
				string[] informations = stringData.Split(new string[] { "\0" }, StringSplitOptions.None);

				//0 = MOTD
				//1 = GameType
				//2 = Map
				//3 = Number of Players
				//4 = Maxnumber of Players
				//5 = Host Port
				//6 = Host IP

				if (informations[5].StartsWith(":k"))
				{
					informations[5] = informations[5].Substring(2);
				}

				Server server = new Server(true)
				{
					Motd = informations[0],
					GameType = informations[1],
					Map = informations[2],
					PlayerCount = int.Parse(informations[3]),
					MaxPlayers = int.Parse(informations[4]),
					Address = informations[5],
					Port = informations[6] //TODO: Port is currently missing... It needs to be fixed.
				};

				return server;
			}

			return null;
		}

		public Server GetFullServerInfo()
		{
			byte[] responseData = GetFullStat(_address, _port);

			if (responseData.Length != 0)
			{
				//Skip first 11 bytes 
				responseData = responseData.Skip(16).ToArray();

				string stringData = Encoding.ASCII.GetString(responseData);

				//This array should contain an array with server informations and an array with playernames
				string[] informations = stringData.Split(new string[] { "player_" }, StringSplitOptions.None);

				string[] serverInfo = informations[0].Split(new string[] { "\0" }, StringSplitOptions.None);
				string[] playerList = informations[1].Split(new string[] { "\0" }, StringSplitOptions.None);

				//Split serverInfo to key - value pair.

				Dictionary<string, string> serverDict = new Dictionary<string, string>();

				for (int i = 0; i < serverInfo.Length; i += 2)
				{
					serverDict.Add(serverInfo[i], serverInfo[i + 1]);
				}

				//0 = MOTD
				//1 = GameType
				//2 = Map
				//3 = Number of Players
				//4 = Maxnumber of Players
				//5 = Host Port
				//6 = Host IP

				Server server = new Server(true)
				{
					Motd = serverDict["hostname"],
					GameType = serverDict["gametype"],
					Map = serverDict["map"],
					PlayerCount = int.Parse(serverDict["numplayers"]),
					MaxPlayers = int.Parse(serverDict["maxplayers"]),
					Plugins = serverDict["plugins"],
					Address = serverDict["hostip"],
					Port = serverDict["hostport"], //TODO: Port is currently missing... It needs to be fixed.
					Version = serverDict["version"]
				};

				return server;
			}

			return null;
		}

		private byte[] GetChallengeToken(byte[] message)
		{
			//Index 0 = Type (Handshake)
			//Index 1 - 4 = SessionId
			//Index 5 and further is a challenge token which we need to extract.

			//byte[] challengeToken = new byte[message.Length - 5];

			string response = "";

			for (int i = 0; i < message.Length; i++)
			{
				if (i >= 5)
				{
					byte item = message[i];
					response += Encoding.ASCII.GetString(new[] { item });
				}
			}
			Int32 tokenInt32 = Int32.Parse(response);

			byte[] challenge = {
				(byte)(tokenInt32 >> 24 & 0xFF),
				(byte)(tokenInt32 >> 16 & 0xFF),
				(byte)(tokenInt32 >> 8 & 0xFF),
				(byte)(tokenInt32 >> 0 & 0xFF)
			};

			return challenge;
		}

		private void RegenerateChallengeToken(Object sender, ElapsedEventArgs e)
		{
			//Run handshake again to obtain new challenge token.
			Handshake(_address, _port);
		}

	    public override bool IsConnected
	    {
	        get => _challengeToken != null || _challengeToken.Length > 0;
	        set => _isAuthenticated = value;
	    }
    }
}
