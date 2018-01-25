using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCQuery
{
    public class Server
    {
        public string Name { get; set; }
        public List<String> PlayerList { get; set; }
        public string Motd { get; set; }
        public string GameMode { get; set; }
        public bool IsOnline { get; set; }

        public Server(string name, bool isOnline)
        {
            if (isOnline)
            {

            }
        }

        public Server(string name, string gamemode, bool status, List<String> playerlist)
        {

        }
    }
}
