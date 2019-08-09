// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples
{
    // This ASP Controller is created to handle a request for a token.
    [Route("api/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        const string TokenGenerationUrl = "https://directline.botframework.com/v3/directline/tokens/generate";
        private IConfiguration _configuration;
        
        public TokenController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        [HttpGet]
        public async Task<DirectLineToken> GetAsync()
        {
            string userId = $"dl_{Guid.NewGuid().ToString()}";

            using (var request = new HttpRequestMessage(HttpMethod.Post, TokenGenerationUrl))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _configuration["DirectLineSecret"]);
                request.Content = new StringContent(JsonConvert.SerializeObject(new { User = new { Id = userId } }), Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    using (HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false))
                    {
                        DirectLineToken result;
                        if (response.IsSuccessStatusCode)
                        {
                            var body = await response.Content.ReadAsStringAsync();
                            var dlToken = JsonConvert.DeserializeObject<DirectLineToken>(body);
                            dlToken.userId = userId;
                            return dlToken;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Calls to generate a token will return json in the 
        /// format of this class.
        /// </summary>
        public class DirectLineToken
        {
            public string userId { get; set; }
            public string conversationId { get; set; }
            public string token { get; set; }
            public int expires_in { get; set; }
            public string streamUrl { get; set; }
        }
    }
}
