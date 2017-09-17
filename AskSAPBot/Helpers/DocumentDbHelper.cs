using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace AskSAPBot.Helpers
{
    public class Step
    {
        public string stepno { get; set; }
        public string text { get; set; }
        public string image { get; set; }
    }

    public class GrcAnswers
    {
        public string id { get; set; }
        public string intent { get; set; }
        public string Ans { get; set; }
        public IList<Step> Steps { get; set; }
    }
    public class DocumentDbHelper
    {
        public List<GrcAnswers> GetAllSteps(string intent)
        {
            string endpointurl = ConfigurationManager.AppSettings["DocDbEndpointurl"];
            string primaryKey = ConfigurationManager.AppSettings["PrimaryKey"]; ;
            string dbName = ConfigurationManager.AppSettings["DocDbName"];
            string dbCollection = ConfigurationManager.AppSettings["DocDbCollection"];

            DocumentClient dc = null;
            List<GrcAnswers> AllDocs = new List<GrcAnswers>();
            try
            {

                dc = new DocumentClient(new Uri(endpointurl), primaryKey);
                IQueryable<GrcAnswers> crossPartitionQuery = dc.CreateDocumentQuery<GrcAnswers>(UriFactory.CreateDocumentCollectionUri(dbName, dbCollection),
                                                                                new FeedOptions { EnableCrossPartitionQuery = true, PartitionKey = new Microsoft.Azure.Documents.PartitionKey(intent) });

                foreach (GrcAnswers dl in crossPartitionQuery)
                {
                    AllDocs.Add(dl);
                }
            
            }
            catch (Exception e)
            {
            }
            return AllDocs;
        }
    }

}
