using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using my_app_server.Models;

namespace my_app_server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //TODO: Important whole in security
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                    });
            });

            services.AddMvc();

            //string path = @"Server=DESKTOP-4906ULP\SQLEXPRESS;Database=my-app;Trusted_Connection=True;";
            //string path2 = @"Server=tcp:shatteredplains.database.windows.net,1433;Initial Catalog=shatteredDB;Persist Security Info=False;User ID=mateuszmichi;Password=Edek123$;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            string path = @"Data Source=SQL6004.site4now.net;Initial Catalog=DB_A40773_shatteredplains;User Id=DB_A40773_shatteredplains_admin;Password=MiOnb55AS1;";
            services.AddDbContext<my_appContext>(options => options.UseSqlServer(path));
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("AllowAll");

            app.UseMvc();
        }
    }
}
