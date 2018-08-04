using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public partial class my_appContext
    {
        public my_appContext(DbContextOptions<my_appContext> options) : base(options)
        {

        }
    }
}
