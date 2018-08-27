using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public class PassedData<T>
    {
        public UserTokenResult UserToken { get; set; }
        public T Data { get; set; }
    }
    public class PassedGameData<T>
    {
        public UserTokenResult UserToken { get; set; }
        public ActionTokenResult ActionToken { get; set; }
        public T Data { get; set; }
    }
    public class PernamentTokenResult
    {
        public string TokenName { get; set; }
        public string Token { get; set; }
        public DateTime ExpireDate { get; set; }
    }
    public class UserTokenResult
    {
        public string UserName { get; set; }
        public string Token { get; set; }
    }
    public class UserBrief
    {
        public string Username { get; set; }
        public HeroBrief[] Characters { get; set; }
    }
    public class ActionTokenResult
    {
        public string HeroName { get; set; }
        public string Token { get; set; }
    }
    public class TravelResult
    {
        public string StartName { get; set; }
        public string TargetName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsReverse { get; set; }
        public DateTime? ReverseTime { get; set; }
    }
    public class EquipmentResult
    {
        public ItemResult[] KnownItems { get; set; }
        public int?[] Backpack { get; set; }
        public int BackpackSize { get; set; }
        public int? FirstHand { get; set; }
        public int? SecondHand { get; set; }
        public int? Armour { get; set; }
        public int? Trousers { get; set; }
        public int? Shoes { get; set; }
        public int? Gloves { get; set; }
        public int? Ring1 { get; set; }
        public int? Ring2 { get; set; }
        public int? Neckles { get; set; }
        public int? Bracelet { get; set; }
        public int Money { get; set; }
    }
    public class ItemResult
    {
        public int ItemID { get; set; }
        public Items.ItemTypes ItemType { get; set; }
        public string Name { get; set; }
        public int[] Attributes { get; set; }
        public int Lvl { get; set; }
        public int PrimaryAttr { get; set; }
        public int SecondaryAttr { get; set; }
    }
}
