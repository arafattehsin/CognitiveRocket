using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AMABot.Models
{

    public class Rootobject
    {
        public Answer[] answers { get; set; }
    }

    public class Answer
    {
        public string answer { get; set; }
        public string[] questions { get; set; }
        public float score { get; set; }
    }

}