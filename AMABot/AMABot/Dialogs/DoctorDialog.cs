using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
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
        public async Task StartAsync(IDialogContext context)
        {
            /* Wait until the first message is received from the conversation and call MessageReceviedAsync 
             *  to process that message. */
            var message = context.MakeMessage();
            message.Text = "Hello! May I know who do you want to book your appointment with?";

            await context.PostAsync(message);
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            /* When MessageReceivedAsync is called, it's passed an IAwaitable<IMessageActivity>. To get the message,
             *  await the result. */
            var message = await result;

            var appointmentMessage = context.MakeMessage();
            appointmentMessage.Text = $"That's great! Your appointment with Dr. { message } is booked.";

            await context.PostAsync(appointmentMessage);
            context.Done("dr. conversation ended.");
        }
    }
}