using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public partial class Traveling
    {
        public bool HasEnded(DateTime now)
        {
            if (this.IsReverse)
            {
                TimeSpan wte = this.ReverseTime.Value.Subtract(this.StartTime);
                TimeSpan spowrotem = now.Subtract(this.ReverseTime.Value);
                return TimeSpan.Compare(wte, spowrotem) <= 0;
            }
            else
            {
                return DateTime.Compare(this.EndTime, now) <= 0;
            }
        }
        public int UpdatedLocationID()
        {
            return (this.IsReverse) ? this.Start : this.Target;
        }

        public TravelResult GenTravelResult(DateTime now)
        {
            double? rev = null;
            if (this.ReverseTime.HasValue)
            {
                rev = (this.ReverseTime.Value - this.StartTime).TotalMilliseconds;
            }
            return (new TravelResult()
            {
                FullDuration = (this.EndTime - this.StartTime).TotalMilliseconds,
                IsReverse = this.IsReverse,
                ReverseDuration = rev,
                StartName = this.StartName,
                CurrentDuration = (now - this.StartTime).TotalMilliseconds,
                TargetName = this.TargetName,
            });
        }
    }
}
