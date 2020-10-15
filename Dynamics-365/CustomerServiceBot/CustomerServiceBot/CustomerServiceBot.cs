// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using CustomerServiceBot.Helpers;
using CustomerServiceBot.ServiceHelpers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Models;

namespace CustomerServiceBot
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
    public class CustomerServiceBot : IBot
    {
        private readonly CustomerServiceBotAccessors _accessors;
        private readonly ILogger _logger;
        private readonly BotServices botServices;

        // Define the dialog set for the bot.
        private readonly DialogSet _dialogs;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="conversationState">The managed conversation state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1#windows-eventlog-provider"/>
        public CustomerServiceBot(BotServices services, CustomerServiceBotAccessors accessors, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<CustomerServiceBot>();
            _logger.LogTrace("Turn start.");

            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));
            botServices = services ?? throw new System.ArgumentNullException(nameof(services));

            // Defining dialogs
            _dialogs = new DialogSet(_accessors.DialogStateAccessor);

            // Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                NameStepAsync,
                PhoneNumberStepAsync,
                EmailAsync,
                ConfirmAsync,
                SummaryAsync
            };

            _dialogs.Add(new WaterfallDialog("LeadDetails", waterfallSteps));
            _dialogs.Add(new TextPrompt("text"));
            _dialogs.Add(new ConfirmPrompt("confirm"));
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
                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                if(results.Status == DialogTurnStatus.Empty)
                {
                    await dialogContext.BeginDialogAsync("LeadDetails", null, cancellationToken);
                }

                // Save the dialog into conversation state.
                await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);

                // Save the dialog into user state.
                await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                // Send a welcome message to the user and tell them what actions they may perform to use this bot
                if (turnContext.Activity.MembersAdded != null)
                {
                    await SendWelcomeMessageAsync(turnContext, cancellationToken);
                }
            }
        }

        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(
                        $"Welcome to the Events Bot. I'm currently responsible for the initial customer registration. Please let me know how may I help you today?",
                        cancellationToken: cancellationToken);
                }
            }
        }

        // Helper Method to modify the Adaptive Card template. 
        private AdaptiveCard GetAdaptiveCard(Prospect userProfile)
        {
            // Found flight number, just return the flight details now.
            string json = System.IO.File.ReadAllText(@"data\\assets\\card.json");
            var card = AdaptiveCards.AdaptiveCard.FromJson(json).Card;
            var body = card.Body;

            // Fact 
            AdaptiveFactSet adaptiveFactSet = (AdaptiveCards.AdaptiveFactSet)body[2];

            // Name
            AdaptiveFact name = (AdaptiveFact)adaptiveFactSet.Facts[0];
            name.Value = userProfile.Name;

            // Phone Number
            AdaptiveFact phoneNumber = (AdaptiveFact)adaptiveFactSet.Facts[1];
            phoneNumber.Value = userProfile.PhoneNumber;

            // Email Address
            AdaptiveFact emailAddr = (AdaptiveFact)adaptiveFactSet.Facts[2];
            emailAddr.Value = userProfile.Email;

            return card;
        }

        #region Waterfall Steps
        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.
            return await stepContext.PromptAsync("text", new PromptOptions { Prompt = MessageFactory.Text("Alright. Please enter your name.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> PhoneNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the state properties from the turn context.
            Prospect userProfile = await _accessors.ProspectInfoAccessor.GetAsync(stepContext.Context, () => new Prospect());
            userProfile.Name = (string)stepContext.Result;

            // Ask for a phone number.
            return await stepContext.PromptAsync("text", new PromptOptions { Prompt = MessageFactory.Text("May I know your phone number?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> EmailAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Prospect userProfile = await _accessors.ProspectInfoAccessor.GetAsync(stepContext.Context, () => new Prospect());
            userProfile.PhoneNumber = (string)stepContext.Result;

            // Ask for a phone number.
            return await stepContext.PromptAsync("text", new PromptOptions { Prompt = MessageFactory.Text("What about your email address?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Prospect userProfile = await _accessors.ProspectInfoAccessor.GetAsync(stepContext.Context, () => new Prospect());
            userProfile.Email = (string)stepContext.Result;

            // We can send messages to the user at any point in the WaterfallStep.
            // Create the message 
            var message = Activity.CreateMessageActivity();
            message.Attachments.Add(new Attachment()
            {
                Content = GetAdaptiveCard(userProfile),
                ContentType = AdaptiveCard.ContentType,
                Name = "Customer Information",
            });

            await stepContext.Context.SendActivityAsync(message, cancellationToken: cancellationToken);

            return await stepContext.PromptAsync("confirm", new PromptOptions { Prompt = MessageFactory.Text("Please confirm the above information.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                Prospect userProfile = await _accessors.ProspectInfoAccessor.GetAsync(stepContext.Context, () => new Prospect());

                try
                {
                    // Lead generation.. 
                    await stepContext.Context.SendActivityAsync("Registering you in our system..", cancellationToken: cancellationToken);

                    string referenceNumber = await D365WebAPIHelper.GenerateLead(userProfile, botServices.CRMUser);

                    await stepContext.Context.SendActivityAsync($"Thanks {userProfile.Name}. You're now registered (Ref: {referenceNumber}). Someone from our company will contact you.", cancellationToken: cancellationToken);
                }
                catch (Exception)
                {
                    await stepContext.Context.SendActivityAsync($"There's an issue with our system. Please retry again later.", cancellationToken: cancellationToken);
                }

                return await stepContext.EndDialogAsync(cancellationToken);
            }
            else
            {
                // Taking customer back to the question no. 1
                return await stepContext.NextAsync(-3, cancellationToken);
            }
        }

        #endregion

    }
}
