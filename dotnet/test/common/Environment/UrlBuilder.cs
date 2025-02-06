// <copyright file="UrlBuilder.cs" company="Selenium Committers">
// Licensed to the Software Freedom Conservancy (SFC) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The SFC licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
// </copyright>

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;

namespace OpenQA.Selenium.Environment
{
    public class UrlBuilder
    {
        private readonly string protocol;
        private readonly string port;
        private readonly string securePort;

        public string AlternateHostName { get; }

        public string HostName { get; }

        public string Path { get; }

        public UrlBuilder(WebsiteConfig config)
        {
            protocol = config.Protocol;
            HostName = config.HostName;
            port = config.Port;
            securePort = config.SecurePort;
            Path = config.Folder;
            //Use the first IPv4 address that we find
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            foreach (IPAddress ip in Dns.GetHostEntry(HostName).AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ip;
                    break;
                }
            }
            AlternateHostName = ipAddress.ToString();
        }

        public string LocalWhereIs(string page)
        {
            return $"http://localhost:{port}/{Path}/{page}";
        }

        public string WhereIs(string page)
        {
            return $"http://{HostName}:{port}/{Path}/{page}";
        }

        public string WhereElseIs(string page)
        {
            return $"http://{AlternateHostName}:{port}/{Path}/{page}";
        }

        public string WhereIsViaNonLoopbackAddress(string page)
        {
            string hostNameAsIPAddress = "127.0.0.1";
            IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress address in addresses)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address))
                {
                    hostNameAsIPAddress = address.ToString();
                    break;
                }
            }

            return $"http://{hostNameAsIPAddress}:{port}/{Path}/{page}";
        }

        public string WhereIsSecure(string page)
        {
            return $"https://{HostName}:{securePort}/{Path}/{page}";
        }

        public string CreateInlinePage(InlinePage page)
        {
            Uri createPageUri = new Uri(new Uri(WhereIs(string.Empty)), "createPage");

            Dictionary<string, object> payloadDictionary = new Dictionary<string, object>
            {
                ["content"] = page.ToString()
            };

            string commandPayload = JsonConvert.SerializeObject(payloadDictionary);

            using var httpClient = new HttpClient();

            var postHttpContent = new StringContent(commandPayload, Encoding.UTF8, "application/json");

            using var response = httpClient.PostAsync(createPageUri, postHttpContent).GetAwaiter().GetResult();

            var responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            // The response string from the Java remote server has trailing null
            // characters. This is due to the fix for issue 288.
            if (responseString.Contains('\0'))
            {
                responseString = responseString[..responseString.IndexOf('\0')];
            }

            if (responseString.Contains("localhost"))
            {
                responseString = responseString.Replace("localhost", this.HostName);
            }

            return responseString;
        }
    }
}
