using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProtoBuf.Meta;

namespace BotEngine;

public static class ProtobufRuntimeTypeModelSetup
{
	private static readonly object Lock = new object();

	private static readonly Dictionary<Assembly, bool> SetupForAssemblyDone = new Dictionary<Assembly, bool>();

	private static int ProtobufFieldNumber = 1;

	public static void SetupForType(Type type)
	{
		bool flag = type.IsEnum || type.IsInterface || type.IsPrimitive;
		MetaType val = RuntimeTypeModel.Default.Add(type, flag);
		if (flag)
		{
			return;
		}
		IEnumerable<MemberInfo> enumerable = from tMember in type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
			where tMember.MemberType == MemberTypes.Field || tMember.MemberType == MemberTypes.Property
			select tMember;
		foreach (MemberInfo item in enumerable)
		{
			val.AddField(ProtobufFieldNumber++, item.Name);
		}
	}

	public static void SetupForAssembly(Assembly assembly)
	{
		if (null == assembly)
		{
			return;
		}
		lock (Lock)
		{
			if (SetupForAssemblyDone.ContainsKey(assembly))
			{
				return;
			}
			SetupForAssemblyDone[assembly] = true;
			Type[] types = assembly.GetTypes();
			Type[] array = types;
			foreach (Type type in array)
			{
				try
				{
					if (!type.IsEnum || !(typeof(int) != Enum.GetUnderlyingType(type)))
					{
						SetupForType(type);
					}
				}
				catch (Exception innerException)
				{
					throw new ApplicationException("Type: " + type.FullName, innerException);
				}
			}
		}
	}
}
