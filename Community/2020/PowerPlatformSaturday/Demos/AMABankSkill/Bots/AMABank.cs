// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace AMABankSkill.Bots
{
    public class AMABank : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var message = Activity.CreateMessageActivity();

            if (turnContext.Activity.Text.Contains("end") || turnContext.Activity.Text.Contains("stop"))
            {
                // Send End of conversation at the end.
                await turnContext.SendActivityAsync(MessageFactory.Text($"ending conversation from the skill..."), cancellationToken);
                var endOfConversation = Activity.CreateEndOfConversationActivity();
                endOfConversation.Code = EndOfConversationCodes.CompletedSuccessfully;
                await turnContext.SendActivityAsync(endOfConversation, cancellationToken);
            }
            else if(turnContext.Activity.Text.Contains("weather"))
            {

              
                message.Attachments.Add(new Attachment()
                {
                    Content = GetAdaptiveCard("Weather"),
                    ContentType = AdaptiveCard.ContentType,
                    Name = "Weather",
                });
                //await turnContext.SendActivityAsync(MessageFactory.Text($"Echo (dotnet) : {turnContext.Activity.Text}"), cancellationToken);
                //await turnContext.SendActivityAsync(MessageFactory.Text("Say \"end\" or \"stop\" and I'll end the conversation and back to the parent."), cancellationToken);
            }
            else
            {
                message.Attachments.Add(new Attachment()
                {
                    Content = GetAdaptiveCard("Balance"),
                    ContentType = AdaptiveCard.ContentType,
                    Name = "Account Balance",
                });
                
            }

            await turnContext.SendActivityAsync(message, cancellationToken: cancellationToken);
            await turnContext.SendActivityAsync(MessageFactory.Text($"What else can I help you with?"), cancellationToken);
        }

        public static AdaptiveCard GetAdaptiveCard(string cardType)
        {
            string json = System.IO.File.ReadAllText($@"Assets\\{cardType}.json");
            var card = AdaptiveCards.AdaptiveCard.FromJson(json).Card;
            return card;
        }
    }
}
