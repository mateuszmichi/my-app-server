using System;
using System.Collections.Generic;

namespace my_app_server.Models
{
    public partial class Healing
    {
        public int HeroId { get; set; }
        public int StartHp { get; set; }
        public int StartHpmax { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public Heros Hero { get; set; }
    }
}
