using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace AMABot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private IEnumerable<string> RoutingOption = new List<string> { "Doctor", "Broadband" };

        #region Event Handlers
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }
        public Task MessageReceivedAsync(IDialogContext context, IAwaitable<IActivity> argument)
        {
            Prompt(context);
            return Task.CompletedTask;
        }

        private Task ResumeAfterDoctorDialog(IDialogContext context, IAwaitable<object> result)
        {
            return Task.CompletedTask;
        }

        private Task ResumeAfterBroadbandDialog(IDialogContext context, IAwaitable<object> result)
        {
            return Task.CompletedTask;
        }
        #endregion

        #region Helper Methods
        void Prompt(IDialogContext context)
        {
            try
            {
                // Prompt Choice
                PromptDialog.Choice<string>(
                    context: context,
                    resume: SelectedOption,
                    options: RoutingOption,
                    prompt: "Hello! How may I help you with?",
                    retry: "Sorry, I did not understand what you mean by that.",
                    attempts: 3,
                    promptStyle: PromptStyle.Keyboard);
            }
            catch (TooManyAttemptsException)
            {
                // Log exception
            }
        }

        private async Task SelectedOption(IDialogContext context, IAwaitable<string> result)
        {
            string selectedOption = await result;
            switch (selectedOption)
            {
                case "Doctor":
                    context.Call(new DoctorDialog(), this.ResumeAfterDoctorDialog);
                    break;
                case "Broadband":
                    await CallBroadbandDialog(context, selectedOption);
                    break;
                default:
                    break;
            }
        }

        Task CallBroadbandDialog(IDialogContext context, string text)
        {
            var message = context.MakeMessage();
            message.Text = text;

            return context.Forward(new BroadbandDialog(), this.ResumeAfterBroadbandDialog, message, CancellationToken.None);
        }

        #endregion
    }


}