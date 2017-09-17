using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Microsoft.Azure.KeyVault;
using System.Web.Configuration;
using AskSAPBot.Helpers;

namespace AskSAPBOT
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected async void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AskSAPBOT.Models.AuthSettings.Mode = ConfigurationManager.AppSettings["ActiveDirectory.Mode"];
            AskSAPBOT.Models.AuthSettings.EndpointUrl = ConfigurationManager.AppSettings["ActiveDirectory.EndpointUrl"];
            AskSAPBOT.Models.AuthSettings.Tenant = ConfigurationManager.AppSettings["ActiveDirectory.Tenant"];
            AskSAPBOT.Models.AuthSettings.RedirectUrl = ConfigurationManager.AppSettings["ActiveDirectory.RedirectUrl"];
            AskSAPBOT.Models.AuthSettings.ClientId = ConfigurationManager.AppSettings["MicrosoftAppId"];
            AskSAPBOT.Models.AuthSettings.ClientSecret = ConfigurationManager.AppSettings["MicrosoftAppPassword"];
            AskSAPBOT.Models.AuthSettings.Scopes = ConfigurationManager.AppSettings["ActiveDirectory.Scopes"].Split(',');

            var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(KeyVaultHelper.GetToken));
            var sec =  kv.GetSecretAsync(ConfigurationManager.AppSettings["SecretUri"],"PrimaryKey").GetAwaiter().GetResult();

             KeyVaultHelper.PrimaryKey = sec.Value;
            var sec1 = kv.GetSecretAsync(ConfigurationManager.AppSettings["SecretUri"], "LuisSecretKey").GetAwaiter().GetResult();
            KeyVaultHelper.LuisSecretKey = sec1.Value;
        }
    }
}
