using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Restaurant_Website.Data;
using Microsoft.EntityFrameworkCore;
using MySql.Data.EntityFrameworkCore.Extensions;
using Restaurant_Website.Models;
using Microsoft.AspNetCore.Identity;

namespace Restaurant_Website
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
            services.AddDistributedMemoryCache();

            services.AddDbContext<MvcFoodContext>(options => options.UseMySQL(Configuration.GetConnectionString("MvcFoodContext")));
            services.AddDbContext<CartContext>(options => options.UseMySQL(Configuration.GetConnectionString("MvcFoodContext")));
            services.AddDbContext<UserContext>(options => options.UseMySQL(Configuration.GetConnectionString("MvcFoodContext")));
            services.AddDbContext<OrderContext>(options => options.UseMySQL(Configuration.GetConnectionString("MvcFoodContext")));
            services.AddDbContext<WorkTimeContext>(options => options.UseMySQL(Configuration.GetConnectionString("MvcFoodContext")));

            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<UserContext>()
            .AddDefaultTokenProviders();

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddScoped<IAccountLogic, AccountLogic>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
