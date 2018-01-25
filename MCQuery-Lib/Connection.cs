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
        //Byte - Magic Number
        private byte[] _magic = { 0xFE, 0xFD };
        //Byte - Connection Type
        private byte _handshake = 0x09;
        private byte _stat = 0x00;
        //Int32 - but written in hex format as byte array - SessionIs has
        private byte[] _sessionId = { 0x01, 0x01, 0x01, 0x01 };
        //Int32 - but written as byte array - Challenge Token
        private byte[] _challengeToken;

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
            message.Add(_handshake);
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
                _challengeToken = GetChallengeToken(udpResponse);
            }
        }

        public Server GetServer()
        {
            //TODO: Return server object with fetched data from the request.
            return new Server("dummy", true);
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

                UdpClient udpClient = new UdpClient();
                udpClient.Connect(address, port);

                udpClient.Send(data, data.Length);
                udpClient.Client.SendTimeout = 5000; //Timeout after 5 seconds
                udpClient.Client.ReceiveTimeout = 5000; //Timeout after 5 seconds

                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(address), port);

                Byte[] receiveData = udpClient.Receive(ref RemoteIpEndPoint);

                _challengeTimer.Elapsed += RegenerateChallengeToken;
                _challengeTimer.Interval = 30000;
                _challengeTimer.Start();

                udpClient.Close();

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

            for (int i = 0; i < message.Length; i++)
            {
                if(i >= 5)
                {
                    byte item = message[i];
                    challengeToken[i - 5] = item;
                }
            }

            challengeToken = challengeToken.Take(challengeToken.Count() - 1).ToArray();

            return challengeToken;
        }

        private void RegenerateChallengeToken(Object sender, ElapsedEventArgs e)
        {
            //Run the init request to get the token again.
        }
    }
}
