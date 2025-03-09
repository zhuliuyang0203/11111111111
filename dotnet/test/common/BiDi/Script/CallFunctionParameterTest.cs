// <copyright file="CallFunctionParameterTest.cs" company="Selenium Committers">
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
using OpenQA.Selenium.BiDi.Modules.Script;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace OpenQA.Selenium.BiDi.Script;

class CallFunctionParameterTest : BiDiTestFixture
{
    [Test]
    public async Task CanCallFunctionWithDeclaration()
    {
        var res = await context.Script.CallFunctionAsync("() => { return 1 + 2; }", false);

        Assert.That(res, Is.Not.Null);
        Assert.That(res.Realm, Is.Not.Null);
        Assert.That((res.Result as RemoteValue.Number).Value, Is.EqualTo(3));
    }

    [Test]
    public async Task CanCallFunctionWithDeclarationImplicitCast()
    {
        var res = await context.Script.CallFunctionAsync<int>("() => { return 1 + 2; }", false);

        Assert.That(res, Is.EqualTo(3));
    }

    [Test]
    public async Task CanEvaluateScriptWithUserActivationTrue()
    {
        await context.Script.EvaluateAsync("window.open();", true);

        var res = await context.Script.CallFunctionAsync<bool>("""
            () => navigator.userActivation.isActive && navigator.userActivation.hasBeenActive
            """, true, new() { UserActivation = true });

        Assert.That(res, Is.True);
    }

    [Test]
    public async Task CanEvaluateScriptWithUserActivationFalse()
    {
        await context.Script.EvaluateAsync("window.open();", true);

        var res = await context.Script.CallFunctionAsync<bool>("""
            () => navigator.userActivation.isActive && navigator.userActivation.hasBeenActive
            """, true);

        Assert.That(res, Is.False);
    }

    [Test]
    public async Task CanCallFunctionWithArguments()
    {
        var res = await context.Script.CallFunctionAsync("(...args)=>{return args}", false, new()
        {
            Arguments = ["abc", 42]
        });

        Assert.That(res.Result, Is.AssignableFrom<RemoteValue.Array>());
        Assert.That((string)(res.Result as RemoteValue.Array).Value[0], Is.EqualTo("abc"));
        Assert.That((int)(res.Result as RemoteValue.Array).Value[1], Is.EqualTo(42));
    }

    [Test]
    public async Task CanCallFunctionToGetIFrameBrowsingContext()
    {
        driver.Url = UrlBuilder.WhereIs("click_too_big_in_frame.html");

        var res = await context.Script.CallFunctionAsync("""
            () => document.querySelector('iframe[id="iframe1"]').contentWindow
            """, false);

        Assert.That(res, Is.Not.Null);
        Assert.That(res.Result, Is.AssignableFrom<RemoteValue.WindowProxy>());
        Assert.That((res.Result as RemoteValue.WindowProxy).Value, Is.Not.Null);
    }

    [Test]
    public async Task CanCallFunctionToGetElement()
    {
        driver.Url = UrlBuilder.WhereIs("bidi/logEntryAdded.html");

        var res = await context.Script.CallFunctionAsync("""
            () => document.getElementById("consoleLog")
            """, false);

        Assert.That(res, Is.Not.Null);
        Assert.That(res.Result, Is.AssignableFrom<RemoteValue.Node>());
        Assert.That((res.Result as RemoteValue.Node).Value, Is.Not.Null);
    }

    [Test]
    public async Task CanCallFunctionWithAwaitPromise()
    {
        var res = await context.Script.CallFunctionAsync<string>("""
            async function() {
                await new Promise(r => setTimeout(() => r(), 0));
                return "SOME_DELAYED_RESULT";
            }
            """, awaitPromise: true);

        Assert.That(res, Is.EqualTo("SOME_DELAYED_RESULT"));
    }

    [Test]
    public async Task CanCallFunctionWithAwaitPromiseFalse()
    {
        var res = await context.Script.CallFunctionAsync("""
            async function() {
                await new Promise(r => setTimeout(() => r(), 0));
                return "SOME_DELAYED_RESULT";
            }
            """, awaitPromise: false);

        Assert.That(res, Is.Not.Null);
        Assert.That(res.Result, Is.AssignableFrom<RemoteValue.Promise>());
    }

    [Test]
    public async Task CanCallFunctionWithThisParameter()
    {
        var thisParameter = new LocalValue.Object([["some_property", 42]]);

        var res = await context.Script.CallFunctionAsync<int>("""
            function(){return this.some_property}
            """, false, new() { This = thisParameter });

        Assert.That(res, Is.EqualTo(42));
    }

    [Test]
    public async Task CanCallFunctionWithOwnershipRoot()
    {
        var res = await context.Script.CallFunctionAsync("async function(){return {a:1}}", true, new()
        {
            ResultOwnership = ResultOwnership.Root
        });

        Assert.That(res, Is.Not.Null);
        Assert.That((res.Result as RemoteValue.Object).Handle, Is.Not.Null);
        Assert.That((string)(res.Result as RemoteValue.Object).Value[0][0], Is.EqualTo("a"));
        Assert.That((int)(res.Result as RemoteValue.Object).Value[0][1], Is.EqualTo(1));
    }

