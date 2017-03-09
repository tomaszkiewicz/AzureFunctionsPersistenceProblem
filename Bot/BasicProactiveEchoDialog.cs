using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Bot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-proactive
    [Serializable]
    public class BasicProactiveEchoDialog : IDialog<object>
    {
        protected int count = 1;

        [OnDeserialized]
        public void OnDeserialized(StreamingContext ctx)
        {
            
        }

        public BasicProactiveEchoDialog()
        {
            
        }
        
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            if (message.Text == "reset")
            {
                PromptDialog.Confirm(
                    context,
                    AfterResetAsync,
                    "Are you sure you want to reset the count?",
                    "Didn't get that!",
                    promptStyle: PromptStyle.Auto);
            }
            else
            {
                // Create a queue Message
                var queueMessage = new Message
                {
                    ResumptionCookie = new ResumptionCookie(message),
                    Text = message.Text
                };

                // write the queue Message to the queue

                await context.PostAsync($"{this.count++}: You said {queueMessage.Text}. Message added to the queue.");
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                this.count = 1;
                await context.PostAsync("Reset count.");
            }
            else
            {
                await context.PostAsync("Did not reset count.");
            }
            context.Wait(MessageReceivedAsync);
        }
    }
}