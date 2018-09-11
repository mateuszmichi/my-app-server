using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public partial class Healing
    {
        public bool HasEnded(DateTime now)
        {
            return DateTime.Compare(this.EndTime, now) <= 0;
        }
        public int FinalHealth(DateTime now)
        {
            if (this.HasEnded(now))
            {
                return this.StartHpmax;
            }
            else
            {
                var last = now - StartTime;
                var would = EndTime - StartTime;
                return (int)Math.Floor((double)(StartHpmax - StartHp) * last.TotalMilliseconds / would.TotalMilliseconds + StartHp);
            }
        }
        public HealingResult GenHealingResult(DateTime now)
        {
            return (new HealingResult()
            {
                FullDuration = (this.EndTime - this.StartTime).TotalMilliseconds,
                CurrentDuration = (now - this.StartTime).TotalMilliseconds,
                FinalHP = this.StartHpmax,
                InitialHP = this.StartHp,
            });
        }
    }
}
