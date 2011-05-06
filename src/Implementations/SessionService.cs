﻿using System.Collections.Generic;
using System.Xml.XPath;
using DotNetOpenAuth.Messaging;
using System.Web;

namespace Visual
{
    public class SessionService : ISessionService
    {
        private IApiProvider _provider;

        public SessionService(IApiProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Returns a Session object containing a signed token and a URL for redeeming with the default relative path (/)
        /// </summary>
        /// <returns></returns>
        public Domain.Session GetToken()
        {
            return GetToken("/");
        }

        /// <summary>
        /// Returns a Session object containing a signed token and a URL for redeeming with a specific relative or absulte path
        /// </summary>
        /// <param name="returnUrl">Specific (http://example.23video.com/...) or relative path (/...) on site</param>
        /// <returns></returns>
        public Domain.Session GetToken(string returnUrl)
        {
            // Build request URL
            List<string> requestUrlParameters = new List<string>();

            requestUrlParameters.Add("return_url=" + HttpUtility.UrlEncode(returnUrl));

            // Do the request
            MessageReceivingEndpoint requestMessage = new MessageReceivingEndpoint(_provider.GetRequestUrl("/api/session/get-token", requestUrlParameters), HttpDeliveryMethods.GetRequest);

            XPathNavigator responseMessage = _provider.DoRequest(requestMessage);
            if (responseMessage == null) return null;

            // Create result
            Domain.Session result = new Domain.Session();

            // Get the access token
            XPathNodeIterator accessTokenNode = responseMessage.Select("/response/access_token");
            if ((!accessTokenNode.MoveNext()) || (accessTokenNode.Current == null)) return null;

            result.AccessToken = accessTokenNode.Current.Value;

            // Get the local return URL
            XPathNodeIterator returnUrlNode = responseMessage.Select("/response/return_url");
            if ((!returnUrlNode.MoveNext()) || (returnUrlNode.Current == null)) return null;

            string localReturnUrl = returnUrlNode.Current.Value;

            // Build the return URL
            List<string> returnUrlParameters = new List<string>();

            returnUrlParameters.Add("session_token=" + result.AccessToken);
            returnUrlParameters.Add("return_url=" + localReturnUrl);

            result.ReturnURL = _provider.GetRequestUrl("/api/session/redeem-token", returnUrlParameters);

            // Return the object
            return result;
        }
    }
}