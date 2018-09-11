using System;
using System.Collections.Generic;

namespace my_app_server.Models
{
    public partial class Backpack
    {
        public int HeroId { get; set; }
        public int SlotNr { get; set; }
        public int ItemId { get; set; }

        public Heros Hero { get; set; }
        public Items Item { get; set; }
    }
}
