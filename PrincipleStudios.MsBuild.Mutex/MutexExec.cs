using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Build.Framework;

public class MutexExec : Microsoft.Build.Tasks.Exec, ITask
{
#pragma warning disable
	[Microsoft.Build.Framework.Required]
	public string MutexName { get; set; }

	public double PreMessageTimeoutSeconds { get; set; } = 1;
	public double TimeoutSeconds { get; set; } = 300;
#pragma warning restore

	public override bool Execute()
	{
		using var waiter = MutexWaiter.WaitWithMessaging(MutexName, TimeoutSeconds, Log);
		if (waiter == null) return false;
		return base.Execute();
	}
}