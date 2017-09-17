// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See full license at the bottom of this file.
namespace AskSAPBOT
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Models;
    using System.Configuration;
    public static class ContextExtensions
    {
        public static async Task<string> GetAccessToken(this IBotContext context, string resourceId)
        {
            AuthResult authResult;
            if (context.UserData.TryGetValue(ContextConstants.AuthResultKey, out authResult))
            {
                try
                {
                    InMemoryTokenCacheADAL tokenCache = new InMemoryTokenCacheADAL(authResult.TokenCache);
                    var result = await AzureActiveDirectoryHelper.GetToken(authResult.UserUniqueId, tokenCache, resourceId);
                    authResult.AccessToken = result.AccessToken;
                    authResult.ExpiresOnUtcTicks = result.ExpiresOnUtcTicks;
                    authResult.TokenCache = tokenCache.Serialize();
                    context.StoreAuthResult(authResult);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Failed to renew token: " + ex.Message);
                    await context.PostAsync("Your credentials expired and could not be renewed automatically!");
                    await context.Logout();
                    return null;
                }
                return authResult.AccessToken;
            }
            return null;
        }
        public static async Task<string> GetAccessToken(this IBotContext context, string[] scopes)
        {
            AuthResult authResult;
            string validated = null;
            if (context.UserData.TryGetValue(ContextConstants.AuthResultKey, out authResult) &&
                context.UserData.TryGetValue(ContextConstants.MagicNumberValidated, out validated) &&
                validated == "true")
            {

                try
                {
                    if (string.Equals(AuthSettings.Mode, "v2", StringComparison.OrdinalIgnoreCase))
                    {
                        InMemoryTokenCacheMSAL tokenCache = new InMemoryTokenCacheMSAL(authResult.TokenCache);
                        var result = await AzureActiveDirectoryHelper.GetToken(authResult.UserUniqueId, tokenCache, scopes);
                        authResult.AccessToken = result.AccessToken;
                        authResult.ExpiresOnUtcTicks = result.ExpiresOnUtcTicks;
                        authResult.TokenCache = tokenCache.Serialize();
                        context.StoreAuthResult(authResult);
                    }
                    else if (string.Equals(AuthSettings.Mode, "b2c", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new NotImplementedException();
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Failed to renew token: " + ex.Message);
                    await context.PostAsync("Your credentials expired and could not be renewed automatically!");
                    await context.Logout();
                    return null;
                }
                return authResult.AccessToken;
            }

            return null;
        }
        public static async Task<string> GetAlias(this IBotContext context)
        {
            AuthResult authResult;
            string validated = null;
            if (context.UserData.TryGetValue(ContextConstants.AuthResultKey, out authResult) &&
                context.UserData.TryGetValue(ContextConstants.MagicNumberValidated, out validated) &&
                validated == "true")
            {

                try
                {
                    if (string.Equals(AuthSettings.Mode, "v2", StringComparison.OrdinalIgnoreCase))
                    {
                        InMemoryTokenCacheMSAL tokenCache = new InMemoryTokenCacheMSAL(authResult.TokenCache);
                        var result = await AzureActiveDirectoryHelper.GetToken(authResult.UserUniqueId, tokenCache, AuthSettings.Scopes);
                        authResult.AccessToken = result.AccessToken;
                        authResult.ExpiresOnUtcTicks = result.ExpiresOnUtcTicks;
                        authResult.TokenCache = tokenCache.Serialize();
                        authResult.Alias = result.Alias;
                        context.StoreAuthResult(authResult);
                    }
                    else if (string.Equals(AuthSettings.Mode, "b2c", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new NotImplementedException();
                    }
                    else if (string.Equals(AuthSettings.Mode, "v1", StringComparison.OrdinalIgnoreCase))
                    {
                        InMemoryTokenCacheADAL tokenCache = new InMemoryTokenCacheADAL(authResult.TokenCache);
                        var result = await AzureActiveDirectoryHelper.GetToken(authResult.UserUniqueId, tokenCache, ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"]);
                        authResult.AccessToken = result.AccessToken;
                        authResult.ExpiresOnUtcTicks = result.ExpiresOnUtcTicks;
                        authResult.TokenCache = tokenCache.Serialize();
                        authResult.Alias = result.Alias;
                        context.StoreAuthResult(authResult);

                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Failed to renew token: " + ex.Message);
                    await context.PostAsync("Your credentials expired and could not be renewed automatically!");
                    await context.Logout();
                    return null;
                }
                return authResult.Alias.Split('@')[0];
            }

            return null;
        }

        public static void StoreAuthResult(this IBotContext context, AuthResult authResult)
        {
            context.UserData.SetValue(ContextConstants.AuthResultKey, authResult);
        }

        public static async Task Logout(this IBotContext context)
        {
            context.UserData.RemoveValue(ContextConstants.AuthResultKey);
            context.UserData.RemoveValue(ContextConstants.MagicNumberKey);
            context.UserData.RemoveValue(ContextConstants.MagicNumberValidated);
            string signoutURl = "https://login.microsoftonline.com/common/oauth2/logout?post_logout_redirect_uri=" + System.Net.WebUtility.UrlEncode(AuthSettings.RedirectUrl);
            await context.PostAsync($"In order to finish the sign out, please click at this [link]({signoutURl}).");
        }

    }
}
