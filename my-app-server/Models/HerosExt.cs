using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public partial class Heros
    {
        public static explicit operator HeroBrief(Heros hero)
        {
            return (new HeroBrief()
            {
                Name = hero.Name,
                Nickname = hero.Nickname,
                Orders = hero.Orders,
                Level = hero.Lvl,
            });
        }
        public static explicit operator HeroResult(Heros hero)
        {
            return (new HeroResult()
            {
                Name = hero.Name,
                Nickname = hero.Nickname,
                Orders = hero.Orders,
                Level = hero.Lvl,
                Attributes = new int[8] { hero.Strength, hero.Endurance, hero.Dexterity, hero.Reflex, hero.Wisdom, hero.Intelligence, hero.Charisma, hero.Willpower },
                Exp = hero.Experience,
                Hp = hero.Hp,
                //TODO calculate max hp
                Hpmax = hero.Hp,
                Sl = hero.Sl,
                Slmax = hero.Sl,
            });
        }
    }

    public class HeroBrief
    {
        public string Name { get; set; }
        public string Nickname { get; set; }
        public int Orders { get; set; }
        public int Level { get; set; }
    }
    public class HeroResult : HeroBrief
    {
        public int[] Attributes { get; set; }
        public int Hp { get; set; }
        public int Hpmax { get; set; }
        public int Sl { get; set; }
        public int Slmax { get; set; }
        public int Exp { get; set; }
    }
}
