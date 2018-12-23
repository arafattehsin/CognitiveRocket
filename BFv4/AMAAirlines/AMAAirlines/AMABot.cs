// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AMAAirlines.Dialogs;
using AMAAirlines.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace AMAAirlines
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
    public class AMABot : IBot
    { 
        private const string BookingDialogID = "bookingDialog";
        private const string StatusDialogID = "statusDialog";
        private const string FAQDialogID = "faqDialog";

        // Define the dialog set for the bot.
        private readonly DialogSet _dialogs;

        private readonly BotAccessors _accessors;
        private readonly ILogger _logger;
        private readonly BotServices botServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="AMABot"/> class.
        /// </summary>
        /// <param name="accessors">A class containing <see cref="IStatePropertyAccessor{T}"/> used to manage state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1#windows-eventlog-provider"/>
        public AMABot(BotServices services, BotAccessors accessors, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<AMABot>();
            _logger.LogTrace("EchoBot turn start.");
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));

            botServices = services ?? throw new System.ArgumentNullException(nameof(services));

            if (!botServices.QnAServices.ContainsKey("AMAQnA"))
            {
                throw new System.ArgumentException($"Invalid configuration. Please check your '.bot' file for a QnA service named AMAQnA'.");
            }

            if (!botServices.LuisServices.ContainsKey("AMALuis"))
            {
                throw new System.ArgumentException($"Invalid configuration. Please check your '.bot' file for a Luis service named AMALuis'.");
            }

            // Defining dialogs
            _dialogs = new DialogSet(accessors.DialogStateAccessor)
                .Add(new BookingDialog(BookingDialogID))
                .Add(new StatusDialog(StatusDialogID));
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
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                DialogContext dc = await _dialogs.CreateContextAsync(turnContext, cancellationToken);

                // Get the state properties from the turn context.
                CustomerInfo userProfile =
                    await _accessors.CustomerInfoAccessor.GetAsync(turnContext, () => new CustomerInfo());

                // Continue any current dialog.
                DialogTurnResult dialogTurnResult = await dc.ContinueDialogAsync();

                if (dialogTurnResult.Result is null)
                {
                    // Get the intent recognition result
                    var recognizerResult = await botServices.LuisServices["AMAAirlinesDispatch"].RecognizeAsync(turnContext, cancellationToken);
                    var topIntent = recognizerResult?.GetTopScoringIntent();

                    if (topIntent == null)
                    {
                        await turnContext.SendActivityAsync("Unable to get the top intent.");
                    }
                    else
                    {
                        await DispatchToTopIntentAsync(turnContext, dc, topIntent, cancellationToken);
                    }
                }
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                // Send a welcome message to the user and tell them what actions they may perform to use this bot
                if (turnContext.Activity.MembersAdded != null)
                {
                    await SendWelcomeMessageAsync(turnContext, cancellationToken);
                }
            }

            // Save the new turn count into the conversation state.
            await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);            
        }

        /// <summary>
        /// On a conversation update activity sent to the bot, the bot will
        /// send a message to the any new user(s) that were added.
        /// </summary>
        /// <param name="turnContext">Provides the <see cref="ITurnContext"/> for the turn of the bot.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>>A <see cref="Task"/> representing the operation result of the Turn operation.</returns>
        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(
                        $"Welcome to Dispatch bot {member.Name}",
                        cancellationToken: cancellationToken);
                }
            }
        }

        /// <summary>
        /// Depending on the intent from Dispatch, routes to the right LUIS model or QnA service.
        /// </summary>
        private async Task DispatchToTopIntentAsync(ITurnContext context, DialogContext dc, (string intent, double score)? topIntent, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string luisDispatchKey = "l_AMALuis";
            const string noneDispatchKey = "None";
            const string qnaDispatchKey = "q_AMAQnA";

            switch (topIntent.Value.intent)
            {
                case luisDispatchKey:
                    await DispatchToLuisModelAsync(context, "AMALuis", dc);

                    // Here, you can add code for calling the hypothetical home automation service, passing in any entity information that you need
                    break;
                case noneDispatchKey:
                // You can provide logic here to handle the known None intent (none of the above).
                // In this example we fall through to the QnA intent.
                case qnaDispatchKey:
                    await DispatchToQnAMakerAsync(context, "AMAQnA", dc);
                    break;

                default:
                    // The intent didn't match any case, so just display the recognition results.
                    await context.SendActivityAsync($"Dispatch intent: {topIntent.Value.intent} ({topIntent.Value.score}).");
                    break;
            }
        }

        /// <summary>
        /// Dispatches the turn to the request QnAMaker app.
        /// </summary>
        private async Task DispatchToQnAMakerAsync(ITurnContext context, string appName, DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!string.IsNullOrEmpty(context.Activity.Text))
            {
                var results = await botServices.QnAServices[appName].GetAnswersAsync(context).ConfigureAwait(false);
                if (results.Any())
                {
                    await context.SendActivityAsync(results.First().Answer, cancellationToken: cancellationToken);
                }
                else
                {
                    await context.SendActivityAsync($"Couldn't find an answer in the {appName}.");
                }
            }
        }

        /// <summary>
        /// Dispatches the turn to the requested LUIS model.
        /// </summary>
        private async Task DispatchToLuisModelAsync(ITurnContext context, string appName, DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await botServices.LuisServices[appName].RecognizeAsync(context, cancellationToken);
            var intent = result.Intents?.FirstOrDefault();

            if (intent?.ToString() == "BookFlight")
            {
                await dc.BeginDialogAsync(BookingDialogID, null, cancellationToken);
            }
            else
            {
                await dc.BeginDialogAsync(StatusDialogID, result, cancellationToken);
            }
        }
    }
}
