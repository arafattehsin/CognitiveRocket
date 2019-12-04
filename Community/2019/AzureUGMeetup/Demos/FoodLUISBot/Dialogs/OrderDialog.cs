using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FoodLUISBot.Dialogs
{
    public class OrderDialog : ComponentDialog
    {
        public OrderDialog()
           : base(nameof(OrderDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PlaceOrderStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        Task<DialogTurnResult> PlaceOrderStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Create message 
            var messageActivity = Activity.CreateMessageActivity();
            messageActivity.Attachments.Add(CreateAdaptiveCardForOrder());
            stepContext.Context.SendActivityAsync(messageActivity, cancellationToken: cancellationToken);
            return Task.FromResult(Dialog.EndOfTurn);
        }

        private Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = JsonConvert.DeserializeObject<Data>(stepContext.Context.Activity.Value.ToString());
            stepContext.Context.SendActivityAsync($"{data.FoodChoice} will be delivered to you shortly.", cancellationToken: cancellationToken);
            return stepContext.EndDialogAsync(cancellationToken);
        }

        private Attachment CreateAdaptiveCardForOrder()
        {
            var cardResourcePath = "FoodLUISBot.Cards.foodOrder.json";

            using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    var adaptiveCard = reader.ReadToEnd();
                    return new Attachment()
                    {
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        Content = JsonConvert.DeserializeObject(adaptiveCard),
                    };
                }
            }
        }
    }
}
