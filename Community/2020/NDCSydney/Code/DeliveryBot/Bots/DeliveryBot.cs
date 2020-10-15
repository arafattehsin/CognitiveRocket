// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.10.3

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Helpers;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Model;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Attachments;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace DeliveryBot.Bots
{
    public class DeliveryBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        { 
            var customer = await Helpers.ServiceHelper.GetCustomerAddress("12345");
            var activityWithCard = MessageFactory.Text("5 minutes away");
            var card = ContentItemFactory.CreateCard($"Your order {customer.RefNumber} is on its way", "Due to safety for our community, we'll leave the order at your doorstep.", 
                new Link()
                {
                    Name = "More details",
                    Open = new OpenUrl() { Url = "https://www.arafattehsin.com/blog" }
                }, 
                new Image()
                {
                    Url = customer.TrackerURL,
                    Alt = "Delivery person on the map"
                }
                );
            activityWithCard.Attachments.Add(card.ToAttachment());
            await turnContext.SendActivityAsync(activityWithCard, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
