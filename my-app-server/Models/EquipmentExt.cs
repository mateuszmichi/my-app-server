using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public partial class Equipment
    {
        public EquipmentResult GenResult(Items[] usedItems, List<Backpack> back)
        {
            int?[] array = new int?[this.BackpackSize];
            for(int i = 0; i < this.BackpackSize; i++)
            {
                array[i] = null;
            }
            foreach(Backpack bp in back)
            {
                array[bp.SlotNr] = bp.ItemId;
            }
            ItemResult[] known = new ItemResult[usedItems.Length];
            for(int i = 0; i < usedItems.Length; i++)
            {
                known[i] = (ItemResult)usedItems[i];
            }
            return (new EquipmentResult()
            {
                Armour = this.Armour,
                Backpack = array,
                BackpackSize = this.BackpackSize,
                Bracelet = this.Bracelet,
                FirstHand = this.FirstHand,
                Gloves = this.Gloves,
                Helmet = this.Helmet,
                KnownItems = known,
                Neckles = this.Neckles,
                Ring1 = this.Ring1,
                Ring2 = this.Ring2,
                SecondHand = this.SecondHand,
                Shoes = this.Shoes,
                Trousers = this.Trousers,
                Money = this.Money,
            });
        }
        public static Equipment GenFreshEquipment(int heroID)
        {
            return new Equipment()
            {
                Armour = null,
                BackpackSize = HeroCalculator.InitialBackpackSize,
                Bracelet = null,
                FirstHand = null,
                Gloves = null,
                Helmet = null,
                HeroId = heroID,
                Money = 0,
                Neckles = null,
                Ring1 = null,
                Ring2 = null,
                SecondHand = null,
                Shoes = null,
                Trousers = null,
            };
        }
    }
}
