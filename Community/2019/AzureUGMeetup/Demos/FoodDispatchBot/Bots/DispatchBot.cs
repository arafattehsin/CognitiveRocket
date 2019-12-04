using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodDispatchBot.Dialogs;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace FoodDispatchBot.Bots
{
    public class DispatchBot : ActivityHandler
    {
        private ILogger<DispatchBot> _logger;
        private IBotServices _botServices;

        protected readonly DialogSet Dialogs;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;

        /// <summary>Gets or sets the state property accessor for the dialog state.</summary>
        /// <value>Accessor for the dialog state.</value>
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        public DispatchBot(ConversationState conversationState, UserState userState, IBotServices botServices, TrackDialog trackDialog, OrderDialog orderDialog, ILogger<DispatchBot> logger)
        {
            _logger = logger;
            _botServices = botServices;
            ConversationState = conversationState;
            UserState = userState;
            Dialogs = new DialogSet(ConversationState.CreateProperty<DialogState>(nameof(DialogState)));
            Dialogs.Add(trackDialog);
            Dialogs.Add(orderDialog);
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            DialogContext dc = await Dialogs.CreateContextAsync(turnContext, cancellationToken);

            // Continue any current dialog.
            DialogTurnResult dialogTurnResult = await dc.ContinueDialogAsync();

            if (dialogTurnResult.Result is null)
            {
                // First, we use the dispatch model to determine which cognitive service (LUIS or QnA) to use.
                var recognizerResult = await _botServices.FoodDispatch.RecognizeAsync(turnContext, cancellationToken);

                // Top intent tell us which cognitive service to use.
                var topIntent = recognizerResult.GetTopScoringIntent();

                // Next, we call the dispatcher with the top intent.
                await DispatchToTopIntentAsync(turnContext, dc, topIntent.intent, recognizerResult, cancellationToken);
            }
        }

        private async Task DispatchToTopIntentAsync(ITurnContext<IMessageActivity> turnContext, DialogContext dc, string intent, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            const string foodDispatchLUISKey = "l_FoodLUIS";
            const string foodDispatchQnAKey = "q_FoodQnA";
            const string noneDispatchKey = "None";

            switch (intent)
            {
                case foodDispatchLUISKey:
                    await ProcessFoodLUISAsync(turnContext, dc, recognizerResult.Properties["luisResult"] as LuisResult, cancellationToken);
                    break;
                case foodDispatchQnAKey:
                    await ProcessFoodQnAAsync(turnContext, cancellationToken);
                    break;
                case noneDispatchKey:
                // You can provide logic here to handle the known None intent (none of the above).
                // In this example we fall through to the QnA intent.
                default:
                    _logger.LogInformation($"Dispatch unrecognized intent: {intent}.");
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Dispatch unrecognized intent: {intent}."), cancellationToken);
                    break;
            }
        }

        private async Task ProcessFoodQnAAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProcessFoodQnAAsync");

            var results = await _botServices.FoodQnA.GetAnswersAsync(turnContext);
            if (results.Any())
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(results.First().Answer), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Sorry, could not find an answer in the Q and A system."), cancellationToken);
            }
        }

        private async Task ProcessFoodLUISAsync(ITurnContext<IMessageActivity> turnContext, DialogContext dc, LuisResult luisResult, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProcessFoodLUISAsync");

            // Retrieve LUIS result for Process Automation.
            var result = luisResult.ConnectedServiceResult;
            var topIntent = result.TopScoringIntent.Intent;

            switch (topIntent)
            {
                case "TrackOrder":
                    // Run the TrackDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
                    await dc.BeginDialogAsync(nameof(TrackDialog), null, cancellationToken);
                    break;
                case "BookOrder":
                    await dc.BeginDialogAsync(nameof(OrderDialog), null, cancellationToken);
                    break;
                default:
                    break;
            }
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
