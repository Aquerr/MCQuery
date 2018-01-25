using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace MCQuery
{
    public class Connection
    {
        private string _address = "";
        private int _port = 0;

        //Byte[] - Magic Number
        private byte[] _magic = { 0xFE, 0xFD };

        //Byte[] - Connection Type
        private byte[] _handshake = { 0x09 };
        private byte[] _stat = { 0x00 };

        //Int32 - but written in hex format as byte array - SessionIs has
        private byte[] _sessionId = { 0x01, 0x01, 0x01, 0x01 };

        //Byte[] - but written as byte array - Challenge Token
        private byte[] _challengeToken;

        private UdpClient udpClient;
        private TcpClient tcpClient;
        private Timer _challengeTimer = new Timer();

        public Connection(string address, int port)
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
                byte[] tcpResponse = SendByTcp(address, port);

                if (tcpResponse.Length == 0) throw new NotImplementedException();
            }
            else
            {
                _address = address;
                _port = port;
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
                byte[] tcpResponse = SendByTcp(address, port);

                if (tcpResponse.Length == 0) return new byte[] { };
                else return tcpResponse;
            }
            else
            {
                return udpResponse;
            }
        }

        public Server GetBasicServerInfo()
        {
            byte[] responseData = GetBasicStat(_address, _port);

            if(responseData.Length != 0)
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
            //TODO: Return server object with fetched data from the request.
            return new Server(false);
        }

        public byte[] SendByUdp(string address, int port, byte[] data)
        {
            try
            {
                //Check if address is a domain name.
                if (IsDomainAddress(address))
                {
                    address = GetIpFromDomain(address);
                }

                if (udpClient == null)
                {
                    //Set up UDP Client
                    udpClient = new UdpClient();
                    udpClient.Connect(address, port);
                    udpClient.Client.SendTimeout = 10000; //Timeout after 10 seconds
                    udpClient.Client.ReceiveTimeout = 10000; //Timeout after 10 seconds

                    _challengeTimer.Elapsed += RegenerateChallengeToken;
                    _challengeTimer.Interval = 30000;
                    _challengeTimer.Start();
                }

                udpClient.Send(data, data.Length);

                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(address), port);

                byte[] receiveData = udpClient.Receive(ref RemoteIpEndPoint);

                Console.WriteLine(Encoding.ASCII.GetString(receiveData));

                if (receiveData.Length == 0)
                {
                    return new byte[] { };
                }
                else
                {
                    return receiveData;
                }
            }
            catch (SocketException exception)
            {
                Console.WriteLine("SocketException: {0}", exception.Message);
                throw new SocketException();
            }
        }

        private string GetIpFromDomain(string address)
        {
            IPAddress[] addresses = Dns.GetHostAddresses(address);

            return addresses[0].ToString();
        }

        private bool IsDomainAddress(string address)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(address);

                if (hostEntry.HostName == address)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (SocketException exception)
            {
                throw new Exception("Exception: " + exception.Message);
            }
        }

        public byte[] SendByTcp(string address, int port)
        {
            //TODO: Implement sending packet by TCP.
            return new byte[] { };
        }

        private byte[] GetChallengeToken(byte[] message)
        {
            //Index 0 = Type (Handshake)
            //Index 1 - 4 = SessionId
            //Index 5 and further is a challenge token which we need to extract.

            byte[] challengeToken = new byte[message.Length - 5];

            string response = "";

            for (int i = 0; i < message.Length; i++)
            {
                if (i >= 5)
                {
                    byte item = message[i];
                    response += Encoding.ASCII.GetString(new byte[] { item });
                }
            }
            Int32 tokenInt32 = Int32.Parse(response);

            byte[] challenge = new byte[]
            {
                (byte)(tokenInt32 >> 24 & 0xFF),
                (byte)(tokenInt32 >> 16 & 0xFF),
                (byte)(tokenInt32 >> 8 & 0xFF),
                (byte)(tokenInt32 >> 0 & 0xFF)
            };

            return challenge;
        }

        private void RegenerateChallengeToken(Object sender, ElapsedEventArgs e)
        {
            Console.WriteLine(_challengeTimer.Interval);
            //Run handshake again to obtain new challenge token.
            Handshake(_address, _port);
        }

        public void Close()
        {
            _challengeTimer.Stop();

            if(udpClient != null)
            {
                udpClient.Close();
            }
            if (tcpClient != null)
            {
                tcpClient.Close();
            }
        }
    }
}
