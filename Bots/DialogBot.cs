﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AsyncKeyedLock;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        private readonly AsyncKeyedLocker<string> AsyncKeyedLocker;
        protected readonly ILogger Logger;
        
        public DialogBot(ConversationState conversationState, UserState userState, T dialog, AsyncKeyedLocker<string> asyncKeyedLocker, ILogger<DialogBot<T>> logger)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Ensure only one message per conversation is processed at a time
            using (await AsyncKeyedLocker.LockAsync(turnContext.Activity.Conversation.Id, cancellationToken).ConfigureAwait(false))
            {
                var channelData = turnContext.Activity.ChannelData;
                if (channelData != null)
                {
                    // check syncId against this conversation's previously stored syncId
                    var channelDataDeserialized = JsonConvert.DeserializeObject(channelData.ToString());
                    var asDictionary = channelDataDeserialized as IDictionary<string, JToken>;
                    if (asDictionary != null && asDictionary.ContainsKey("syncId"))
                    {
                        string repliedToId = asDictionary["syncId"].Value<string>();

                        var lastIdProperty = ConversationState.CreateProperty<string>(AdapterWithErrorHandler.LastActivityIdPropertyName);
                        string expectedReplyToId = await lastIdProperty.GetAsync(turnContext, () => string.Empty);

                        if (!string.IsNullOrEmpty(repliedToId) && !string.IsNullOrEmpty(expectedReplyToId))
                        {
                            // If the ids do not match, just ignore this message
                            // (this means the user sent an unexpected message)
                            if (expectedReplyToId != repliedToId)
                            {
                                return;
                            }
                        }
                    }
                }
                await base.OnTurnAsync(turnContext, cancellationToken);

                // Save any state changes that might have occured during the turn.
                await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }
    }
}
