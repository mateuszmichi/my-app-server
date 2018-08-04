using System;
using System.Collections.Generic;

namespace my_app_server.Models
{
    public partial class UsersHeros
    {
        public string UserName { get; set; }
        public int HeroId { get; set; }

        public Heros Hero { get; set; }
        public Users UserNameNavigation { get; set; }
    }
}
