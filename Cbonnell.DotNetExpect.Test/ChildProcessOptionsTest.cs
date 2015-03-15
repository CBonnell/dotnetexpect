/*
DotNetExpect
Copyright (c) Corey Bonnell, All rights reserved.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 3.0 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library. */

using NUnit.Framework;
using System;
using System.Linq;

namespace Cbonnell.DotNetExpect.Test
{
    [TestFixture]
    public class ChildProcessOptionsTest
    {
        [SetUp]
        public void SetUp()
        {
            TestUtilities.EnsureProxyExit();
        }

        [Test]
        public void DefaultValuesExpected()
        {
            ChildProcessOptions options = new ChildProcessOptions();
            Assert.AreEqual(Environment.NewLine, options.WriteAppendString);
            Assert.IsTrue(options.AttachConsole);
            Assert.AreEqual(10 * 1000, options.AttachConsoleTimeoutMilliseconds);
            Assert.AreEqual(60 * 1000, options.TimeoutMilliseconds);
            Assert.IsTrue(options.ClearConsole);
        }

        [Test]
        public void DefaultConsoleClearsAfterMatch()
        {
            using (ChildProcess childProc = new ChildProcess(TestEnvironment.CMD_EXE_NAME))
            {
                string content = childProc.Read(TestEnvironment.PROMPT_CHAR.ToString());
                childProc.Write("echo \"hello world\"");
                content = childProc.Read(TestEnvironment.PROMPT_CHAR.ToString());
                Assert.AreEqual(1, content.Count((c) => c.Equals(TestEnvironment.PROMPT_CHAR)));
            }
        }
    }
}
