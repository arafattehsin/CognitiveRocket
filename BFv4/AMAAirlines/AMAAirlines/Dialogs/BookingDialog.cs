using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AMAAirlines.Dialogs
{
    public class BookingDialog : ComponentDialog
    {
        public BookingDialog(string id)
        : base(id)
        {
            InitialDialogId = Id;

            // Define the conversation flow using a waterfall model.
            WaterfallStep[] waterfallSteps = new WaterfallStep[]
            {
                BookFlight
            };
            AddDialog(new WaterfallDialog(Id, waterfallSteps));
        }

        private Task<DialogTurnResult> BookFlight(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
