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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Cbonnell.DotNetExpect
{
    internal class ProxyProcessManager : IDisposable
    {
        private const string PIPE_NAME_FMT = "Cbonnell.DotNetExpect.{0}";
        private const string POWERSHELL_COMMAND_LINE = "powershell.exe -Command \"$asm = [System.Reflection.Assembly]::LoadFile('{0}'); $proxyType = $asm.GetType('Cbonnell.DotNetExpect.ProxyProcess'); $proxy = [System.Activator]::CreateInstance($proxyType, @('{1}')); $proxy.Run() | Out-Null;\"";

        private readonly NamedPipeServerStream commandPipe;
        private readonly BinaryReader commandPipeReader;
        private readonly BinaryWriter commandPipeWriter;
        private readonly string commandPipeName;

        private Process proxyProcess = null;

        public ProxyProcessManager()
        {
            this.commandPipeName = String.Format(ProxyProcessManager.PIPE_NAME_FMT, Guid.NewGuid());
            this.commandPipe = new NamedPipeServerStream(this.commandPipeName);
            this.commandPipeReader = new BinaryReader(this.commandPipe);
            this.commandPipeWriter = new BinaryWriter(this.commandPipe);
        }

        public BinaryReader CommandPipeReader
        {
            get
            {
                return this.commandPipeReader;
            }
        }

        public BinaryWriter CommandPipeWriter
        {
            get
            {
                return this.commandPipeWriter;
            }
        }

        public void Start()
        {
            NativeMethods.STARTUPINFO startInfo = default(NativeMethods.STARTUPINFO);
            startInfo.cb = Marshal.SizeOf(startInfo);
            startInfo.dwFlags = NativeMethods.STARTF_USE_SHOWWINDOW;
            startInfo.wShowWindow = NativeMethods.SW_HIDE;

            string commandLine = String.Format(ProxyProcessManager.POWERSHELL_COMMAND_LINE, Assembly.GetExecutingAssembly().Location, this.commandPipeName);

            NativeMethods.PROCESS_INFORMATION procInfo;
            if (!NativeMethods.CreateProcess(null, commandLine, IntPtr.Zero, IntPtr.Zero, false, NativeMethods.CREATE_NEW_CONSOLE, IntPtr.Zero, null, ref startInfo, out procInfo))
            {
                throw new Win32Exception();
            }

            NativeMethods.CloseHandle(procInfo.hProcess);
            NativeMethods.CloseHandle(procInfo.hThread);
            this.proxyProcess = Process.GetProcessById(procInfo.dwProcessId);
            this.commandPipe.WaitForConnection();
        }

        public void Dispose()
        {
            this.commandPipe.Dispose();
            if (this.proxyProcess != null)
            {
                this.proxyProcess.Dispose();
            }
        }

        private static class NativeMethods
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_INFORMATION
            {
                public IntPtr hProcess;
                public IntPtr hThread;
                public int dwProcessId;
                public int dwThreadId;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct STARTUPINFO
            {
                public int cb;
                public string lpReserved;
                public string lpDesktop;
                public string lpTitle;
                public int dwX;
                public int dwY;
                public int dwXSize;
                public int dwYSize;
                public int dwXCountChars;
                public int dwYCountChars;
                public int dwFillAttribute;
                public int dwFlags;
                public short wShowWindow;
                public short cbReserved2;
                public IntPtr lpReserved2;
                public IntPtr hStdInput;
                public IntPtr hStdOutput;
                public IntPtr hStdError;
            }

            [DllImport("kernel32", SetLastError = true)]
            public static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

            [DllImport("kernel32", SetLastError = true)]
            public static extern bool CloseHandle(IntPtr hObject);

            public const int STARTF_USE_SHOWWINDOW = 0x1;
            public const int SW_HIDE = 0;
            public const int CREATE_NEW_CONSOLE = 0x10;
        }
    }
}
