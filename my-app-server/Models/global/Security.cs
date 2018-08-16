using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using my_app_server.Controllers;

namespace my_app_server.Models
{
    public static class Security
    {
        public static UserToken CheckUserToken(my_appContext context, UserTokenResult token)
        {
            return context.UserToken.FirstOrDefault(e => (e.UserName == token.UserName && e.HashedToken == token.Token));
        }
        public static UserToken GenerateUsersToken(string username, my_appContext _context)
        {
            DateTime time = DateTime.UtcNow;
            UserToken token;
            if ((token = _context.UserToken.FirstOrDefault(e => e.UserName == username)) == null)
            {
                token = UserToken.GenToken(username, time);
                _context.UserToken.Add(token);
            }
            else
            {
                UserToken ntoken = UserToken.GenToken(username, time);
                token.HashedToken = ntoken.HashedToken;
                token.ExpireDate = ntoken.ExpireDate;
            }
            return token;
        }
        public static ActionToken GenerateActionToken(int heroid, my_appContext _context)
        {
            DateTime time = DateTime.UtcNow;
            ActionToken token;
            if ((token = _context.ActionToken.FirstOrDefault(e => e.HeroId == heroid)) == null)
            {
                token = ActionToken.GenToken(heroid, time);
                _context.ActionToken.Add(token);
            }
            else
            {
                ActionToken ntoken = ActionToken.GenToken(heroid, time);
                token.HashedToken = ntoken.HashedToken;
                token.ExpireDate = ntoken.ExpireDate;
            }
            return token;
        }
    }
}
