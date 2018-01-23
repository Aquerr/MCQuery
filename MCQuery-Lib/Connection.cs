using System.Net.Sockets;

namespace MCQuery_Lib
{
    public class Connection
    {
        public Connection(string address, string port)
        {
            TcpClient client = new TcpClient(address, int.Parse(port));

            try
            {
             //   client.Connect();
            }
            catch (SocketException socketException)
            {
                //TODO:Do something with exception...
            }
        }
    }
}
