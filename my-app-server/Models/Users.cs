using System;
using System.Collections.Generic;

namespace my_app_server.Models
{
    public partial class Users
    {
        public Users()
        {
            UsersHeros = new HashSet<UsersHeros>();
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime RegistryDate { get; set; }
        public DateTime LastLogin { get; set; }

        public Tokens Tokens { get; set; }
        public UserToken UserToken { get; set; }
        public ICollection<UsersHeros> UsersHeros { get; set; }
    }
}
