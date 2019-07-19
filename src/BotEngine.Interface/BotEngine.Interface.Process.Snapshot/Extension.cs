using Bib3;
using Bib3.FCL;
using BotEngine.WinApi;
using BotEngine.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace BotEngine.Interface.Process.Snapshot
{
	public static class Extension
	{
		private const string MainWindowEntryName = "MainWindow";

		private const string WindowClientRectRasterEntryName = "ClientRect.Raster";

		private const string MainWindowClientRectRasterEntryName = "MainWindow\\ClientRect.Raster";

		private const string ProcessRangeOfPagesEntryName = "Process\\RangeOfPages";

		private const string ProcessMemoryEntryName = "Process\\Memory";

		public static ProcessSnapshot ProcessSnapshotFromWindowHandle(IntPtr mainWindowHandle)
		{
			User32.GetWindowThreadProcessId(mainWindowHandle, out uint lpdwProcessId);
			RangeOfPages[] setRangeOfPages = BotEngine.Windows.Extension.ListRangeOfPagesFromProcessWithId(lpdwProcessId, pageContentRead: true);
			System.Diagnostics.Process processById = System.Diagnostics.Process.GetProcessById((int)lpdwProcessId);
			return new ProcessSnapshot(setRangeOfPages);
		}

		public static WindowSnapshot WindowSnapshotFromWindowHandle(IntPtr mainWindowHandle)
		{
			KeyValuePair<uint[], int> clientRectRaster = BotEngine.Windows.Extension.Raster32BitVonClientRectVonWindowMitHandle(mainWindowHandle);
			return new WindowSnapshot(clientRectRaster);
		}

		public static Snapshot SnapshotFromWindowHandle(IntPtr mainWindowHandle)
		{
			return new Snapshot(ProcessSnapshotFromWindowHandle(mainWindowHandle), WindowSnapshotFromWindowHandle(mainWindowHandle));
		}

		public static string MemoryEntryName(long baseAddress)
		{
			return "0x" + baseAddress.ToString("X");
		}

		public static string MemoryEntryName(this RangeOfPages rangeOfPages)
		{
			return (rangeOfPages == null) ? null : MemoryEntryName((long)rangeOfPages.BasicInfo.BaseAddress);
		}

		public static long? BaseAddressFromMemoryEntryName(string memoryEntryName)
		{
			if (memoryEntryName == null)
			{
				return null;
			}
			Match match = Regex.Match(memoryEntryName, "0x([\\d\\w]+)");
			if (!match.Success)
			{
				return null;
			}
			return match.Groups[1].Value?.TryParseInt64(NumberStyles.HexNumber);
		}

		public static byte[] AsZipArchive(this Snapshot snapshot)
		{
			if (snapshot == null)
			{
				return null;
			}
			MemoryStream memoryStream = new MemoryStream();
			RangeOfPages[] obj = snapshot?.ProcessSnapshot?.RangeOfPages?.Select((RangeOfPages rangeOfPages) => new RangeOfPages(rangeOfPages.BasicInfo))?.ToArray();
			byte[] entryListOctet = obj.SerializeToUtf8();
			ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true);
			try
			{
				zipArchive.EntryCreateAndWrite("Process\\RangeOfPages", entryListOctet);
				snapshot?.ProcessSnapshot?.MemoryBaseAddressAndListOctet?.ForEach(delegate(KeyValuePair<long, byte[]> memoryBaseAddressAndListOctet)
				{
					if (!memoryBaseAddressAndListOctet.Value.IsNullOrEmpty())
					{
						zipArchive.EntryCreateAndWrite("Process\\Memory\\" + MemoryEntryName(memoryBaseAddressAndListOctet.Key), memoryBaseAddressAndListOctet.Value);
					}
				});
				BitmapSource bitmapSource = Bib3.FCL.Extension.BitmapSourceB8G8R8A8FromRasterO8R8G8B8HeightOut(snapshot.MainWindowSnapshot.ClientRectRaster.Key, snapshot.MainWindowSnapshot.ClientRectRaster.Value, 4.0, 4.0);
				byte[] entryListOctet2 = bitmapSource.AsListOctetWindowMediaImagingPng();
				zipArchive.EntryCreateAndWrite("MainWindow\\ClientRect.Raster", entryListOctet2);
			}
			finally
			{
				if (zipArchive != null)
				{
					((IDisposable)zipArchive).Dispose();
				}
			}
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return memoryStream.LeeseGesamt();
		}

		public static Snapshot SnapshotFromZipArchive(this byte[] zipArchiveSerial)
		{
			using (ZipArchive zipArchive = new ZipArchive(new MemoryStream(zipArchiveSerial), ZipArchiveMode.Read))
			{
				IEnumerable<ZipArchiveEntry> enumerable = zipArchive.EntryFromDirectory("Process\\Memory", StringComparison.OrdinalIgnoreCase);
				RangeOfPages[] rangeOfPages = zipArchive.GetEntry("Process\\RangeOfPages")?.ReadListOctet()?.DeserializeFromUtf8<RangeOfPages[]>();
				return new Snapshot(new ProcessSnapshot(rangeOfPages, (enumerable?.Select((ZipArchiveEntry entry) => new
				{
					BaseAddress = BaseAddressFromMemoryEntryName(entry.Name),
					ListOctet = entry.ReadListOctet()
				})?.ToArray())?.Where(addressAndListOctet => addressAndListOctet.BaseAddress.HasValue && addressAndListOctet.ListOctet != null)?.Select(addressAndListOctet => new KeyValuePair<long, byte[]>(addressAndListOctet.BaseAddress ?? 0, addressAndListOctet.ListOctet))?.ToArray()), null);
			}
		}
	}
}
