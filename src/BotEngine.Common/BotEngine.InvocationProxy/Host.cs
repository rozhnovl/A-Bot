using System;
using System.Linq;
using System.Reflection;
using Bib3;
using BotEngine.InvocationProxy.SerialStruct;
using Newtonsoft.Json;

namespace BotEngine.InvocationProxy;

public class Host
{
	private readonly object @lock = new object();

	private readonly HostOrProxyBase HostBase;

	private readonly Map<long, object> remoteObjectMap = new Map<long, object>();

	protected RemoteObject RemoteObjectFromClrRef(object clrObject)
	{
		return HostBase?.RemoteObjectFromClrRef(clrObject, create: true);
	}

	public Host(RemotingConfig config = null)
	{
		HostBase = new HostOrProxyBase(config, @lock, remoteObjectMap);
	}

	public string Invoke(string invocationSerial)
	{
		Invocation invocation = invocationSerial?.DeserializeFromString<Invocation>();
		if (invocation == null)
		{
			return null;
		}
		object obj = HostBase.ClrRefFromRemoteObjectId(invocation.objectId);
		if (obj == null)
		{
			throw new ArgumentException("unknown objectId");
		}
		Type type = obj.GetType();
		MethodInfo methodInfo = new Type[1] { type }.ConcatNullable(type.GetInterfaces())?.Select((Type c) => c.GetMethods())?.ConcatNullable()?.Where((MethodInfo c) => c.Name == invocation.method.name && c.ReturnType.FullName == invocation.method.returnTypeName)?.WhereNotDefault()?.FirstOrDefault();
		object[] parameters = invocation.listArgument?.Select((RemoteObject argRemoteObject) => HostBase.ClrRefFromRemoteObject(argRemoteObject, null))?.ToArray();
		object clrObject = methodInfo.Invoke(obj, parameters);
		return RemoteObjectFromClrRef(clrObject)?.SerializeToString((Formatting)0);
	}

	public string GarbageCollect(string collectionSerial)
	{
		lock (@lock)
		{
			GarbageCollectionParam collectionParam = collectionSerial?.DeserializeFromString<GarbageCollectionParam>();
			long[] array = (from objectId in remoteObjectMap.Forward.Keys()
				where objectId <= collectionParam?.ObjectToCollectIdMax
				select objectId).Except((collectionParam?.SetObjectRemainingId).EmptyIfNull()).ToArray();
			long[] array2 = array;
			foreach (long t in array2)
			{
				remoteObjectMap.RemoveForward(t);
			}
			return new BotEngine.InvocationProxy.SerialStruct.GarbageCollectionResult
			{
				CollectedCount = array.Length,
				RemainingCount = remoteObjectMap.Forward.Count()
			}.SerializeToString((Formatting)0);
		}
	}
}
