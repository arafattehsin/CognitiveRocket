using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FoodDispatchBot.Dialogs
{
    public class TrackDialog : ComponentDialog
    {
        private const string RequestReferenceNumberText = "Alright! Can you please provide me the order reference number?";

        public TrackDialog()
           : base(nameof(TrackDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                OrderNumberStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private Task<DialogTurnResult> OrderNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptMessage = MessageFactory.Text(RequestReferenceNumberText, RequestReferenceNumberText, InputHints.ExpectingInput);
            return stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var referenceNumber = stepContext.Result; // Although it's not being used but just for the reference.
            var messageActivity = CreateMessage();
            messageActivity.Text = $"Your food is just 5 minutes away. Here's the latest snapshot of our rider.";
            stepContext.Context.SendActivityAsync(messageActivity, cancellationToken: cancellationToken);
            return stepContext.EndDialogAsync(cancellationToken);
        }

        private static Attachment GetInlineAttachment()
        {
            var imagePath = Path.Combine(Environment.CurrentDirectory, @"Assets\food-track.PNG");
            var imageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));

            return new Attachment
            {
                Name = @"Assets\food-track.PNG",
                ContentType = "image/png",
                ContentUrl = $"data:image/png;base64,{imageData}",
            };
        }

        private IMessageActivity CreateMessage()
        {

            // Create message 
            var message = Activity.CreateMessageActivity();
            message.Attachments.Add(GetInlineAttachment());

            return message;
        }
    }
}
