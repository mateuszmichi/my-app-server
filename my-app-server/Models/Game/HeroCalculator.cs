using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public static class HeroCalculator
    {
        public static int InitialBackpackSize = 30;
        public static (int hp, int sl) HPSLmax(Heros hero, my_appContext _context)
        {
            Equipment Equipment = _context.Equipment.FirstOrDefault(e => e.HeroId == hero.HeroId);
            List<Items> itemsOn = new List<Items>();
            List<int?> itemsPossible = new List<int?>()
            {
                Equipment.Armour,Equipment.Bracelet,Equipment.FirstHand, Equipment.Gloves,Equipment.Helmet,
                Equipment.Neckles,Equipment.Ring1,
                Equipment.Ring2,Equipment.SecondHand,Equipment.Shoes,Equipment.Trousers
            };
            int[] attributes = new int[8] { hero.Strength, hero.Endurance, hero.Dexterity, hero.Reflex, hero.Wisdom, hero.Intelligence, hero.Charisma, hero.Willpower };
            foreach (int? it in itemsPossible)
            {
                if (it.HasValue)
                {
                    var found = _context.Items.FirstOrDefault(e => e.ItemId == it.Value);
                    if (found == null)
                    {
                        throw new Exception("Unknown item in equipment");
                    }
                    itemsOn.Add(found);
                }
            }
            foreach (Items it in itemsOn)
            {
                int[] itAttr = new int[8] { it.Strength, it.Endurance, it.Dexterity, it.Reflex, it.Wisdom, it.Intelligence, it.Charisma, it.Willpower };
                for (int i = 0; i < 8; i++)
                {
                    attributes[i] += itAttr[i];
                }
            }
            return (BaseHP(hero.Lvl) + (int)Math.Ceiling(attributes[1] / 2.0), (int)Math.Ceiling(hero.Slbase * (1.0 + attributes[4] / 100)));
        }
        public static int PureMaxHP(int BaseHP, int[] Attr)
        {
            return BaseHP + (int)Math.Ceiling(Attr[1] / 2.0);
        }
        public static int BaseHP(int level)
        {
            return (int)Math.Ceiling(7.0 + level * (level + 5) / 2.0);
        }
        public static int RecoveryTime(int HPMax, int HP, int lvl)
        {
            double scale = (lvl > 10) ? (16.0 * 60.0 / BaseHP(lvl)) : (60.0 * (1.3 * lvl + 1.7) / BaseHP(lvl));
            double time = scale * (HPMax - HP);
            return (int)Math.Ceiling(time);
        }
        public static bool CheckHeroHP(Heros hero, my_appContext _context)
        {
            var (hp, sl) = HeroCalculator.HPSLmax(hero, _context);
            if(hero.Hp > hp)
            {
                hero.Hp = hp;
                return false;
            }
            return true;
        }
        public static bool CheckHeroHP(Heros hero, int MaxHP)
        {
            if(hero.Hp > MaxHP)
            {
                hero.Hp = MaxHP;
                return false;
            }
            return true;
        }
        public static int ExpToLevel(int level)
        {
            if (level <= 10)
            {
                return 10 * (level * level - level);
            }
            else
            {
                return 25 * level * (level - 13 * level) + 1650;
            }
        }
    }
}
