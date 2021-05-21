using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PavolsBookStore.Models.DataLayer;
using PavolsBookStore.Models.DomainModels;

namespace PavolsBookStore
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
      services.AddRouting(options => options.LowercaseUrls = true);
      services.AddMemoryCache();
      services.AddSession();

      services.AddControllersWithViews().AddNewtonsoftJson();

      services.AddDbContext<BookstoreContext>(options =>
          options.UseSqlServer(Configuration.GetConnectionString("BookstoreContext")).ReplaceService<IQueryTranslationPostprocessorFactory, SqlServer2008QueryTranslationPostprocessorFactory>());
             

            services.AddIdentity<User, IdentityRole>(options =>
       {
         options.Password.RequiredLength = 6;
         options.Password.RequireNonAlphanumeric = false;
         options.Password.RequireDigit = false;
       }).AddEntityFrameworkStores<BookstoreContext>()
         .AddDefaultTokenProviders();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseDeveloperExceptionPage();

      app.UseStaticFiles();

      app.UseRouting();      

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseSession();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapAreaControllerRoute(
          name: "admin",
          areaName: "Admin",
          pattern: "Admin/{controller=Book}/{action=Index}/{id?}");

        //paging, sorting, filtering
        endpoints.MapControllerRoute(
          name: "",
          pattern: "{controller}/{action}/page/{pagenumber}/size/{pagesize}/sort/{sortfield}/{sortdirection}/filter/{author}/{genre}/{price}");

        //paging and sorting
        endpoints.MapControllerRoute(
          name: "",
          pattern: "{controller}/{action}/page/{pagenumber}/size/{pagesize}/sort/{sortfield}/{sortdirection}");

        endpoints.MapControllerRoute(
                  name: "default",
                  pattern: "{controller=Home}/{action=Index}/{id?}/{slug?}");
      });

      BookstoreContext.CreateAdminUser(app.ApplicationServices).Wait();
    }
  }
}
