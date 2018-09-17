using System;
using System.Collections.Generic;

namespace my_app_server.Models
{
    public partial class Fighting
    {
        public int HeroId { get; set; }
        public int EnemyId { get; set; }
        public int EnemyHp { get; set; }
        public bool IsOver { get; set; }
        public int? Loot { get; set; }
        public double Initiative { get; set; }

        public Enemies Enemy { get; set; }
        public Heros Hero { get; set; }
    }
}
