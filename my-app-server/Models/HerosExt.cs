﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public partial class Heros
    {
        public static explicit operator HeroBrief(Heros hero)
        {
            return (new HeroBrief()
            {
                Name = hero.Name,
                Nickname = hero.Nickname,
                Orders = hero.Orders,
                Level = hero.Lvl,
            });
        }
        public HeroResult GenResult(EquipmentResult eqRes, object locRes, object statusData = null )
        {
            return (new HeroResult()
            {
                Name = this.Name,
                Nickname = this.Nickname,
                Orders = this.Orders,
                Level = this.Lvl,
                Attributes = new int[8] { this.Strength, this.Endurance, this.Dexterity, this.Reflex, this.Wisdom, this.Intelligence, this.Charisma, this.Willpower },
                Exp = this.Experience,
                Hp = this.Hp,
                Hpmax = HeroCalculator.BaseHP(this.Lvl),
                Sl = this.Sl,
                Slmax = this.Slbase,
                Equipment = eqRes,
                Location = locRes,
                Status = this.Status,
                StatusData = statusData,
                IsInvitational = this.Invitational,
                VelocityFactor = this.VelocityFactor,
            });
        }
    }

    public class HeroBrief
    {
        public string Name { get; set; }
        public string Nickname { get; set; }
        public int Orders { get; set; }
        public int Level { get; set; }
    }
    public class HeroResult : HeroBrief
    {
        public int[] Attributes { get; set; }
        public int Hp { get; set; }
        public int Hpmax { get; set; }
        public int Sl { get; set; }
        public int Slmax { get; set; }
        public int Exp { get; set; }
        public EquipmentResult Equipment { get; set; }
        public object Location { get; set; }
        public int Status { get; set; }
        public object StatusData { get; set; }
        public int VelocityFactor { get; set; }
        public bool IsInvitational { get; set; }
    }
}
