using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Tweetinvi;
using Tweetinvi.Parameters;
using Tweetinvi.Streaming;

namespace AMABot
{
    public class TweetController
    {
        public static void ConnectToTwitter()
        {
            try
            {

                Auth.SetUserCredentials("<Consumer Key>", "<Consumer Secret>", "<Access Token>", "<Access Token Secret>");

                var authenticatedUser = Tweetinvi.User.GetAuthenticatedUser();

                var stream = Stream.CreateUserStream();
                stream.StreamStopped += Stream_StreamStopped;
                stream.StreamPaused += Stream_StreamPaused;
                SubscribeStream(stream);


            }
            catch (Exception ex)
            {
                // Log the exception
            }

        }

        private async static void SubscribeStream(IUserStream stream)
        {
            try
            {


                stream.TweetCreatedByAnyoneButMe -= Stream_TweetCreatedByAnyoneButMe;
                stream.TweetCreatedByAnyoneButMe += Stream_TweetCreatedByAnyoneButMe;
                await stream.StartStreamAsync();


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async static void Stream_StreamPaused(object sender, EventArgs e)
        {
            try
            {


                var stream = Stream.CreateUserStream();
                stream.StreamPaused -= Stream_StreamPaused;
                stream.StreamPaused += Stream_StreamPaused;
                await stream.StartStreamAsync();


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void Stream_StreamStopped(object sender, Tweetinvi.Events.StreamExceptionEventArgs e)
        {
            try
            {
                var stream = Stream.CreateUserStream();
                stream.StreamStopped -= Stream_StreamStopped;
                stream.StreamStopped += Stream_StreamStopped;
                SubscribeStream(stream);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static async void Stream_TweetCreatedByAnyoneButMe(object sender, Tweetinvi.Events.TweetReceivedEventArgs e)
        {

            try
            {
                // string tweet 
                string tweet = e.Tweet.Text.Replace("@<Twitter Handle>", string.Empty).Trim();

                if (!String.IsNullOrEmpty(tweet))
                {
                    string processedTweet = await GetResponse(tweet);

                    ReplyTweet(processedTweet, e.Tweet.Id);
                }
            }
            catch (Exception)
            {
                // Log the exception
            }

        }

        public static void ReplyTweet(string text, long tweetIdtoReplyTo)
        {
            try
            {
                // With the new version of Twitter you no longer have to specify the mentions. Twitter can do that for you automatically.
                var reply = Tweet.PublishTweet(text, new PublishTweetOptionalParameters
                {
                    InReplyToTweetId = tweetIdtoReplyTo,
                    AutoPopulateReplyMetadata = true // Auto populate the @mentions
                });

                var tweetToReplyTo = Tweet.GetTweet(tweetIdtoReplyTo);

                // We must add @screenName of the author of the tweet we want to reply to
                var textToPublish = string.Format("@{0} {1}", tweetToReplyTo.CreatedBy.ScreenName, text);
                var tweet = Tweet.PublishTweetInReplyTo(textToPublish, tweetIdtoReplyTo);
            }
            catch (Exception)
            {
                // Log the exception
            }
        }

        #region Archive
        static async Task<string> GetResponse(string query)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(query);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "{Subscription Key}");           

            var uri = "https://westus.api.cognitive.microsoft.com/qnamaker/v2.0/knowledgebases/{KnowledgeBaseID}/generateAnswer?" + queryString;

            HttpResponseMessage response;

            // Request body
            var postBody = $"{{\"question\": \"{query}\"}}";
            byte[] byteData = Encoding.UTF8.GetBytes(postBody);

            try
            {
                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await client.PostAsync(uri, content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    Models.Rootobject cleanServAnswer = JsonConvert.DeserializeObject<Models.Rootobject>(responseString);
                    return cleanServAnswer.answers[0].answer;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        #endregion

    }
}
