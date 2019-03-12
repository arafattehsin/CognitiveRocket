// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Models;
using System;
using System.Collections.Generic;

namespace CustomerServiceBot
{
    /// <summary>
    /// Stores counter state for the conversation.
    /// Stored in <see cref="Microsoft.Bot.Builder.ConversationState"/> and
    /// backed by <see cref="Microsoft.Bot.Builder.MemoryStorage"/>.
    /// </summary>
    public class BotServices
    {
        public CRMCredentials CRMUser;
        public BotServices(CRMCredentials credentials)
        {
            CRMUser = credentials;
        }      
    }

}
