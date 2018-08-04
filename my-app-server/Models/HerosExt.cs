using System;
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
    }

    public class HeroBrief
    {
        public string Name { get; set; }
        public string Nickname { get; set; }
        public int Orders { get; set; }
        public int Level { get; set; }
    }
}
