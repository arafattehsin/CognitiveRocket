// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.5.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace VABotFramework.Bots
{
    public class VABot : ActivityHandler
    {
        private ILogger<VABot> _logger;
        private IBotServices _botServices;

        public VABot(IBotServices botServices, ILogger<VABot> logger)
        {
            _logger = logger;
            _botServices = botServices;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<Microsoft.Bot.Schema.IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // First, we use the dispatch model to determine which cognitive service (LUIS or QnA) to use.
            var recognizerResult = await _botServices.VABotDispatch.RecognizeAsync(turnContext, cancellationToken);

            // Top intent tell us which cognitive service to use.
            var topIntent = recognizerResult.GetTopScoringIntent();

            // Next, we call the dispatcher with the top intent.
            await DispatchToTopIntentAsync(turnContext, topIntent.intent, recognizerResult, cancellationToken);
        }

        private async Task DispatchToTopIntentAsync(ITurnContext<Microsoft.Bot.Schema.IMessageActivity> turnContext, string intent, object recognizerResult, CancellationToken cancellationToken)
        {
            switch (intent)
            {
                case "l_ATBankLUIS":
                    await turnContext.SendActivityAsync(MessageFactory.Text($"l_ATBankLUIS top intent {intent}."), cancellationToken);
                    break;
                case "l_DynamicsVA":
                    await ProcessVABotAsync(turnContext, cancellationToken);
                    break;                
                default:
                    _logger.LogInformation($"Dispatch unrecognized intent: {intent}.");
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Dispatch unrecognized intent: {intent}."), cancellationToken);
                    break;
            }
        }

        protected override async Task OnMembersAddedAsync(IList<Microsoft.Bot.Schema.ChannelAccount> membersAdded, ITurnContext<Microsoft.Bot.Schema.IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome!"), cancellationToken);
                }
            }
        }

        private async Task ProcessVABotAsync(ITurnContext<Microsoft.Bot.Schema.IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var token = await _botServices.VABotService.GetTokenAsync();

            using (var directLineClient = new DirectLineClient(token))
            {
                var conversation = await directLineClient.Conversations.StartConversationAsync();
                var conversationtId = conversation.ConversationId;

                var response = await directLineClient.Conversations.PostActivityAsync(conversationtId, new Microsoft.Bot.Connector.DirectLine.Activity()
                {
                    Type = Microsoft.Bot.Connector.DirectLine.ActivityTypes.Message,
                    From = new Microsoft.Bot.Connector.DirectLine.ChannelAccount { Id = "userId", Name = "userName" },
                    Text = turnContext.Activity.Text,
                    ChannelData = JObject.FromObject(_botServices.VABotService.ChannelData),
                    TextFormat = "plain",
                    Locale = "en-Us",
                });

                Thread.Sleep(4000);

                var activities = await GetActivitiesAsync(directLineClient, conversationtId, _botServices.VABotService.BotName);

                var activity = turnContext.Activity as Microsoft.Bot.Schema.Activity;

                await turnContext.SendActivitiesAsync(
                           activities
                           .Select(message =>
                           {
                               var reply = activity.CreateReply(message.Text);
                               reply.Attachments = message?.Attachments?.Select(a => new Microsoft.Bot.Schema.Attachment()
                               {
                                   Content = a.Content,
                                   ContentType = a.ContentType,
                                   ContentUrl = a.ContentUrl
                               }).ToList();

                               reply.SuggestedActions = new Microsoft.Bot.Schema.SuggestedActions()
                               {
                                   Actions = message?.SuggestedActions?.Actions?.Select(a => new Microsoft.Bot.Schema.CardAction()
                                   {
                                       Title = a.Title,
                                       Value = a.Value,
                                       Type = a.Type,
                                       Image = a.Image
                                   }).ToList(),
                               };

                               return reply;
                           })
                           .ToArray());
            }
        }

        private async Task<List<Microsoft.Bot.Connector.DirectLine.Activity>> GetActivitiesAsync(DirectLineClient directLineClient, string conversationtId, string botName)
        {
            ActivitySet response = null;
            List<Microsoft.Bot.Connector.DirectLine.Activity> result = new List<Microsoft.Bot.Connector.DirectLine.Activity>();
            string watermark = null;

            do
            {
                response = await directLineClient.Conversations.GetActivitiesAsync(conversationtId, watermark);
                watermark = response.Watermark;

                result = response?.Activities?.Where(x =>
                  x.Type == Microsoft.Bot.Connector.DirectLine.ActivityTypes.Message &&
                    string.Equals(x.From.Name, botName, StringComparison.Ordinal)).ToList();

                if (result != null && result.Any())
                { return result; }

                Thread.Sleep(1000);
            } while (response.Activities.Any());

            return result;
        }
    }
}
