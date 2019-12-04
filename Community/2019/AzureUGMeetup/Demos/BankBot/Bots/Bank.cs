// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.5.0

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BankBot.Helpers;
using Bot.Builder.Community.Adapters.Google;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BankBot.Bots
{
    public class Bank : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity.AsMessageActivity();

            // For demo purposes, I am just using the hard-coded one. 
            string userId = "274dc4c7-0e94-46e3-86ce-ce9f141d8ad4";
            var customer = await FlowHelper.GetFlowOutput(userId);
            string imageUrl = $"https://dev.virtualearth.net/REST/v1/Imagery/Map/Road/{customer.Address}/16?key=At-9uaLoghwXOeWXbc1bmz4XLcczj0hzL7YeBweBRr0Zx8BFIO9cl6XCU_Jj4sCw";
            var url = MapMaker.Process(imageUrl, "map");

            // There's an issue which will be looked by the community. 
            Button button = new Button();
            button.Title = "Navigate";
            button.OpenUrlAction = new OpenUrlAction() { Url = $"https://www.google.com/maps/search/?api=1&query={customer.Address}" };
            Button[] buttons = { button };

            var card = new GoogleBasicCard()
            {
                Content = new GoogleBasicCardContent()
                {
                    Title = $"Home Delivery - {customer.Name}",
                    Subtitle = customer.Address,
                    FormattedText = "Kindly note that the customer requested for a call before arrival",
                    Image = new Image()
                    {
                        // Compression issue of an image, have to look into it later on. 
                        Url = $"<temp url>"
                    },
                    Buttons = buttons,
                    Display = ImageDisplayOptions.DEFAULT
                },
            };

            activity.Text = $"You have to deliver a debit card for a new customer. You can reach him at {customer.MobileNumber}";

            turnContext.GoogleSetCard(card);

            await turnContext.SendActivityAsync(activity);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome!"), cancellationToken);
                }
            }
        }
    }
}
