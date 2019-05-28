using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using NodaTime;
using Newtonsoft.Json;
using DMTServices;
using TideAPI.Controllers;
using Alexa.NET;
using Alexa.NET.Response;

namespace YOURNAMESPACE
{
    public class AlexaAPIHelper
    {
        private string Host { get; set; }
        private string AccessToken { get; set; }
        private string DeviceID { get; set; }
        public enum RequestType {timeZone, distanceUnit, address}

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
                case RequestType.address:
                    uriStr = string.Format("{0}/v1/devices/{1}/settings/address",Host, DeviceID);
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
        public string GetResponse(HttpWebRequest request, MessLog mess)
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
