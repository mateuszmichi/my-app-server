using System;
using System.Collections.Generic;

namespace my_app_server.Models
{
    public partial class Admins
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public AdminsTokens AdminsTokens { get; set; }
    }
}
