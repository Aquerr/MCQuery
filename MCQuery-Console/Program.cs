using MCQuery;
using System;

namespace MCQuery
{
    class Program
    {
        static void Main(string[] args)
        {
//            Console.WriteLine("Server IP Address: ");
//            string ipAddress = Console.ReadLine();
//
//            Console.WriteLine("Server Port: ");
//            string portString = Console.ReadLine();
            string ipAddress = "192.168.0.7";
            string portString = "25575";

            int.TryParse(portString, out int port);

            if (port != 0)
            {
                //Connection connection = new Connection(ipAddress, port); //This should give us a challenge token needed for getting data from the sever.

                //using (Query query = new Query(ipAddress, port))
                //{

                //}

                //Query serverQuery = new Query(ipAddress, port);
				//Server basicServer = serverQuery.GetBasicServerInfo();
                //serverQuery.Close();
                Rcon rconServer = new Rcon(ipAddress, port, "yolo");
                rconServer.Login();
//                string test = rconServer.Address;
//                Query query = new Query(ipAddress, port);
//                Console.WriteLine(query.IsConnected);
//
//                Server server = query.GetFullServerInfo();
//                Console.WriteLine(server.Address);
//                Console.WriteLine(server.GameType);
//                Console.WriteLine(server.Map);
//                Console.WriteLine(server.MaxPlayers);
//                Console.WriteLine(server.Motd);
//                Console.WriteLine(server.Port);
//                Console.WriteLine(server.Version);

                while (true)
                {
                    Console.WriteLine("Wpisz komende: ");
                    string input = Console.ReadLine();
                    rconServer.SendCommand(input);
                }

                //Console.WriteLine("Printing out server info: ");
                //Console.WriteLine("Server MOTD: {0}", basicServer.Motd);
                //Console.WriteLine("Server GameType: {0}", basicServer.GameType);
                //Console.WriteLine("Server Map: {0}", basicServer.Map);
                //Console.WriteLine("Server Player Count: {0}", basicServer.PlayerCount);
                //Console.WriteLine("Server Max Players: {0}", basicServer.MaxPlayers);
                //Console.WriteLine("Server Status: {0}", basicServer.IsOnline);

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
