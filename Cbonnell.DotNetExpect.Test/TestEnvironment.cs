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

using System.Text.RegularExpressions;

namespace Cbonnell.DotNetExpect.Test
{
    internal static class TestEnvironment
    {
        public const string CMD_EXE_NAME = "cmd.exe";
        public const string DUMMY_EXE_NAME = "myprocess.exe";
        public const string DUMMY_ARGUMENTS = "foo hoge";

        public const string PROXY_PROCESS_NAME = "powershell";

        public const char PROMPT_CHAR = '>';

        public static readonly Regex CMD_PROMPT_REGEX = new Regex(TestEnvironment.PROMPT_CHAR.ToString());
    }
}
