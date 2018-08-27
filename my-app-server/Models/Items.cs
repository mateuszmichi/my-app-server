using System;
using System.Collections.Generic;

namespace my_app_server.Models
{
    public partial class Items
    {
        public Items()
        {
            Backpack = new HashSet<Backpack>();
            EquipmentArmourNavigation = new HashSet<Equipment>();
            EquipmentBraceletNavigation = new HashSet<Equipment>();
            EquipmentFirstHandNavigation = new HashSet<Equipment>();
            EquipmentGlovesNavigation = new HashSet<Equipment>();
            EquipmentNecklesNavigation = new HashSet<Equipment>();
            EquipmentRing1Navigation = new HashSet<Equipment>();
            EquipmentRing2Navigation = new HashSet<Equipment>();
            EquipmentSecondHandNavigation = new HashSet<Equipment>();
            EquipmentShoesNavigation = new HashSet<Equipment>();
            EquipmentTrousersNavigation = new HashSet<Equipment>();
        }

        public int ItemId { get; set; }
        public int ItemType { get; set; }
        public string Name { get; set; }
        public int Lvl { get; set; }
        public int Strength { get; set; }
        public int Endurance { get; set; }
        public int Dexterity { get; set; }
        public int Reflex { get; set; }
        public int Willpower { get; set; }
        public int Wisdom { get; set; }
        public int Intelligence { get; set; }
        public int Charisma { get; set; }
        public string Modifier { get; set; }
        public int PrimaryAttr { get; set; }
        public int SecondaryAttr { get; set; }

        public ICollection<Backpack> Backpack { get; set; }
        public ICollection<Equipment> EquipmentArmourNavigation { get; set; }
        public ICollection<Equipment> EquipmentBraceletNavigation { get; set; }
        public ICollection<Equipment> EquipmentFirstHandNavigation { get; set; }
        public ICollection<Equipment> EquipmentGlovesNavigation { get; set; }
        public ICollection<Equipment> EquipmentNecklesNavigation { get; set; }
        public ICollection<Equipment> EquipmentRing1Navigation { get; set; }
        public ICollection<Equipment> EquipmentRing2Navigation { get; set; }
        public ICollection<Equipment> EquipmentSecondHandNavigation { get; set; }
        public ICollection<Equipment> EquipmentShoesNavigation { get; set; }
        public ICollection<Equipment> EquipmentTrousersNavigation { get; set; }
    }
}
