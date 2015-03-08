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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Cbonnell.DotNetExpect
{
    /// <summary>
    /// Represents a child process whose console input and output can be accessed.
    /// </summary>
    public class ChildProcess : IDisposable
    {
        private readonly string filePath;
        private readonly string arguments;
        private readonly string workingDirectory;
        private readonly ChildProcessOptions options;
        private ProxyProcessManager proxy = null;

        /// <summary>
        /// Creates a new instance of <see cref="ChildProcess"/>.
        /// </summary>
        /// <param name="filePath">The path to the child process. The semantics in terms of how relative paths are handled are the same as <see cref="System.Diagnostics.ProcessStartInfo.FileName"/> with <see cref="System.Diagnostics.ProcessStartInfo.UseShellExecute"/> set to <b>false</b>.</param>
        public ChildProcess(string filePath) : this(filePath, String.Empty) { }

        /// <summary>
        /// Creates a new instance of <see cref="ChildProcess"/>.
        /// </summary>
        /// <param name="filePath">The path to the child process. The semantics in terms of how relative paths are handled are the same as <see cref="System.Diagnostics.ProcessStartInfo.FileName"/> with <see cref="System.Diagnostics.ProcessStartInfo.UseShellExecute"/> set to <b>false</b>.</param>
        /// <param name="arguments">The command line arguments for the child process.</param>
        public ChildProcess(string filePath, string arguments) : this(filePath, arguments, Environment.CurrentDirectory) { }

        /// <summary>
        /// Creates a new instance of <see cref="ChildProcess"/>.
        /// </summary>
        /// <param name="filePath">The path to the child process. The semantics in terms of how relative paths are handled are the same as <see cref="System.Diagnostics.ProcessStartInfo.FileName"/> with <see cref="System.Diagnostics.ProcessStartInfo.UseShellExecute"/> set to <b>false</b>.</param>
        /// <param name="arguments">The command line arguments for the child process.</param>
        /// <param name="workingDirectory">The working directory for the child process.</param>
        public ChildProcess(string filePath, string arguments, string workingDirectory) : this(filePath, arguments, workingDirectory, ChildProcessOptions.Default) { }

        /// <summary>
        /// Creates a new instance of <see cref="ChildProcess"/>.
        /// </summary>
        /// <param name="filePath">The path to the child process. The semantics in terms of how relative paths are handled are the same as <see cref="System.Diagnostics.ProcessStartInfo.FileName"/> with <see cref="System.Diagnostics.ProcessStartInfo.UseShellExecute"/> set to <b>false</b>.</param>
        /// <param name="arguments">The command line arguments for the child process.</param>
        /// <param name="workingDirectory">The working directory for the child process.</param>
        /// <param name="options">The <see cref="ChildProcessOptions"/> to use when accessing the console input and output of the child process.</param>
        public ChildProcess(string filePath, string arguments, string workingDirectory, ChildProcessOptions options)
        {
            if(filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }
            if(arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            if(workingDirectory == null)
            {
                throw new ArgumentNullException("workingDirectory");
            }
            if(options == null)
            {
                throw new ArgumentNullException("options");
            }

            this.filePath = filePath;
            this.arguments = arguments;
            this.workingDirectory = workingDirectory;
            this.options = options;
        }

        /// <summary>
        /// Spawns the child process.
        /// </summary>
        /// <remarks>This method must be called before calling other methods/properties on <see cref="ChildProcess"/>.</remarks>
        public void Spawn()
        {
            if(this.proxy != null)
            {
                throw new InvalidOperationException();
            }

            this.proxy = new ProxyProcessManager();
            this.proxy.Start();

            this.proxy.CommandPipeWriter.Write((byte)ProxyCommand.StartProcess);
            this.proxy.CommandPipeWriter.Write(this.filePath);
            this.proxy.CommandPipeWriter.Write(this.arguments);
            this.proxy.CommandPipeWriter.Write(this.workingDirectory);
            this.proxy.CommandPipeWriter.Flush();

            this.readResponseAndThrow();
        }

        /// <summary>
        /// Reads the child process's console output (screen buffer) until the content of the screen output matches the specified <see cref="Regex"/>.
        /// </summary>
        /// <param name="regex">The regular expression to match the console screen output against.</param>
        /// <returns>The content of the child process's console output.</returns>
        public string Read(Regex regex)
        {
            if(regex == null)
            {
                throw new ArgumentNullException("regex");
            }

            if(this.proxy == null)
            {
                throw new InvalidOperationException();
            }

            Match m = null;
            string output = null;
            do
            {
                if (this.options.AttachConsoleOnReadOrWrite)
                {
                    this.attachConsole();
                }

                this.proxy.CommandPipeWriter.Write((byte)ProxyCommand.ReadConsole);
                this.proxy.CommandPipeWriter.Flush();
                this.readResponseAndThrow();

                output = this.proxy.CommandPipeReader.ReadString();
                m = regex.Match(output);
            } while (!m.Success);

            if(this.options.ClearConsoleOnReadMatch)
            {
                this.ClearConsole();
            }

            return output;
        }

        /// <summary>
        /// Writes the specified string to the child process's console.
        /// </summary>
        /// <param name="data">The string data to write to the child process's console.</param>
        public void Write(string data)
        {
            if(data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (this.proxy == null)
            {
                throw new InvalidOperationException();
            }

            if(this.options.AppendNewLineOnWrite)
            {
                data = data + Environment.NewLine;
            }

            if (this.options.AttachConsoleOnReadOrWrite)
            {
                this.attachConsole();
            }

            this.proxy.CommandPipeWriter.Write((byte)ProxyCommand.WriteConsole);
            this.proxy.CommandPipeWriter.Write(data);
            this.proxy.CommandPipeWriter.Flush();
            this.readResponseAndThrow();
        }

        /// <summary>
        /// Kills the child process.
        /// </summary>
        /// <returns>The exit code of the killed child process.</returns>
        public int Kill()
        {
            if(this.proxy == null)
            {
                throw new InvalidOperationException();
            }

            this.proxy.CommandPipeWriter.Write((byte)ProxyCommand.KillProcess);
            this.proxy.CommandPipeWriter.Flush();
            this.readResponseAndThrow();
            int exitCode = this.proxy.CommandPipeReader.ReadInt32(); // read the exit code
            this.proxy.Dispose();
            this.proxy = null;
            return exitCode;
        }

        /// <summary>
        /// Clears the child process's console output.
        /// </summary>
        public void ClearConsole()
        {
            if (this.proxy == null)
            {
                throw new InvalidOperationException();
            }

            this.proxy.CommandPipeWriter.Write((byte)ProxyCommand.ClearConsole);
            this.proxy.CommandPipeWriter.Flush();
            this.readResponseAndThrow();
        }

        /// <summary>
        /// Retrieves the process ID of the spawned child process.
        /// </summary>
        public int ChildProcessId
        {
            get
            {
                if (this.proxy == null)
                {
                    throw new InvalidOperationException();
                }

                this.proxy.CommandPipeWriter.Write((byte)ProxyCommand.GetChildPid);
                this.proxy.CommandPipeWriter.Flush();
                this.readResponseAndThrow();

                return this.proxy.CommandPipeReader.ReadInt32();
            }
        }

        /// <summary>
        /// Retrieves the exit code of a child process that has exited (stopped executing).
        /// </summary>
        public int ChildExitCode
        {
            get
            {
                if (this.proxy == null)
                {
                    throw new InvalidOperationException();
                }

                this.proxy.CommandPipeWriter.Write((byte)ProxyCommand.GetChildExitCode);
                this.proxy.CommandPipeWriter.Flush();
                this.readResponseAndThrow();

                return this.proxy.CommandPipeReader.ReadInt32();
            }
        }

        /// <summary>
        /// Retrieves whether or not a child process has exited (stopped executing).
        /// </summary>
        public bool HasChildExited
        {
            get
            {
                if (this.proxy == null)
                {
                    throw new InvalidOperationException();
                }

                this.proxy.CommandPipeWriter.Write((byte)ProxyCommand.GetHasChildExited);
                this.proxy.CommandPipeWriter.Flush();
                this.readResponseAndThrow();

                return this.proxy.CommandPipeReader.ReadBoolean();
            }
        }

        /// <summary>
        /// Disposes the current instance of <see cref="ChildProcess"/>.
        /// </summary>
        public void Dispose()
        {
            if(this.proxy != null)
            {
                this.proxy.Dispose();
                this.proxy = null;
            }
        }

        private void attachConsole()
        {
            this.proxy.CommandPipeWriter.Write((byte)ProxyCommand.AttachConsole);
            this.proxy.CommandPipeWriter.Write(this.options.AttachConsoleTimeoutMilliseconds);
            this.proxy.CommandPipeWriter.Flush();
            this.readResponseAndThrow();
        }

        private void readResponseAndThrow()
        {
            CommandResult result = (CommandResult)this.proxy.CommandPipeReader.ReadByte();
            if (result != CommandResult.Success)
            {
                throw new OperationFailedException(result);
            }
        }
    }
}
