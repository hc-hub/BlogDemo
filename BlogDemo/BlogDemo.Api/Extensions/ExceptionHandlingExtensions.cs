using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogDemo.Api.Extensions
{
    public static class ExceptionHandlingExtensions
    {
        public static void UseMyExceptionHandler(this IApplicationBuilder app,ILoggerFactory loggerFactory)
        {
            app.UseExceptionHandler(build=> {
                build.Run(async context=> {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";  //获取或设置内容类型响应标头的值。

                    var ex = context.Features.Get<IExceptionHandlerFeature>();//获取异常
                    if (ex!=null)
                    {
                        var logger = loggerFactory.CreateLogger("BlogDemo.Api.Extensions.ExceptionHandlingExtensions");
                        logger.LogError(500, ex.Error, ex.Error.Message);
                    }
                    await context.Response.WriteAsync(ex?.Error?.Message ?? "An Error Occurred.");
                });
            });
        }
    }
}
