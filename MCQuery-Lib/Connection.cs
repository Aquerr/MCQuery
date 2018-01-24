using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MCQuery
{
    public class Connection
    {
        private byte[] Magic = { 0xFE, 0xFD };
        private byte Handshake = 0x09;
        private byte Stat = 0x00;
        private byte[] SessionID = { 0x01, 0x01, 0x01, 0x01 };

        public Connection(string address, int port)
        {
            try
            {
                string udpResponse = sendByUdp(address, port);

                if (udpResponse.Equals(String.Empty))
                {
                    string tcpResponse = sendByTcp(address, port);

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
                }
            }
            catch (SocketException socketException)
            {
                throw new SocketException();
            }
        }

        public Server getServer()
        {
            //TODO: Return server object with fetched data from the request.
            return new Server("dummy", true);
        }

        public string sendByUdp(string address, int port)
        {
            try
            {
                //Check if address is a domain name.
                if (isDomainAddress(address))
                {
                    address = getIpFromDomain(address);
                }

                UdpClient udpClient = new UdpClient();

                udpClient.Connect(address, port);

                List<byte> message = new List<byte>();
                message.AddRange(Magic);
                message.Add(Handshake);
                message.Add(Stat);
                message.AddRange(SessionID);

                Byte[] handshakeMessage = message.ToArray().ToArray();

                udpClient.Send(handshakeMessage, handshakeMessage.Length);

                //IPEndPoint object will allow us to read datagrams sent from any source.
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(address), port);

                // Blocks until a message returns on this socket from a remote host.
                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.ASCII.GetString(receiveBytes);

                udpClient.Close();

                if (returnData.Equals(String.Empty))
                {
                    return String.Empty;
                }
                else
                {
                    return returnData;
                }
            }
            catch (SocketException exception)
            {
                throw new SocketException();
            }
        }

        private string getIpFromDomain(string address)
        {
            IPAddress[] addresses = Dns.GetHostAddresses(address);

            return addresses[0].AddressFamily.ToString();
        }

        private bool isDomainAddress(string address)
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

        public string sendByTcp(string address, int port)
        {
            //TODO: Implement sending packet by TCP.
            return String.Empty;
        }
    }
}
