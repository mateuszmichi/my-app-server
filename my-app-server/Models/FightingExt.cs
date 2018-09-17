using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public partial class Fighting
    {
        public FightResult GenResult(my_appContext _context, Heros hero, Enemies enemy)
        {
            ItemResult resItem = null;
            int Exp = 0;
            if(this.IsOver && this.Loot.HasValue)
            {
                var ItemEq = _context.Items.FirstOrDefault(e => e.ItemId == this.Loot.Value);
                if(ItemEq == null)
                {
                    throw new OperationException("lootErr", "Unknown looted item.");
                }
                resItem = (ItemResult)ItemEq;
            }
            if (this.IsOver && this.EnemyHp <= 0)
            {
                Exp = HeroCalculator.ExpForFight(hero.Lvl, enemy.Lvl);
            }
            return new FightResult()
            {
                EnemyID = enemy.GraphicsId,
                EnemyLevel = enemy.Lvl,
                Hp = this.EnemyHp,
                HpMax = enemy.MaxHp,
                IsOver = this.IsOver,
                Loot = resItem,
                EnemyName = enemy.EnemyName,
                Log = new BattleLog[0],
                Experience = Exp,
            };
        }
        public FightResult GenResult(my_appContext _context, Heros hero)
        {
            Enemies enemy = _context.Enemies.FirstOrDefault(e => e.EnemyId == this.EnemyId);
            if(enemy == null)
            {
                throw new OperationException("fightingErr", "Unknown enemy.");
            }
            // TODO item?
            return GenResult(_context,hero, enemy);
        }
        public class LootCalculator
        {
            public int GeneralProb { get; set; }
            public ItemProb[] Items { get; set; }

            public int? GenLoot(Random rand)
            {
                int los = rand.Next(0, 100);
                if (los < this.GeneralProb)
                {
                    int maxfactor = 0;
                    foreach(ItemProb it in this.Items)
                    {
                        maxfactor += it.Factor;
                    }
                    los = rand.Next(0, maxfactor);
                    int i = 0;
                    maxfactor = 0;
                    while(los>maxfactor)
                    {
                        maxfactor += this.Items[i++].Factor;
                    }
                    return this.Items[i].ItemID;
                }
                else
                {
                    return null;
                }
            }
            public struct ItemProb
            {
                public int ItemID;
                public int Factor;
            }
        }
    }
}
