// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Models;

namespace CustomerServiceBot
{
    /// <summary>
    /// This class is created as a Singleton and passed into the IBot-derived constructor.
    ///  - See <see cref="EchoWithCounterBot"/> constructor for how that is injected.
    ///  - See the Startup.cs file for more details on creating the Singleton that gets
    ///    injected into the constructor.
    /// </summary>
    public class CustomerServiceBotAccessors
    {
        // The property accessor keys to use.
        public const string CustomerInfoAccessorName = "CustomerServiceBotAccessors.CustomerInfo";
        public const string DialogStateAccessorName = "CustomerServiceBotAccessors.DialogState";

        /// <summary>
        /// Initializes a new instance of the <see cref="BotAccessors"/> class.
        /// Contains the <see cref="ConversationState"/> and associated <see cref="IStatePropertyAccessor{T}"/>.
        /// </summary>
        /// <param name="conversationState">The state object that stores the counter.</param>
        public CustomerServiceBotAccessors(ConversationState conversationState, UserState userState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
        }

        /// <summary>Gets or sets the state property accessor for the user information we're tracking.</summary>
        /// <value>Accessor for user information.</value>
        public IStatePropertyAccessor<Prospect> ProspectInfoAccessor { get; set; }

        /// <summary>Gets or sets the state property accessor for the dialog state.</summary>
        /// <value>Accessor for the dialog state.</value>
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        /// <summary>Gets the conversation state for the bot.</summary>
        /// <value>The conversation state for the bot.</value>
        public ConversationState ConversationState { get; }

        /// <summary>Gets the user state for the bot.</summary>
        /// <value>The user state for the bot.</value>
        public UserState UserState { get; }
    }
}
