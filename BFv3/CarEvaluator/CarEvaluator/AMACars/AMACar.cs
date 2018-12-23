using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;

namespace AMACars
{
    public class AMACar : IBot
    {
        private readonly DialogSet dialogs;

        public AMACar()
        {
            dialogs = new DialogSet();

            var choicePrompt = new ChoicePrompt(Culture.English)
            {
                Style = Microsoft.Bot.Builder.Prompts.ListStyle.Auto
            };

            // Register the card prompt
            dialogs.Add("choicePrompt", choicePrompt);

            var buyingOptions = GenerateBuyingOptions();
            var maintenceOptions = GenerateMaintenanceOptions();

            dialogs.Add("firstRun",
                 new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                        dc.ActiveDialog.State = new Dictionary<string, object>();
                        await dc.Prompt("choicePrompt", "What was the buying price?", GenerateBuyingOptions());
                    },
                    async (dc, args, next) =>
                    {
                        dc.ActiveDialog.State["buying"] = FoundChoice(args["Value"] as FoundChoice);
                        await dc.Prompt("choicePrompt", "What's the over all price for maintenance?", GenerateMaintenanceOptions());
                    },
                     async (dc, args, next) =>
                    {
                        dc.ActiveDialog.State["maintenance"] = FoundChoice(args["Value"] as FoundChoice);
                        await dc.Prompt("choicePrompt", "How many doors does your car have?", GenerateDoorOptions());
                    },
                    async (dc, args, next) =>
                    {
                        dc.ActiveDialog.State["doors"] = FoundChoice(args["Value"] as FoundChoice);
                        await dc.Prompt("choicePrompt", "How many persons can sit in this car?", GeneratePersonOptions());
                    },
                        async (dc, args, next) =>
                    {
                        dc.ActiveDialog.State["persons"] = FoundChoice(args["Value"] as FoundChoice);
                        await dc.Prompt("choicePrompt", "How big is the size of luggage boot?", GenerateLugBootOptions());
                    },
                           async (dc, args, next) =>
                    {
                        dc.ActiveDialog.State["lugboot"] = FoundChoice(args["Value"] as FoundChoice);
                        await dc.Prompt("choicePrompt", "How do you rate the safety of your car?", GenerateSafetyOptions());
                    },
                    async (dc, args, next) =>
                    {
                        dc.ActiveDialog.State["safety"] = FoundChoice(args["Value"] as FoundChoice);

                        // Calculation of model and sending option.                        
                       var result = Car.CarEvalPredictor.GetPredictedResult(dc.ActiveDialog.State["buying"].ToString(),
                            dc.ActiveDialog.State["maintenance"].ToString(),
                            dc.ActiveDialog.State["doors"].ToString(),
                            dc.ActiveDialog.State["persons"].ToString(),
                            dc.ActiveDialog.State["lugboot"].ToString(),
                            dc.ActiveDialog.State["safety"].ToString());

                        // Send the result
                        await dc.Context.SendActivity($"Your car value is: {result}");

                        await dc.End(dc.ActiveDialog.State);
                    }
                }
            );

        }
        /// <summary>
        /// Every Conversation turn for our EchoBot will call this method. In here
        /// the bot checks the Activty type to verify it's a message, bumps the 
        /// turn conversation 'Turn' count, and then echoes the users typing
        /// back to them. 
        /// </summary>
        /// <param name="context">Turn scoped context containing all the data needed
        /// for processing this conversation turn. </param>        
        public async Task OnTurn(ITurnContext turnContext)
        {
            try
            {
                switch (turnContext.Activity.Type)
                {
                    case ActivityTypes.ConversationUpdate:
                        foreach (var newMember in turnContext.Activity.MembersAdded)
                        {
                            if (newMember.Id != turnContext.Activity.Recipient.Id)
                            {
                                await turnContext.SendActivity("Hello and welcome to the Car Evaluation Bot.");
                            }
                        }
                        break;

                    case ActivityTypes.Message:
                        var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                        var dc = dialogs.CreateContext(turnContext, state);

                        await dc.Continue();

                        if (!turnContext.Responded)
                        {
                            await dc.Begin("firstRun");
                        }

                        break;
                }
            }
            catch (Exception e)
            {
                await turnContext.SendActivity($"Exception: {e.Message}");
            }
        }

        private string FoundChoice(FoundChoice choice)
        {
            return choice.Value;
        }

        private ChoicePromptOptions GenerateBuyingOptions()
        {
            return new ChoicePromptOptions()
            {
                Choices = new List<Choice>()
                {
                    new Choice()
                    {
                        Value = "Very High"
                    },
                    new Choice()
                    {
                        Value = "High"
                    },
                    new Choice()
                    {
                        Value = "Medium"
                    },
                     new Choice()
                    {
                        Value = "Low"
                    }

                }
            };
        }

        private ChoicePromptOptions GenerateMaintenanceOptions()
        {
            return new ChoicePromptOptions()
            {
                Choices = new List<Choice>()
                {
                    new Choice()
                    {
                        Value = "Very High"
                    },
                    new Choice()
                    {
                        Value = "High"
                    },
                    new Choice()
                    {
                        Value = "Medium"
                    },
                     new Choice()
                    {
                        Value = "Low"
                    }

                }
            };
        }

        private ChoicePromptOptions GenerateDoorOptions()
        {
            return new ChoicePromptOptions()
            {
                Choices = new List<Choice>()
                {
                    new Choice()
                    {
                        Value = "2"
                    },
                    new Choice()
                    {
                        Value = "3"
                    },
                    new Choice()
                    {
                        Value = "4"
                    },
                     new Choice()
                    {
                        Value = "5 or more"
                    }

                }
            };
        }

        private ChoicePromptOptions GeneratePersonOptions()
        {
            return new ChoicePromptOptions()
            {
                Choices = new List<Choice>()
                {
                    new Choice()
                    {
                        Value = "2"
                    },
                    new Choice()
                    {
                        Value = "4"
                    },
                     new Choice()
                    {
                        Value = "More"
                    }

                }
            };
        }

        private ChoicePromptOptions GenerateLugBootOptions()
        {
            return new ChoicePromptOptions()
            {
                Choices = new List<Choice>()
                {
                    new Choice()
                    {
                        Value = "Small"
                    },
                    new Choice()
                    {
                        Value = "Medium"
                    },
                    new Choice()
                    {
                        Value = "Big"
                    },
                }
            };
        }

        private ChoicePromptOptions GenerateSafetyOptions()
        {
            return new ChoicePromptOptions()
            {
                Choices = new List<Choice>()
                {
                    new Choice()
                    {
                        Value = "Low"
                    },
                    new Choice()
                    {
                        Value = "Medium"
                    },
                    new Choice()
                    {
                        Value = "High"
                    },
                }
            };
        }
    }
}
