using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
                return 25 * level * (level - 13) + 1650;
            }
        }
        public static int ExpForFight(int herolvl,int enemylvl)
        {
            int BaseExp = (enemylvl > 10) ? (50 * enemylvl - 300) : (20 * enemylvl);
            int BaseDivider = (herolvl < 10) ? (2 * herolvl) : (herolvl + 10);
            double Difficulty = 1.0 * enemylvl / herolvl;
            double Multiply = 1.0;
            if (herolvl < 10)
            {
                if (herolvl <= 5)
                {
                    if (herolvl < enemylvl)
                    {
                        Multiply = Difficulty;
                    }
                }
                else
                {
                    Multiply = (herolvl < enemylvl) ? (Difficulty * Difficulty) : Difficulty;
                }
            }
            else
            {
                Multiply = (herolvl < enemylvl) ? (1.0 + (enemylvl - herolvl) / 10) : (Difficulty * Difficulty);
            }
            return (int)Math.Ceiling(Multiply * BaseExp / BaseDivider);
        }
        public static Fighter GenHeroFightStats(Heros hero, my_appContext _context)
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
            int AttackMin = 0;
            int AttackMax = 0;
            int Armour = 0;
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
                AttackMin += it.DmgMin;
                AttackMax += it.DmgMax;
                Armour += it.Armour;
            }
            AttackMin += (int)Math.Ceiling(attributes[0] * 0.1);
            AttackMax += (int)Math.Ceiling(attributes[0] * 0.1);
            Armour += (int)Math.Floor(attributes[1] * 0.1);
            return new Fighter()
            {
                Strength = attributes[0],
                Endurance = attributes[1],
                Dexterity = attributes[2],
                Reflex = attributes[3],
                Wisdom = attributes[4],
                Intelligence = attributes[5],
                Charisma = attributes[6],
                Willpower = attributes[7],
                AttackMax = AttackMax,
                AttackMin = AttackMin,
                Armour = Armour,
                Hp = hero.Hp,
            };
        }
    }
}
