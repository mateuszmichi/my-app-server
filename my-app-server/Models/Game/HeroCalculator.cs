using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public static class HeroCalculator
    {
        public static int InitialBackpackSize = 30;
        public static (int hp, int sl) HPSLmax(HeroResult Hero)
        {
            List<ItemResult> itemsOn = new List<ItemResult>();
            List<int?> itemsPossible = new List<int?>()
            {
                Hero.Equipment.Armour,Hero.Equipment.Bracelet,Hero.Equipment.FirstHand, Hero.Equipment.Gloves,Hero.Equipment.Helmet,
                Hero.Equipment.Neckles,Hero.Equipment.Ring1,
                Hero.Equipment.Ring2,Hero.Equipment.SecondHand,Hero.Equipment.Shoes,Hero.Equipment.Trousers
            };
            int[] attributes = (int[])Hero.Attributes.Clone();
            foreach (int? it in itemsPossible)
            {
                if (it.HasValue)
                {
                    var found = Hero.Equipment.KnownItems.FirstOrDefault(e => e.ItemID == it.Value);
                    if (found == null)
                    {
                        throw new Exception("Unknown item in equipment");
                    }
                    itemsOn.Add(found);
                }
            }
            foreach (ItemResult it in itemsOn)
            {
                for (int i = 0; i < 8; i++)
                {
                    attributes[i] += it.Attributes[i];
                }
            }
            return (Hero.Hpmax + (int)Math.Ceiling(attributes[1] / 2.0), (int)Math.Ceiling(Hero.Slmax * (1.0 + attributes[4] / 100)));
        }
        public static int PureMaxHP(int BaseHP, int[] Attr)
        {
            return BaseHP + (int)Math.Ceiling(Attr[1] / 2.0);
        }
        public static int BaseHP(int level)
        {
            return (int)Math.Ceiling(7.0 + level * (level + 5) / 2.0);
        }
    }
}
