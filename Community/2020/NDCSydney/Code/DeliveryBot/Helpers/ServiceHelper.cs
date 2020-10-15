using Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DeliveryBot.Helpers
{
    public class ServiceHelper
    {
        public static async Task<Customer> GetCustomerAddress(string referenceId)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetStringAsync($"http://localhost:7071/api/TrackFood?orderReferenceNumber={referenceId}");
                    return JsonConvert.DeserializeObject<Customer>(response);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
