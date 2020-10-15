using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Model;

namespace FoodTracker
{
    public static class FoodTracker
    {
        [FunctionName("TrackFood")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string orderRefNumber = req.Query["orderReferenceNumber"];

            Customer customer = new Customer()
            {
                Name = "Arafat Tehsin",
                Address = "89 York St. Sydney",
                RefNumber = orderRefNumber,
                FoodChoice = "CHICKEN",
                TrackerURL = "https://i.imgur.com/7DiWP3z.png",
                Status = TrackerStatus.OnTheWay
            };

            return new OkObjectResult(customer);
        }
        public static string GetImage()
        {
            string imagePath = Path.Combine(Environment.CurrentDirectory, @"food-track.PNG");
            return Convert.ToBase64String(File.ReadAllBytes(imagePath));
        }
    }

   
}
