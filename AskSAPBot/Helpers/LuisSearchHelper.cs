using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace AskSAPBot.Helpers
{
    public class TopScoringIntent
    {
        public string intent { get; set; }
        public double score { get; set; }
    }

    public class Intent
    {
        public string intent { get; set; }
        public double score { get; set; }
    }

    public class Value
    {
        public string timex { get; set; }
        public string type { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string value { get; set; }
    }

    public class Resolution
    {
        public string value { get; set; }
        public IList<Value> values { get; set; }
    }

    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public Resolution resolution { get; set; }
    }

    public class LUIS
    {
        public string query { get; set; }
        public TopScoringIntent topScoringIntent { get; set; }
        public IList<Intent> intents { get; set; }
        public IList<Entity> entities { get; set; }
        
    }

    public class LuisSearchHelper
    {
        public async System.Threading.Tasks.Task<List<String>> GetIntentEntities(String query, string type)
        {
            LUIS LUISResult = new LUIS();
            List<String> Entities = new List<String>();
            try
            {
                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                {
                    // Get key values from the web.config
                    string LUIS_Url = ConfigurationManager.AppSettings["LuisURL"];
                    string LuisAppId = ConfigurationManager.AppSettings["LuisAppId"];
                    string LUIS_Subscription_Key = KeyVaultHelper.LuisSecretKey;
                    string RequestURI = String.Format("{0}{1}?subscription-key={2}&q={3}",
                        LUIS_Url,LuisAppId, LUIS_Subscription_Key, query);
                    System.Net.Http.HttpResponseMessage msg = await client.GetAsync(RequestURI);
                    if (msg.IsSuccessStatusCode)
                    {
                        var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                        LUISResult = JsonConvert.DeserializeObject<LUIS>(JsonDataResponse);
                    }
                    if (LUISResult.entities != null)
                    {
                       var ents = from ent in LUISResult.entities
                        where ent.type == type
                        select ent;
                        foreach(Entity e in ents)
                        {                         
                            if (e.resolution.value != null)
                            {
                                Entities.Add(e.resolution.value);
                            }
                            if (e.resolution.values!=null)
                            {
                                foreach(Value v in e.resolution.values)
                                {
                                    if (v.value != null)
                                        Entities.Add(v.value);
                                    else
                                    {
                                        Entities.Add(v.start);
                                        Entities.Add(v.end);
                                    }
                                
                                }

                            }
                        }
                    }
                }


            }
            catch (Exception e)
            {

            }
            return Entities;
        }
    }
}