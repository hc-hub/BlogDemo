using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogDemo.Api.Controllers
{
    [Route("api/values")]
    public class ValueController:Controller
    {
        public IActionResult Get()
        {
            return Ok("Hello");
        }
    }
}
