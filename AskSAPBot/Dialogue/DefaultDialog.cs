using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace SAPBot.Dialogue

{
    [LuisModel("{42803763-e6b8-4117-b328-4b839d857549}", "{8419891b-9304-4f9f-90ff-ef1a5dc4d00f}")] 
    [Serializable]
    public class DefaultDialog : LuisDialog<object>
    {
        [LuisIntent("")]
        public async Task None(IDialogContext context,LuisResult res)
        {
            await context.PostAsync("I don't know what you want!");
            context.Wait(MessageReceived);
        }
        [LuisIntent("know tcode")]
        public async Task KnowTcode(IDialogContext context, LuisResult res)
        {
            await context.PostAsync("You wanna know about tcode!");
            context.Wait(MessageReceived);
        }
    }
}