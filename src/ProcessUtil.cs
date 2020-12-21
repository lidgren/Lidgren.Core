using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Lidgren.Core
{
    public static class ProcessUtil
    {
		public enum RunProcessResult
		{
			FailedToStartProcess,
			NoSuchExecutableFile,
			TimedOut,
			ExitCodeZero,
			ExitCodeNonZero
		}

		/// <summary>
		/// Run shell command; f.ex. for opening html links. Do not block, return immediately
		/// </summary>
		public static void RunShell(string command, string arguments)
		{
			Process.Start(command, arguments);
		}

		/// <summary>
		/// Run shell command; f.ex. for opening html links; blocks until complete or timeout and returns error code
		/// </summary>
		public static RunProcessResult RunShell(string command, string arguments, out int exitCode, TimeSpan timeout)
		{
			var process = Process.Start(command, arguments);
			bool exited = process.WaitForExit((int)timeout.TotalMilliseconds);
			if (!exited)
			{
				exitCode = -1;
				return RunProcessResult.TimedOut;
			}

			exitCode = process.ExitCode;
			if (exitCode == 0)
				return RunProcessResult.ExitCodeZero;
			return RunProcessResult.ExitCodeNonZero;
		}

		/// <summary>
		/// Do not use shell execute and redirect output; blocks until complete or timeout and returns error code
		/// </summary>
		public static RunProcessResult RunBlocking(
			string executable,
			string arguments,
			string workingDirectory,
			TimeSpan timeout,
			out int exitCode,
			out string stdOut,
			out string stdErr)
		{
			stdOut = "";
			stdErr = "";
			exitCode = -1;

			if (File.Exists(executable) == false)
				return RunProcessResult.NoSuchExecutableFile;

			var process = new Process();
			process.StartInfo.FileName = executable;
			process.StartInfo.Arguments = arguments;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.UseShellExecute = false;

			var stdOutBdr = new StringBuilder();
			process.StartInfo.RedirectStandardOutput = true;
			process.OutputDataReceived += (sender, data) => {
				stdOutBdr.AppendLine(data.Data);
			};

			var stdErrBdr = new StringBuilder();
			process.StartInfo.RedirectStandardError = true;
			process.ErrorDataReceived += (sender, data) => {
				stdErrBdr.AppendLine(data.Data);
			};

			bool ok = process.Start();
			if (!ok)
				return RunProcessResult.FailedToStartProcess;

			// read output
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			bool exited = process.WaitForExit((int)timeout.TotalMilliseconds);
			if (!exited)
				return RunProcessResult.TimedOut;

			stdOut = stdOutBdr.ToString();
			stdErr = stdErrBdr.ToString();
			exitCode = process.ExitCode;

			if (exitCode == 0)
				return RunProcessResult.ExitCodeZero;
			return RunProcessResult.ExitCodeNonZero;
		}
	}
}
