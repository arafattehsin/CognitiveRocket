using Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BankBot.Helpers
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
                    HttpResponseMessage response = await httpClient.PostAsync("https://prod-123.westus.logic.azure.com:443/workflows/e3a77fa66f25479d9c9633ed5c579946/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=gKmbRSPO1BzvPUc8RxRZtcttqSQErPXDbB8bK28PXO0", stringContent);
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
