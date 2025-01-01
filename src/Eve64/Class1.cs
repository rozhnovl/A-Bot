using System.Collections.Immutable;
using System.Runtime.InteropServices;
using PythonStructures;

namespace Eve64
{
	public class EveOnline64
	{
		static public IImmutableList<ulong> EnumeratePossibleAddressesForUIRootObjects(IEnumerable<(ulong baseAddress, byte[] content)> memoryRegions)
		{
			var memoryRegionsOrderedByAddress =
				memoryRegions
				.OrderBy(memoryRegion => memoryRegion.baseAddress)
				.Select(memoryRegion =>
				new
				{
					baseAddress = memoryRegion.baseAddress,
					content = memoryRegion.content,
					contentAsULongArray = TransformMemoryContent.AsULongArray(memoryRegion.content)
				})
				.ToImmutableList();

			string ReadNullTerminatedAsciiStringFromAddress(ulong address)
			{
				var memoryRegion =
					memoryRegions
					.Where(c => c.baseAddress <= address)
					.OrderBy(c => c.baseAddress)
					.LastOrDefault();

				if (memoryRegion.content == null)
					return null;

				var bytes =
					memoryRegion.content
					.Skip((int)(address - memoryRegion.baseAddress))
					.TakeWhile(character => 0 < character)
					.ToArray();

				return System.Text.Encoding.ASCII.GetString(bytes);
			}

			IEnumerable<ulong> EnumerateCandidatesForPythonTypeObjectType()
			{
				foreach (var memoryRegion in memoryRegionsOrderedByAddress)
				{
					for (var candidateAddressIndex = 0; candidateAddressIndex < memoryRegion.contentAsULongArray.Length - 4; ++candidateAddressIndex)
					{
						var candidateAddressInProcess = memoryRegion.baseAddress + (ulong)candidateAddressIndex * 8;

						var candidate_ob_type = memoryRegion.contentAsULongArray[candidateAddressIndex + 1];

						if (candidate_ob_type != candidateAddressInProcess)
							continue;

						var candidate_tp_name =
							ReadNullTerminatedAsciiStringFromAddress(memoryRegion.contentAsULongArray[candidateAddressIndex + 3]);

						if (candidate_tp_name != "type")
							continue;

						yield return candidateAddressInProcess;
					}
				}
			}

			IEnumerable<(ulong address, string tp_name)> EnumerateCandidatesForPythonTypeObjects(
				IImmutableList<ulong> typeObjectCandidatesAddresses)
			{
				if (typeObjectCandidatesAddresses.Count < 1)
					yield break;

				var typeAddressMin = typeObjectCandidatesAddresses.Min();
				var typeAddressMax = typeObjectCandidatesAddresses.Max();

				foreach (var memoryRegion in memoryRegionsOrderedByAddress)
				{
					for (var candidateAddressIndex = 0; candidateAddressIndex < memoryRegion.contentAsULongArray.Length - 4; ++candidateAddressIndex)
					{
						var candidateAddressInProcess = memoryRegion.baseAddress + (ulong)candidateAddressIndex * 8;

						var candidate_ob_type = memoryRegion.contentAsULongArray[candidateAddressIndex + 1];

						{
							//  This check is redundant with the following one. It just implements a specialization to optimize runtime expenses.
							if (candidate_ob_type < typeAddressMin || typeAddressMax < candidate_ob_type)
								continue;
						}

						if (!typeObjectCandidatesAddresses.Contains(candidate_ob_type))
							continue;

						var candidate_tp_name =
							ReadNullTerminatedAsciiStringFromAddress(memoryRegion.contentAsULongArray[candidateAddressIndex + 3]);

						if (candidate_tp_name == null)
							continue;

						yield return (candidateAddressInProcess, candidate_tp_name);
					}
				}
			}

			IEnumerable<ulong> EnumerateCandidatesForInstancesOfPythonType(
				IImmutableList<ulong> typeObjectCandidatesAddresses)
			{
				if (typeObjectCandidatesAddresses.Count < 1)
					yield break;

				var typeAddressMin = typeObjectCandidatesAddresses.Min();
				var typeAddressMax = typeObjectCandidatesAddresses.Max();

				foreach (var memoryRegion in memoryRegionsOrderedByAddress)
				{
					for (var candidateAddressIndex = 0; candidateAddressIndex < memoryRegion.contentAsULongArray.Length - 4; ++candidateAddressIndex)
					{
						var candidateAddressInProcess = memoryRegion.baseAddress + (ulong)candidateAddressIndex * 8;

						var candidate_ob_type = memoryRegion.contentAsULongArray[candidateAddressIndex + 1];

						{
							//  This check is redundant with the following one. It just implements a specialization to optimize runtime expenses.
							if (candidate_ob_type < typeAddressMin || typeAddressMax < candidate_ob_type)
								continue;
						}

						if (!typeObjectCandidatesAddresses.Contains(candidate_ob_type))
							continue;

						yield return candidateAddressInProcess;
					}
				}
			}

			var uiRootTypeObjectCandidatesAddresses =
				EnumerateCandidatesForPythonTypeObjects(EnumerateCandidatesForPythonTypeObjectType().ToImmutableList())
				.Where(typeObject => typeObject.tp_name == "UIRoot")
				.Select(typeObject => typeObject.address)
				.ToImmutableList();

			return
				EnumerateCandidatesForInstancesOfPythonType(uiRootTypeObjectCandidatesAddresses)
				.ToImmutableList();
		}

		struct PyDictEntry
		{
			public ulong hash;
			public ulong key;
			public ulong value;
		}

		static readonly IImmutableSet<string> DictEntriesOfInterestKeys = ImmutableHashSet.Create(
			"_top", "_left", "_width", "_height", "_displayX", "_displayY",
			"_displayHeight", "_displayWidth",
			"_name", "_text", "_setText",
			"children",
			"texturePath", "_bgTexturePath",
			"_texture", "_hint", "_display", "_lastValue", "_rotation", "_color", "_opacity", "_texturePath", "itemID", "id", "charge", "moduleinfo", "quantity"
		);

