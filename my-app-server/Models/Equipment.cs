using System;
using System.Collections.Generic;

namespace my_app_server.Models
{
    public partial class Equipment
    {
        public int HeroId { get; set; }
        public int BackpackSize { get; set; }
        public int? FirstHand { get; set; }
        public int? SecondHand { get; set; }
        public int? Armour { get; set; }
        public int? Helmet { get; set; }
        public int? Trousers { get; set; }
        public int? Shoes { get; set; }
        public int? Gloves { get; set; }
        public int? Ring1 { get; set; }
        public int? Ring2 { get; set; }
        public int? Neckles { get; set; }
        public int? Bracelet { get; set; }
        public int Money { get; set; }

        public Items ArmourNavigation { get; set; }
        public Items BraceletNavigation { get; set; }
        public Items FirstHandNavigation { get; set; }
        public Items GlovesNavigation { get; set; }
        public Heros Hero { get; set; }
        public Items NecklesNavigation { get; set; }
        public Items Ring1Navigation { get; set; }
        public Items Ring2Navigation { get; set; }
        public Items SecondHandNavigation { get; set; }
        public Items ShoesNavigation { get; set; }
        public Items TrousersNavigation { get; set; }
    }
}
