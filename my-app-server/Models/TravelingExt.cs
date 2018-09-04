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

        public static explicit operator TravelResult(Traveling travel)
        {
            DateTime? rev = null;
            if (travel.ReverseTime.HasValue)
            {
                rev = ChangeTimeToPureUTC(travel.ReverseTime.Value);
            }
            return (new TravelResult()
            {
                EndTime = ChangeTimeToPureUTC(travel.EndTime),
                IsReverse = travel.IsReverse,
                ReverseTime = rev,
                StartName = travel.StartName,
                StartTime = ChangeTimeToPureUTC(travel.StartTime),
                TargetName = travel.TargetName,
            });
        }
        private static DateTime ChangeTimeToPureUTC(DateTime time)
        {
            string s = time.ToString();
            var dateTimeOffset = DateTimeOffset.Parse(s, null);
            return dateTimeOffset.DateTime;
        }
    }
}
