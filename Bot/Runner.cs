using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot
{
    public static class Runner
    {
        public static async Task<object> Run(HttpRequestMessage req)
        {
            // Initialize the azure bot
            using (BotService.Initialize())
            {
                // Deserialize the incoming activity
                string jsonContent = await req.Content.ReadAsStringAsync();
                var activity = JsonConvert.DeserializeObject<Activity>(jsonContent);

                // authenticate incoming request and add activity.ServiceUrl to MicrosoftAppCredentials.TrustedHostNames
                // if request is authenticated
                if (!await BotService.Authenticator.TryAuthenticateAsync(req, new[] {activity}, CancellationToken.None))
                {
                    return BotAuthenticator.GenerateUnauthorizedResponse(req);
                }

                if (activity != null)
                {
                    // one of these will have an interface and process it
                    switch (activity.GetActivityType())
                    {
                        case ActivityTypes.Message:
                            await Conversation.SendAsync(activity, () => new BasicProactiveEchoDialog());
                            break;
                        case ActivityTypes.ConversationUpdate:
                            var client = new ConnectorClient(new Uri(activity.ServiceUrl));
                            IConversationUpdateActivity update = activity;
                            if (update.MembersAdded.Any())
                            {
                                var reply = activity.CreateReply();
                                var newMembers = update.MembersAdded?.Where(t => t.Id != activity.Recipient.Id);
                                foreach (var newMember in newMembers)
                                {
                                    reply.Text = "Welcome";
                                    if (!string.IsNullOrEmpty(newMember.Name))
                                    {
                                        reply.Text += $" {newMember.Name}";
                                    }
                                    reply.Text += "!";
                                    await client.Conversations.ReplyToActivityAsync(reply);
                                }
                            }
                            break;
                        case ActivityTypes.Trigger:
                            // handle proactive Message from function
                            ITriggerActivity trigger = activity;
                            var message = JsonConvert.DeserializeObject<Message>(((JObject) trigger.Value).GetValue("Message").ToString());
                            var messageactivity = (Activity) message.ResumptionCookie.GetMessage();

                            client = new ConnectorClient(new Uri(messageactivity.ServiceUrl));
                            var triggerReply = messageactivity.CreateReply();
                            triggerReply.Text = $"This is coming back from the trigger! {message.Text}";
                            await client.Conversations.ReplyToActivityAsync(triggerReply);
                            break;
                        case ActivityTypes.ContactRelationUpdate:
                        case ActivityTypes.Typing:
                        case ActivityTypes.DeleteUserData:
                        case ActivityTypes.Ping:
                        default:
                            break;
                    }
                }
                return req.CreateResponse(HttpStatusCode.Accepted);
            }
        }
    }
}