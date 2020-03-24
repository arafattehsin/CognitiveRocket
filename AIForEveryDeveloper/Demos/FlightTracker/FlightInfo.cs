using System;
using System.Collections.Generic;
using System.Text;

namespace FlightTracker
{
    public class FlightInfo
    {
        public string FlightNumber { get; set; }

        public string Source { get; set; }
        public string Destination { get; set; }

        public string SourceShortCode { get; set; }

        public string DestinationShortCode { get; set; }

        public string DepartureTime { get; set; }
        
        public string ArrivalTime { get; set; }

    }

    //public class Location
    //{
    //    public string Name { get; set; }
    //    public string ShortCode { get; set; }
    //}
}
