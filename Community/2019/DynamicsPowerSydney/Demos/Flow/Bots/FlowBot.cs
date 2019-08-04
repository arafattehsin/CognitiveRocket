// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Google;
using Flow.Helpers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Flow.Bots
{
    public class FlowBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity.AsMessageActivity();

            // For demo purposes, I am just using the hard-coded one. 
            string userId = "userId of the staff";
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
                        Url = $"http://6826849a.ngrok.io/images/map.jpeg"
                    },
                    Buttons = buttons,
                    Display = ImageDisplayOptions.DEFAULT
                },
            };

            activity.Text = $"You have to deliver a debit card for a new customer. You can reach him at {customer.MobileNumber}";

            turnContext.GoogleSetCard(card);

            await turnContext.SendActivityAsync(activity);
        }

    }
}
