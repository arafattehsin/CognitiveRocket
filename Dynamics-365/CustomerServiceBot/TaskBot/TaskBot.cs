// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Google;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using TaskBot.Helpers;

namespace TaskBot
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class TaskBot : IBot
    {
        private readonly TaskBotAccessors _accessors;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="conversationState">The managed conversation state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1#windows-eventlog-provider"/>
        public TaskBot(ConversationState conversationState, ILoggerFactory loggerFactory)
        {
            if (conversationState == null)
            {
                throw new System.ArgumentNullException(nameof(conversationState));
            }

            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _accessors = new TaskBotAccessors(conversationState)
            {
                CounterState = conversationState.CreateProperty<CounterState>(TaskBotAccessors.CounterStateName),
            };

            _logger = loggerFactory.CreateLogger<TaskBot>();
            _logger.LogTrace("Turn start.");
        }

        /// <summary>
        /// Every conversation turn for our Echo Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Handle Message activity type, which is the main activity type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                Activity activity = turnContext.Activity.CreateReply();

                if (turnContext.Activity.ChannelId == "google")
                {

                    // This could be saved in UserState object and retrieve as per the requirement. 
                    // For demo purposes, I am just using the hard-coded one. 
                    string userId = "<bank staff user id>";
                    var customer = await FlowHelper.GetFlowOutput(userId);
                    string imageUrl = $"https://dev.virtualearth.net/REST/v1/Imagery/Map/Road/{customer.Address}/16?key=<bingmapskey>";
                    string processedImage = MapMaker.Process(imageUrl, "map");

                    // There's an issue which will be looked by the community. 
                    //Button button = new Button();
                    //button.Title = "Navigate";
                    //button.OpenUrlAction = new OpenUrlAction() { Url = "https://www.arafattehsin.com/" };
                    //Button[] buttons = { button };

                    var card = new GoogleBasicCard()
                    {
                        Content = new GoogleBasicCardContent()
                        {
                            Title = $"Home Delivery - {customer.Name}",
                            Subtitle = customer.Address,
                            FormattedText = "Kindly note that the customer requested for a call before arrival",
                            Image = new Image()
                            {
                                Url = "https://d104b558.ngrok.io/images/map.jpg"
                            },
                            // Buttons = { button },
                            Display = ImageDisplayOptions.DEFAULT
                        },
                    };

                    activity.Text = $"You have to deliver a debit card for a new customer. You can reach him at {customer.MobileNumber}";

                    turnContext.GoogleSetCard(card);

                }

                await turnContext.SendActivityAsync(activity);
            }
        }
    }
}
