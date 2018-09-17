using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public partial class Enemies
    {
        public Fighter GetFighter(int currenthp)
        {
            return new Fighter()
            {
                Armour = this.Armour,
                AttackMax = this.MaxAttack,
                AttackMin = this.MinAttack,
                Charisma = this.Charisma,
                Dexterity = this.Dexterity,
                Endurance = this.Endurance,
                Hp = currenthp,
                Intelligence = this.Intelligence,
                Reflex = this.Reflex,
                Strength = this.Strength,
                Willpower = this.Willpower,
                Wisdom = this.Wisdom,
            };
        }
    }
}
