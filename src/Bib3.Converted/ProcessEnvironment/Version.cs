// Decompiled with JetBrains decompiler
// Type: Bib3.ProcessEnvironment.Version
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

namespace Bib3.ProcessEnvironment
{
  public class Version
  {
    public readonly int Major;
    public readonly short MajorRevision;
    public readonly int Minor;
    public readonly short MinorRevision;
    public readonly int Build;
    public readonly int Revision;

    public Version()
    {
    }

    public Version(
      int major,
      short majorRevision,
      int minor,
      short minorRevision,
      int build,
      int revision)
    {
      this.Major = major;
      this.MajorRevision = majorRevision;
      this.Minor = minor;
      this.MinorRevision = minorRevision;
      this.Build = build;
      this.Revision = revision;
    }
  }
}
