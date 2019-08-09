// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        ConversationState _conversationState;
        public const string LastActivityIdPropertyName = "LastActivityId";

        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            var lastMessage = activities.Where(a => a.Type == ActivityTypes.Message).LastOrDefault();
            if (lastMessage != null)
            {
                // Add an arbitrary id to the last outgoing message's channelData
                // This is then sent back with the next reply from WebChat (see default.html DIRECT_LINE/POST_ACTIVITY)
                var lastIdProperty = _conversationState.CreateProperty<string>(LastActivityIdPropertyName);
                var lastId = Guid.NewGuid().ToString();
                lastMessage.ChannelData = new { syncId = lastId };
                await lastIdProperty.SetAsync(turnContext, lastId);
            }

            return await base.SendActivitiesAsync(turnContext, activities, cancellationToken);
        }

        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger, ConversationState conversationState = null)
            : base(configuration, logger)
        {
            _conversationState = conversationState;

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message}");

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync("Sorry, it looks like something went wrong.");

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Exception caught on attempting to Delete ConversationState : {e.Message}");
                    }
                }
            };
        }

    }
}
