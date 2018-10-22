﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ObligatorioDA2.Data.Repositories.Interfaces;
using ObligatorioDA2.Data.Repositories;
using ObligatorioDA2.Services;
using ObligatorioDA2.Data.DataAccess;
using ObligatorioDA2.Services.Interfaces;

namespace ObligatorioDA2.WebAPI
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
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITeamRepository ,TeamRepository>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IMatchService, MatchService>();
            services.AddScoped<IMatchRepository, MatchRepository>();
            services.AddScoped<ISportRepository, SportRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFixtureService, FixtureService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
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