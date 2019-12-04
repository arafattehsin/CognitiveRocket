using AdaptiveCards;
using EventsBot.Helpers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventsBot.Dialogs
{
    public class EventDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<Attendee> _attendeeProperty;

        public EventDialog(UserState userState) : base(nameof(EventDialog))
        {
            _attendeeProperty = userState.CreateProperty<Attendee>("Attendee");

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
             {
                NameStepAsync,
                PhoneNumberStepAsync,
                EmailAsync,
                ConfirmAsync,
                SummaryAsync
             };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        #region Waterfall Steps
        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Alright. Please enter your name.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> PhoneNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the state properties from the turn context.
            Attendee userProfile = await _attendeeProperty.GetAsync(stepContext.Context, () => new Attendee());
            userProfile.Name = (string)stepContext.Result;

            // Ask for a phone number.
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("May I know your phone number?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> EmailAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Attendee userProfile = await _attendeeProperty.GetAsync(stepContext.Context, () => new Attendee());
            userProfile.PhoneNumber = (string)stepContext.Result;

            // Ask for a phone number.
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("What about your email address?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Attendee userProfile = await _attendeeProperty.GetAsync(stepContext.Context, () => new Attendee());
            userProfile.Email = (string)stepContext.Result;

            // We can send messages to the user at any point in the WaterfallStep.
            // Create the message 
            var message = Activity.CreateMessageActivity();
            message.Attachments.Add(new Attachment()
            {
                Content = Helpers.Common.GetAdaptiveCard(userProfile),
                ContentType = AdaptiveCard.ContentType,
                Name = "Attendee Information",
            });

            await stepContext.Context.SendActivityAsync(message, cancellationToken: cancellationToken);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please confirm the above information.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                Attendee userProfile = await _attendeeProperty.GetAsync(stepContext.Context, () => new Attendee());

                try
                {
                    // Generate a lead in your CRM.
                    await stepContext.Context.SendActivityAsync("Registering you in our system..", cancellationToken: cancellationToken);
                    string referenceNumber = Common.GenerateReferenceID(6);
                    await stepContext.Context.SendActivityAsync($"Thanks {userProfile.Name}. You're now registered (Ref: {referenceNumber}). Someone from our company will contact you.", cancellationToken: cancellationToken);
                }
                catch (Exception)
                {
                    await stepContext.Context.SendActivityAsync($"There's an issue with our system. Please retry again later.", cancellationToken: cancellationToken);
                }

                return await stepContext.EndDialogAsync();
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
