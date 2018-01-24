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
        //Int32 - SessionIs has
        private Int32 _sessionId = 1;
        //Int32 - Challenge Token
        private Int32 _challengeToken = 0;

        private Timer _challengeTimer = new Timer();

        public Connection(string address, int port)
        {
            string udpResponse = SendByUdp(address, port);

            if (udpResponse.Equals(String.Empty))
            {
                string tcpResponse = SendByTcp(address, port);

                if (tcpResponse.Equals(String.Empty)) throw new NotImplementedException();
            }
            else
            {
                // Uses the IPEndPoint object to determine which of these two hosts responded.
                Console.WriteLine("This is the message you received " +
                                             udpResponse.ToString());
                Console.WriteLine("This message was sent from " +
                                            address +
                                            " on their port number " +
                                            port.ToString());

                //_challengeToken = GetChallengeToken(udpResponse);
            }
        }

        public Server GetServer()
        {
            //TODO: Return server object with fetched data from the request.
            return new Server("dummy", true);
        }

        public string SendByUdp(string address, int port)
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

                List<byte> message = new List<byte>();
                message.AddRange(_magic);
                message.Add(_handshake);
                message.Add(_stat);
                byte[] sessionBytes = BitConverter.GetBytes(_sessionId);
                Array.Reverse(sessionBytes);
                message.AddRange(sessionBytes);

                Byte[] handshakeMessage = message.ToArray().ToArray();

                udpClient.Send(handshakeMessage, handshakeMessage.Length);

                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(address), port);

                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.ASCII.GetString(receiveBytes);
                string test = returnData.Trim();

                _challengeTimer.Elapsed += RegenerateChallengeToken;
                _challengeTimer.Interval = 30000;
                _challengeTimer.Start();

                _challengeToken = GetChallengeToken(receiveBytes);

                udpClient.Close();

                if (receiveBytes.Length == 0)
                {
                    return string.Empty;
                    //return new byte[] { };
                }
                else
                {
                    return returnData;
                    //return receiveBytes;
                }
            }
            catch (SocketException exception)
            {
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
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw new SocketException();
            }
        }

        public string SendByTcp(string address, int port)
        {
            //TODO: Implement sending packet by TCP.
            return String.Empty;
        }

        private Int32 GetChallengeToken(byte[] message)
        {
            //TODO: Remove 0-4 and 13 index from receiveBytes to get the ChallengeToken

            byte[] challengeToken = new byte[4];

            string response = "";

            for (int i = 0; i < message.Length; i++)
            {
                if(i > 5)
                {
                    byte item = message[i];
                    response += Encoding.ASCII.GetString(new byte[] { item });
                }
            }

            response = response.Remove(response.Length - 1, 1);
            Int32 token = Int32.Parse(response);

            if (!token.Equals(0))
            {
                return token;
            }

            return 0;
        }

        private void RegenerateChallengeToken(Object sender, ElapsedEventArgs e)
        {
            //Run the init request to get the token again.
        }
    }
}
