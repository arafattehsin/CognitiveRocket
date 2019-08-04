using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace S2S.ServiceHelpers
{
    /// <summary>
    /// This class is good for all kind of operations in Dynamics 365 using Web API. 
    /// Still in the early stages. 
    /// </summary>
    public class D365WebAPIHelper
    {
        public async static Task<string> GenerateLead(Prospect prospect, CRMCredentials crmUser)
        {
            HttpMessageHandler messageHandler = null;
            Version webAPIVersion = new Version(9, 0);

            //One message handler for OAuth authentication, and the other for Windows integrated 
            // authentication.  (Assumes that HTTPS protocol only used for CRM Online.)
            if (crmUser.ServiceUrl.StartsWith("https://"))
            {
                messageHandler = new OAuthMessageHandler(crmUser.ServiceUrl, crmUser.ClientID, crmUser.RedirectUrl, crmUser.AuthpointEnd, crmUser.Key,
                         new HttpClientHandler());
            }

            try
            {
                //Create an HTTP client to send a request message to the CRM Web service.
                using (HttpClient httpClient = new HttpClient(messageHandler))
                {
                    //Specify the Web API address of the service and the period of time each request 
                    // has to execute.
                    httpClient.BaseAddress = new Uri(crmUser.ServiceUrl);
                    httpClient.Timeout = new TimeSpan(0, 2, 0);  //2 minutes

                    httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                    httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
                    httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                    // Get the current userId of the service account (owner incase of Lead)
                    HttpResponseMessage whoAmIResponse = await httpClient.GetAsync("api/data/v9.0/WhoAmI");
                    Guid userId;
                    if (whoAmIResponse.IsSuccessStatusCode)
                    {
                        JObject jWhoAmIResponse =
                            JObject.Parse(whoAmIResponse.Content.ReadAsStringAsync().Result);
                        userId = (Guid)jWhoAmIResponse["UserId"];

                        // Populate the fields of prospect customer (lead)
                        JObject lead = new JObject();
                        lead.Add("subject", "Lead from the Event Bot");
                        lead.Add("lastname", prospect.Name);
                        lead.Add("mobilephone", prospect.PhoneNumber);
                        lead.Add("emailaddress1", prospect.Email);
                        lead.Add("ownerid@odata.bind", $"/systemusers({userId})");

                        // Create request and response
                        HttpRequestMessage createRequest = new HttpRequestMessage(HttpMethod.Post, "api/data/v9.0/leads");
                        createRequest.Content = new StringContent(lead.ToString(), Encoding.UTF8, "application/json");
                        HttpResponseMessage createResponse = await httpClient.SendAsync(createRequest);

                        // Check if the status is good to go (204)
                        if (createResponse.StatusCode == HttpStatusCode.NoContent) 
                        {
                            return Helpers.Common.GenerateReferenceID(6);
                        }
                        else
                        {
                            return $"Failed to register {createResponse.StatusCode}";
                        }
                    }
                    else
                        return $"Failed - {whoAmIResponse.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    /// <summary>
    /// // This class is available in https://docs.microsoft.com/en-us/dynamics365/customer-engagement/developer/webapi/simple-web-api-quick-start-sample-csharp
    /// </summary>
    class OAuthMessageHandler : DelegatingHandler
    {        
        public AuthenticationHeaderValue authHeader;

        public OAuthMessageHandler(string serviceUrl, string clientId, string redirectUrl, string authEndPoint, string key,
                HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            AuthenticationContext authContext = new AuthenticationContext(authEndPoint, false);
            ClientCredential clientCred = new ClientCredential(clientId, key);

            //Note that an Azure AD access token has finite lifetime, default expiration is 60 minutes.
            AuthenticationResult authResult = authContext.AcquireTokenAsync(serviceUrl, clientCred).Result;

            authHeader = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        }

        protected override Task<HttpResponseMessage> SendAsync(
                 HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            request.Headers.Authorization = authHeader;
            return base.SendAsync(request, cancellationToken);
        }
    }
}
