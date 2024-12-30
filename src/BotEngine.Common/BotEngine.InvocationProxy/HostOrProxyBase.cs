using System;
using System.Collections.Generic;
using System.Linq;
using Bib3;
using BotEngine.InvocationProxy.SerialStruct;
using Newtonsoft.Json;

namespace BotEngine.InvocationProxy;

public class HostOrProxyBase
{
	protected readonly object @lock;

	private long RemoteObjectCreateId = 1L;

	public RemotingConfig Config;

	protected readonly Map<long, object> RemoteObjectMap;

	public HostOrProxyBase(RemotingConfig config = null, object @lock = null, Map<long, object> remoteObjectMap = null)
	{
		Config = config;
		this.@lock = @lock ?? new object();
		RemoteObjectMap = remoteObjectMap ?? new Map<long, object>();
	}

	public object ClrRefFromRemoteObjectId(long remoteObjectId)
	{
		return RemoteObjectMap.TryGetForwardOrDefault(remoteObjectId);
	}

	public static object ClrRefFromRemoteObjectValue(RemoteObject remoteObject)
	{
		string[] array = remoteObject?.setTypeName?.WhereNotDefault()?.ToArrayIfNotEmpty();
		if (array.IsNullOrEmpty())
		{
			return null;
		}
		Type type = Proxy.TryFindType(array?.FirstOrDefault());
		return JsonConvert.DeserializeObject(remoteObject.value as string, type);
	}

	public object ClrRefFromRemoteObject(RemoteObject remoteObject, Func<RemoteObject, object> callbackInstanceConstruct)
	{
		if (remoteObject == null)
		{
			return null;
		}
		lock (@lock)
		{
			object obj = ClrRefFromRemoteObjectId(remoteObject.id);
			if (obj == null)
			{
				if (callbackInstanceConstruct == null)
				{
					return ClrRefFromRemoteObjectValue(remoteObject) ?? remoteObject?.value;
				}
				obj = callbackInstanceConstruct(remoteObject);
				if (obj != null)
				{
					RemoteObjectMap.Add(remoteObject.id, obj);
				}
			}
			return obj;
		}
	}

	public RemoteObject RemoteObjectFromClrRef(object clrRef, bool create = false)
	{
		if (clrRef == null)
		{
			return new RemoteObject();
		}
		Type type = clrRef.GetType();
		if (type.IsPrimitive || typeof(string) == type)
		{
			return new RemoteObject
			{
				value = clrRef
			};
		}
		lock (@lock)
		{
			if (Config?.SetValueTypeEnabled?.Contains(type) ?? false)
			{
				RemoteObject remoteObject = new RemoteObject();
				remoteObject.setTypeName = new string[1] { type?.FullName };
				remoteObject.value = clrRef?.SerializeToString((Formatting)0);
				return remoteObject;
			}
			long? num = RemoteObjectIdFromClrRef(clrRef, create);
			if (!num.HasValue)
			{
				return null;
			}
			Type[] listRemoteObjectInterfaceTypeFromObjectType = GetListRemoteObjectInterfaceTypeFromObjectType(type);
			if (listRemoteObjectInterfaceTypeFromObjectType != null)
			{
				string[] setTypeName = listRemoteObjectInterfaceTypeFromObjectType?.Select((Type @interface) => @interface.FullName)?.ToArray();
				return new RemoteObject
				{
					id = num.Value,
					setTypeName = setTypeName
				};
			}
			return new RemoteObject();
		}
	}

	public Type[] GetListRemoteObjectInterfaceTypeFromObjectType(Type objectType)
	{
		Type[] setInterface = objectType?.GetInterfaces();
		Type type = Config?.ListInterfaceEnabled?.FirstOrDefault((Type c) => setInterface.Contains(c));
		return new Type[1] { type }?.WhereNotDefault()?.ToArray();
	}

	public long? RemoteObjectIdFromClrRef(object clrRef, bool create = false)
	{
		lock (@lock)
		{
			long num = RemoteObjectMap.Reverse.FirstOrDefault((KeyValuePair<object, long> c) => c.Key == clrRef || (c.Key as WeakReference)?.Target == clrRef).Value;
			if (num == 0)
			{
				if (!create)
				{
					return null;
				}
				num = RemoteObjectCreateId++;
				RemoteObjectMap.Add(num, clrRef);
			}
			return num;
		}
	}
}
