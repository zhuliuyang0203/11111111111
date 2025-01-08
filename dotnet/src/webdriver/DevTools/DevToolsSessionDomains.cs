// <copyright file="DevToolsSessionDomains.cs" company="Selenium Committers">
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

using System;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

namespace OpenQA.Selenium.DevTools
{
    /// <summary>
    /// Provides an abstract base class for version-specific domain implementations.
    /// </summary>
    public abstract class DevToolsSessionDomains
    {
        private CommandResponseTypeMap responseTypeMap = new CommandResponseTypeMap();

        [EditorBrowsable(EditorBrowsableState.Never)] // Generated code use only
        internal static JsonSerializerOptions DevToolsSerializerOptions { get; } = new JsonSerializerOptions()
        {
            Converters =
            {
                new InvalidUtf16Converter(),
            }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="DevToolsSessionDomains"/> class.
        /// </summary>
        protected DevToolsSessionDomains()
        {
            this.PopulateCommandResponseTypeMap();
        }

        /// <summary>
        /// Gets the <see cref="CommandResponseTypeMap"/> containing information about the types returned by DevTools Protocol commands.,
        /// </summary>
        internal CommandResponseTypeMap ResponseTypeMap => this.responseTypeMap;

        /// <summary>
        /// Populates the command response type map.
        /// </summary>
        protected abstract void PopulateCommandResponseTypeMap();

        private sealed class InvalidUtf16Converter : JsonConverter<string>
        {
            public override bool HandleNull => true;

            public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                try
                {
                    return reader.GetString();
                }
                catch (InvalidOperationException)
                {
                    var bytes = reader.ValueSpan;
                    var sb = new StringBuilder(bytes.Length);
                    foreach (byte b in bytes)
                        sb.Append(Convert.ToChar(b));
                    return sb.ToString();
                }
            }

            public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
                writer.WriteStringValue(value);
        }
    }
}