		struct LocalMemoryReadingTools
		{
			public IMemoryReader memoryReader;

			public Func<ulong, IImmutableDictionary<string, ulong>> getDictionaryEntriesWithStringKeys;

			public Func<ulong, string> GetPythonTypeNameFromPythonObjectAddress;

			public Func<ulong, object> GetDictEntryValueRepresentation;
		}

		static readonly IImmutableDictionary<string, Func<ulong, LocalMemoryReadingTools, object>> specializedReadingFromPythonType =
				ImmutableDictionary<string, Func<ulong, LocalMemoryReadingTools, object>>.Empty
					.Add("str", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_str))
					.Add("unicode", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_unicode))
					.Add("int", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_int))
					.Add("bool", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_bool))
					.Add("float", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_float))
					.Add("PyColor", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_PyColor))
					.Add("Bunch", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_Bunch))
					.Add("Link", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_Link));

		static object ReadingFromPythonType_str(ulong address, LocalMemoryReadingTools memoryReadingTools)
		{
			return ReadPythonStringValue(address, memoryReadingTools.memoryReader, 0x1000);
		}

		static object ReadingFromPythonType_unicode(ulong address, LocalMemoryReadingTools memoryReadingTools)
		{
			var pythonObjectMemory = memoryReadingTools.memoryReader.ReadBytes(address, 0x20);

			if (!(pythonObjectMemory?.Length == 0x20))
				return "Failed to read python object memory.";

			var unicode_string_length = BitConverter.ToUInt64(pythonObjectMemory[0x10..]);

			if (0x1000 < unicode_string_length)
				return "String too long.";

			var stringBytesCount = (int)unicode_string_length * 2;

			var stringBytes = memoryReadingTools.memoryReader.ReadBytes(
				BitConverter.ToUInt64(pythonObjectMemory[0x18..]), stringBytesCount);

			if (!(stringBytes?.Length == stringBytesCount))
				return "Failed to read string bytes.";

			return System.Text.Encoding.Unicode.GetString(stringBytes);
		}

		static object ReadingFromPythonType_int(ulong address, LocalMemoryReadingTools memoryReadingTools)
		{
			var intObjectMemory = memoryReadingTools.memoryReader.ReadBytes(address, 0x18);

			if (!(intObjectMemory?.Length == 0x18))
				return "Failed to read int object memory.";

			var value = BitConverter.ToInt64(intObjectMemory[0x10..]);

			var asInt32 = (int)value;

			if (asInt32 == value)
				return asInt32;

			return new
			{
				@int = value,
				int_low32 = asInt32,
			};
		}

		static object ReadingFromPythonType_bool(ulong address, LocalMemoryReadingTools memoryReadingTools)
		{
			var pythonObjectMemory = memoryReadingTools.memoryReader.ReadBytes(address, 0x18);

			if (!(pythonObjectMemory?.Length == 0x18))
				return "Failed to read python object memory.";

			return BitConverter.ToInt64(pythonObjectMemory[0x10..]) != 0;
		}

		static object ReadingFromPythonType_float(ulong address, LocalMemoryReadingTools memoryReadingTools)
		{
			return ReadPythonFloatObjectValue(address, memoryReadingTools.memoryReader);
		}

		static object ReadingFromPythonType_PyColor(ulong address, LocalMemoryReadingTools memoryReadingTools)
		{
			var pyColorObjectMemory = memoryReadingTools.memoryReader.ReadBytes(address, 0x18);

			if (!(pyColorObjectMemory?.Length == 0x18))
				return "Failed to read pyColorObjectMemory.";

			var dictionaryAddress = BitConverter.ToUInt64(pyColorObjectMemory[0x10..]);

			var dictionaryEntries = memoryReadingTools.getDictionaryEntriesWithStringKeys(dictionaryAddress);

			if (dictionaryEntries == null)
				return "Failed to read dictionary entries.";

			int? readValuePercentFromDictEntryKey(string dictEntryKey)
			{
				if (!dictionaryEntries.TryGetValue(dictEntryKey, out var valueAddress))
					return null;

				var valueAsFloat = ReadPythonFloatObjectValue(valueAddress, memoryReadingTools.memoryReader);

				if (!valueAsFloat.HasValue)
					return null;

				return (int)(valueAsFloat.Value * 100);
			}

			return new
			{
				aPercent = readValuePercentFromDictEntryKey("_a"),
				rPercent = readValuePercentFromDictEntryKey("_r"),
				gPercent = readValuePercentFromDictEntryKey("_g"),
				bPercent = readValuePercentFromDictEntryKey("_b"),
			};
		}

		static double? ReadPythonFloatObjectValue(ulong floatObjectAddress, IMemoryReader memoryReader)
		{
			//  https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Include/floatobject.h

			var pythonObjectMemory = memoryReader.ReadBytes(floatObjectAddress, 0x20);

			if (!(pythonObjectMemory?.Length == 0x20))
				return null;

			return BitConverter.ToDouble(pythonObjectMemory[0x10..]);
		}

		static object ReadingFromPythonType_Bunch(ulong address, LocalMemoryReadingTools memoryReadingTools)
		{
			var dictionaryEntries = memoryReadingTools.getDictionaryEntriesWithStringKeys(address);

			if (dictionaryEntries == null)
				return "Failed to read dictionary entries.";

			var entriesOfInterest = new List<UITreeNode.DictEntry>();

			foreach (var entry in dictionaryEntries)
			{
				if (!DictEntriesOfInterestKeys.Contains(entry.Key))
				{
					continue;
				}

				entriesOfInterest.Add(new UITreeNode.DictEntry
				{
					key = entry.Key,
					value = memoryReadingTools.GetDictEntryValueRepresentation(entry.Value)
				});
			}

			var entriesOfInterestJObject =
				new System.Text.Json.Nodes.JsonObject(
					entriesOfInterest.Select(dictEntry =>
					new KeyValuePair<string, System.Text.Json.Nodes.JsonNode?>
						(dictEntry.key,
						System.Text.Json.Nodes.JsonNode.Parse(SerializeMemoryReadingNodeToJson(dictEntry.value)))));

			return new UITreeNode.Bunch(entriesOfInterestJObject);
		}

		static public string SerializeMemoryReadingNodeToJson(object obj) =>
			System.Text.Json.JsonSerializer.Serialize(obj, MemoryReadingJsonSerializerOptions);

		static public System.Text.Json.JsonSerializerOptions MemoryReadingJsonSerializerOptions =>
			new()
			{
				Converters =
				{
					//  Support common JSON parsers: Wrap large integers in a string to work around limitations there. (https://discourse.elm-lang.org/t/how-to-parse-a-json-object/4977)
					new Int64JsonConverter(),
					new UInt64JsonConverter()
				}
			};
		public class Int64JsonConverter : System.Text.Json.Serialization.JsonConverter<long>
		{
			public override long Read(
				ref System.Text.Json.Utf8JsonReader reader,
				Type typeToConvert,
				System.Text.Json.JsonSerializerOptions options) =>
				long.Parse(reader.GetString()!);

			public override void Write(
				System.Text.Json.Utf8JsonWriter writer,
				long integer,
				System.Text.Json.JsonSerializerOptions options) =>
				writer.WriteStringValue(integer.ToString());
		}

		public class UInt64JsonConverter : System.Text.Json.Serialization.JsonConverter<ulong>
		{
			public override ulong Read(
				ref System.Text.Json.Utf8JsonReader reader,
				Type typeToConvert,
				System.Text.Json.JsonSerializerOptions options) =>
				ulong.Parse(reader.GetString()!);

			public override void Write(
				System.Text.Json.Utf8JsonWriter writer,
				ulong integer,
				System.Text.Json.JsonSerializerOptions options) =>
				writer.WriteStringValue(integer.ToString());
		}
		static object ReadingFromPythonType_Link(ulong address, LocalMemoryReadingTools memoryReadingTools)
		{
			var pythonObjectTypeName = memoryReadingTools.GetPythonTypeNameFromPythonObjectAddress(address);
			var linkMemory = memoryReadingTools.memoryReader.ReadBytes(address, 0x40);
			if (linkMemory is null)
				return null;
			var linkMemoryAsLongMemory = TransformMemoryContent.AsULongMemory(linkMemory);
			/*
			 * 2024-05-26 observed a reference to a dictionary object at offset 6 * 4 bytes.
			 * */
			var firstDictReference =
				linkMemoryAsLongMemory
				.ToArray()
				.Where(reference =>
				{
					var referencedObjectTypeName = memoryReadingTools.GetPythonTypeNameFromPythonObjectAddress(reference);
					return referencedObjectTypeName is "dict";
				})
				.FirstOrDefault();
			if (firstDictReference is 0)
				return null;
			var dictEntries =
				memoryReadingTools.getDictionaryEntriesWithStringKeys(firstDictReference)
				?.ToImmutableDictionary(
					keySelector: dictEntry => dictEntry.Key,
					elementSelector: dictEntry => memoryReadingTools.GetDictEntryValueRepresentation(dictEntry.Value));
			return new UITreeNode{
				pythonObjectAddress= address,
				PythonObjectTypeName= pythonObjectTypeName,
				DictEntriesOfInterest= dictEntries,
				otherDictEntriesKeys= null,
				children= null};
		}

		class MemoryReadingCache
		{
			IDictionary<ulong, string> PythonTypeNameFromPythonObjectAddress;

			IDictionary<ulong, string> PythonStringValueMaxLength4000;

			IDictionary<ulong, object> DictEntryValueRepresentation;

			public MemoryReadingCache()
			{
				PythonTypeNameFromPythonObjectAddress = new Dictionary<ulong, string>();
				PythonStringValueMaxLength4000 = new Dictionary<ulong, string>();
				DictEntryValueRepresentation = new Dictionary<ulong, object>();
			}

			public string GetPythonTypeNameFromPythonObjectAddress(ulong address, Func<ulong, string> getFresh) =>
				GetFromCacheOrUpdate(PythonTypeNameFromPythonObjectAddress, address, getFresh);

			public string GetPythonStringValueMaxLength4000(ulong address, Func<ulong, string> getFresh) =>
				GetFromCacheOrUpdate(PythonStringValueMaxLength4000, address, getFresh);

			public object GetDictEntryValueRepresentation(ulong address, Func<ulong, object> getFresh) =>
				GetFromCacheOrUpdate(DictEntryValueRepresentation, address, getFresh);

			static TValue GetFromCacheOrUpdate<TKey, TValue>(IDictionary<TKey, TValue> cache, TKey key, Func<TKey, TValue> getFresh)
			{
				if (cache.TryGetValue(key, out var fromCache))
					return fromCache;

				var fresh = getFresh(key);

				cache[key] = fresh;
				return fresh;
			}
		}

		static UITreeNode ReadUITreeFromAddress(ulong nodeAddress, IMemoryReader memoryReader, int maxDepth,
			MemoryReadingCache cache)
		{
			cache ??= new MemoryReadingCache();

			var uiNodeObjectMemory = memoryReader.ReadBytes(nodeAddress, 0x30);

			if (!(0x30 == uiNodeObjectMemory?.Length))
				return null;

			string getPythonTypeNameFromPythonTypeObjectAddress(ulong typeObjectAddress)
			{
				var typeObjectMemory = memoryReader.ReadBytes(typeObjectAddress, 0x20);

				if (!(typeObjectMemory?.Length == 0x20))
					return null;

				var tp_name = BitConverter.ToUInt64(typeObjectMemory[0x18..]);

				var nameBytes = memoryReader.ReadBytes(tp_name, 100)?.ToArray();

				if (!(nameBytes?.Contains((byte)0) ?? false))
					return null;

				return System.Text.Encoding.ASCII.GetString(nameBytes.TakeWhile(character => character != 0).ToArray());
			}

			string getPythonTypeNameFromPythonObjectAddress(ulong objectAddress)
			{
				return cache.GetPythonTypeNameFromPythonObjectAddress(objectAddress, objectAddress =>
				{
					var objectMemory = memoryReader.ReadBytes(objectAddress, 0x10);

					if (!(objectMemory?.Length == 0x10))
						return null;

					return getPythonTypeNameFromPythonTypeObjectAddress(BitConverter.ToUInt64(objectMemory[8..]));
				});
			}

			string readPythonStringValueMaxLength4000(ulong strObjectAddress)
			{
				return cache.GetPythonStringValueMaxLength4000(
					strObjectAddress,
					strObjectAddress => ReadPythonStringValue(strObjectAddress, memoryReader, 4000));
			}

			PyDictEntry[] ReadActiveDictionaryEntriesFromDictionaryAddress(ulong dictionaryAddress)
			{
				/*
				Sources:
				https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Include/dictobject.h
				https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Objects/dictobject.c
				*/

				var dictMemory = memoryReader.ReadBytes(dictionaryAddress, 0x30);

				//  Console.WriteLine($"dictMemory is {(dictMemory == null ? "not " : "")}ok for 0x{dictionaryAddress:X}");

				if (!(dictMemory?.Length == 0x30))
					return null;

				var dictMemoryAsLongMemory = TransformMemoryContent.AsULongMemory(dictMemory);

				//  var dictTypeName = getPythonTypeNameFromObjectAddress(dictionaryAddress);

				//  Console.WriteLine($"Type name for dictionary 0x{dictionaryAddress:X} is '{dictTypeName}'.");

				//  https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Include/dictobject.h#L60-L89

				var ma_fill = dictMemoryAsLongMemory.Span[2];
				var ma_used = dictMemoryAsLongMemory.Span[3];
				var ma_mask = dictMemoryAsLongMemory.Span[4];
				var ma_table = dictMemoryAsLongMemory.Span[5];

				//  Console.WriteLine($"Details for dictionary 0x{dictionaryAddress:X}: type_name = '{dictTypeName}' ma_mask = 0x{ma_mask:X}, ma_table = 0x{ma_table:X}.");

				var numberOfSlots = (int)ma_mask + 1;

				if (numberOfSlots < 0 || 10_000 < numberOfSlots)
				{
					//  Avoid stalling the whole reading process when a single dictionary contains garbage.
					return null;
				}

				var slotsMemorySize = numberOfSlots * 8 * 3;

				var slotsMemory = memoryReader.ReadBytes(ma_table, slotsMemorySize);

				//  Console.WriteLine($"slotsMemory (0x{ma_table:X}) has length of {slotsMemory?.Length} and is {(slotsMemory?.Length == slotsMemorySize ? "" : "not ")}ok for 0x{dictionaryAddress:X}");

				if (!(slotsMemory?.Length == slotsMemorySize))
					return null;

				var slotsMemoryAsLongMemory = TransformMemoryContent.AsULongMemory(slotsMemory);

				var entries = new List<PyDictEntry>();

				for (var slotIndex = 0; slotIndex < numberOfSlots; ++slotIndex)
				{
					var hash = slotsMemoryAsLongMemory.Span[slotIndex * 3];
					var key = slotsMemoryAsLongMemory.Span[slotIndex * 3 + 1];
					var value = slotsMemoryAsLongMemory.Span[slotIndex * 3 + 2];

					if (key == 0 || value == 0)
						continue;

					entries.Add(new PyDictEntry { hash = hash, key = key, value = value });
				}

				return [.. entries];
			}

			IImmutableDictionary<string, ulong> GetDictionaryEntriesWithStringKeys(ulong dictionaryObjectAddress)
			{
				var dictionaryEntries = ReadActiveDictionaryEntriesFromDictionaryAddress(dictionaryObjectAddress);

				if (dictionaryEntries == null)
					return null;

				return
					dictionaryEntries
						.Select(entry => new
							{ key = readPythonStringValueMaxLength4000(entry.key), value = entry.value })
						.Aggregate(
							seed: ImmutableDictionary<string, ulong>.Empty,
							func: (dict, entry) => dict.SetItem(entry.key, entry.value));
			}

			var localMemoryReadingTools = new LocalMemoryReadingTools
			{
				memoryReader = memoryReader,
				getDictionaryEntriesWithStringKeys = GetDictionaryEntriesWithStringKeys,
				GetPythonTypeNameFromPythonObjectAddress = getPythonTypeNameFromPythonObjectAddress,
			};

			var pythonObjectTypeName = getPythonTypeNameFromPythonObjectAddress(nodeAddress);

			if (!(0 < pythonObjectTypeName?.Length))
				return null;

			var dictAddress = BitConverter.ToUInt64(uiNodeObjectMemory[0x10..]);

			var dictionaryEntries = ReadActiveDictionaryEntriesFromDictionaryAddress(dictAddress);

			if (dictionaryEntries == null)
				return null;

			var dictEntriesOfInterest = new List<UITreeNode.DictEntry>();

			var otherDictEntriesKeys = new List<string>();

			object GetDictEntryValueRepresentation(ulong valueOjectAddress)
			{
				return cache.GetDictEntryValueRepresentation(valueOjectAddress, valueOjectAddress =>
				{
					var value_pythonTypeName = getPythonTypeNameFromPythonObjectAddress(valueOjectAddress);

					var genericRepresentation = new UITreeNode.DictEntryValueGenericRepresentation
					{
						address = valueOjectAddress,
						pythonObjectTypeName = value_pythonTypeName
					};

					if (value_pythonTypeName == null)
						return genericRepresentation;

					specializedReadingFromPythonType.TryGetValue(value_pythonTypeName,
						out var specializedRepresentation);

					if (specializedRepresentation == null)
						return genericRepresentation;

					return specializedRepresentation(genericRepresentation.address, localMemoryReadingTools);
				});
			}

			localMemoryReadingTools.GetDictEntryValueRepresentation = GetDictEntryValueRepresentation;

			foreach (var dictionaryEntry in dictionaryEntries)
			{
				var keyObject_type_name = getPythonTypeNameFromPythonObjectAddress(dictionaryEntry.key);

				//  Console.WriteLine($"Dict entry type name is '{keyObject_type_name}'");

				if (keyObject_type_name != "str")
					continue;

				var keyString = readPythonStringValueMaxLength4000(dictionaryEntry.key);

				if (!DictEntriesOfInterestKeys.Contains(keyString))
				{
					otherDictEntriesKeys.Add(keyString);
					continue;
				}

				dictEntriesOfInterest.Add(new UITreeNode.DictEntry
				{
					key = keyString,
					value = GetDictEntryValueRepresentation(dictionaryEntry.value)
				});
			}

			{
				var _displayDictEntry = dictEntriesOfInterest.FirstOrDefault(entry => entry.key == "_display");

				if (_displayDictEntry != null && (_displayDictEntry.value is bool displayAsBool))
					if (!displayAsBool)
						return null;
			}

			UITreeNode[] ReadChildren()
			{
				if (maxDepth < 1)
					return null;

				//  https://github.com/Arcitectus/Sanderling/blob/b07769fb4283e401836d050870121780f5f37910/guide/image/2015-01.eve-online-python-ui-tree-structure.png

				var childrenDictEntry = dictEntriesOfInterest.FirstOrDefault(entry => entry.key == "children");

				if (childrenDictEntry == null)
					return null;

				var childrenEntryObjectAddress =
					((UITreeNode.DictEntryValueGenericRepresentation)childrenDictEntry.value).address;

				//  Console.WriteLine($"'children' dict entry of 0x{nodeAddress:X} points to 0x{childrenEntryObjectAddress:X}.");

				var pyChildrenListMemory = memoryReader.ReadBytes(childrenEntryObjectAddress, 0x18);

				if (!(pyChildrenListMemory?.Length == 0x18))
					return null;

				var pyChildrenDictAddress = BitConverter.ToUInt64(pyChildrenListMemory[0x10..]);

				var pyChildrenDictEntries = ReadActiveDictionaryEntriesFromDictionaryAddress(pyChildrenDictAddress);

				//  Console.WriteLine($"Found {(pyChildrenDictEntries == null ? "no" : "some")} children dictionary entries for 0x{nodeAddress:X}");

				if (pyChildrenDictEntries == null)
					return null;

				var childrenEntry =
					pyChildrenDictEntries
						.FirstOrDefault(dictionaryEntry =>
						{
							if (getPythonTypeNameFromPythonObjectAddress(dictionaryEntry.key) != "str")
								return false;

							var keyString = readPythonStringValueMaxLength4000(dictionaryEntry.key);

							return keyString == "_childrenObjects";
						});

				//  Console.WriteLine($"Found {(childrenEntry.value == 0 ? "no" : "a")} dictionary entry for children of 0x{nodeAddress:X}");

				if (childrenEntry.value == 0)
					return null;

				var pythonListObjectMemory = memoryReader.ReadBytes(childrenEntry.value, 0x20);

				if (!(pythonListObjectMemory?.Length == 0x20))
					return null;

				//  https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Include/listobject.h

				var list_ob_size = BitConverter.ToUInt64(pythonListObjectMemory[0x10..]);

				if (4000 < list_ob_size)
					return null;

				var listEntriesSize = (int)list_ob_size * 8;

				var list_ob_item = BitConverter.ToUInt64(pythonListObjectMemory[0x18..]);

				var listEntriesMemory = memoryReader.ReadBytes(list_ob_item, listEntriesSize);

				if (!(listEntriesMemory?.Length == listEntriesSize))
					return null;

				var listEntries = TransformMemoryContent.AsULongMemory(listEntriesMemory);

				//  Console.WriteLine($"Found {listEntries.Length} children entries for 0x{nodeAddress:X}: " + String.Join(", ", listEntries.Select(childAddress => $"0x{childAddress:X}").ToArray()));

				return
					listEntries
						.ToArray()
						.Select(childAddress => ReadUITreeFromAddress(childAddress, memoryReader, maxDepth - 1, cache))
						.ToArray();
			}

			var dictEntriesOfInterestLessNoneType =
				dictEntriesOfInterest
					.Where(entry =>
						!((entry.value as UITreeNode.DictEntryValueGenericRepresentation)?.pythonObjectTypeName ==
						  "NoneType"))
					.ToArray();

			var dictEntriesOfInterestDict =
				dictEntriesOfInterestLessNoneType.Aggregate(
					seed: ImmutableDictionary<string, object>.Empty,
					func: (dict, entry) => dict.SetItem(entry.key, entry.value));

			return new UITreeNode
			{

				pythonObjectAddress = nodeAddress,
				PythonObjectTypeName = pythonObjectTypeName,
				DictEntriesOfInterest = dictEntriesOfInterestDict,
				otherDictEntriesKeys = [.. otherDictEntriesKeys],
				children = ReadChildren()?.Where(child => child != null)?.ToArray()
			};
		}

		static string ReadPythonStringValue(ulong stringObjectAddress, IMemoryReader memoryReader, int maxLength)
		{
			//  https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Include/stringobject.h

			var stringObjectMemory = memoryReader.ReadBytes(stringObjectAddress, 0x20);

			if (!(stringObjectMemory?.Length == 0x20))
				return "Failed to read string object memory.";

			var stringObject_ob_size = BitConverter.ToUInt64(stringObjectMemory, 0x10);

			if (0 < maxLength && maxLength < (int)stringObject_ob_size || int.MaxValue < stringObject_ob_size)
				return "String too long.";

			var stringBytes = memoryReader.ReadBytes(stringObjectAddress + 8 * 4, (int)stringObject_ob_size);

			if (!(stringBytes?.Length == (int)stringObject_ob_size))
				return "Failed to read string bytes.";

			return System.Text.Encoding.ASCII.GetString(stringBytes, 0, stringBytes.Length);
		}

		static public UITreeNode ReadUITreeFromAddress(ulong nodeAddress, IMemoryReader memoryReader, int maxDepth) =>
			ReadUITreeFromAddress(nodeAddress, memoryReader, maxDepth, null);

		static public IImmutableList<ulong> EnumeratePossibleAddressesForUIRootObjectsFromProcessId(int processId)
		{
			var memoryReader = new MemoryReaderFromLiveProcess(processId);

			var (committedMemoryRegions, _) = ReadCommittedMemoryRegionsWithoutContentFromProcessId(processId);

			return EnumeratePossibleAddressesForUIRootObjects(committedMemoryRegions, memoryReader);
		}

		static public (IImmutableList<SampleMemoryRegion> memoryRegions, IImmutableList<string> logEntries) ReadCommittedMemoryRegionsWithContentFromProcessId(int processId)
		{
			var genericResult = ReadCommittedMemoryRegionsFromProcessId(processId, readContent: true);

			return genericResult;
		}

		static public (IImmutableList<(ulong baseAddress, int length)> memoryRegions, IImmutableList<string> logEntries) ReadCommittedMemoryRegionsWithoutContentFromProcessId(int processId)
		{
			var genericResult = ReadCommittedMemoryRegionsFromProcessId(processId, readContent: false);

			var memoryRegions =
				genericResult.memoryRegions
				.Select(memoryRegion => (baseAddress: memoryRegion.baseAddress, length: (int)memoryRegion.length))
				.ToImmutableList();

			return (memoryRegions, genericResult.logEntries);
		}
		public record SampleMemoryRegion(
			ulong baseAddress,
			ulong length,
			ReadOnlyMemory<byte>? content);

		static public (IImmutableList<SampleMemoryRegion> memoryRegions, IImmutableList<string> logEntries) ReadCommittedMemoryRegionsFromProcessId(
			int processId,
			bool readContent)
		{
			var logEntries = new List<string>();

			void logLine(string lineText)
			{
				logEntries.Add(lineText);
				Console.WriteLine(lineText);
			}

			logLine("Reading from process " + processId + ".");

			var processHandle = WinApi.OpenProcess(
				(int)(WinApi.ProcessAccessFlags.QueryInformation | WinApi.ProcessAccessFlags.VirtualMemoryRead), false, processId);

			long address = 0;

			var committedRegions = new List<SampleMemoryRegion>();

			do
			{
				int result = WinApi.VirtualQueryEx(
					processHandle,
					(IntPtr)address,
					out WinApi.MEMORY_BASIC_INFORMATION64 m,
					(uint)Marshal.SizeOf(typeof(WinApi.MEMORY_BASIC_INFORMATION64)));

				var regionProtection = (WinApi.MemoryInformationProtection)m.Protect;

				logLine($"{m.BaseAddress}-{(uint)m.BaseAddress + (uint)m.RegionSize - 1} : {m.RegionSize} bytes result={result}, state={(WinApi.MemoryInformationState)m.State}, type={(WinApi.MemoryInformationType)m.Type}, protection={regionProtection}");

				if (address == (long)m.BaseAddress + (long)m.RegionSize)
					break;

				address = (long)m.BaseAddress + (long)m.RegionSize;

				if (m.State != (int)WinApi.MemoryInformationState.MEM_COMMIT)
					continue;

				var protectionFlagsToSkip = WinApi.MemoryInformationProtection.PAGE_GUARD | WinApi.MemoryInformationProtection.PAGE_NOACCESS;

				var matchingFlagsToSkip = protectionFlagsToSkip & regionProtection;

				if (matchingFlagsToSkip != 0)
				{
					logLine($"Skipping region beginning at {m.BaseAddress:X} as it has flags {matchingFlagsToSkip}.");
					continue;
				}

				var regionBaseAddress = m.BaseAddress;

				byte[] regionContent = null;

				if (readContent)
				{
					UIntPtr bytesRead = UIntPtr.Zero;
					var regionContentBuffer = new byte[(long)m.RegionSize];

					WinApi.ReadProcessMemory(processHandle, regionBaseAddress, regionContentBuffer, (UIntPtr)regionContentBuffer.LongLength, ref bytesRead);

					if (bytesRead.ToUInt64() != (ulong)regionContentBuffer.LongLength)
						throw new Exception($"Failed to ReadProcessMemory at 0x{regionBaseAddress:X}: Only read " + bytesRead + " bytes.");

					regionContent = regionContentBuffer;
				}

				committedRegions.Add(new SampleMemoryRegion(
					baseAddress: regionBaseAddress,
					length: m.RegionSize,
					content: regionContent));

			} while (true);

			logLine($"Found {committedRegions.Count} committed regions with a total size of {committedRegions.Select(region => (long)region.length).Sum()}.");

			return (committedRegions.ToImmutableList(), logEntries.ToImmutableList());
		}

		public class MemoryReaderFromLiveProcess : IMemoryReader, IDisposable
		{
			readonly IntPtr processHandle;

			public MemoryReaderFromLiveProcess(int processId)
			{
				processHandle = WinApi.OpenProcess(
					(int)(WinApi.ProcessAccessFlags.QueryInformation | WinApi.ProcessAccessFlags.VirtualMemoryRead), false, processId);
			}

			public void Dispose()
			{
				if (processHandle != IntPtr.Zero)
					WinApi.CloseHandle(processHandle);
			}

			public byte[]? ReadBytes(ulong startAddress, int length)
			{
				var buffer = new byte[length];

				UIntPtr numberOfBytesReadAsPtr = UIntPtr.Zero;

				if (!WinApi.ReadProcessMemory(processHandle, startAddress, buffer, (UIntPtr)buffer.LongLength, ref numberOfBytesReadAsPtr))
					return null;

				var numberOfBytesRead = numberOfBytesReadAsPtr.ToUInt64();

				if (numberOfBytesRead == 0)
					return null;

				if (int.MaxValue < numberOfBytesRead)
					return null;

				if (numberOfBytesRead == (ulong)buffer.LongLength)
					return buffer;

				return buffer;
			}
		}

		static public IImmutableList<ulong> EnumeratePossibleAddressesForUIRootObjects(
			IEnumerable<(ulong baseAddress, int length)> memoryRegions,
			IMemoryReader memoryReader)
		{
			var memoryRegionsOrderedByAddress =
				memoryRegions
				.OrderBy(memoryRegion => memoryRegion.baseAddress)
				.ToImmutableArray();

			string? ReadNullTerminatedAsciiStringFromAddressUpTo255(ulong address)
			{
				var asMemory = memoryReader.ReadBytes(address, 0x100);

				if (asMemory == null)
					return null;

				var asSpan = asMemory.AsSpan();

				var length = 0;

				for (var i = 0; i < asSpan.Length; ++i)
				{
					length = i;

					if (asSpan[i] == 0)
						break;
				}

				return System.Text.Encoding.ASCII.GetString(asSpan[..length]);
			}

			ReadOnlyMemory<ulong>? ReadMemoryRegionContentAsULongArray((ulong baseAddress, int length) memoryRegion)
			{
				var asByteArray = memoryReader.ReadBytes(memoryRegion.baseAddress, memoryRegion.length);

				if (asByteArray == null)
					return null;

				return TransformMemoryContent.AsULongMemory(asByteArray);
			}

			IEnumerable<ulong> EnumerateCandidatesForPythonTypeObjectType()
			{
				IEnumerable<ulong> EnumerateCandidatesForPythonTypeObjectTypeInMemoryRegion((ulong baseAddress, int length) memoryRegion)
				{
					var memoryRegionContentAsULongArray = ReadMemoryRegionContentAsULongArray(memoryRegion);

					if (memoryRegionContentAsULongArray == null)
						yield break;

					for (var candidateAddressIndex = 0; candidateAddressIndex < memoryRegionContentAsULongArray.Value.Length - 4; ++candidateAddressIndex)
					{
						var candidateAddressInProcess = memoryRegion.baseAddress + (ulong)candidateAddressIndex * 8;

						var candidate_ob_type = memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 1];

						if (candidate_ob_type != candidateAddressInProcess)
							continue;

						var candidate_tp_name =
							ReadNullTerminatedAsciiStringFromAddressUpTo255(
								memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 3]);

						if (candidate_tp_name != "type")
							continue;

						yield return candidateAddressInProcess;
					}
				}

				return
					memoryRegionsOrderedByAddress
					.AsParallel()
					//.WithDegreeOfParallelism(2)
					.SelectMany(EnumerateCandidatesForPythonTypeObjectTypeInMemoryRegion)
					.ToImmutableArray();
			}

			IEnumerable<(ulong address, string tp_name)> EnumerateCandidatesForPythonTypeObjects(
				IImmutableList<ulong> typeObjectCandidatesAddresses)
			{
				if (typeObjectCandidatesAddresses.Count < 1)
					yield break;

				var typeAddressMin = typeObjectCandidatesAddresses.Min();
				var typeAddressMax = typeObjectCandidatesAddresses.Max();

				foreach (var memoryRegion in memoryRegionsOrderedByAddress)
				{
					var memoryRegionContentAsULongArray = ReadMemoryRegionContentAsULongArray(memoryRegion);

					if (memoryRegionContentAsULongArray == null)
						continue;

					for (var candidateAddressIndex = 0; candidateAddressIndex < memoryRegionContentAsULongArray.Value.Length - 4; ++candidateAddressIndex)
					{
						var candidateAddressInProcess = memoryRegion.baseAddress + (ulong)candidateAddressIndex * 8;

						var candidate_ob_type = memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 1];

						{
							//  This check is redundant with the following one. It just implements a specialization to optimize runtime expenses.
							if (candidate_ob_type < typeAddressMin || typeAddressMax < candidate_ob_type)
								continue;
						}

						if (!typeObjectCandidatesAddresses.Contains(candidate_ob_type))
							continue;

						var candidate_tp_name =
							ReadNullTerminatedAsciiStringFromAddressUpTo255(
								memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 3]);

						if (candidate_tp_name == null)
							continue;

						yield return (candidateAddressInProcess, candidate_tp_name);
					}
				}
			}

			IEnumerable<ulong> EnumerateCandidatesForInstancesOfPythonType(
				IImmutableList<ulong> typeObjectCandidatesAddresses)
			{
				if (typeObjectCandidatesAddresses.Count < 1)
					yield break;

				var typeAddressMin = typeObjectCandidatesAddresses.Min();
				var typeAddressMax = typeObjectCandidatesAddresses.Max();

				foreach (var memoryRegion in memoryRegionsOrderedByAddress)
				{
					var memoryRegionContentAsULongArray = ReadMemoryRegionContentAsULongArray(memoryRegion);

					if (memoryRegionContentAsULongArray == null)
						continue;

					for (var candidateAddressIndex = 0; candidateAddressIndex < memoryRegionContentAsULongArray.Value.Length - 4; ++candidateAddressIndex)
					{
						var candidateAddressInProcess = memoryRegion.baseAddress + (ulong)candidateAddressIndex * 8;

						var candidate_ob_type = memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 1];

						{
							//  This check is redundant with the following one. It just implements a specialization to reduce processing time.
							if (candidate_ob_type < typeAddressMin || typeAddressMax < candidate_ob_type)
								continue;
						}

						if (!typeObjectCandidatesAddresses.Contains(candidate_ob_type))
							continue;

						yield return candidateAddressInProcess;
					}
				}
			}

			var uiRootTypeObjectCandidatesAddresses =
				EnumerateCandidatesForPythonTypeObjects(EnumerateCandidatesForPythonTypeObjectType().ToImmutableList())
				.Where(typeObject => typeObject.tp_name == "UIRoot")
				.Select(typeObject => typeObject.address)
				.ToImmutableList();

			Console.WriteLine($"Found {uiRootTypeObjectCandidatesAddresses.Count} candidate addresses");
			return
				EnumerateCandidatesForInstancesOfPythonType(uiRootTypeObjectCandidatesAddresses)
				.ToImmutableList();
		}

		static public class WinApi
		{
			[DllImport("kernel32.dll")]
			static public extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

			[DllImport("kernel32.dll")]
			static public extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, uint dwLength);

			[DllImport("kernel32.dll")]
			static public extern bool ReadProcessMemory(IntPtr hProcess, ulong lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, ref UIntPtr lpNumberOfBytesRead);

			[DllImport("kernel32.dll", SetLastError = true)]
			static public extern bool CloseHandle(IntPtr hHandle);

			[DllImport("user32.dll", SetLastError = true)]
			static public extern bool SetProcessDPIAware();

			[DllImport("user32.dll")]
			static public extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

			[DllImport("user32.dll")]
			static public extern IntPtr GetClientRect(IntPtr hWnd, ref Rect rect);

			[DllImport("user32.dll")]
			[return: MarshalAs(UnmanagedType.Bool)]
			static public extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

			[StructLayout(LayoutKind.Sequential)]
			public struct Rect
			{
				public int left;
				public int top;
				public int right;
				public int bottom;
			}

			[StructLayout(LayoutKind.Sequential)]
			public struct Point
			{
				public int x;

				public int y;
			}

			//  http://www.pinvoke.net/default.aspx/kernel32.virtualqueryex
			//  https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-memory_basic_information
			[StructLayout(LayoutKind.Sequential)]
			public struct MEMORY_BASIC_INFORMATION64
			{
				public ulong BaseAddress;
				public ulong AllocationBase;
				public int AllocationProtect;
				public int __alignment1;
				public ulong RegionSize;
				public int State;
				public int Protect;
				public int Type;
				public int __alignment2;
			}

			public enum AllocationProtect : uint
			{
				PAGE_EXECUTE = 0x00000010,
				PAGE_EXECUTE_READ = 0x00000020,
				PAGE_EXECUTE_READWRITE = 0x00000040,
				PAGE_EXECUTE_WRITECOPY = 0x00000080,
				PAGE_NOACCESS = 0x00000001,
				PAGE_READONLY = 0x00000002,
				PAGE_READWRITE = 0x00000004,
				PAGE_WRITECOPY = 0x00000008,
				PAGE_GUARD = 0x00000100,
				PAGE_NOCACHE = 0x00000200,
				PAGE_WRITECOMBINE = 0x00000400
			}

			[Flags]
			public enum ProcessAccessFlags : uint
			{
				All = 0x001F0FFF,
				Terminate = 0x00000001,
				CreateThread = 0x00000002,
				VirtualMemoryOperation = 0x00000008,
				VirtualMemoryRead = 0x00000010,
				VirtualMemoryWrite = 0x00000020,
				DuplicateHandle = 0x00000040,
				CreateProcess = 0x000000080,
				SetQuota = 0x00000100,
				SetInformation = 0x00000200,
				QueryInformation = 0x00000400,
				QueryLimitedInformation = 0x00001000,
				Synchronize = 0x00100000
			}

			//  https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-memory_basic_information
			public enum MemoryInformationState : int
			{
				MEM_COMMIT = 0x1000,
				MEM_FREE = 0x10000,
				MEM_RESERVE = 0x2000,
			}

			//  https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-memory_basic_information
			public enum MemoryInformationType : int
			{
				MEM_IMAGE = 0x1000000,
				MEM_MAPPED = 0x40000,
				MEM_PRIVATE = 0x20000,
			}

			//  https://docs.microsoft.com/en-au/windows/win32/memory/memory-protection-constants
			[Flags]
			public enum MemoryInformationProtection : int
			{
				PAGE_EXECUTE = 0x10,
				PAGE_EXECUTE_READ = 0x20,
				PAGE_EXECUTE_READWRITE = 0x40,
				PAGE_EXECUTE_WRITECOPY = 0x80,
				PAGE_NOACCESS = 0x01,
				PAGE_READONLY = 0x02,
				PAGE_READWRITE = 0x04,
				PAGE_WRITECOPY = 0x08,
				PAGE_TARGETS_INVALID = 0x40000000,
				PAGE_TARGETS_NO_UPDATE = 0x40000000,

				PAGE_GUARD = 0x100,
				PAGE_NOCACHE = 0x200,
				PAGE_WRITECOMBINE = 0x400,
			}
		}

	}

	public interface IMemoryReader
	{
		byte[]? ReadBytes(ulong startAddress, int length);
	}

	public class MemoryReaderFromProcessSample : IMemoryReader
	{
		readonly IImmutableList<(ulong baseAddress, byte[] content)> memoryRegionsOrderedByAddress;

		public MemoryReaderFromProcessSample(IImmutableList<(ulong baseAddress, byte[] content)> memoryRegions)
		{
			memoryRegionsOrderedByAddress =
				memoryRegions
				.OrderBy(memoryRegion => memoryRegion.baseAddress)
				.ToImmutableList();
		}

		public byte[]? ReadBytes(ulong startAddress, int length)
		{
			var memoryRegion =
				memoryRegionsOrderedByAddress
				.Where(c => c.baseAddress <= startAddress)
				.OrderBy(c => c.baseAddress)
				.LastOrDefault();

			if (memoryRegion.content == null)
				return null;

			return
				memoryRegion.content
				.Skip((int)(startAddress - memoryRegion.baseAddress))
				.Take(length)
				.ToArray();
		}
	}

	/// <summary>
	/// Offsets from https://docs.python.org/2/c-api/structures.html
	/// </summary>
	public class PyObject
	{
		public const int Offset_ob_refcnt = 0;
		public const int Offset_ob_type = 8;
	}

	/// <summary>
	/// Offsets from https://docs.python.org/2/c-api/typeobj.html
	/// </summary>
	public class PyTypeObject : PyObject
	{
	}

	static class TransformMemoryContent
	{
		static public ulong[] AsULongArray(byte[] byteArray)
		{
			var ulongArray = new ulong[byteArray.Length / 8];
			Buffer.BlockCopy(byteArray, 0, ulongArray, 0, ulongArray.Length * 8);
			return ulongArray;
		}
		static public ReadOnlyMemory<ulong> AsULongMemory(ReadOnlyMemory<byte> byteMemory) =>
			MemoryMarshal.Cast<byte, ulong>(byteMemory.Span).ToArray();
	}
}
