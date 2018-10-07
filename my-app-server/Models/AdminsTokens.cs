using System;
using System.Collections.Generic;

namespace my_app_server.Models
{
    public partial class AdminsTokens
    {
        public string Login { get; set; }
        public string Token { get; set; }

        public Admins LoginNavigation { get; set; }
    }
}
