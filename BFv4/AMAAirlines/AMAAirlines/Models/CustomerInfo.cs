using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AMAAirlines.Models
{
    public class CustomerInfo
    {
        public string Name { get; set; }

        public FlightInfo FlightInfo { get; set; }
    }

}
