using System;
using System.Collections.Generic;
using System.Text;
using static CarEvaluator.Car;

namespace CarEvaluator
{
    internal class CarEvalTests
    {
        internal static readonly CarEval Eval = new CarEval
        {
            Buying = "vhigh",
            Maintenance = "vhigh",
            Doors = "2",
            Persons = "2",
            LugBoot = "med",
            Safety = "high"
        };
    }
}
