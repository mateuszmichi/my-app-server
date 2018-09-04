using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public partial class Items
    {
        public enum ItemTypes
        {
            SINGLEHAND_WEAPON,
            HELMET,
            ARMOUR,
            TROUSERS,
            SHOES,
            GLOVES,
            RING,
            NECKLES,
            BRACELET,
            DOUBLEHAND_WEAPON,
            SECONDARY_WEAPON,
            SHIELD,
        }
        public static explicit operator ItemResult(Items item)
        {
            return (new ItemResult()
            {
                ItemID = item.ItemId,
                ItemType = (ItemTypes)item.ItemType,
                Name = item.Name,
                Attributes = new int[8] { item.Strength, item.Endurance, item.Dexterity, item.Reflex, item.Wisdom, item.Intelligence, item.Charisma, item.Willpower },
                Lvl = item.Lvl,
                DmgMin = item.DmgMin,
                DmgMax = item.DmgMax,
                Armour = item.Armour,
            });
        }
    }
}
