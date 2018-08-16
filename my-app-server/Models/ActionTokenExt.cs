using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public partial class ActionToken
    {
        public static ActionToken GenToken(int heroid, DateTime time)
        {
            return new ActionToken()
            {
                ExpireDate = time.AddMinutes(ServerOptions.ActionTokenMinutes),
                HashedToken = HashClass.GenToken(),
                HeroId = heroid,
            };
        }
        public void UpdateToken(DateTime time)
        {
            if (!this.IsTimeValid(time))
            {
                throw new Exception("Updating expired token");
            }
            this.ExpireDate = time.AddMinutes(ServerOptions.ActionTokenMinutes);
        }
        public bool IsTimeValid(DateTime time)
        {
            return (DateTime.Compare(this.ExpireDate, time) >= 0);
        }
    }
}
