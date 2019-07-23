using AutoMapper;
using BlogDemo.Core.Interfaces;
using BlogDemo.Infrastructure.Database;
using BlogDemo.Infrastructure.Repository;
using BlogDemo.Infrastructure.Resources;
using BlogDemo.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;

namespace BlogDemo.Api
{
    public class StartupDevelopment
    {
        public static IConfiguration Configuration { get; set; }
        public StartupDevelopment(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(
                options =>
                {
                    options.ReturnHttpNotAcceptable = true;
                    //option.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                    var outputFormatter = options.OutputFormatters.OfType<JsonOutputFormatter>().FirstOrDefault();
                    if (outputFormatter != null)
                    {
                        outputFormatter.SupportedMediaTypes.Add("application/vnd.cgzl.hateoas+json");
                    }
                })
                .AddJsonOptions(option =>
                {  //将返回实体名称首字母改成小写
                    option.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });
            //重定向
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                options.HttpsPort = 5001;
            });
            services.AddDbContext<MyContext>(options =>
            {
                //var connectionString = Configuration["ConnectionStrings:DefaultConnection"];  
                var connectionString = Configuration.GetConnectionString("DefaultConnection");
                options.UseSqlite("Data Source=BlogDemo.db");
            });
            services.AddHsts(option =>
            {
                option.Preload = true;
                option.IncludeSubDomains = true;
                option.MaxAge = TimeSpan.FromDays(60);
                option.ExcludedHosts.Add("example.com");
                option.ExcludedHosts.Add("www.example.com");
            });
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddAutoMapper();

            services.AddTransient<IValidator<PostResource>, PostResourceValidator>();
            services.AddTransient<IValidator<PostAddResource>,PostAddOrUpdateResourceValidator<PostAddResource>>();
            services.AddTransient<IValidator<PostUpdateResource>, PostAddOrUpdateResourceValidator<PostUpdateResource>>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(factory =>
            {
                var actionContext = factory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });
            var propertyMappingContainer = new PropertyMappingContainer();
            propertyMappingContainer.Register<PostPropertyMapping>();
            services.AddSingleton<IPropertyMappingContainer>(propertyMappingContainer);
            services.AddTransient<ITypeHelperService, TypeHelperService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.UseHsts();
            app.UseDeveloperExceptionPage();//捕获同步和异步系统。管道中的异常实例并生成HTML错误响应。
            //app.UseMyExceptionHandler(loggerFactory);
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
