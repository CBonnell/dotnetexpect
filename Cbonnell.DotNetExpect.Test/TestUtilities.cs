using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Cbonnell.DotNetExpect.Test
{
    internal static class TestUtilities
    {
        public const int PROCESS_EXIT_TIMEOUT_MS = 10 * 1000;

        // Wait for any proxies to exit before running each test
        public static void EnsureProxyExit()
        {
            Process[] powerShells = Process.GetProcessesByName(TestEnvironment.PROXY_PROCESS_NAME);
            try
            {
                foreach (Process p in powerShells)
                {
                    TestUtilities.WaitForProcessExitAndThrow(p);
                }
            }
            finally
            {
                Array.ForEach(powerShells, (p) => p.Dispose());
            }
        }

        public static void WaitForProcessExitAndThrow(Process p)
        {
            if (!p.WaitForExit(TestUtilities.PROCESS_EXIT_TIMEOUT_MS))
            {
                throw new TimeoutException();
            }
        }
    }
}
