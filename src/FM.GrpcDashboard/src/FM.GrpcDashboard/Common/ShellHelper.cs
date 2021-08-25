using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace System
{
    public static class ShellHelper
    {
        public static string Bash(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"{escapedArgs}\" 2>&1",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();
            return !string.IsNullOrWhiteSpace(result) ? result : error;
        }

        public static string Cmd(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c \"{escapedArgs}\" 2>&1",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();

            process.WaitForExit();
            return result;
        }

        public static string Execute(this string exeFileName, string arguments)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exeFileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
    }
}
