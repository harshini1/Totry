using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading.Tasks;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace AskSAPBot.Helpers
{
    public class KeyVaultHelper
    {
        public static string PrimaryKey { get; set; }
        public static string LuisSecretKey { get; set; }

        //the method that will be provided to the KeyVaultClient
        public static async Task<string> GetToken(string authority, string resource, string scope)
        { //to check local repo
            string toGit = "check";
            var authenticationContext = new AuthenticationContext(authority, null);
            var authContext = new AuthenticationContext(authority);
            X509Certificate2 certificate;
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certificateCollection = store.Certificates.Find(X509FindType.FindByThumbprint, "C559955E3A3F59A6DB5F258D15F07EE98D207E79", false);
                if (certificateCollection == null || certificateCollection.Count == 0)
                {
                    throw new Exception("Certificate not installed in the store");
                }

                certificate = certificateCollection[0];
            }
            finally
            {
                store.Close();
            }
            var clientAssertionCertificate = new ClientAssertionCertificate(ConfigurationManager.AppSettings["MicrosoftAppId"], certificate);
            var result = await authenticationContext.AcquireTokenAsync(resource, clientAssertionCertificate);
            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
        }
    }
}