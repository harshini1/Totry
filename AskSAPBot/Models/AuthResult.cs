// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See full license at the bottom of this file.
namespace AskSAPBOT.Models
{
    using System;
    using System.Linq;

    [Serializable]
    public class AuthResult
    {
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
        public string UserName { get; set; }
        public string UserUniqueId { get; set; }
        public long ExpiresOnUtcTicks { get; set; }
        public byte[] TokenCache { get; set; }
        public string Alias { get; set; }
        public static AuthResult FromADALAuthenticationResult(Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult authResult, Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache tokenCache)
        {
            var TokenClaim = new System.IdentityModel.Tokens.JwtSecurityToken(authResult.IdToken);
            var alias = TokenClaim.Claims.FirstOrDefault(m => m.Type == "upn").Value;
            var result = new AuthResult
            {
                AccessToken = authResult.AccessToken,
                IdToken = authResult.IdToken,
                UserName = $"{authResult.UserInfo.GivenName} {authResult.UserInfo.FamilyName}",
                UserUniqueId = authResult.UserInfo.UniqueId,
                ExpiresOnUtcTicks = authResult.ExpiresOn.UtcTicks,
                TokenCache = tokenCache.Serialize(),
                Alias = alias
            };

            return result;
        }

        public static AuthResult FromMSALAuthenticationResult(Microsoft.Identity.Client.AuthenticationResult authResult, Microsoft.Identity.Client.TokenCache tokenCache)
        {
            var TokenClaim = new System.IdentityModel.Tokens.JwtSecurityToken(authResult.IdToken);
            var alias = TokenClaim.Claims.FirstOrDefault(m => m.Type == "preferred_username").Value;

            var result = new AuthResult
            {
                AccessToken = authResult.Token,
                IdToken = authResult.IdToken,
                UserName = $"{authResult.User.Name}",
                UserUniqueId = authResult.User.UniqueId,
                ExpiresOnUtcTicks = authResult.ExpiresOn.UtcTicks,
                TokenCache = tokenCache.Serialize(),
                Alias = alias
            };

            return result;
        }
    }
}
//*********************************************************
//
//AuthBot, https://github.com/microsoftdx/AuthBot
//
//Copyright (c) Microsoft Corporation
//All rights reserved.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// ""Software""), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:




// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.




// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//*********************************************************
