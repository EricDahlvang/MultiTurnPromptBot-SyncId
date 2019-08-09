// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        protected readonly ILogger Logger;
        
        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Ensure only one message per conversation is processed at a time
            using (var x = new AsyncDuplicateLock().Lock(turnContext.Activity.Conversation.Id))
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

        // https://stackoverflow.com/questions/31138179/asynchronous-locking-based-on-a-key
        public sealed class AsyncDuplicateLock
        {
            private sealed class RefCounted<T>
            {
                public RefCounted(T value)
                {
                    RefCount = 1;
                    Value = value;
                }

                public int RefCount { get; set; }
                public T Value { get; private set; }
            }

            private static readonly Dictionary<object, RefCounted<SemaphoreSlim>> SemaphoreSlims
                                  = new Dictionary<object, RefCounted<SemaphoreSlim>>();

            private SemaphoreSlim GetOrCreate(object key)
            {
                RefCounted<SemaphoreSlim> item;
                lock (SemaphoreSlims)
                {
                    if (SemaphoreSlims.TryGetValue(key, out item))
                    {
                        ++item.RefCount;
                    }
                    else
                    {
                        item = new RefCounted<SemaphoreSlim>(new SemaphoreSlim(1, 1));
                        SemaphoreSlims[key] = item;
                    }
                }
                return item.Value;
            }

            public IDisposable Lock(object key)
            {
                GetOrCreate(key).Wait();
                return new Releaser { Key = key };
            }

            public async Task<IDisposable> LockAsync(object key)
            {
                await GetOrCreate(key).WaitAsync().ConfigureAwait(false);
                return new Releaser { Key = key };
            }

            private sealed class Releaser : IDisposable
            {
                public object Key { get; set; }

                public void Dispose()
                {
                    RefCounted<SemaphoreSlim> item;
                    lock (SemaphoreSlims)
                    {
                        item = SemaphoreSlims[Key];
                        --item.RefCount;
                        if (item.RefCount == 0)
                            SemaphoreSlims.Remove(Key);
                    }
                    item.Value.Release();
                }
            }
        }
    }
}
