// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using BlogIdp.Data;
using BlogIdp.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlogIdp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }
        // 此方法由运行时调用。使用此方法向容器添加服务。
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            //配置IdentityServer4
            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
                .AddInMemoryIdentityResources(Config.GetIdentityResources()) //Identity资源配置  
                .AddInMemoryApiResources(Config.GetApis()) //Api资源配置
                .AddInMemoryClients(Config.GetClients()) //客户端资源配置
                .AddAspNetIdentity<ApplicationUser>(); //Identity管理用户配置

            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }
            //添加HTTPS重定向服务。
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                options.HttpsPort = 5001;
            });
            //添加Hsts服务。
            services.AddHsts(option =>
            {
                option.Preload = true;
                option.IncludeSubDomains = true;
                option.MaxAge = TimeSpan.FromDays(60);
            });

            services.AddAuthentication()
                .AddGoogle(options =>   //第三方身份认证
                {
                    // register your IdentityServer with Google at https://console.developers.google.com
                    // enable the Google+ API
                    // set the redirect URI to http://localhost:5000/signin-google
                    options.ClientId = "copy client ID from Google here";
                    options.ClientSecret = "copy client secret from Google here";
                });
        }
        // 此方法由运行时调用。使用此方法配置HTTP请求管道。
        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment()) //Development:开发环境
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else //Production:生产环境
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }


            app.UseHttpsRedirection();//添加用于将HTTP请求重定向到HTTPS的中间件。

            app.UseStaticFiles(); //为当前请求路径启用静态文件服务

            app.UseIdentityServer();//将IdentityServer添加到管道中。

            app.UseMvcWithDefaultRoute();//将MVC添加到Microsoft.AspNetCore.Builder。IApplicationBuilder请求执行
                                         //管道中有一个名为“default”的默认路由和以下模板:“{ controller = Home}/{ action = Index}/{ id ?}”。
        }
    }
}