using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace MCQuery
{
	public abstract class Connection : IDisposable
	{
		protected internal string _address = "";
		protected internal int _port = 0;

		private UdpClient _udpClient;
		private TcpClient _tcpClient;

	    protected Connection(string address, int port)
		{
			_address = address;
			_port = port;
		}

		protected byte[] SendByUdp(string address, int port, byte[] data)
		{
			try
			{
				//Check if address is a domain name.
				if (IsDomainAddress(address))
				{
					address = GetIpFromDomain(address);
				}

				if (_udpClient == null)
				{
					//Set up UDP Client
					_udpClient = new UdpClient();
					_udpClient.Connect(address, port);
					_udpClient.Client.SendTimeout = 10000; //Timeout after 10 seconds
					_udpClient.Client.ReceiveTimeout = 10000; //Timeout after 10 seconds
				}

				_udpClient.Send(data, data.Length);

				IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Parse(address), port);

				byte[] receiveData = _udpClient.Receive(ref remoteIpEndPoint);

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
				throw;
			}
		}

		protected string GetIpFromDomain(string address)
		{
			IPAddress[] addresses = Dns.GetHostAddresses(address);

			return addresses[0].ToString();
		}

		protected bool IsDomainAddress(string address)
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

		protected byte[] SendByTcp(string address, int port, byte[] data)
        {
            //TODO: Implement sending packet by TCP.

            try
            {
                if (_tcpClient == null)
                {
                    _tcpClient = new TcpClient(address, port);
                    NetworkStream networkStream = _tcpClient.GetStream();

                    byte[] buffer = new byte[1024];

                    networkStream.Write(data, 0, data.Length);
                    int byteCount = networkStream.Read(buffer, 0, buffer.Length);

                    string receivedText = Encoding.ASCII.GetString(buffer, 0, byteCount);
                }
            }
            catch (SocketException exception)
            {
                Console.WriteLine(exception);
                throw;
            }

            return new byte[] { };
        }

		public virtual void Close()
        {
            if(_udpClient != null)
            {
                _udpClient.Close();
            }
            if (_tcpClient != null)
            {
                _tcpClient.Close();
            }

			Dispose();
        }

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
				}



				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~Connection() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
