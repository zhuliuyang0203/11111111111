// <copyright file="IgnorePlatformAttribute.cs" company="Selenium Committers">
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

using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using OpenQA.Selenium.Environment;
using System;
using System.Runtime.InteropServices;

namespace OpenQA.Selenium
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class IgnorePlatformAttribute : NUnitAttribute, IApplyToTest
    {
        public const string Windows = nameof(Windows);
        public const string Linux = nameof(Linux);
        public const string Mac = nameof(Mac);

        public IgnorePlatformAttribute(string platform)
        {
            this.Value = platform.ToLowerInvariant();
        }

        public IgnorePlatformAttribute(string platform, string reason)
            : this(platform)
        {
            this.Reason = reason;
        }

        public string Value { get; }

        public string Reason { get; } = string.Empty;

        public void ApplyToTest(Test test)
        {
            if (test.RunState != RunState.NotRunnable)
            {
                Attribute[] ignoreAttributes;
                if (test.IsSuite)
                {
                    ignoreAttributes = test.TypeInfo.GetCustomAttributes<IgnorePlatformAttribute>(true);
                }
                else
                {
                    ignoreAttributes = test.Method.GetCustomAttributes<IgnorePlatformAttribute>(true);
                }

                foreach (Attribute attr in ignoreAttributes)
                {
                    if (attr is IgnorePlatformAttribute platformToIgnoreAttr
                        && IgnoreTestForPlatform(platformToIgnoreAttr.Value))
                    {
                        string ignoreReason = $"Ignoring platform {EnvironmentManager.Instance.Browser}.";
                        if (!string.IsNullOrEmpty(platformToIgnoreAttr.Reason))
                        {
                            ignoreReason = ignoreReason + " " + platformToIgnoreAttr.Reason;
                        }

                        test.RunState = RunState.Ignored;
                        test.Properties.Set(PropertyNames.SkipReason, ignoreReason);
                    }
                }
            }
        }

        private static bool IgnoreTestForPlatform(string platformToIgnore)
        {
            return platformToIgnore.Equals(CurrentPlatform(), StringComparison.OrdinalIgnoreCase);
        }

        private static string CurrentPlatform()
        {
            if (OperatingSystem.IsWindows())
            {
                return Windows;
            }
            else if (OperatingSystem.IsLinux())
            {
                return Linux;
            }
            else if (OperatingSystem.IsMacOS())
            {
                return Mac;
            }
            else
            {
                throw new PlatformNotSupportedException($"Selenium Manager did not find supported operating system: {RuntimeInformation.OSDescription}");
            }
        }
    }
}
