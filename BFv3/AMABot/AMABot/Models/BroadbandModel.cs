using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AMABot.Models
{
    [Serializable]
    public class BroadbandModel
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsValidID { get; set; }

    }
}