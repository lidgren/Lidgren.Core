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
			TimedOut,
			ExitCodeZero,
			ExitCodeNonZero
		}

		/// <summary>
		/// Run shell command; f.ex. for opening html links. Do not block, return immediately
		/// </summary>
		public static Process RunShell(string command, string arguments, string workingDirectory = null)
		{
			var process = new Process();
			process.StartInfo.Verb = command;
			process.StartInfo.Arguments = arguments;
			process.StartInfo.UseShellExecute = true;
			if (string.IsNullOrWhiteSpace(workingDirectory) == false)
			{
				process.StartInfo.WorkingDirectory = workingDirectory;
			}
			process.Start();
			return process;
		}

		/// <summary>
		/// Run shell command; f.ex. for opening html links; blocks until complete or timeout and returns error code
		/// </summary>
		public static RunProcessResult RunShell(string command, string arguments, out int exitCode, TimeSpan timeout, string workingDirectory = null)
		{
			var process = RunShell(command, arguments, workingDirectory);
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

			var process = new Process();
			process.StartInfo.FileName = executable;
			process.StartInfo.Arguments = arguments;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.UseShellExecute = false;
			if (string.IsNullOrWhiteSpace(workingDirectory) == false)
			{
				process.StartInfo.WorkingDirectory = workingDirectory;
			}

			var stdOutBdr = new StringBuilder();
			process.StartInfo.RedirectStandardOutput = true;
			process.OutputDataReceived += (sender, data) =>
			{
				lock (stdOutBdr)
				{
					stdOutBdr.AppendLine(data.Data);
				}
			};

			var stdErrBdr = new StringBuilder();
			process.StartInfo.RedirectStandardError = true;
			process.ErrorDataReceived += (sender, data) =>
			{
				lock (stdErrBdr)
				{
					stdErrBdr.AppendLine(data.Data);
				}
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

			exitCode = process.ExitCode;
			lock (stdOutBdr)
			{
				stdOut = stdOutBdr.ToString();
			}
			lock (stdErrBdr)
			{
				stdErr = stdErrBdr.ToString();
			}

			if (exitCode == 0)
				return RunProcessResult.ExitCodeZero;
			return RunProcessResult.ExitCodeNonZero;
		}

		/// <summary>
		/// Does not use shell execute and redirect output; blocks until complete or timeout and returns error code
		/// </summary>
		public static RunProcessResult RunBlockingWithInput(
			string executable,
			string arguments,
			string workingDirectory,
			TimeSpan timeout,
			string stdIn,
			out int exitCode,
			out string stdOut,
			out string stdErr)
		{
			stdOut = "";
			stdErr = "";
			exitCode = -1;

			var process = new Process();
			process.StartInfo.FileName = executable;
			process.StartInfo.Arguments = arguments;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.UseShellExecute = false;
			if (string.IsNullOrWhiteSpace(workingDirectory) == false)
			{
				process.StartInfo.WorkingDirectory = workingDirectory;
			}

			var stdOutBdr = new StringBuilder();
			process.StartInfo.RedirectStandardOutput = true;
			process.OutputDataReceived += (sender, data) =>
			{
				stdOutBdr.AppendLine(data.Data);
			};

			var stdErrBdr = new StringBuilder();
			process.StartInfo.RedirectStandardError = true;
			process.ErrorDataReceived += (sender, data) =>
			{
				stdErrBdr.AppendLine(data.Data);
			};

			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.StandardInputEncoding = Encoding.Latin1;

			bool ok = process.Start();
			if (!ok)
				return RunProcessResult.FailedToStartProcess;

			// write input
			using (var writer = process.StandardInput)
			{
				writer.Write(stdIn);
			}

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
