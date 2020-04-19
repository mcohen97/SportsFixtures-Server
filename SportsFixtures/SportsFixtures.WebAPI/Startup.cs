using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Data.Repositories;
using ObligatorioDA2.Services;
using ObligatorioDA2.Data.DataAccess;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.WebAPI.Controllers;
using System.Diagnostics.CodeAnalysis;
using ObligatorioDA2.Services.Logging;

namespace ObligatorioDA2.WebAPI
{
    [ExcludeFromCodeCoverage]
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
            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = "http://localhost:5000",
                ValidAudience = "http://localhost:5000",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345"))
            };
        });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddDbContext<DatabaseConnection>(options => options.UseSqlServer(Configuration.GetConnectionString("ObligatorioDA2")));
            services.AddDbContext<LoggingContext>(options => options.UseSqlServer(Configuration.GetConnectionString("LogDB")));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITeamRepository ,TeamRepository>();
            services.AddScoped<ILogInfoRepository, LogInfoRepository>();
            services.AddScoped<ILoggerService, LoggerService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ILogInService, AuthenticationService>();
            services.AddScoped<IInnerEncounterService, EncounterService>();
            services.AddScoped<IEncounterService, EncounterService>();
            services.AddScoped<IEncounterRepository, EncounterRepository>();
            services.AddScoped<ISportRepository, SportRepository>();
            services.AddScoped<ISportService, SportService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFixtureService, FixtureService>();
            services.AddScoped<ISportTableService, SportTableService>();
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<IImageService>(provider => new ImageService(Configuration.GetSection("TeamsImages").GetValue<string>("DirPath")));
            services.Configure<FixtureStrategies>(Configuration.GetSection("FixtureStrategies"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors("MyPolicy");
        
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
