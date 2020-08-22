// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Linq;
using HomeServing.SSO.Data;
using HomeServing.SSO.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using IdentityServer4.EntityFramework.Storage;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using System.Collections.Generic;

namespace HomeServing.SSO
{
    public static class SeedData
    {
        public static void EnsureSeedData(string connectionString)
        {
            var services = new ServiceCollection();

            services.AddLogging();
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddOperationalDbContext(options =>
            {
                options.ConfigureDbContext = db => db.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName));
            });
            services.AddConfigurationDbContext(options =>
            {
                options.ConfigureDbContext = db => db.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName));
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            // 迁移数据库
            MigrateDatabase(scope);
            // 创建角色
            CreateDefaultRoles(scope);
            // 创建默认用户
            CreateDefaultAdminUser(scope);
            // 创建客户端
            CreateClients(scope);
        }

        public static void CreateDefaultRoles(IServiceScope scope)
        {
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var defaultRoles = new List<IdentityRole>
            {
                new IdentityRole {Name = "Root"},
                new IdentityRole {Name = "Administrator"},
                new IdentityRole {Name = "Friends"},
                new IdentityRole {Name = "User"}
            };

            foreach (var role in defaultRoles)
            {
                var role_s = roleMgr.FindByNameAsync(role.Name).Result;

                if (role_s == null)
                {
                    var result = roleMgr.CreateAsync(role).Result;
                    if (!result.Succeeded)
                    {
                        Exception exception = new Exception(result.Errors.First().Description);
                        throw exception;
                    }

                    Log.Debug($"Role {role.Name} created.");
                }
                else
                {
                    var result = roleMgr.UpdateAsync(role).Result;

                    if (result.Succeeded)
                    {
                        Log.Debug($"Role {role.Name} already exists");
                    }
                }
            }
        }

        public static void CreateDefaultAdminUser(IServiceScope scope)
        {
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = userMgr.FindByNameAsync("Administrator").Result;
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = "Administrator",
                    NikeName = $"nick_administrator",
                    Bio = "这个人很懒, 什么都没有写.",
                    Avatar = "",
                    Gender = Gender.男,
                };

                var result = userMgr.CreateAsync(admin, "Password@1234").Result;
                if (!result.Succeeded)
                {
                    Exception exception = new Exception(result.Errors.First().Description);
                    throw exception;
                }

                AddDefaultAdminToRole(userMgr);

                Log.Debug("admin created");
            }
            else
            {
                Log.Debug("admin already exists");
            }
        }

        public static void AddDefaultAdminToRole(UserManager<ApplicationUser> userManager)
        {
            var user = userManager.FindByNameAsync("Administrator").Result;

            var result = userManager.AddToRoleAsync(user, "Root").Result;
            if (!result.Succeeded)
            {
                Exception exception = new Exception(result.Errors.First().Description);
                throw exception;
            }
        }

        public static void CreateClients(IServiceScope scope)
        {
            var configurationContext = scope.ServiceProvider.GetService<ConfigurationDbContext>();

            // 创建认证资源
            foreach (var resource in Config.IdentityResources.ToList())
            {
                var resource_c = configurationContext.IdentityResources.Where(c => c.Name == resource.ToEntity().Name).Count();

                if (resource_c == 0)
                {
                    configurationContext.IdentityResources.Add(resource.ToEntity());
                    Log.Debug("a identity resources created.");
                }
                else
                {
                    configurationContext.Update(configurationContext.IdentityResources.Where(c => c.Name == resource.ToEntity().Name).FirstOrDefault());
                    Log.Debug("identity resources already exists.");
                }
            }

            // 创建API Scope
            foreach (var resource in Config.ApiScopes.ToList())
            {
                var resource_c = configurationContext.ApiScopes.Where(c => c.Name == resource.ToEntity().Name).Count();

                if (resource_c == 0)
                {
                    configurationContext.ApiScopes.Add(resource.ToEntity());
                    Log.Debug("a api scope created.");
                }
                else
                {
                    configurationContext.Update(configurationContext.ApiScopes.Where(c => c.Name == resource.ToEntity().Name).FirstOrDefault());
                    Log.Debug("api scope already exists.");
                }
            }
            
            // 创建客户端
            foreach (var resource in Config.Clients())
            {
                var resource_c = configurationContext.Clients.Where(c => c.ClientId == resource.ToEntity().ClientId).Count();

                if (resource_c == 0)
                {
                    configurationContext.Clients.Add(resource.ToEntity());
                    Log.Debug("a clients created.");
                }
                else
                {
                    configurationContext.Update(configurationContext.Clients.Where(c => c.ClientId == resource.ToEntity().ClientId).FirstOrDefault());
                    Log.Debug("clients already exists.");
                }
            }

            configurationContext.SaveChanges();
        }

        public static void MigrateDatabase(IServiceScope scope)
        {
            scope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();
            scope.ServiceProvider.GetService<PersistedGrantDbContext>().Database.Migrate();
            scope.ServiceProvider.GetService<ConfigurationDbContext>().Database.Migrate();
        }
    }
}