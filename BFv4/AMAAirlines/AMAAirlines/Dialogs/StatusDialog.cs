using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AMAAirlines.Dialogs
{
    public class StatusDialog : ComponentDialog
    {
        private const string MissingInformationPrompt = "missingInfoPrompt";

        public StatusDialog(string id)
       : base(id)
        {
            InitialDialogId = Id;

            // Define the conversation flow using a waterfall model.
            WaterfallStep[] waterfallSteps = new WaterfallStep[]
            {
                FlightStatusAsync
            };

            AddDialog(new WaterfallDialog(Id, waterfallSteps));
            AddDialog(new TextPrompt(MissingInformationPrompt));
        }

        private async Task<DialogTurnResult> FlightStatusAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            RecognizerResult result = stepContext.Options as RecognizerResult;

            // We can send messages to the user at any point in the WaterfallStep.
            var message = result.Intents.FirstOrDefault().ToString();
            await stepContext.Context.SendActivityAsync(message, cancellationToken: cancellationToken);
            return await stepContext.ReplaceDialogAsync("statusDialog", result, cancellationToken);
        }
    }
}
