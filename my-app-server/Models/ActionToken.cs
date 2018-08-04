using System;
using System.Collections.Generic;

namespace my_app_server.Models
{
    public partial class ActionToken
    {
        public int HeroId { get; set; }
        public string HashedToken { get; set; }
        public DateTime ExpireDate { get; set; }

        public Heros Hero { get; set; }
    }
}
