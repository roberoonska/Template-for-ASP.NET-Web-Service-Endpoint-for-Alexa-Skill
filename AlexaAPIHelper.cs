using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Data;
using Newtonsoft.Json;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;

namespace YOURNAMESPACE
{
    public class AlexaAPIHelper
    {
        private string Host { get; set; }
        private string AccessToken { get; set; }
        private string DeviceID { get; set; }
        public enum RequestType {timeZone, distanceUnit, temperatureUnit, address, fullName, givenName, emailAddress, phoneNumber}

        /// <summary>
        /// Creates an AlexaAPIHelper.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="accessToken"></param>
        /// <param name="deviceID"></param>
        public AlexaAPIHelper(string host, string accessToken, string deviceID)
        {
            this.Host = host;
            this.AccessToken = accessToken;
            this.DeviceID = deviceID;
        }
        

        /// <summary>
        /// Returns a request for a timezone, distance unit or address for the alexa settings api given a 
        /// request type.
        /// </summary>
        /// <param name="requestType"></param>
        public HttpWebRequest AlexaAPIRequest(RequestType requestType)
        {
            string uriStr = string.Empty;
            switch (requestType)
            {
                case RequestType.timeZone:
                    uriStr = string.Format("{0}/v2/devices/{1}/settings/System.timeZone", Host, DeviceID);
                    break;
                case RequestType.distanceUnit:
                    uriStr = string.Format("{0}/v2/devices/{1}/settings/System.distanceUnits", Host, DeviceID);
                    break;
                case RequestType.temperatureUnit:
                    uriStr = string.Format("{0}/v2/devices/{1}/settings/System.temperatureUnit", Host, DeviceID);
                    break;
                case RequestType.address:
                    uriStr = string.Format("{0}/v1/devices/{1}/settings/address",Host, DeviceID);
                    break;
                case RequestType.fullName:
                    uriStr = string.Format("{0}/v2/accounts/~current/settings/Profile.name",Host);
                    break;
                case RequestType.givenName:
                    uriStr = string.Format("{0}/v2/accounts/~current/settings/Profile.givenName",Host);
                    break;
                case RequestType.emailAddress:
                    uriStr = string.Format("{0}/v2/accounts/~current/settings/Profile.email",Host);
                    break;
                case RequestType.phoneNumber:
                    uriStr = string.Format("{0}/v2/accounts/~current/settings/Profile.mobileNumber",Host);
                    break;
            }
            
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uriStr);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Headers["Authorization"] = string.Format("Bearer {0}", AccessToken);
            httpWebRequest.Method = "GET";
            httpWebRequest.Timeout = 60000;
            return httpWebRequest;
        }

        
        /// <summary>
        /// Returns the response from a web request as a json string.
        /// If the request fails, returns an empty string.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="mess"></param>
        public string GetResponse(HttpWebRequest request)
        {
            string jsonStr = string.Empty;

            try
            {
                using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream stream = resp.GetResponseStream())
                    {
                        jsonStr = new StreamReader(stream).ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return jsonStr;
        }
     
        
        /// <summary>
        /// Returns a skill response that tells the use to grant permissions, and sends an AskForPermissionConsentCard.
        /// </summary>
        /// <param name="requestType"></param>
        public SkillResponse AskForPermission(RequestType requestType)
        {
            SkillResponse response = new SkillResponse();
            switch (requestType)
            {
                case RequestType.address:
                    response = ResponseBuilder.TellWithAskForPermissionConsentCard("YOUR PERMISSION REQUEST TEXT HERE" ,
                        new List<string>() { "read::alexa:device:all:address" });
                    break;
                case RequestType.fullName:
                    response = ResponseBuilder.TellWithAskForPermissionConsentCard("YOUR PERMISSION REQUEST TEXT HERE" ,
                        new List<string>() { "alexa::profile:name:read" });
                    break;
                case RequestType.givenName:
                    response = ResponseBuilder.TellWithAskForPermissionConsentCard("YOUR PERMISSION REQUEST TEXT HERE" ,
                        new List<string>() { "alexa::profile:given_name:read" });
                    break;
                case RequestType.emailAddress:
                    response = ResponseBuilder.TellWithAskForPermissionConsentCard("YOUR PERMISSION REQUEST TEXT HERE" ,
                        new List<string>() { "alexa::profile:email:read" });
                    break;
                case RequestType.phoneNumber:
                    response = ResponseBuilder.TellWithAskForPermissionConsentCard("YOUR PERMISSION REQUEST TEXT HERE" ,
                        new List<string>() { "alexa::profile:mobile_number:read" });
                    break;
            }
            return response;
        }
    }
}
        
