using System;

namespace ServerlessDotnetAngularKinesis.Controllers
{
    public class EventData
    {
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public DateTime ReceivedTime { get; set; }
        public int CountEvents { get; set; }
    }
}