﻿using HomeServing.SSO.Data;
using HomeServing.SSO.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Aliyun.OSS;
using AutoMapper;
using HomeServing.SSO.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Http;

namespace HomeServing.SSO
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    options.EmitStaticAudienceClaim = true;
                })
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString, b => b.MigrationsAssembly("HomeServing.SSO"));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString, b => b.MigrationsAssembly("HomeServing.SSO"));

                    options.EnableTokenCleanup = true;
                })
                .AddAspNetIdentity<ApplicationUser>()
                .AddDeveloperSigningCredential(persistKey: true, filename: Configuration["RSASigningCredential"]);

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 3;
                options.Password.RequiredUniqueChars = 1;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = false;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSingleton(new OssClient(Configuration["AliyunOSS:Endpoint"],
                Configuration["AliyunOSS:AccessKeyId"],
                Configuration["AliyunOSS:AccessKeySecret"]));

            services.AddAutoMapper(typeof(AutoMapperConfig));
            services.AddSingleton(new FileExtensionContentTypeProvider());

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}