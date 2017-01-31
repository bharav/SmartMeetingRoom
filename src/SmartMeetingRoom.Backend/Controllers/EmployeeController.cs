using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingRoom.Common.DTO;
using SmartMeetingRoom.Common.Services;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SmartMeetingRoom.Backend.Controllers
{

    [Route("api/[controller]")]
    public class EmployeeController : Controller
    {
        private DocumentDbService _ddbService;

        public EmployeeController(DocumentDbService ddBServices)
        {
            _ddbService = ddBServices;
        }
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            Employee emp = await _ddbService.GetEmployeeById(id);
            HttpContext.Response.StatusCode = 201;
            return Json(emp);
        }

        [Route("api/employee")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody]Employee value)
        {
            await _ddbService.SaveAsync(value);
            HttpContext.Response.StatusCode = 201;
            return Json(new
            {
            });
        }

        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
