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

using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Cbonnell.DotNetExpect
{
    internal static class ConsoleInterface
    {
        private const char SPACE_CHAR = ' ';

        #region Public Interface

        public static string ReadConsole()
        {
            char[,] output = ConsoleInterface.ReadConsoleRaw();
            StringBuilder buffer = new StringBuilder();

            for (int y = 0; y < output.GetLength(1); y++)
            {
                char[] lineChars = new char[output.GetLength(0)];
                for (int i = 0; i < lineChars.Length; i++)
                {
                    lineChars[i] = output[i, y];
                }
                buffer.AppendLine(new String(lineChars).TrimEnd());
            }

            return buffer.ToString().TrimEnd();
        }

        public static char[,] ReadConsoleRaw()
        {
            NativeInterface.CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo = ConsoleInterface.getScreenBufferInfo();

            NativeInterface.SMALL_RECT readRegion = default(NativeInterface.SMALL_RECT);
            readRegion.Right = screenBufferInfo.dwSize.X;
            readRegion.Bottom = (short)(screenBufferInfo.dwCursorPosition.Y + 1); // read all lines from the top of the screen buffer to the current cursor position (inclusive)

            NativeInterface.CHAR_INFO[] charInfos = new NativeInterface.CHAR_INFO[screenBufferInfo.dwSize.X * screenBufferInfo.dwSize.Y];
            using (SafeHandle hConsoleOutput = ConsoleInterface.getConsoleOutputHandle())
            {
                ConsoleInterface.callWin32Func(() => NativeInterface.ReadConsoleOutput(hConsoleOutput, charInfos, screenBufferInfo.dwSize, default(NativeInterface.COORD), ref readRegion));
            }

            char[,] output = new char[screenBufferInfo.dwSize.X, readRegion.Bottom];
            for (int y = 0; y < readRegion.Bottom; y++)
            {
                for (int x = 0; x < screenBufferInfo.dwSize.X; x++)
                {
                    output[x, y] = charInfos[y * screenBufferInfo.dwSize.X + x].UnicodeChar;
                }
            }

            return output;
        }

        public static void WriteConsole(string data)
        {
            using (SafeHandle hConsoleInput = ConsoleInterface.getConsoleInputHandle())
            {
                // we need 2x the number of characters because we need to simulate key press and key release events for each character
                NativeInterface.INPUT_RECORD[] inputRecords = new NativeInterface.INPUT_RECORD[data.Length * 2];
                for (int i = 0; i < data.Length; i++)
                {
                    NativeInterface.KEY_EVENT_RECORD keyPressEvent = default(NativeInterface.KEY_EVENT_RECORD);
                    keyPressEvent.bKeyDown = true;
                    keyPressEvent.wRepeatCount = 1;
                    keyPressEvent.UnicodeChar = data[i];

                    NativeInterface.KEY_EVENT_RECORD keyReleaseEvent = keyPressEvent; // same values for all fields as the key press event, but with the bKeyDown field as "false"
                    keyReleaseEvent.bKeyDown = false;

                    inputRecords[i * 2].EventType = NativeInterface.KEY_EVENT;
                    inputRecords[i * 2].Event = keyPressEvent;
                    inputRecords[i * 2 + 1].EventType = NativeInterface.KEY_EVENT;
                    inputRecords[i * 2 + 1].Event = keyReleaseEvent;
                }

                int recordsWritten;
                ConsoleInterface.callWin32Func(() => NativeInterface.WriteConsoleInput(hConsoleInput, inputRecords, inputRecords.Length, out recordsWritten));
            }
        }

        public static void AttachToConsole(int processId, TimeSpan timeout)
        {
            ConsoleInterface.callWin32Func(() => NativeInterface.FreeConsole());
            DateTime startTime = DateTime.Now;
            do
            {
                if (NativeInterface.AttachConsole(processId))
                {
                    return;
                }
            } while (startTime + timeout > DateTime.Now);
            throw new TimeoutException();
        }

        public static void ClearConsole()
        {
            using (SafeHandle hConsoleOutput = ConsoleInterface.getConsoleOutputHandle())
            {
                // get dimensions of the screen buffer
                NativeInterface.CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo = ConsoleInterface.getScreenBufferInfo();

                int charsToWrite = screenBufferInfo.dwSize.X * screenBufferInfo.dwSize.Y;
                int charsWritten;
                ConsoleInterface.callWin32Func(() => NativeInterface.FillConsoleOutputCharacter(hConsoleOutput, ConsoleInterface.SPACE_CHAR, charsToWrite, default(NativeInterface.COORD), out charsWritten));
                ConsoleInterface.callWin32Func(() => NativeInterface.SetConsoleCursorPosition(hConsoleOutput, default(NativeInterface.COORD)));
            }
        }

        #endregion Public Interface

        #region Helper Methods

        private static void callWin32Func(Func<bool> func)
        {
            if (!func.Invoke())
            {
                throw new Win32Exception();
            }
        }

        private static SafeHandle getConsoleHandle(int nStdHandle)
        {
            IntPtr hConsole = NativeInterface.GetStdHandle(nStdHandle);
            SafeHandle handle = new ConsoleHandle(hConsole, true);
            if (handle.IsInvalid)
            {
                throw new Win32Exception();
            }

            return handle;
        }

        private static SafeHandle getConsoleInputHandle()
        {
            return ConsoleInterface.getConsoleHandle(NativeInterface.STD_INPUT_HANDLE);
        }

        private static SafeHandle getConsoleOutputHandle()
        {
            return ConsoleInterface.getConsoleHandle(NativeInterface.STD_OUTPUT_HANDLE);
        }

        private static NativeInterface.CONSOLE_SCREEN_BUFFER_INFO getScreenBufferInfo()
        {
            using (SafeHandle hConsoleOutput = ConsoleInterface.getConsoleOutputHandle())
            {
                NativeInterface.CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo = default(NativeInterface.CONSOLE_SCREEN_BUFFER_INFO);
                ConsoleInterface.callWin32Func(() => NativeInterface.GetConsoleScreenBufferInfo(hConsoleOutput, out screenBufferInfo));
                return screenBufferInfo;
            }
        }

        #endregion Helper Methods

        private class ConsoleHandle : SafeHandleMinusOneIsInvalid
        {
            public ConsoleHandle(IntPtr hConsole, bool ownsHandle)
                : base(ownsHandle)
            {
                this.SetHandle(hConsole);
            }

            protected override bool ReleaseHandle()
            {
                return true; // don't close console handles as that will be done for us at process termination time
            }
        }

        private static class NativeInterface
        {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct CHAR_INFO
            {
                public char UnicodeChar;
                public short Attributes;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SMALL_RECT
            {
                public short Left;
                public short Top;
                public short Right;
                public short Bottom;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct COORD
            {
                public short X;
                public short Y;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct INPUT_RECORD
            {
                public short EventType;
                public KEY_EVENT_RECORD Event;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct KEY_EVENT_RECORD
            {
                public bool bKeyDown;
                public short wRepeatCount;
                public short wVirtualKeyCode;
                public short wVirtualScanCode;
                public char UnicodeChar;
                public int dwControlKeyState;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct CONSOLE_SCREEN_BUFFER_INFO
            {
                public NativeInterface.COORD dwSize;
                public NativeInterface.COORD dwCursorPosition;
                public short wAttributes;
                public NativeInterface.SMALL_RECT srWindow;
                public NativeInterface.COORD dwMaximumWindowSize;
            }

            [DllImport("kernel32", SetLastError = true)]
            public static extern bool SetConsoleMode(SafeHandle hConsoleHandle, int dwMode);

            [DllImport("kernel32", SetLastError = true)]
            public static extern bool GetConsoleMode(SafeHandle hConsoleHandle, out int dwMode);

            [DllImport("kernel32", SetLastError = true)]
            public static extern bool ReadConsoleOutput(SafeHandle hConsoleHandle, [Out] CHAR_INFO[] lpBuffer, COORD dwBufferSize, COORD dwBufferCoord, ref SMALL_RECT lpReadRegion);

            [DllImport("kernel32", SetLastError = true)]
            public static extern IntPtr GetStdHandle(int nStdHandle);

            [DllImport("kernel32", SetLastError = true)]
            public static extern bool FreeConsole();

            [DllImport("kernel32", SetLastError = true)]
            public static extern bool AttachConsole(int dwProcessId);

            [DllImport("kernel32", SetLastError = true)]
            public static extern int GetConsoleProcessList([Out] int[] lpdwProcessList, int dwProcessCount);

            [DllImport("kernel32", SetLastError = true)]
            public static extern bool WriteConsoleInput(SafeHandle hConsoleInput, [In] INPUT_RECORD[] lpBuffer, int nLength, out int lpNumberOfEventsWritten);

            [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern bool FillConsoleOutputCharacter(SafeHandle hConsoleOutput, char cCharacter, int nLength, NativeInterface.COORD dwWriteCoord, out int lpNumberOfCharsWritten);

            [DllImport("kernel32", SetLastError = true)]
            public static extern bool GetConsoleScreenBufferInfo(SafeHandle hConsoleOutput, out NativeInterface.CONSOLE_SCREEN_BUFFER_INFO result);

            [DllImport("kernel32", SetLastError = true)]
            public static extern bool SetConsoleCursorPosition(SafeHandle hConsoleOutput, NativeInterface.COORD dwCursorPosition);

            public const int ENABLE_ECHO_INPUT = 0x4;

            public const int STD_INPUT_HANDLE = -10;
            public const int STD_OUTPUT_HANDLE = -11;

            public const int KEY_EVENT = 0x1;

        }
    }
}
