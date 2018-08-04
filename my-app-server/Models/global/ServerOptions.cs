using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    static public class ServerOptions
    {
        public static readonly int PermamentTokenDays = 14;
        public static readonly int ActionTokenMinutes = 20;
        public static readonly int MaxHerosPerAccount = 5;
    }
}
