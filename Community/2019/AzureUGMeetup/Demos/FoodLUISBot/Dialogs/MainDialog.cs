using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FoodLUISBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly FoodRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
        
        public MainDialog(FoodRecognizer luisRecognizer, TrackDialog trackDialog, OrderDialog orderDialog, ILogger<MainDialog> logger)
           : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(trackDialog);
            AddDialog(orderDialog);

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Activity messageActivity = new Activity();

            if (stepContext.Options != null)
                messageActivity = MessageFactory.Text(stepContext.Options.ToString());
            else
                messageActivity = MessageFactory.Text("Hello! How may I help you with?");

            messageActivity.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "I want to order food", Type = ActionTypes.ImBack, Value = "I want to order food" },
                    new CardAction() { Title = "I want to track my food", Type = ActionTypes.ImBack, Value = "I want to track my food" }
                },
            };
            
            return stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = messageActivity }, cancellationToken);

        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            var luisResult = await _luisRecognizer.RecognizeAsync(stepContext.Context, cancellationToken);
            switch (luisResult.GetTopScoringIntent().intent)
            {
                case "TrackOrder":
                    // Run the TrackDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
                    return await stepContext.BeginDialogAsync(nameof(TrackDialog), null, cancellationToken);
                case "BookOrder":
                    return await stepContext.BeginDialogAsync(nameof(OrderDialog), null, cancellationToken);
                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.GetTopScoringIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Is there anything else I can help you with today?")
            };

            return stepContext.PromptAsync(nameof(ConfirmPrompt), promptOptions, cancellationToken);
        }

        private Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string promptMessage = "What else can I do for you?";

            if ((bool)stepContext.Result == true)
                return stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
            else
                return stepContext.EndDialogAsync(cancellationToken);
            
        }

    }
}
