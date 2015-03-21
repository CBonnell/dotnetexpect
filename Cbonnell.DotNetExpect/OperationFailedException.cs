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
    /// Represents an error that occurred when accessing a child process's console input or output.
    /// </summary>
    public class OperationFailedException : Exception
    {
        private const string MESSAGE_FMT = "An operation failed with the following error: {0}";

        internal OperationFailedException(CommandResult reason)
            : base(String.Format(OperationFailedException.MESSAGE_FMT, reason))
        {
            this.Reason = reason;
        }

        /// <summary>
        /// Retrieves the reason for the error.
        /// </summary>
        public CommandResult Reason
        {
            get;
            private set;
        }
    }
}
