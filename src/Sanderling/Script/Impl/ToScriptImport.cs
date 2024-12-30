﻿using Bib3;
using BotEngine;
using BotEngine.Interface;
using BotEngine.Motor;
using Sanderling.Motor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Script.Impl
{
	static public class ToScriptImport
	{
		static readonly Type[] AssemblyAndNamespaceAdditionType = new[]
		{
			typeof(List<>),
			typeof(System.Text.RegularExpressions.Regex),
			typeof(Uri),
			typeof(Enumerable),
			typeof(IHostToScript),
			//todo typeof(BotSharp.ToScript.IHostToScript),
			typeof(Bib3.Geometrik.Vektor2DInt),
			typeof(ObjectIdInt64),
			typeof(FromProcessMeasurement<>),
			typeof(MemoryStruct.IMemoryMeasurement),
			typeof(MotionParam),
			typeof(BotEngineC.MouseButtonIdEnum),
			typeof(BotEngine.Common.Extension),
			typeof(Sanderling.Extension),
			typeof(ToScriptExtension),
			typeof(Parse.IMemoryMeasurement),
			typeof(WindowsInput.Native.VirtualKeyCode),
		};

		static readonly Type[] NamespaceStaticAdditionType = new[]
		{
			typeof(Bib3.Extension),
		};

		static IEnumerable<Type> AssemblyAdditionType => new[]
		{
			AssemblyAndNamespaceAdditionType,
			NamespaceStaticAdditionType,
		}.ConcatNullable();

		static public IEnumerable<string> AssemblyName =>
			AssemblyAdditionType?.Select(t => t.Assembly.GetName()?.Name)?.Distinct();

		static public IEnumerable<Microsoft.CodeAnalysis.MetadataReference> ImportAssembly =>
			AssemblyName?.Select(assemblyName => GetAssemblyReference(assemblyName));

		static public IEnumerable<string> ImportNamespace =>
			new[]
			{
				AssemblyAndNamespaceAdditionType?.Select(t => t.Namespace),
				NamespaceStaticAdditionType?.Select(t => t.FullName),
			}.ConcatNullable();

		static readonly Func<string, Stream> CosturaAssemblyResolver = Costura.AssemblyResolverCosturaConstruct();

		static public Microsoft.CodeAnalysis.MetadataReference GetAssemblyReference(string assemblyName)
		{
			var FromCosturaStream = CosturaAssemblyResolver?.Invoke(assemblyName);

			if (null != FromCosturaStream)
			{
				return Microsoft.CodeAnalysis.MetadataReference.CreateFromStream(FromCosturaStream);
			}

			return AssemblyReferenceFromLoadedAssembly(assemblyName);
		}

		static Microsoft.CodeAnalysis.MetadataReference AssemblyReferenceFromLoadedAssembly(string assemblyName)
		{
			var Assembly = AppDomain.CurrentDomain.GetAssemblies()?.FirstOrDefault(candidate => candidate?.GetName()?.Name == assemblyName);

			if (null == Assembly)
			{
				return null;
			}

			return Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(Assembly.Location);
		}
	}
}
