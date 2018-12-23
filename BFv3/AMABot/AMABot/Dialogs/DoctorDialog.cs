using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AMABot.Dialogs
{
    [Serializable]
    public class DoctorDialog : IDialog<object>
    {
        List<string> doctorNames = new List<string> { "Ibrahim","Saleh","Robert","Katherine","Hussain",
                                                            "Sanderson","Michelle","Anita" };
        string doctorName = string.Empty;

        #region Event Handlers
        public Task StartAsync(IDialogContext context)
        {
            /* Wait until the first message is received from the conversation and call MessageReceviedAsync 
             *  to process that message. */
            SendBookApptMessage(context);
            context.Wait(this.MessageReceivedAsync);
            return Task.CompletedTask;
        }


        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            /* When MessageReceivedAsync is called, it's passed an IAwaitable<IMessageActivity>. To get the message,
             *  await the result. */
            var message = await result;

            doctorName = CognitiveLibrary.Utilities.LevenshteinDistance.GetClosest(doctorNames, message.Text);

            var appointmentMessage = context.MakeMessage();
            appointmentMessage.Text = $"So, you want to meet Dr. { doctorName } right?";

            await context.PostAsync(appointmentMessage);
            Prompt(context);
        }

        private async Task SubmitMessageReceived(IDialogContext context, IAwaitable<object> result)
        {
            IMessageActivity message = context.Activity.AsMessageActivity();
            var jsonresult = JsonConvert.DeserializeObject<Appointment>(message.Value.ToString());

            var finalReply = context.MakeMessage();
            finalReply.Text = $"Your appointment is successfully booked with Dr. {doctorName} at {jsonresult.time} on {jsonresult.date}";
            await context.PostAsync(finalReply);
            context.Done<string>("conversation ended.");
        }

#endregion

        #region Helper Methods
        void Prompt(IDialogContext context)
        {
            try
            {
                // Prompt Choice
                PromptDialog.Confirm(
                    context: context,
                    resume: SelectedOption,
                    prompt: "Please select either Yes or No.",
                    retry: "Sorry, I did not understand what you mean by that.",
                    attempts: 3,
                    promptStyle: PromptStyle.Keyboard);
            }
            catch (TooManyAttemptsException)
            {
                // Log exception
            }
        }

        private async Task SelectedOption(IDialogContext context, IAwaitable<bool> result)
        {
            bool selectedOption = await result;
            if (selectedOption)
            {
                // Show Adaptive Card Method
                AdaptiveCard card = new AdaptiveCard();
                // Add text to the card.
                card.Body.Add(new TextBlock()
                {
                    Text = $"Appointment with Dr. { doctorName }",
                    Size = TextSize.Large,
                    Weight = TextWeight.Bolder
                });

                card.Body.Add(new DateInput()
                {
                    Id = "date"
                });

                // Add list of choices to the card.
                card.Body.Add(new ChoiceSet()
                {
                    Id = "time",
                    Style = ChoiceInputStyle.Compact,
                    Choices = new List<Choice>()
                    {
                        new Choice() { Title = "11:30", Value = "1130", IsSelected = true },
                        new Choice() { Title = "14:00", Value = "1400" },
                        new Choice() { Title = "16:45", Value = "1645" }
                    }
                });

                card.Actions.Add(new SubmitAction()
                {
                    Title = "Submit",
                });

                // Create the attachment.
                Attachment attachment = new Attachment()
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card
                };

                var message = context.MakeMessage();
                message.Attachments.Add(attachment);
                await context.PostAsync(message);
                context.Wait(SubmitMessageReceived);
            }
            else
                SendBookApptMessage(context);
        }


        private async void SendBookApptMessage(IDialogContext context)
        {
            var message = context.MakeMessage();
            message.Text = "Hello! May I know who do you want to book your appointment with?";

            await context.PostAsync(message);
        }

        #endregion
    }


    public class Appointment
    {
        public string date { get; set; }
        public string time { get; set; }
    }

}