    [Test]
    public async Task CanCallFunctionWithOwnershipNone()
    {
        var res = await context.Script.CallFunctionAsync("async function(){return {a:1}}", true, new()
        {
            ResultOwnership = ResultOwnership.None
        });

        Assert.That(res, Is.Not.Null);
        Assert.That((res.Result as RemoteValue.Object).Handle, Is.Null);
        Assert.That((string)(res.Result as RemoteValue.Object).Value[0][0], Is.EqualTo("a"));
        Assert.That((int)(res.Result as RemoteValue.Object).Value[0][1], Is.EqualTo(1));
    }

    [Test]
    public void CanCallFunctionThatThrowsException()
    {
        var action = () => context.Script.CallFunctionAsync("))) !!@@## some invalid JS script (((", false);

        Assert.That(action, Throws.InstanceOf<ScriptEvaluateException>().And.Message.Contain("SyntaxError:"));
    }

    [Test]
    public async Task CanCallFunctionInASandBox()
    {
        // Make changes without sandbox
        await context.Script.CallFunctionAsync("() => { window.foo = 1; }", true);

        var res = await context.Script.CallFunctionAsync("() => window.foo", true, targetOptions: new() { Sandbox = "sandbox" });

        Assert.That(res.Result, Is.AssignableFrom<RemoteValue.Undefined>());

        // Make changes in the sandbox
        await context.Script.CallFunctionAsync("() => { window.foo = 2; }", true, targetOptions: new() { Sandbox = "sandbox" });

        // Check if the changes are present in the sandbox
        res = await context.Script.CallFunctionAsync("() => window.foo", true, targetOptions: new() { Sandbox = "sandbox" });

        Assert.That(res.Result, Is.AssignableFrom<RemoteValue.Number>());
        Assert.That((res.Result as RemoteValue.Number).Value, Is.EqualTo(2));
    }

    [Test]
    public async Task CanCallFunctionInARealm()
    {
        await bidi.BrowsingContext.CreateAsync(Modules.BrowsingContext.ContextType.Tab);

        var realms = await bidi.Script.GetRealmsAsync();

        await bidi.Script.CallFunctionAsync("() => { window.foo = 3; }", true, new Target.Realm(realms[0].Realm));
        await bidi.Script.CallFunctionAsync("() => { window.foo = 5; }", true, new Target.Realm(realms[1].Realm));

        var res1 = await bidi.Script.CallFunctionAsync<int>("() => window.foo", true, new Target.Realm(realms[0].Realm));
        var res2 = await bidi.Script.CallFunctionAsync<int>("() => window.foo", true, new Target.Realm(realms[1].Realm));

        Assert.That(res1, Is.EqualTo(3));
        Assert.That(res2, Is.EqualTo(5));
    }

