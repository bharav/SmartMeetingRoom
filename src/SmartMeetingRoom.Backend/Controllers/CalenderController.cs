using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingRoom.Common.DTO;
using SmartMeetingRoom.Common.Services;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SmartMeetingRoom.Backend.Controllers
{
    
    public class CalenderController : Controller
    {
        private MongoDBService _ddbService;

        public CalenderController(MongoDBService ddbServices) {
            _ddbService = ddbServices;
        }

        [Route("api/calender")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCalenderById(string id)
        {
            Calender calender = await _ddbService.GetCalenderById(id);
            HttpContext.Response.StatusCode = 201;
            return Json(calender);
        }

        [Route("api/calender")]
        [HttpGet]
        public async Task<IActionResult> GetAllCalender()
        {
            List<Calender> calenders = await _ddbService.GetAllCalender();
            HttpContext.Response.StatusCode = 201;
            return Json(calenders);
        }
        
        [Route("api/calender/latest")]
        [HttpGet]
        public async Task<IActionResult> GetLatestCalender()
        {
            Calender calender = await _ddbService.LatestCalender();
            HttpContext.Response.StatusCode = 201;
            return Json(calender);
        }
        [Route("api/calender")]
        [HttpPost]
        public async Task<IActionResult> CreateCalender([FromBody]Calender cal)
        {
            await _ddbService.SaveAsync(cal);
            HttpContext.Response.StatusCode = 201;
            return Json(cal);
        }

    }
}
