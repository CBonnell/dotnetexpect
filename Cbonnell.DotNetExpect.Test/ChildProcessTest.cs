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

namespace Cbonnell.DotNetExpect.Test
{
    [TestFixture]
    public class ChildProcessTest
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullFilePath()
        {
            new ChildProcess(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullArguments()
        {
            new ChildProcess(TestEnvironment.DUMMY_EXE_NAME, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullWorkingDirectory()
        {
            new ChildProcess(TestEnvironment.DUMMY_EXE_NAME, TestEnvironment.DUMMY_ARGUMENTS, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullOptions()
        {
            new ChildProcess(TestEnvironment.DUMMY_EXE_NAME, TestEnvironment.DUMMY_ARGUMENTS, Environment.CurrentDirectory, null);
        }

        [Test]
        [ExpectedException(typeof(OperationFailedException))]
        public void ProcessNoExist()
        {
            using(ChildProcess childProc = new ChildProcess(Guid.NewGuid().ToString() + ".exe"))
            {
                childProc.Spawn();
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReadNullRegex()
        {
            using (ChildProcess childProc = new ChildProcess(TestEnvironment.CMD_EXE_NAME))
            {
                childProc.Spawn();
                childProc.Read(null);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WriteNullString()
        {
            using (ChildProcess childProc = new ChildProcess(TestEnvironment.CMD_EXE_NAME))
            {
                childProc.Spawn();
                childProc.Write(null);
            }
        }

        [Test]
        public void SimpleReadUntilPrompt()
        {
            using (ChildProcess childProc = new ChildProcess(TestEnvironment.CMD_EXE_NAME))
            {
                childProc.Spawn();
                string content = childProc.Read(TestEnvironment.CMD_PROMPT_REGEX);
                Console.WriteLine(content);
                Assert.IsTrue(content.EndsWith(TestEnvironment.PROMPT_CHAR.ToString()));
            }
        }
    }
}
