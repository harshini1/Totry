using AskSAPBot.Helpers;
using AskSAPBOT.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AskSAPBot.Dialogs
{
    [Serializable]
    public class GrcQ1Dialog : IDialog<object>
    {
        private string title = null;
        private string docDbName = null;

        public GrcQ1Dialog(string titl, string docName)
        {
            title = titl;
            docDbName = docName;
        }
        private List<CardAction> GetCardButton(string value, string act, string tex)
        {
            List<CardAction> cardButtons = new List<CardAction>();
            CardAction plButton = new CardAction()
            {
                Value = value,
                Type = act,
                Title = tex,
                Text = value,
                DisplayText = ""
            };
            cardButtons.Add(plButton);
            return cardButtons;

        }
        private IMessageActivity GetStepInfoCard(IDialogContext context, string step, string text, string imageUrl)
        {

            Attachment plAttachment = null;
            List<CardImage> cardImage = GetCardImage(imageUrl);
            HeroCard plCard = new HeroCard()
            {
                Text = text,
                Images = cardImage,
            };
            plAttachment = plCard.ToAttachment();

            IMessageActivity response = context.MakeMessage();

            response.Attachments = new List<Attachment>();

            response.Attachments.Add(plAttachment);

            return response;
        }

        private List<CardImage> GetCardImage(string url)
        {
            List<CardImage> cI = new List<CardImage>();
            if (!string.IsNullOrWhiteSpace(url))
            {
                CardImage cImage = new CardImage(url, "Couldn't display");

                cI.Add(cImage);
            }
            return cI;
        }

        public async Task StartAsync(IDialogContext context)
        {
            // Root dialog initiates and waits for the next message from the user. 
            // When a message arrives, call MessageReceivedAsync.  
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            DocumentDbHelper dH = new DocumentDbHelper();
            List<GrcAnswers> s = new List<GrcAnswers>();
            s = dH.GetAllSteps(docDbName);
            if (s.Count > 0)
            {
                GrcAnswers gA = new GrcAnswers();
                gA = s.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(gA.Ans))
                {
                    await context.PostAsync(gA.Ans);
                    context.Done("grc detail");
                }
                else
                {
                    IMessageActivity res = null;
                    foreach (Step sT in gA.Steps)
                    {
                        res = GetStepInfoCard(context, sT.stepno, sT.text, sT.image);
                        await context.PostAsync(res);
                    }
                    context.Done("grc detail");
                }
            }
            else
            {
                context.Done("grc detail");
            }
        }

    }
}