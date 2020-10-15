using System;

namespace Model
{
    public class Customer
    {
        public string Name { get; set; }
        public string RefNumber { get; set; }
        public string Address { get; set; }
        public string FoodChoice { get; set; }

        public string TrackerURL { get; set; }

        public TrackerStatus Status { get; set; }
    }

    public enum TrackerStatus
    {
        OnTheWay,
        ReadyToPickUp,
        Delivered
    }
}