    [Test]
    [TestCaseSource(nameof(RoundtripOptions))]
    public async Task CanCallFunctionAndRoundtrip_Five(LocalValue local, RemoteValue expected, string javaScriptAssert)
    {
        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if ({{javaScriptAssert}}) {
                return arg;
              }

              throw new Error("Assert failed: " + arg);
            }
            """, false, new() { Arguments = [local] });

        if (response.Result is RemoteValue.Array actualArray && expected is RemoteValue.Array expectedArray)
        {
            Assert.That(actualArray.Value, Is.EqualTo(expectedArray.Value));
        }
        else if (response.Result is RemoteValue.Object actualObject && expected is RemoteValue.Object expectedObject)
        {
            Assert.That(actualObject.Value, Is.EqualTo(expectedObject.Value));
        }
        else if (response.Result is RemoteValue.Map actualMap && expected is RemoteValue.Map expectedMap)
        {
            Assert.That(actualMap.Value, Is.EqualTo(expectedMap.Value));
        }
        else if (response.Result is RemoteValue.Set actualSet && expected is RemoteValue.Set expectedSet)
        {
            Assert.That(actualSet.Value, Is.EqualTo(expectedSet.Value));
        }
        else if (response.Result is RemoteValue.Date actualDate && expected is RemoteValue.Date expectedDate)
        {
            var actualDateParsed = DateTime.SpecifyKind(DateTime.Parse(actualDate.Value, CultureInfo.InvariantCulture), DateTimeKind.Utc);
            Assert.That(actualDateParsed.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(actualDateParsed, Is.EqualTo(DateTime.Parse(expectedDate.Value)).Within(TimeSpan.FromMilliseconds(1)));
        }
        else
        {
            Assert.That(response.Result, Is.EqualTo(expected));
        }
    }
    private const string PinnedDateTimeString = "2025-03-09T00:30:33.083Z";
    private static IEnumerable<TestCaseData> RoundtripOptions()
    {

        yield return new TestCaseData(new LocalValue.Null(), new RemoteValue.Null(), "arg === null")
        {
            TestName = nameof(CanCallFunctionAndRoundtrip_Five) + "(Null)"
        };
        yield return new TestCaseData(new LocalValue.Undefined(), new RemoteValue.Undefined(), "typeof arg === 'undefined'")
        {
            TestName = nameof(CanCallFunctionAndRoundtrip_Five) + "(Undefined)",
        };
        //yield return new TestCaseData(new LocalValue.Boolean(true), new RemoteValue.Boolean(true), "typeof arg === true")
        //{
        //    TestName = nameof(CanCallFunctionAndRoundtrip_Five) + "(true)",
        //};
        //yield return new TestCaseData(new LocalValue.Boolean(false), new RemoteValue.Boolean(false), "typeof arg === false")
        //{
        //    TestName = nameof(CanCallFunctionAndRoundtrip_Five) + "(false)",
        //};
        yield return new TestCaseData(new LocalValue.String("whoa"), new RemoteValue.String("whoa"), "arg === 'whoa'")
        {
            TestName = nameof(CanCallFunctionAndRoundtrip_Five) + "(String('whoa'))",
        };
        yield return new TestCaseData(new LocalValue.String(string.Empty), new RemoteValue.String(string.Empty), "arg === ''")
        {
            TestName = nameof(CanCallFunctionAndRoundtrip_Five) + "(String(''))",
        };
        yield return new TestCaseData(new LocalValue.Date(PinnedDateTimeString), new RemoteValue.Date(PinnedDateTimeString), $"arg.toISOString() === '{PinnedDateTimeString}'")
        {
            TestName = nameof(CanCallFunctionAndRoundtrip_Five) + "(Date)",
        };
        yield return new TestCaseData(new LocalValue.Number(5), new RemoteValue.Number(5), "arg === 5")
        {
            TestName = nameof(CanCallFunctionAndRoundtrip_Five) + "(Number(5))",
        };
        yield return new TestCaseData(new LocalValue.Number(0), new RemoteValue.Number(0), "arg === 0")
        {
            TestName = nameof(CanCallFunctionAndRoundtrip_Five) + "(Number(0))",
        };
        yield return new TestCaseData(new LocalValue.Number(-5), new RemoteValue.Number(-5), "arg === -5")
        {
            TestName = nameof(CanCallFunctionAndRoundtrip_Five) + "(Number(-5))",
        };
        yield return new TestCaseData(new LocalValue.Number(double.PositiveInfinity), new RemoteValue.Number(double.PositiveInfinity), "arg === Number.POSITIVE_INFINITY")
        {
            TestName = nameof(CanCallFunctionAndRoundtrip_Five) + "(Number(Infinity))",
        };
        yield return new TestCaseData(new LocalValue.Number(double.NegativeInfinity), new RemoteValue.Number(double.NegativeInfinity), "arg === Number.NEGATIVE_INFINITY")
        {
            TestName = nameof(CanCallFunctionAndRoundtrip_Five) + "(Number(-Infinity))",
        };
        yield return new TestCaseData(new LocalValue.Number(double.NegativeZero), new RemoteValue.Number(double.NegativeZero), "arg === -0")
        {
            TestName = nameof(CanCallFunctionAndRoundtrip_Five) + "(Number(-0))",
        };
        yield return new TestCaseData(
            new LocalValue.RegExp(new LocalValue.RegExp.RegExpValue("foo*") { Flags = "g" }),
            new RemoteValue.RegExp(new RemoteValue.RegExp.RegExpValue("foo*") { Flags = "g" }),
            "arg.test('foo') && arg.source === 'foo*' && arg.global"
        )
        {
            TestName = nameof(CanCallFunctionAndRoundtrip_Five) + "(RegExp(/foo/g))",
        };
        yield return new TestCaseData(

                new LocalValue.Array([new LocalValue.String("hi")]),
                new RemoteValue.Array { Value = [new RemoteValue.String("hi")] },
                "arg.length === 1 && arg[0] === 'hi'"
            )
        {
            TestName = nameof(CanCallFunctionAndRoundtrip_Five) + "(Array(['hi']))",
        };
        yield return new TestCaseData
        (
            new LocalValue.Object([[new LocalValue.String("key"), new LocalValue.String("value")]]),
            new RemoteValue.Object
            {
                Value = [[new RemoteValue.String("key"), new RemoteValue.String("value")]]
            },
            "arg.key === 'value' && Object.keys(arg).length === 1"
        )
        {
            TestName = nameof(CanCallFunctionAndRoundtrip_Five) + "(Object({key: 'value'}))",
        };
        yield return new TestCaseData
        (
            new LocalValue.Map([[new LocalValue.String("key"), new LocalValue.String("value")]]),
            new RemoteValue.Map
            {
                Value = [[new RemoteValue.String("key"), new RemoteValue.String("value")]]
            },
            "arg.get('key') === 'value' && arg.size === 1"
        )
        {
            TestName = nameof(CanCallFunctionAndRoundtrip_Five) + "(Map({'key': 'value'}))",
        };

        yield return new TestCaseData
        (
            new LocalValue.Set([new LocalValue.String("key")]),
            new RemoteValue.Set { Value = [new RemoteValue.String("key")] },
            "arg.has('key') && arg.size === 1"
        )
        {
            TestName = nameof(CanCallFunctionAndRoundtrip_Five) + "(Set({'key'}))",

        };
    }
}
