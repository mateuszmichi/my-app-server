using System;
using System.Collections.Generic;

namespace my_app_server.Models
{
    public partial class Enemies
    {
        public Enemies()
        {
            Fighting = new HashSet<Fighting>();
        }

        public int EnemyId { get; set; }
        public int GraphicsId { get; set; }
        public string EnemyName { get; set; }
        public int Lvl { get; set; }
        public int MaxHp { get; set; }
        public int Strength { get; set; }
        public int Endurance { get; set; }
        public int Dexterity { get; set; }
        public int Reflex { get; set; }
        public int Willpower { get; set; }
        public int Wisdom { get; set; }
        public int Intelligence { get; set; }
        public int Charisma { get; set; }
        public int MinAttack { get; set; }
        public int MaxAttack { get; set; }
        public int Armour { get; set; }
        public string Loot { get; set; }

        public ICollection<Fighting> Fighting { get; set; }
    }
}
