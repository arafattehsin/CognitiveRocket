using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkpoint
{

    public class Rootobject
    {
        public string uuid { get; set; }
        public string data_type { get; set; }
        public long epoch_time { get; set; }
        public Processing_Time processing_time { get; set; }
        public int img_height { get; set; }
        public int img_width { get; set; }
        public Result[] results { get; set; }
        public int credits_monthly_used { get; set; }
        public int version { get; set; }
        public int credits_monthly_total { get; set; }
        public bool error { get; set; }
        public Regions_Of_Interest[] regions_of_interest { get; set; }
        public int credit_cost { get; set; }
    }

    public class Processing_Time
    {
        public float plates { get; set; }
        public float total { get; set; }
    }

    public class Result
    {
        public string plate { get; set; }
        public float confidence { get; set; }
        public int region_confidence { get; set; }
        public Vehicle_Region vehicle_region { get; set; }
        public string region { get; set; }
        public int plate_index { get; set; }
        public float processing_time_ms { get; set; }
        public Candidate[] candidates { get; set; }
        public Coordinate[] coordinates { get; set; }
        public int matches_template { get; set; }
        public int requested_topn { get; set; }
    }

    public class Vehicle_Region
    {
        public int y { get; set; }
        public int x { get; set; }
        public int height { get; set; }
        public int width { get; set; }
    }

    public class Candidate
    {
        public int matches_template { get; set; }
        public string plate { get; set; }
        public float confidence { get; set; }
    }

    public class Coordinate
    {
        public int y { get; set; }
        public int x { get; set; }
    }

    public class Regions_Of_Interest
    {
        public int y { get; set; }
        public int x { get; set; }
        public int height { get; set; }
        public int width { get; set; }
    }

}
