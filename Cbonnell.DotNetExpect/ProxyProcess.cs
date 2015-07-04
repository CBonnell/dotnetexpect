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
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;

namespace Cbonnell.DotNetExpect
{
    internal enum ProxyCommand
    {
        StartProcess = 0,
        SetPid,
        KillProcess,
        ClearConsole,
        ReadConsole,
        WriteConsole,
        AttachConsole,
        GetPid,
        GetProcessExitCode,
        GetHasProcessExited,
    }

    internal class ProxyProcess
    {
        private readonly NamedPipeClientStream commandPipe;
        private readonly BinaryReader commandReader;
        private readonly BinaryWriter commandWriter;
        private Process process = null;
        private bool spawnedProcess = false;

        public ProxyProcess(string commandPipeName)
        {
            this.commandPipe = new NamedPipeClientStream(commandPipeName);
            this.commandReader = new BinaryReader(this.commandPipe);
            this.commandWriter = new BinaryWriter(this.commandPipe);
        }

        public void Run()
        {
            this.commandPipe.Connect();

            bool shouldContinue = true;
            try
            {
                while (shouldContinue)
                {
                    int opcode = this.commandReader.ReadByte();
                    CommandResult result = CommandResult.GeneralFailure;
                    bool shouldWriteResult = true; // individual opcodes can set this to false to return specialized data

                    switch ((ProxyCommand)opcode)
                    {
                        case ProxyCommand.StartProcess:
                            if (this.process == null)
                            {
                                this.process = new Process();
                                this.process.StartInfo.FileName = this.commandReader.ReadString();
                                this.process.StartInfo.Arguments = this.commandReader.ReadString();
                                this.process.StartInfo.WorkingDirectory = this.commandReader.ReadString();
                                this.process.StartInfo.UseShellExecute = false;
                                this.process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                try
                                {
                                    this.process.Start();
                                    this.spawnedProcess = true;
                                    result = CommandResult.Success;
                                }
                                catch (Exception)
                                {
                                    result = CommandResult.CouldNotSpawnChild;
                                    this.process.Dispose();
                                    this.process = null;
                                }
                            }
                            else
                            {
                                result = CommandResult.ChildAlreadySpawned;
                            }
                            break;
                        case ProxyCommand.SetPid:
                            if (this.process == null)
                            {
                                try
                                {
                                    int pid = this.commandReader.ReadInt32();
                                    this.process = Process.GetProcessById(pid);
                                    result = CommandResult.Success;
                                }
                                catch (Exception)
                                {
                                    result = CommandResult.ProcessDoesNotExist;
                                }
                            }
                            break;
                        case ProxyCommand.KillProcess:
                            if (this.process != null)
                            {
                                this.process.Kill();
                                this.process.WaitForExit();

                                this.commandWriter.Write((byte)CommandResult.Success);
                                this.commandWriter.Write(this.process.ExitCode);

                                this.process.Dispose();
                                this.process = null;

                                shouldWriteResult = false;
                                shouldContinue = false;
                            }
                            else
                            {
                                result = CommandResult.ChildNotSpawned;
                            }
                            break;
                        case ProxyCommand.AttachConsole:
                            int timeoutMs = this.commandReader.ReadInt32();

                            if (this.process != null)
                            {
                                ConsoleInterface.AttachToConsole(this.process.Id, TimeSpan.FromMilliseconds(timeoutMs));
                                result = CommandResult.Success;
                            }
                            else
                            {
                                result = CommandResult.ChildNotSpawned;
                            }
                            break;
                        case ProxyCommand.ReadConsole:
                            string outputData = ConsoleInterface.ReadConsole();
                            shouldWriteResult = false;
                            this.commandWriter.Write((byte)CommandResult.Success);
                            this.commandWriter.Write(outputData);
                            break;
                        case ProxyCommand.WriteConsole:
                            string inputData = this.commandReader.ReadString();
                            ConsoleInterface.WriteConsole(inputData);
                            result = CommandResult.Success;
                            break;
                        case ProxyCommand.ClearConsole:
                            ConsoleInterface.ClearConsole();
                            result = CommandResult.Success;
                            break;
                        case ProxyCommand.GetHasProcessExited:
                            if (this.process != null)
                            {
                                this.commandWriter.Write((byte)CommandResult.Success);
                                this.commandWriter.Write(this.process.HasExited);
                                shouldWriteResult = false;
                            }
                            else
                            {
                                result = CommandResult.ChildNotSpawned;
                            }
                            break;
                        case ProxyCommand.GetPid:
                            if (this.process != null)
                            {
                                this.commandWriter.Write((byte)CommandResult.Success);
                                this.commandWriter.Write(this.process.Id);
                                shouldWriteResult = false;
                            }
                            else
                            {
                                result = CommandResult.ChildNotSpawned;
                            }
                            break;
                        case ProxyCommand.GetProcessExitCode:
                            if (this.process != null)
                            {
                                if (this.process.HasExited)
                                {
                                    this.commandWriter.Write((byte)CommandResult.Success);
                                    this.commandWriter.Write(this.process.ExitCode);
                                    shouldWriteResult = false;
                                }
                                else
                                {
                                    result = CommandResult.ChildHasNotExited;
                                }
                            }
                            else
                            {
                                result = CommandResult.ChildNotSpawned;
                            }
                            break;
                        default:
                            result = CommandResult.GeneralFailure;
                            break;
                    }

                    if (shouldWriteResult)
                    {
                        this.commandWriter.Write((byte)result);
                    }
                    this.commandWriter.Flush();
                }
            }
            finally // ensure that the child process is killed if we exit, but only if we spawned the process
            {
                if (this.process != null && this.spawnedProcess)
                {
                    this.process.Kill();
                }
            }
        }
    }
}
