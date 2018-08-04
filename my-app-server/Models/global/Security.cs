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
    }
}
