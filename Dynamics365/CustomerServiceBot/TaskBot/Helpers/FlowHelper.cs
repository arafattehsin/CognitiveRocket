using Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TaskBot.Helpers
{
    public class FlowHelper
    {
        public static async Task<Customer> GetFlowOutput(string userId)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    httpClient.Timeout = new TimeSpan(0, 2, 0);  //2 minutes

                    // Create a JSON object
                    JObject user = new JObject();
                    user.Add("userId", userId);

                    // Get response from Microsoft Flow
                    var stringContent = new StringContent(user.ToString(), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await httpClient.PostAsync("https://prod-03.westus.logic.azure.com:443/workflows/f446abefc3734bc589de93712347d6e5/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=aS5_STOxFTMx2hND4zLMQSqZDNyFqlZse0t5VgguCMM", stringContent);
                    var array = response.Content.ReadAsStringAsync().Result.Split('|');

                    // Customer 
                    return new Customer()
                    {
                        Name = array[0],
                        Address = array[1],
                        MobileNumber = array[2]
                    };                   
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
             
        }
    }
}
