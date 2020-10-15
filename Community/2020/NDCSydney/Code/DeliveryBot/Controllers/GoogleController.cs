using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.ActionsSDK;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;

namespace DeliveryBot.Controllers
{
    [Route("api/actionssdk")]
    [ApiController]
    public class ActionsSdkController : ControllerBase
    {
        private readonly ActionsSdkAdapter Adapter;
        private readonly IBot Bot;

        public ActionsSdkController(ActionsSdkAdapter adapter, IBot bot)
        {
            Adapter = adapter;
            Bot = bot;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await Adapter.ProcessAsync(Request, Response, Bot);
        }
    }
}
