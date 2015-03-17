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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace Cbonnell.DotNetExpect.Test
{
    [TestFixture]
    public class ChildProcessTest
    {
        [SetUp]
        public void SetUp()
        {
            TestUtilities.EnsureProxyExit();
        }

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
            new ChildProcess(TestEnvironment.DUMMY_EXE_NAME, null, new ChildProcessOptions());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullWorkingDirectory()
        {
            new ChildProcess(TestEnvironment.DUMMY_EXE_NAME, TestEnvironment.DUMMY_ARGUMENTS, null, new ChildProcessOptions());
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
            using (ChildProcess childProc = new ChildProcess(Guid.NewGuid().ToString() + ".exe")) { }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReadNullRegex()
        {
            using (ChildProcess childProc = new ChildProcess(TestEnvironment.CMD_EXE_NAME))
            {
                childProc.Match(null);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WriteNullString()
        {
            using (ChildProcess childProc = new ChildProcess(TestEnvironment.CMD_EXE_NAME))
            {
                childProc.WriteLine(null);
            }
        }

        [Test]
        public void SimpleReadUntilPrompt()
        {
            using (ChildProcess childProc = new ChildProcess(TestEnvironment.CMD_EXE_NAME))
            {
                string content = childProc.Read(TestEnvironment.PROMPT_CHAR.ToString());
                Console.WriteLine(content);
                Assert.IsTrue(content.EndsWith(TestEnvironment.PROMPT_CHAR.ToString()));
            }
        }

        [Test]
        public void SimpleMatch()
        {
            using (ChildProcess childProc = new ChildProcess(TestEnvironment.CMD_EXE_NAME))
            {
                childProc.WriteLine("dir");
                Match m = childProc.Match(new Regex(@"Volume Serial Number is (?<VolumeSerial>[0-9A-F]{4}-[0-9A-F]{4})"));
                Console.WriteLine("Primary volume serial number: {0}", m.Groups["VolumeSerial"].Value);
                Assert.IsTrue(m.Success);
            }
        }

        [Test]
        public void SimpleRead()
        {
            using (ChildProcess childProc = new ChildProcess(TestEnvironment.CMD_EXE_NAME))
            {
                string content = childProc.Read(Environment.CurrentDirectory + ">");
                Assert.IsTrue(content.Contains(Environment.CurrentDirectory + ">"));
            }
        }

        [Test]
        [ExpectedException(typeof(TimeoutException))]
        public void ReadTimeout()
        {
            using (ChildProcess childProc = new ChildProcess(TestEnvironment.CMD_EXE_NAME, new ChildProcessOptions() { TimeoutMilliseconds = 0 }))
            {
                childProc.Read(Guid.NewGuid().ToString());
            }
        }

        [Test]
        [ExpectedException(typeof(TimeoutException))]
        public void MatchTimeout()
        {
            using (ChildProcess childProc = new ChildProcess(TestEnvironment.CMD_EXE_NAME, new ChildProcessOptions() { TimeoutMilliseconds = 0 }))
            {
                childProc.Match(new Regex(Guid.NewGuid().ToString()));
            }
        }

        [Test]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void ObjectDisposed()
        {
            ChildProcess childProc;
            using (childProc = new ChildProcess(TestEnvironment.CMD_EXE_NAME)) { }
            childProc.ClearConsole();
        }

        [Test]
        public void VerifyChildDeadOnDispose()
        {
            Process p = null;
            try
            {
                using (ChildProcess childProc = new ChildProcess(TestEnvironment.CMD_EXE_NAME))
                {
                    p = Process.GetProcessById(childProc.ChildProcessId);
                }
                TestUtilities.WaitForProcessExitAndThrow(p);
            }
            finally
            {
                if (p != null)
                {
                    p.Dispose();
                }
            }
        }

        [Test]
        public void VerifyProxyDeadOnDispose()
        {
            Process[] powerShells = null;
            Process proxyProcess = null;

            try
            {
                using (ChildProcess childProc = new ChildProcess(TestEnvironment.CMD_EXE_NAME))
                {
                    powerShells = Process.GetProcessesByName(TestEnvironment.PROXY_PROCESS_NAME);
                    proxyProcess = Array.Find(powerShells, (p) => TestUtilities.IsProxyProcess(p));
                    Assert.AreEqual(1, Array.FindAll(powerShells, (p) => TestUtilities.IsProxyProcess(p)).Length);                    
                }
                TestUtilities.WaitForProcessExitAndThrow(proxyProcess);
            }
            finally
            {
                if(powerShells != null)
                {
                    Array.ForEach(powerShells, (p) => p.Dispose());
                }
            }
        }
    }
}
