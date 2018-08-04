using System;
using System.Collections.Generic;

namespace my_app_server.Models
{
    public partial class UserToken
    {
        public string UserName { get; set; }
        public string HashedToken { get; set; }
        public DateTime ExpireDate { get; set; }

        public Users UserNameNavigation { get; set; }
    }
}
