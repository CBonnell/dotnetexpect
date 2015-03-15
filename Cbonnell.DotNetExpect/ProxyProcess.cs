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
        KillProcess,
        ClearConsole,
        ReadConsole,
        WriteConsole,
        AttachConsole,
        GetChildPid,
        GetChildExitCode,
        GetHasChildExited,
    }

    internal class ProxyProcess
    {
        private readonly NamedPipeClientStream commandPipe;
        private readonly BinaryReader commandReader;
        private readonly BinaryWriter commandWriter;
        private Process childProcess = null;

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
                            if (this.childProcess == null)
                            {
                                this.childProcess = new Process();
                                this.childProcess.StartInfo.FileName = this.commandReader.ReadString();
                                this.childProcess.StartInfo.Arguments = this.commandReader.ReadString();
                                this.childProcess.StartInfo.WorkingDirectory = this.commandReader.ReadString();
                                this.childProcess.StartInfo.UseShellExecute = false;
                                this.childProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                try
                                {
                                    this.childProcess.Start();
                                    result = CommandResult.Success;
                                }
                                catch(Exception)
                                {
                                    result = CommandResult.CouldNotSpawnChild;
                                    this.childProcess.Dispose();
                                    this.childProcess = null;
                                }
                            }
                            else
                            {
                                result = CommandResult.ChildAlreadySpawned;
                            }
                            break;
                        case ProxyCommand.KillProcess:
                            if (this.childProcess != null)
                            {
                                this.childProcess.Kill();
                                this.childProcess.WaitForExit();

                                this.commandWriter.Write((byte)CommandResult.Success);
                                this.commandWriter.Write(this.childProcess.ExitCode);

                                this.childProcess.Dispose();
                                this.childProcess = null;

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

                            if (this.childProcess != null)
                            {
                                ConsoleInterface.AttachToConsole(this.childProcess.Id, TimeSpan.FromMilliseconds(timeoutMs));
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
                        case ProxyCommand.GetHasChildExited:
                            if(this.childProcess != null)
                            {
                                this.commandWriter.Write((byte)CommandResult.Success);
                                this.commandWriter.Write(this.childProcess.HasExited);
                                shouldWriteResult = false;
                            }
                            else
                            {
                                result = CommandResult.ChildNotSpawned;
                            }
                            break;
                        case ProxyCommand.GetChildPid:
                            if(this.childProcess != null)
                            {
                                this.commandWriter.Write((byte)CommandResult.Success);
                                this.commandWriter.Write(this.childProcess.Id);
                                shouldWriteResult = false;
                            }
                            else
                            {
                                result = CommandResult.ChildNotSpawned;
                            }
                            break;
                        case ProxyCommand.GetChildExitCode:
                            if(this.childProcess != null)
                            {
                                if(this.childProcess.HasExited)
                                {
                                    this.commandWriter.Write((byte)CommandResult.Success);
                                    this.commandWriter.Write(this.childProcess.ExitCode);
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
            finally // ensure that the child process is killed if we exit
            {
                if(this.childProcess != null)
                {
                    this.childProcess.Kill();
                }
            }
        }
    }
}
