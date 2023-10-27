using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Build.Framework;

sealed class MutexWaiter : IDisposable
{
	private readonly Mutex mutex;
	private const double preMessageTimeoutSeconds = 1;

	private MutexWaiter(string mutexName, double timeoutSeconds)
	{
		mutex = new Mutex(false, mutexName);
		MutexName = mutexName;
		TimeoutSeconds = timeoutSeconds;
	}

	public static MutexWaiter? WaitWithMessaging(string mutexName, double timeoutSeconds, Microsoft.Build.Utilities.TaskLoggingHelper log)
	{
		var result = new MutexWaiter(mutexName, timeoutSeconds);
		if (!result.WaitWithMessaging(log))
		{
			result.Dispose();
			return null;
		}
		return result;
	}

	private bool WaitWithMessaging(Microsoft.Build.Utilities.TaskLoggingHelper log)
	{
		if (!mutex.WaitOne(millisecondsTimeout: (int)(Math.Min(preMessageTimeoutSeconds, TimeoutSeconds) * 1000)))
		{
			log.LogMessage(MessageImportance.High, "Waiting to acquire mutex '{0}'...", MutexName);
			if (!mutex.WaitOne(millisecondsTimeout: (int)(Math.Max(TimeoutSeconds - preMessageTimeoutSeconds, 0) * 1000)))
			{
				log.LogMessage(MessageImportance.Normal, "Failed to acquire mutex '{0}'. If this persists, restart your machine.", MutexName);
				return false;
			}
		}
		return true;
	}

	public string MutexName { get; }
	public double TimeoutSeconds { get; }

	public void Dispose()
	{
		mutex.ReleaseMutex();
		mutex.Dispose();
	}
}