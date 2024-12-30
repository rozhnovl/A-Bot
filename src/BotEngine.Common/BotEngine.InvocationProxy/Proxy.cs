using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bib3;
using Bib3.RefNezDiferenz;
using BotEngine.InvocationProxy.SerialStruct;
using Castle.DynamicProxy;
using Newtonsoft.Json;

namespace BotEngine.InvocationProxy;

public class Proxy : HostOrProxyBase
{
	private class Interceptor : IInterceptor
	{
		private Proxy Proxy;

		private long RemoteObjectId;

		public Interceptor(Proxy proxy, long remoteObjectId)
		{
			Proxy = proxy;
			RemoteObjectId = remoteObjectId;
		}

		public void Intercept(IInvocation invocation)
		{
			Type type = ((invocation == null) ? null : invocation.Method?.ReturnType);
			object obj = Proxy?.Invoke(RemoteObjectId, invocation.Method, invocation.Arguments);
			Type[] array = obj?.GetType().GetInterfaces();
			if (obj != null && !obj.GetType().InheritsOrImplementsOrEquals(type))
			{
				Type type2 = type?.ListeTypeArgumentZuBaseOderInterface(typeof(IEnumerable<>))?.FirstOrDefault();
				Type type3 = type?.ListeTypeArgumentZuBaseOderInterface(typeof(IEnumerator<>))?.FirstOrDefault();
				Type type4 = type?.ListeTypeArgumentZuBaseOderInterface(typeof(Nullable<>))?.FirstOrDefault();
				if (null != type2 && obj is IEnumerable)
				{
					obj = (obj as IEnumerable).MakeGenericMethodCast(type2);
				}
				if (null != type3 && obj is IEnumerator)
				{
					obj = (obj as IEnumerator).MakeGenericMethodCast(type3);
				}
				if (null != type4 && type4.IsPrimitive)
				{
					obj = Convert.ChangeType(obj, type4);
				}
			}
			invocation.ReturnValue = obj;
		}
	}

	private static ProxyGenerator ProxyGenerator = new ProxyGenerator();

	private readonly IHost host;

	private GarbageCollectionResult GarbageCollectionLastResult;

	private bool GarbageCollectionDue => Math.Max(100, GarbageCollectionLastResult?.RemainingCount ?? 0) * 4 / 3 < RemoteObjectMap.Forward.Count();

	public Proxy(IHost host, RemotingConfig config = null)
		: base(config)
	{
		this.host = host;
	}

	public object Invoke(long remoteObjectId, MethodInfo method, params object[] listArgument)
	{
		RemoteObject remoteObject = (host?.Invoke(new Invocation
		{
			objectId = remoteObjectId,
			method = new MethodId
			{
				name = method?.Name,
				returnTypeName = method?.ReturnType?.FullName
			},
			listArgument = listArgument?.Select((object arg) => RemoteObjectFromClrRef(arg))?.ToArrayIfNotEmpty()
		}.SerializeToString((Formatting)0)))?.DeserializeFromString<RemoteObject>();
		if (0 < remoteObject?.id)
		{
			return ClrRefFromRemoteObject(remoteObject);
		}
		string text = remoteObject?.setTypeName?.FirstOrDefault();
		if (text != null)
		{
			Type type = TryFindType(text);
			return JsonConvert.DeserializeObject(remoteObject.value as string, type);
		}
		return remoteObject?.value;
	}

	public object ClrRefFromRemoteObject(RemoteObject remoteObject)
	{
		return (ClrRefFromRemoteObject(remoteObject, InstanceConstruct) as WeakReference).Target;
	}

	public static Type TryFindType(string typeName)
	{
		return AppDomain.CurrentDomain.GetAssemblies()?.Select((Assembly a) => a.GetType(typeName))?.WhereNotDefault()?.FirstOrDefault();
	}

	private object InstanceConstruct(RemoteObject remoteObject)
	{
		string[] array = remoteObject?.setTypeName?.WhereNotDefault()?.ToArrayIfNotEmpty();
		if (array.IsNullOrEmpty())
		{
			return null;
		}
		if (GarbageCollectionDue)
		{
			GarbageCollect();
		}
		Type[] array2 = array?.Select(TryFindType)?.ToArray();
		object target = ProxyGenerator.CreateInterfaceProxyWithoutTarget(array2?.FirstOrDefault(), array2.Skip(1)?.ToArray(), (IInterceptor[])(object)new IInterceptor[1]
		{
			new Interceptor(this, remoteObject.id)
		});
		return new WeakReference(target);
	}

	public GarbageCollectionResult GarbageCollect()
	{
		lock (@lock)
		{
			KeyValuePair<long, WeakReference>[] array = RemoteObjectMap.Forward.Select((KeyValuePair<long, object> elem) => new KeyValuePair<long, WeakReference>(elem.Key, elem.Value as WeakReference)).ToArray();
			List<KeyValuePair<long, WeakReference>> list = array.Take(0).ToList();
			List<KeyValuePair<long, WeakReference>> list2 = array.Take(0).ToList();
			KeyValuePair<long, WeakReference>[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				KeyValuePair<long, WeakReference> item = array2[i];
				(item.Value.IsAlive ? list : list2).Add(item);
			}
			GarbageCollectionParam garbageCollectionParam = new GarbageCollectionParam
			{
				ObjectToCollectIdMax = list2.Keys().DefaultIfEmpty(0L).Max(),
				SetObjectRemainingId = list.Keys().ToArray()
			};
			BotEngine.InvocationProxy.SerialStruct.GarbageCollectionResult fromHostResult = host?.GarbageCollect(garbageCollectionParam.SerializeToString((Formatting)0))?.DeserializeFromString<BotEngine.InvocationProxy.SerialStruct.GarbageCollectionResult>();
			foreach (KeyValuePair<long, WeakReference> item2 in list2)
			{
				RemoteObjectMap.RemoveForward(item2.Key);
			}
			return GarbageCollectionLastResult = new GarbageCollectionResult
			{
				ToHostParam = garbageCollectionParam,
				FromHostResult = fromHostResult,
				RemainingCount = list.Count
			};
		}
	}
}
