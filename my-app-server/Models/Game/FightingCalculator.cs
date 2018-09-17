using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace my_app_server.Models
{
    public static class FightingCalculator
    {
        public enum AttackType
        {
            Normal,
            Strong,
            Defense,
        };
        private static readonly Random Rand = new Random();
        public static int RandomAttack(Fighter fighter)
        {
            return Rand.Next(fighter.AttackMin, fighter.AttackMax + 1);
        }
        public static int DmgDealt(Fighter attacker, Fighter victime, AttackType type, double ArmourMulti = 1.0)
        {
            double dmg = RandomAttack(attacker);
            double armour = ArmourMulti * victime.Armour;
            switch (type)
            {
                case AttackType.Defense:
                    dmg *= 0.75;
                    break;
                case AttackType.Strong:
                    dmg *= 1.5;
                    break;
            }
            return CalcDMG(dmg, armour);
        }
        public static double InitiativeCost(Fighter fighter, AttackType type)
        {
            double multi = 1.0;
            switch (type)
            {
                case AttackType.Defense:
                    multi = 0.75;
                    break;
                case AttackType.Strong:
                    multi = 2.0;
                    break;
            }
            return multi / (1 + fighter.Dexterity * 0.005 + fighter.Reflex * 0.001);
        }
        private static int CalcDMG(double dmg, double def)
        {
            return (int)Math.Ceiling(dmg * Math.Pow(4.0 / 9.0, def / dmg));
        }

        public static BattleLog[] MakeTurn(Fighting initial, Heros hero, Enemies enemy, AttackType action, my_appContext _context)
        {
            Fighter ally = HeroCalculator.GenHeroFightStats(hero, _context);
            Fighter vict = enemy.GetFighter(initial.EnemyHp);

            List<BattleLog> log = new List<BattleLog>();

            int dmg = DmgDealt(ally, vict, action);
            log.Add(new BattleLog() { Damage = dmg, Target = 1, AttackType = action });
            if (dmg >= vict.Hp)
            {
                initial.IsOver = true;
                initial.EnemyHp = 0;
                initial.Loot = GenerateLoot(enemy);
                return log.ToArray();
            }
            else
            {
                vict.Hp -= dmg;
                initial.Initiative -= InitiativeCost(ally, action);
                double armourMod = (action == AttackType.Defense) ? 1.4 : 1.0;
                int it = 0;
                while (initial.Initiative < 0)
                {
                    int rand = Rand.Next(0, 10);
                    AttackType aType = (rand == 0) ? AttackType.Strong : AttackType.Normal;
                    dmg = DmgDealt(vict, ally, aType, armourMod);
                    log.Add(new BattleLog() { Damage = dmg, Target = 0, AttackType = aType });
                    if (dmg >= hero.Hp)
                    {
                        initial.IsOver = true;
                        hero.Hp = 0;
                        initial.EnemyHp = vict.Hp;
                        return log.ToArray();
                    }
                    else
                    {
                        hero.Hp -= dmg;
                        initial.Initiative += InitiativeCost(vict, aType);
                    }
                    it++;
                    if (it > 10)
                    {
                        throw new OperationException("battleErr", "Enemy is attacking in infinite loop.");
                    }
                }
            }
            initial.EnemyHp = vict.Hp;
            return log.ToArray();
        }

        public static int? GenerateLoot(Enemies enemy)
        {
            if (enemy.Loot == "")
            {
                return null;
            }
            else
            {
                Fighting.LootCalculator loot = JsonConvert.DeserializeObject<Fighting.LootCalculator>(enemy.Loot);
                return loot.GenLoot(Rand);
            }
        }
    }
    public class Fighter
    {
        //hero.Strength, hero.Endurance, hero.Dexterity, hero.Reflex, hero.Wisdom, hero.Intelligence, hero.Charisma, hero.Willpower
        public int Strength { get; set; }
        public int Endurance { get; set; }
        public int Dexterity { get; set; }
        public int Reflex { get; set; }
        public int Wisdom { get; set; }
        public int Intelligence { get; set; }
        public int Charisma { get; set; }
        public int Willpower { get; set; }
        public int AttackMin { get; set; }
        public int AttackMax { get; set; }
        public int Armour { get; set; }
        public int Hp { get; set; }
    }
}
