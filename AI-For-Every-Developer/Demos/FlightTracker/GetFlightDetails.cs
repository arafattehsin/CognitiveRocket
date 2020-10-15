using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;

namespace FlightTracker
{
    public static class GetFlightDetails
    {
        
        [FunctionName("GetFlightDetails")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var flightNumber  = req.Query["flightNumber"];
            
            // Pre-checking for invalid IDs (server-sided verification)
            if (String.IsNullOrEmpty(flightNumber))
                return new BadRequestObjectResult("Invalid Flight Number. Make sure you are sending it " +
                        "to the queryString and that it's a valid integer");

            // Make a request based upon your API. 
            // < your code >

            // For the demo purpose, I am just hard-coding everything. 
            FlightInfo flightInfo = new FlightInfo()
            {
                FlightNumber = flightNumber,
                SourceShortCode = "SYD",
                Source = "SYDNEY",
                DestinationShortCode = "KHI",
                Destination = "KARACHI",
                ArrivalTime = "10:20 AM",
                DepartureTime = "08:30 PM"
            };

            return (ActionResult)new OkObjectResult(flightInfo);
        }

        private static async Task<string> GetResponse(string requiredParameter)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(requiredParameter);
                var jsonContent = await response.Content.ReadAsStringAsync();
                string output;

                if ((int)response.StatusCode != 200)
                {
                    // throw an error
                }
                else
                {
                    // flightInfo
                }

                // In real case, this should be a proper output. 
                return string.Empty;
            }
        }
    }
}
