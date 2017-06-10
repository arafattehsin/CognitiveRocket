using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AMABot.Dialogs
{
    [Serializable]
    [LuisModel("AppID", "SubscriptionKey")]
    public class BroadbandDialog : LuisDialog<object>
    {
        public BroadbandDialog()
        {

        }

        [LuisIntent("")]
        [LuisIntent("None")]
        public Task None(IDialogContext context, LuisResult result)
        {
            context.Wait(this.MessageReceived);
            return Task.CompletedTask;   
        }

        [LuisIntent("Greetings")]
        public Task Greetings(IDialogContext context, LuisResult result)
        {
            context.Wait(this.MessageReceived);
            return Task.CompletedTask;
        }

        [LuisIntent("ApplyPackage")]
        public Task ApplyPackage(IDialogContext context, LuisResult result)
        {
            context.Wait(this.MessageReceived);
            return Task.CompletedTask;
        }

        [LuisIntent("GetBroadbandInfo")]
        public Task GetBroadbandInfo(IDialogContext context, LuisResult result)
        {
            context.Wait(this.MessageReceived);
            return Task.CompletedTask;
        }
    }
}