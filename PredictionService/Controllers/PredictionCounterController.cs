using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.Collections;

namespace PredictionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionCounterController : ControllerBase
    {
        private static Dictionary<DayOfWeek, int> clicks = new Dictionary<DayOfWeek, int>();

        [HttpPost]
        public IActionResult RecordClick()
        {
            DayOfWeek dayOfWeek = DateTime.Today.DayOfWeek;
            if (!clicks.ContainsKey(dayOfWeek))
            {
                clicks[dayOfWeek] = 0;
            }
            clicks[dayOfWeek]++;
            return Ok();
            
        }

        [HttpGet]
        public IActionResult GetClicksToday()
        {
            DayOfWeek dayOfWeek = DateTime.Today.DayOfWeek;
            var clickCounts = new List<int>();
            if (!clicks.ContainsKey(dayOfWeek))
            {
                clicks[dayOfWeek] = 0;
            }
            clickCounts.Add(clicks[dayOfWeek]);

            return Ok(clickCounts);
        }


    }
}
