using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AMAAirlines.Models
{

    public class FlightInfo
    {
        public string FlightNumber { get; set; }

        public enum FlightType { OneWay, Return }

        public enum FlightClass { Economy, Business }

        public int Passengers { get; set; }

        public DateTime? FlightDate { get; set; }

        public GeoCoordinates Source { get; set; }

        public GeoCoordinates Destination { get; set; }
    }

}
