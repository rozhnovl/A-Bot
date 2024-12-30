using System;
//using System.Runtime.Remoting.Lifetime;

namespace BotEngine.InvocationProxy;

public class HostDelegate : MarshalByRefObject, IHost
{
	public Func<string> GetAppObjectDelegate;

	public Func<string, string> InvokeDelegate;

	public Func<string, string> GarbageCollectDelegate;

	public TimeSpan RemotingLeaseTime = TimeSpan.FromMinutes(60.0);

	//public ILease InitializeLifetimeServiceLease;

	public string GetAppObject()
	{
		return GetAppObjectDelegate?.Invoke();
	}

	public string Invoke(string invocationSerial)
	{
		return InvokeDelegate?.Invoke(invocationSerial);
	}

	public string GarbageCollect(string collectionSerial)
	{
		return GarbageCollectDelegate?.Invoke(collectionSerial);
	}

	public override object InitializeLifetimeService()
	{
		throw new NotImplementedException();
		/*ILease lease = (ILease)base.InitializeLifetimeService();
		if (lease.CurrentState == LeaseState.Initial)
		{
			lease.InitialLeaseTime = RemotingLeaseTime;
			lease.SponsorshipTimeout = RemotingLeaseTime;
			lease.RenewOnCallTime = RemotingLeaseTime;
			InitializeLifetimeServiceLease = lease;
		}
		return lease;*/
	}
}
