using MCQuery;
using System;

namespace MCQuery
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server IP Address: ");
            string ipAddress = Console.ReadLine();

            Console.WriteLine("Server Port: ");
            string portString = Console.ReadLine();

            int.TryParse(portString, out int port);

            if (port != 0)
            {
                Connection connection = new Connection(ipAddress, port); //This should give us a challenge token needed for getting data from the sever.
                Server basicServer = connection.GetBasicServerInfo();

                Console.WriteLine("Printing out server info: ");
                Console.WriteLine("Server MOTD: {0}", basicServer.Motd);
                Console.WriteLine("Server GameType: {0}", basicServer.GameType);
                Console.WriteLine("Server Map: {0}", basicServer.Map);
                Console.WriteLine("Server Player Count: {0}", basicServer.PlayerCount);
                Console.WriteLine("Server Max Players: {0}", basicServer.MaxPlayers);
                Console.WriteLine("Server Status: {0}", basicServer.IsOnline);

            }
            else
            {
                Console.WriteLine("Wrong port number!");
            }

            Console.WriteLine("\n Press any key to continue...");
            Console.Read();
        }
    }
}
