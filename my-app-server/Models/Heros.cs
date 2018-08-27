using System;
using System.Collections.Generic;

namespace my_app_server.Models
{
    public partial class Heros
    {
        public Heros()
        {
            Backpack = new HashSet<Backpack>();
            HerosLocations = new HashSet<HerosLocations>();
        }

        public int HeroId { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public int Lvl { get; set; }
        public int Experience { get; set; }
        public int Hp { get; set; }
        public int Sl { get; set; }
        public int Country { get; set; }
        public int Origin { get; set; }
        public int Strength { get; set; }
        public int Endurance { get; set; }
        public int Dexterity { get; set; }
        public int Reflex { get; set; }
        public int Willpower { get; set; }
        public int Wisdom { get; set; }
        public int Intelligence { get; set; }
        public int Charisma { get; set; }
        public int Orders { get; set; }
        public int CurrentLocation { get; set; }
        public int Status { get; set; }

        public ActionToken ActionToken { get; set; }
        public Equipment Equipment { get; set; }
        public Traveling Traveling { get; set; }
        public UsersHeros UsersHeros { get; set; }
        public ICollection<Backpack> Backpack { get; set; }
        public ICollection<HerosLocations> HerosLocations { get; set; }
    }
}
