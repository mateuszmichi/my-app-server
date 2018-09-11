using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public static class EquipmentControll
    {
        public static Items.ItemTypes[][] AcceptTypes = new Items.ItemTypes[][]
        {
            new Items.ItemTypes[]{ Items.ItemTypes.HELMET},
            new Items.ItemTypes[]{ Items.ItemTypes.RING},
            new Items.ItemTypes[]{ Items.ItemTypes.NECKLES},
            new Items.ItemTypes[]{ Items.ItemTypes.RING},
            new Items.ItemTypes[]{ Items.ItemTypes.GLOVES},
            new Items.ItemTypes[]{ Items.ItemTypes.ARMOUR},
            new Items.ItemTypes[]{ Items.ItemTypes.BRACELET},
            new Items.ItemTypes[]{ Items.ItemTypes.SINGLEHAND_WEAPON,Items.ItemTypes.DOUBLEHAND_WEAPON,Items.ItemTypes.SECONDARY_WEAPON},
            new Items.ItemTypes[]{ Items.ItemTypes.TROUSERS},
            new Items.ItemTypes[]{ Items.ItemTypes.SHIELD,Items.ItemTypes.SECONDARY_WEAPON},
            new Items.ItemTypes[]{ Items.ItemTypes.SHOES},
        };
        public static bool CanEquip(Items item, Heros hero, int targetSlot)
        {
            if(!AcceptTypes[targetSlot].Contains((Items.ItemTypes)item.ItemType))
            {
                return false;
            }
            return (hero.Lvl >= item.Lvl);
        }
    }
}
