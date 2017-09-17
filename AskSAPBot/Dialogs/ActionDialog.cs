namespace AskSAPBOT.Dialogs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AskSAPBOT.Models;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using System.Configuration;
    using AskSAPBOT;

    [Serializable]
    public class ActionDialog : IDialog<string>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task TokenSample(IDialogContext context)
        {
            //endpoint v2
            var accessToken = await context.GetAccessToken(AuthSettings.Scopes);

            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            await context.PostAsync($"Your access token is: {accessToken}");

            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;

            if (message.Text == "logon")
            {
                //endpoint v2
                if (string.IsNullOrEmpty(await context.GetAccessToken(AuthSettings.Scopes)))
                {
                    await context.Forward(new AzureAuthDialog(AuthSettings.Scopes), this.ResumeAfterAuth, message, CancellationToken.None);
                }
                else
                {       
                    context.Wait(MessageReceivedAsync);
                }
            }
            else if (message.Text == "logout")
            {
                await context.Logout();
                context.Wait(this.MessageReceivedAsync);
            }
            else
            {
                AuthResult authResult;
                context.UserData.TryGetValue(ContextConstants.AuthResultKey, out authResult);
                    if (authResult != null)
                {
                    SAPLuisDialog dialog = new SAPLuisDialog();
                    try
                    {
                        await context.Forward(dialog, this.resumeFunc, message, CancellationToken.None);
                }
                    catch (Exception e)
                {
                }
            }
                else {
                   
                        await context.PostAsync("I don't Interact with Strangers,..");
                    if (string.IsNullOrEmpty(await context.GetAccessToken(AuthSettings.Scopes)))
                    {
                        await context.Forward(new AzureAuthDialog(AuthSettings.Scopes), this.ResumeAfterAuth, message, CancellationToken.None);
                    }
                    else
                    {
                        context.Wait(MessageReceivedAsync);
                    }
                  //  context.Wait(MessageReceivedAsync);
                   
                }
            }
        }
                private async Task resumeFunc(IDialogContext context, IAwaitable<object> result)
        {
            var response = await result;
            context.Wait(MessageReceivedAsync);
        }

        private async Task ResumeAfterAuth(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;

            await context.PostAsync(message);
            context.Wait(MessageReceivedAsync);
        }
    }
}


