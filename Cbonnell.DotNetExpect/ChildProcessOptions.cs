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
        /// The default options. See the &quot;remarks&quot; for each option property for the default value of each property.
        /// </summary>
        public static ChildProcessOptions Default
        {
            get
            {
                return new ChildProcessOptions()
                {
                    AttachConsoleOnReadOrWrite = true,
                    AppendNewLineOnWrite = true,
                    AttachConsoleTimeoutMilliseconds = 10 * ChildProcessOptions.MSEC_PER_SEC,
                    ClearConsoleOnReadMatch = true,
                };
            }
        }

        /// <summary>
        /// Instantiates a new instance of <see cref="ChildProcessOptions"/>.
        /// </summary>
        public ChildProcessOptions() { }

        /// <summary>
        /// Whether or not to attach to the child process console on each Read or Write operation. This is sometimes required, as many command line applications create their own console.
        /// </summary>
        /// <remarks>The default value is <b>true</b>.</remarks>
        public bool AttachConsoleOnReadOrWrite
        {
            get;
            set;
        }

        /// <summary>
        /// Whether or not to append <see cref="Environment.NewLine"/> to each specified string on Write.
        /// </summary>
        /// <remarks>The default value is <b>true</b>.</remarks>
        public bool AppendNewLineOnWrite
        {
            get;
            set;
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
                if(value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.attachConsoleTimeoutMilliseconds = value;
            }
        }
        
        /// <summary>
        /// Whether or not to clear the console of all content after a successful Read match. This is useful so that previously read content is not returned on subsequent Reads.
        /// </summary>
        /// <remarks>The default value is <b>true</b>.</remarks>
        public bool ClearConsoleOnReadMatch
        {
            get;
            set;
        }
    }
}
