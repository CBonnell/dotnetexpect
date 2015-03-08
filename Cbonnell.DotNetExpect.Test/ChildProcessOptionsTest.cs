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
using System.Linq;

namespace Cbonnell.DotNetExpect.Test
{
    [TestFixture]
    public class ChildProcessOptionsTest
    {
        [Test]
        public void DefaultConsoleClearsAfterMatch()
        {
            using (ChildProcess childProc = new ChildProcess(TestEnvironment.CMD_EXE_NAME))
            {
                childProc.Spawn();
                string content = childProc.Read(TestEnvironment.CMD_PROMPT_REGEX);
                childProc.Write("echo \"hello world\"");
                content = childProc.Read(TestEnvironment.CMD_PROMPT_REGEX);
                Assert.AreEqual(1, content.Count((c) => c.Equals(TestEnvironment.PROMPT_CHAR)));
            }
        }
    }
}
