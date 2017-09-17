
namespace AskSAPBOT.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Models;
    using System.Xml;
    using System.Web;
    using System.IO;
    using AskSAPBot.Helpers;
    using AskSAPBot.Dialogs;
    using System.Threading;
    using Microsoft.Azure.Documents;
    using Microsoft.Bot.Connector;
    using System.Collections.Generic;
    using System.Linq;

    [LuisModel("01f57024-70b3-4c4f-b219-803102e1ed28", "8ae492516bbf4aadbc78ed0cec81945e")]
    [Serializable]
    public class SAPLuisDialog : LuisDialog<string>
    {

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string possibleQuery;
            if (context.PrivateConversationData.TryGetValue("lastquery", out possibleQuery))
            {
                result.Intents[0].Intent =  possibleQuery;
                if (possibleQuery == "ClosingComment")
                {
                    await context.Logout();
                    ActionDialog AD = new ActionDialog();
                    context.Wait(AD.MessageReceivedAsync);
                    await context.PostAsync("Bye Stranger!;)");
                    return;
                }
            }
            context.PrivateConversationData.SetValue("lastquery", result.Intents[0].Intent);
            string message = "Sorry I didn't understand that:(";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
       
        }

        [LuisIntent("greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            //Sharepoint sp = new Sharepoint();
            // sp.SharePointCheck();
            string message = $"Hi... ";
            AuthResult authResult;
            if (context.UserData.TryGetValue(ContextConstants.AuthResultKey, out authResult))
            {
                message = message + " " + authResult.UserName;
            }
            message = message + "! How may I help You?:)";
            string possibleQuery;
            if (context.PrivateConversationData.TryGetValue("lastquery", out possibleQuery))
            {
                if (possibleQuery == "greeting")
                {
                   
                    message = "I heard you, How can i help you";
                }
            }
            context.PrivateConversationData.SetValue("lastquery", result.Intents[0].Intent);
            await context.PostAsync(message);
            context.Done("Intent found: Greeting");
        }

        [LuisIntent("ClosingComment")]
        public async Task ClosingComment(IDialogContext context, LuisResult result)
        {
            context.PrivateConversationData.SetValue("lastquery", result.Intents[0].Intent);
            string message = $"Hope the conversation is helpful, have nice day, Bye...DO you want me to forget your crendentials?";
            await context.PostAsync(message);
            context.Done("Intent found: ClosingComment");
        }

        private List<CardAction> GetCardButton(List<string> opts)
        {
            List<CardAction> cardButtons = new List<CardAction>();
            int i = 1;
            foreach(string opt in opts)
            {
                CardAction plButton = new CardAction()
                {
                    Title = opt,
                    Type = "postBack",
                    Text = i.ToString(),
                    Value = i.ToString()
                };
                cardButtons.Add(plButton);
                i++;
            }
            
            return cardButtons;

        }
        [LuisIntent("grcCreateReq")]
        public async Task grcCreateReq(IDialogContext context, LuisResult result)
        {
            context.PrivateConversationData.SetValue("lastquery", result.Intents[0].Intent);
            List<Microsoft.Bot.Connector.Attachment> plAttachment = new List<Microsoft.Bot.Connector.Attachment>();
            List<string> cardOpts = new List<string>();
            cardOpts.Add("known roles & system");
            cardOpts.Add("model ID");
            cardOpts.Add("for others with roles & system");
            cardOpts.Add("for others by using model ID");
            cardOpts.Add("bulk (many)");
            cardOpts.Add("Extending SAP system Access");
            List<CardAction> cardButtons = GetCardButton(cardOpts);
            plAttachment = GetCardsAttachments(cardButtons, plAttachment);
        
            IMessageActivity response = context.MakeMessage();          
            response.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            response.Attachments = plAttachment;
            await context.PostAsync(response);
            context.Wait(ResumeAfterFirstDialog);
        }
        private Microsoft.Bot.Connector.Attachment GetHeroCard(List<CardAction> buttons)
        {
            var heroCard = new HeroCard();
            heroCard.Title = "Creating GRC Request";
            heroCard.Text = "with:";
            heroCard.Buttons = buttons;
            return heroCard.ToAttachment();
        }
        private List<Microsoft.Bot.Connector.Attachment> GetCardsAttachments(List<CardAction> cardButtons, List<Microsoft.Bot.Connector.Attachment> plAttachment)
        {
            if (cardButtons.Count <= 3)
            {
                plAttachment.Add(GetHeroCard(cardButtons));
            }
            else
            {
                var temp = new List<CardAction>();
                for (int i = 0; i < cardButtons.Count; i++)
                {
                    if (i % 3 != 0)
                    {
                        temp.Add(cardButtons.ElementAt(i));
                    }
                    else
                    {
                        if (i != 0)
                        {
                            plAttachment.Add(GetHeroCard(temp));
                        }

                        temp = new List<CardAction>();
                        temp.Add(cardButtons.ElementAt(i));
                    }

                }
                plAttachment.Add(GetHeroCard(temp));
            }
            return plAttachment;
        }

        public virtual async Task ResumeAfterFirstDialog(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            try
            {
                    var retVal = await result;
                    string possibleQuery;
                    string DocName=null;
                    int st;
                    if (context.PrivateConversationData.TryGetValue("lastquery", out possibleQuery))
                    {
                        if (possibleQuery == "grcCreateReq" && int.TryParse(retVal.Text, out st) == true && st>=1 && st<=6)
                        {
                        switch (st)
                        {
                            case 1: DocName = "grcCreateReq"; break;
                            case 2: DocName = "grcCreateReqId"; break;
                            case 3: DocName = "grcCreateReqOth"; break;
                            case 4: DocName = "grcCreateReqIdOth"; break;
                            case 5: DocName = "grcCreateReqBulk"; break;
                            case 6: DocName = "grcCreateReqExt"; break;
                            
                        }
                            GrcQ1Dialog dialog = new GrcQ1Dialog("Grc Create Request", DocName);
                            await context.Forward(dialog, this.doneMethod, "PassOnmsg", CancellationToken.None);
                    
                        }
                    else
                    {
                        context.Done("grc req complete");
                    }
                    }
            }
            catch (Exception e)
            {
                await (context.PostAsync("Your query failed with " + e.Message));
            }

        }

        [LuisIntent("grc")]
        public async Task grc(IDialogContext context,LuisResult result)
        {

            GrcQ1Dialog dialog = new GrcQ1Dialog("Grc", "grc");
            try
            {
                await context.Forward(dialog, this.doneMethod, "PassOnmsg", CancellationToken.None);
            }
            catch (Exception e)
            {
                await (context.PostAsync("Your query failed with " + e.Message));
            }
        } 
        //Done method for all intents
        private async Task doneMethod(IDialogContext context, IAwaitable<object> result)
        {
            var response = await result;
            context.Done("done method called");
        }
       
    }
}
