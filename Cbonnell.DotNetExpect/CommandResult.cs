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


namespace Cbonnell.DotNetExpect
{
    /// <summary>
    /// Represents the possible results that can be returned from a command invoked on a child process.
    /// </summary>
    public enum CommandResult
    {
        /// <summary>
        /// The command succeeded.
        /// </summary>
        Success = 0,
        /// <summary>
        /// A non-specific error occurred.
        /// </summary>
        GeneralFailure,
        /// <summary>
        /// An error occurred when attempting to spawn the child process.
        /// </summary>
        /// <remarks>Generally this error is returned when the specified application does not exist.</remarks>
        CouldNotSpawnChild,
        /// <summary>
        /// The the child process was already spawned for the current instance of <see cref="ChildProcess"/>.
        /// </summary>
        ChildAlreadySpawned,
        /// <summary>
        /// A call to retrieve the child process's exit code was attempted while the child process was still executing.
        /// </summary>
        ChildHasNotExited,
        /// <summary>
        /// The child process has not yet been spawned for the current instance <see cref="ChildProcess"/>.
        /// </summary>
        ChildNotSpawned,
        /// <summary>
        /// A process with the specified PID does not exist.
        /// </summary>
        ProcessDoesNotExist,
    }
}
