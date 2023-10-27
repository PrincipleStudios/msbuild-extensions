using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Build.Framework;

public class MutexMSBuild : Microsoft.Build.Tasks.MSBuild, ITask
{
#pragma warning disable
	[Microsoft.Build.Framework.Required]
	public string MutexName { get; set; }

	public double TimeoutSeconds { get; set; } = 300;
#pragma warning restore

	public override bool Execute()
	{
		using var waiter = MutexWaiter.WaitWithMessaging(MutexName, TimeoutSeconds, Log);
		if (waiter == null) return false;
		return base.Execute();
	}
}