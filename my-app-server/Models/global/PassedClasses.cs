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
        public double CurrentDuration { get; set; }
        public double FullDuration { get; set; }
        public bool IsReverse { get; set; }
        public double? ReverseDuration { get; set; }
    }
    public class EquipmentResult
    {
        public ItemResult[] KnownItems { get; set; }
        public int?[] Backpack { get; set; }
        public int BackpackSize { get; set; }
        public int? FirstHand { get; set; }
        public int? SecondHand { get; set; }
        public int? Armour { get; set; }
        public int? Helmet { get; set; }
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
        public int Armour { get; set; }
        public int DmgMin { get; set; }
        public int DmgMax { get; set; }
    }
    public class EquipmentModifyResult
    {
        public EquipmentModification[] Removed { get; set; }
        public EquipmentModification[] Added { get; set; }
        public class EquipmentModification
        {
            public string Target { get; set; }
            public int? ItemID { get; set; }
        }
    }
    public class HealingResult
    {
        public double CurrentDuration { get; set; }
        public double FullDuration { get; set; }
        public int InitialHP { get; set; }
        public int FinalHP { get; set; }
    }
    public class FightResult
    {
        public string EnemyName { get; set; }
        public int EnemyID { get; set; }
        public int EnemyLevel { get; set; }
        public int Hp { get; set; }
        public int HpMax { get; set; }
        public bool IsOver { get; set; }
        public ItemResult Loot { get; set; }
        public int Experience { get; set; }
        public BattleLog[] Log { get; set; }
    }
    public class BattleLog
    {
        public int Target { get; set; }
        public int Damage { get; set; }
        public FightingCalculator.AttackType AttackType { get; set; }
    }
}
