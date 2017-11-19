using CognitiveLibrary.Controllers;
using CognitiveLibrary.Utilities;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace AMABot.Dialogs
{

    [Serializable]
    [LuisModel("AppID", "SubscriptionKey")]
    public class BroadbandDialog : LuisDialog<object>
    {
        string requestLang = "ur";
        string responseLang = "en";

        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            IMessageActivity message = context.Activity.AsMessageActivity();
            var response = await Utilities.TranslateText(message.Text, responseLang);
            message.Text = response;

            //Create a Translating Context to translate any messages that gets sent subsequently.
            await base.MessageReceived(context, item);
        }


        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string sendMessage = string.Empty;
            var message = context.Activity.AsMessageActivity();
            if (message.Attachments != null && message.Attachments.Any())
            {
                var attachment = message.Attachments.First();

                try
                {
                    if (CustomVisionController.IsValidCard(attachment.ContentUrl))
                    {
                        using (HttpClient httpClient = new HttpClient())
                        {
                            // Skype attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.
                            if (message.ChannelId.Equals("skype", StringComparison.InvariantCultureIgnoreCase) && new Uri(attachment.ContentUrl).Host.EndsWith("skype.com"))
                            {
                                var token = await new MicrosoftAppCredentials().GetTokenAsync();
                                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            }

                            var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);
                            var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;

                            sendMessage = "Thanks for ID. Our staff will contact you on your phone number.";
                            await context.PostAsync(sendMessage);
                            context.Done("conversatiion ended.");
                            return;
                        }
                    }
                    else
                        sendMessage = "The provided ID does not seem to be valid. Please try with the correct ID.";
                }
                catch(Exception)
                {
                    // Log Exception
                }
            }
            else
            {
                sendMessage = "Thanks for the information. Please provide the scanned image or photo of your ID for verification purpose.";
            }

            await context.PostAsync(sendMessage);
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("Greetings")]
        public async Task Greetings(IDialogContext context, LuisResult result)
        {
            var response = await Utilities.TranslateText("Welcome! Thanks for your message. How can we help you?", requestLang);
            await context.PostAsync(response);
            context.Wait(this.MessageReceived);

        }

        [LuisIntent("ApplyPackage")]
        public Task ApplyPackage(IDialogContext context, LuisResult result)
        {
            context.Wait(this.MessageReceived);
            return Task.CompletedTask;
        }

        [LuisIntent("GetBroadbandInfo")]
        public async Task GetBroadbandInfo(IDialogContext context, LuisResult result)
        {
            var response = await Utilities.TranslateText("Ok! What do you want to know about them?", requestLang);
            await context.PostAsync(response);
            context.Wait(this.MessageReceived);
        }
    }
}