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

namespace Cbonnell.DotNetExpect
{
    /// <summary>
    /// Contains the various options for interacting with a <see cref="ChildProcess"/>.
    /// </summary>
    public class ChildProcessOptions
    {
        private const int MSEC_PER_SEC = 1000;

        /// <summary>
        /// Instantiates a new instance of <see cref="ChildProcessOptions"/>.
        /// </summary>
        public ChildProcessOptions()
        {
            this.AttachConsole = true;
            this.NewLine = Environment.NewLine;
            this.AttachConsoleTimeoutMilliseconds = 10 * ChildProcessOptions.MSEC_PER_SEC;
            this.TimeoutMilliseconds = 60 * ChildProcessOptions.MSEC_PER_SEC;
            this.ClearConsole = true;
        }

        /// <summary>
        /// Whether or not to attach to the child process console on each input/output operation. This is sometimes required, as many command line applications create their own console.
        /// </summary>
        /// <remarks>The default value is <b>true</b>.</remarks>
        public bool AttachConsole
        {
            get;
            set;
        }

        private string newLine;
        /// <summary>
        /// The string to append to each specified string when calling <see cref="ChildProcess.WriteLine"/>. This is generally useful so that one does not need to manually add newline/carriage return characters to each string to be written.
        /// </summary>
        /// <remarks>The default value is <b><see cref="Environment.NewLine"/></b>.</remarks>
        public string NewLine
        {
            get
            {
                return this.newLine;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.newLine = value;
            }
        }

        private int attachConsoleTimeoutMilliseconds;
        /// <summary>
        /// The amount of time to attempt to attach to a child process's console. This value may need to be adjusted if the child process takes a considerable amount of time to initialize before allocating its own console.
        /// </summary>
        /// <remarks>The default value is <b>10,000 millseconds (10 seconds)</b>.</remarks>
        public int AttachConsoleTimeoutMilliseconds
        {
            get
            {
                return this.attachConsoleTimeoutMilliseconds;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.attachConsoleTimeoutMilliseconds = value;
            }
        }

        private int timeoutMilliseconds;
        /// <summary>
        /// The amount of time to wait for matching input when calling <see cref="ChildProcess.Read"/> or <see cref="ChildProcess.Match"/> before a <see cref="TimeoutException"/> is thrown.
        /// </summary>
        /// <remarks>The default value is <b>60,000 milliseconds (60 seconds, or 1 minute)</b>. A negative value denotes that there is no timeout (will wait forever for matching input).</remarks>
        public int TimeoutMilliseconds
        {
            get
            {
                return this.timeoutMilliseconds;
            }
            set
            {
                this.timeoutMilliseconds = value;
            }
        }

        /// <summary>
        /// Whether or not to clear the console of all content after successfully reading console content. This is useful so that previously read content is not returned on subsequent Reads.
        /// </summary>
        /// <remarks>The default value is <b>true</b>.</remarks>
        public bool ClearConsole
        {
            get;
            set;
        }
    }
}
