using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class Rootobject
    {
        public string schema { get; set; }
        public string type { get; set; }
        public string version { get; set; }
        public Body[] body { get; set; }
        public Action[] actions { get; set; }
    }

    public class Body
    {
        public string type { get; set; }
        public string text { get; set; }
        public string size { get; set; }
        public string weight { get; set; }
        public bool wrap { get; set; }
        public string imageSize { get; set; }
        public Image[] images { get; set; }
    }

    public class Image
    {
        public string type { get; set; }
        public string url { get; set; }
        public string size { get; set; }
    }

    public class Action
    {
        public string type { get; set; }
        public string title { get; set; }
        public Card card { get; set; }
    }

    public class Card
    {
        public string type { get; set; }
        public Body1[] body { get; set; }
        public Action1[] actions { get; set; }
        public string schema { get; set; }
    }

    public class Body1
    {
        public string type { get; set; }
        public string text { get; set; }
        public string size { get; set; }
        public bool wrap { get; set; }
        public string id { get; set; }
        public string style { get; set; }
        public Choice[] choices { get; set; }
        public bool isMultiline { get; set; }
        public string placeholder { get; set; }
        public bool isMultiSelect { get; set; }
        public string title { get; set; }
        public string valueOn { get; set; }
        public string valueOff { get; set; }
        public string value { get; set; }
    }

    public class Choice
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class Action1
    {
        public string type { get; set; }
        public string title { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        public string FoodChoice { get; set; }
    }



}
