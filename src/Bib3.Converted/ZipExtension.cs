// Decompiled with JetBrains decompiler
// Type: Bib3.ZipExtension
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Bib3
{
  public static class ZipExtension
  {
    public static byte[] ReadListOctet(this ZipArchiveEntry entry)
    {
      if (entry == null)
        return (byte[]) null;
      using (Stream stream = entry.Open())
        return stream.LeeseGesamt();
    }

    public static IEnumerable<ZipArchiveEntry> EntryFromDirectory(
      this ZipArchive zipArchive,
      string directoryPath,
      StringComparison comparisonType = StringComparison.InvariantCulture)
    {
      if (zipArchive != null)
      {
        foreach (ZipArchiveEntry entry in zipArchive.Entries)
        {
          ZipArchiveEntry Entry = entry;
          string EntryVerzaicnisPfaad = Path.GetDirectoryName(Entry.FullName);
          if (string.Equals(EntryVerzaicnisPfaad, directoryPath, comparisonType))
          {
            yield return Entry;
            EntryVerzaicnisPfaad = (string) null;
            Entry = (ZipArchiveEntry) null;
          }
        }
      }
    }

    public static ZipArchiveEntry EntryCreateAndWrite(
      this ZipArchive zipArchive,
      string entryFullName,
      byte[] entryListOctet)
    {
      if (zipArchive == null)
        return (ZipArchiveEntry) null;
      ZipArchiveEntry entry = zipArchive.CreateEntry(entryFullName);
      if (entryListOctet != null)
      {
        using (Stream stream = entry.Open())
          stream.Write(entryListOctet, 0, entryListOctet.Length);
      }
      return entry;
    }
  }
}
