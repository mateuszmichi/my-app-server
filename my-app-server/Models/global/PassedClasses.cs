using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public class PassedData<T>
    {
        public UserTokenResult UserToken { get; set; }
        public T Data { get; set; }
    }
    public class PernamentTokenResult
    {
        public string TokenName { get; set; }
        public string Token { get; set; }
        public DateTime ExpireDate { get; set; }
    }
    public class UserTokenResult
    {
        public string UserName { get; set; }
        public string Token { get; set; }
    }
    public class UserBrief
    {
        public string Username { get; set; }
        public HeroBrief[] Characters { get; set; }
    }
    public class ActionTokenResult
    {
        public string HeroName { get; set; }
        public string Token { get; set; }
    }
}
