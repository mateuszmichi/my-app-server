using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public partial class Tokens
    {
        public static Tokens GenToken(string username, DateTime time)
        {
            return new Tokens()
            {
                CreateDate = time,
                ExpireDate = time.AddDays(ServerOptions.PermamentTokenDays),
                HashedToken = HashClass.GenToken(),
                TokenName = HashClass.GenToken(),
                UpdateDate = time,
                UserName = username
            };
        }
        public void UpdateToken(DateTime time)
        {
            if (!this.IsTimeValid(time))
            {
                throw new Exception("Updating expired token");
            }
            this.ExpireDate = time.AddDays(ServerOptions.PermamentTokenDays);
            this.UpdateDate = time;
        }
        public bool IsTimeValid(DateTime time)
        {
            return (DateTime.Compare(this.ExpireDate, time) >= 0);
        }
    }
}
