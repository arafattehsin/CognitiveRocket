using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Models;
using S2S.ServiceHelpers;
// using S2S.ServiceHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace S2S.Dialogs
{
    public class LeadDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<Prospect> _prospectProperty;
        protected readonly IConfiguration _configuration;
        CRMCredentials cRMCredentials;

        public LeadDialog(IConfiguration configuration, UserState userState) 
            : base(nameof(LeadDialog))
        {
            _prospectProperty = userState.CreateProperty<Prospect>("ProspectCustomer");
            _configuration = configuration;

            cRMCredentials = new CRMCredentials()
            {
                ClientID = _configuration["clientId"],
                ServiceUrl = _configuration["serviceUrl"],
                AuthpointEnd = _configuration["authEndpoint"],
                RedirectUrl = _configuration["redirectUrl"],
                Key = _configuration["key"]
            };

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
            Prospect userProfile = await _prospectProperty.GetAsync(stepContext.Context, () => new Prospect());
            userProfile.Name = (string)stepContext.Result;

            // Ask for a phone number.
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("May I know your phone number?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> EmailAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Prospect userProfile = await _prospectProperty.GetAsync(stepContext.Context, () => new Prospect());
            userProfile.PhoneNumber = (string)stepContext.Result;

            // Ask for a phone number.
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("What about your email address?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Prospect userProfile = await _prospectProperty.GetAsync(stepContext.Context, () => new Prospect());
            userProfile.Email = (string)stepContext.Result;

            // We can send messages to the user at any point in the WaterfallStep.
            // Create the message 
            var message = Activity.CreateMessageActivity();
            message.Attachments.Add(new Attachment()
            {
                Content = Helpers.Common.GetAdaptiveCard(userProfile),
                ContentType = AdaptiveCard.ContentType,
                Name = "Customer Information",
            });

            await stepContext.Context.SendActivityAsync(message, cancellationToken: cancellationToken);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please confirm the above information.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                Prospect userProfile = await _prospectProperty.GetAsync(stepContext.Context, () => new Prospect());

                try
                {
                    await stepContext.Context.SendActivityAsync("Fix the code of Lead Generation in D365 first.", cancellationToken: cancellationToken);

                    // Lead generation.. 
                    await stepContext.Context.SendActivityAsync("Registering you in our system..", cancellationToken: cancellationToken);
                    string referenceNumber = await D365WebAPIHelper.GenerateLead(userProfile, cRMCredentials);
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
