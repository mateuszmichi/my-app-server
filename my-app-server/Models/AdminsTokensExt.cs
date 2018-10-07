using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public partial class AdminsTokens
    {
        public static AdminsTokens GenToken(string username)
        {
            return new AdminsTokens()
            {
                Login = username,
                Token = HashClass.GenToken(),
            };
        }
    }
}
