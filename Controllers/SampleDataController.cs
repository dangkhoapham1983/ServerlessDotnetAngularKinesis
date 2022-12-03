using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ServerlessDotnetAngularKinesis.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : ControllerBase
    {
        private const string eventListCacheKey = "eventList";
        private const string eventKinesisListCacheKey = "eventKinesisList";
        private IMemoryCache _cache;

        public SampleDataController(IMemoryCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            if (_cache.TryGetValue(eventListCacheKey, out IEnumerable<EventData> eventDataList))
            {
                var maxDate = eventDataList.Max(x => x.ReceivedTime);
                var rs = eventDataList.FirstOrDefault(x => x.ReceivedTime == maxDate);
                rs.CountEvents = eventDataList.Count(x => x.ReceivedTime <= maxDate && x.ReceivedTime >= maxDate.AddHours(-1));
                return Ok(rs);
            }
          
            return Ok(new EventData());
        }

        [HttpGet("/api/Kinesis")]
        public async Task<IActionResult> GetKinesisAsync()
        {
            List<KinesisShared> lsKineis = new List<KinesisShared>();
            if (_cache.TryGetValue(eventKinesisListCacheKey, out IEnumerable<KinesisShared> eventKinesisDataList))
            {
                if (eventKinesisDataList != null)
                {
                    lsKineis = eventKinesisDataList.ToList();
                }
            }
            var strCheck = lsKineis.FirstOrDefault(x => x.Id == 0);
            if (strCheck != null)
            {
                var fun = new FunctionKinesis();
                var rs = await fun.GetAsyncRecord(lsKineis);
                if (rs != null)
                {
                    var maxDate = rs.Max(x => x.ReceivedTime);
                    var res = rs.FirstOrDefault(x => x.ReceivedTime == maxDate);
                    res.CountEvents = rs.Count(x => x.ReceivedTime <= maxDate && x.ReceivedTime >= maxDate.AddHours(-1));

                    return Ok(res);
                }
            }

            return Ok(new EventData());
        }

        [HttpPost]
        [Produces("application/json")]
        public EventData Post([FromBody] EventData eventData)
        {
            if (eventData != null)
            {
                if(!string.IsNullOrEmpty(eventData.ImageUrl) && !string.IsNullOrEmpty(eventData.Description))
                {
                    eventData.ReceivedTime = DateTime.Now;
                    List<EventData> ls = new List<EventData>();
                    if (_cache.TryGetValue(eventListCacheKey, out IEnumerable<EventData> eventDataList))
                    {
                        if(eventDataList != null)
                        {
                            ls = eventDataList.ToList();
                        }
                    }
                    
                    ls.Add(eventData);
                    _cache.Set(eventListCacheKey, ls);
                    
                    var fun = new FunctionKinesis();
                    var rs = fun.PutRecords(eventData).Result;

                    List<KinesisShared> lsKineis = new List<KinesisShared>();
                    if (_cache.TryGetValue(eventKinesisListCacheKey, out IEnumerable<KinesisShared> eventKinesisDataList))
                    {
                        if (eventKinesisDataList != null)
                        {
                            lsKineis = eventKinesisDataList.ToList();
                        }
                    }
                    var vCheck = lsKineis.Find(x => x.ShardId == rs.ShardId && x.SequenceNumber == rs.SequenceNumber);

                    if (vCheck == null)
                    {
                        rs.Id = lsKineis.Count;
                        lsKineis.Add(rs);
                        _cache.Set(eventKinesisListCacheKey, lsKineis);
                    }

                    return eventData;
                }
            }
            return new EventData();
        }
    }
}
