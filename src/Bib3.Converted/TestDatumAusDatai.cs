// Decompiled with JetBrains decompiler
// Type: Bib3.TestDatumAusDatai
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bib3
{
  public static class TestDatumAusDatai
  {
    public static string[] AusVerzaicnisPfaadListeKomponente(string verzaicnisPfaad) => verzaicnisPfaad?.Split(new char[1]
    {
      Path.DirectorySeparatorChar
    }, StringSplitOptions.RemoveEmptyEntries);

    public static IEnumerable<KeyValuePair<string[], FileInfo>> AusVerzaicnisMengeDataiMitPfaadListeKomponente(
      this string verzaicnisPfaad,
      bool unterverzaicnisLaseAus = false,
      string searchPattern = null)
    {
      if (!verzaicnisPfaad.IsNullOrEmpty())
      {
        DirectoryInfo Verzaicnis = new DirectoryInfo(verzaicnisPfaad);
        string[] VerzaicnisPfaadListeKomponente = TestDatumAusDatai.AusVerzaicnisPfaadListeKomponente(verzaicnisPfaad);
        SearchOption searchOption = unterverzaicnisLaseAus ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
        IEnumerable<FileInfo> Enumerator = Verzaicnis.EnumerateFiles(searchPattern ?? "*", searchOption);
        foreach (FileInfo Datai in Enumerator)
        {
          string DataiPfaad = Datai.FullName;
          string[] DataiPfaadListeKomponente = TestDatumAusDatai.AusVerzaicnisPfaadListeKomponente(DataiPfaad);
          string[] DataiPfaadListeKomponenteTailmengeUnterVerzaicnis = ((IEnumerable<string>) DataiPfaadListeKomponente).Skip<string>(VerzaicnisPfaadListeKomponente.Length).ToArray<string>();
          yield return new KeyValuePair<string[], FileInfo>(DataiPfaadListeKomponenteTailmengeUnterVerzaicnis, Datai);
          DataiPfaad = (string) null;
          DataiPfaadListeKomponente = (string[]) null;
          DataiPfaadListeKomponenteTailmengeUnterVerzaicnis = (string[]) null;
        }
      }
    }

    public static IEnumerable<KeyValuePair<KeyValuePair<string[], FileInfo>, KeyValuePair<string, string>[]>> AusVerzaicnisMengeDataiMitPfaadListeKomponenteUndMengeParam(
      string verzaicnisPfaad)
    {
      IEnumerable<KeyValuePair<string[], FileInfo>> MengeDatai = verzaicnisPfaad.AusVerzaicnisMengeDataiMitPfaadListeKomponente();
      foreach (KeyValuePair<string[], FileInfo> keyValuePair in MengeDatai)
      {
        KeyValuePair<string[], FileInfo> Datai = keyValuePair;
        string DataiPfaad = Datai.Value.FullName;
        string DataiNaame = Datai.Value.Name;
        MatchCollection DataiNaameMengeParamMatch = Regex.Matches(DataiNaame, "([\\w\\d]+)=([\\w\\d\\.]+)", RegexOptions.IgnoreCase);
        KeyValuePair<string, string>[] MengeParam = DataiNaameMengeParamMatch.OfType<Match>().Select<Match, KeyValuePair<string, string>>((Func<Match, KeyValuePair<string, string>>) (match => new KeyValuePair<string, string>(match.Groups[1].Value, match.Groups[2].Value))).ToArray<KeyValuePair<string, string>>();
        string[] PfaadListeKomponenteOoneDataiNaame = ((IEnumerable<string>) Datai.Key).Take<string>(Datai.Key.Length - 1).ToArray<string>();
        yield return new KeyValuePair<KeyValuePair<string[], FileInfo>, KeyValuePair<string, string>[]>(Datai, MengeParam);
        DataiPfaad = (string) null;
        DataiNaame = (string) null;
        DataiNaameMengeParamMatch = (MatchCollection) null;
        MengeParam = (KeyValuePair<string, string>[]) null;
        PfaadListeKomponenteOoneDataiNaame = (string[]) null;
        Datai = new KeyValuePair<string[], FileInfo>();
      }
    }

    public static int ZuVerzaicnisMengeDataiCallbackDataiPfaadListeKomponenteUndInhaltUndMengeParam(
      string verzaicnisPfaad,
      Action<string[], byte[], KeyValuePair<string, string>[]> callbackDataiInhaltUndPfaadListeKomponenteUndMengeParam)
    {
      IEnumerable<KeyValuePair<KeyValuePair<string[], FileInfo>, KeyValuePair<string, string>[]>> keyValuePairs = TestDatumAusDatai.AusVerzaicnisMengeDataiMitPfaadListeKomponenteUndMengeParam(verzaicnisPfaad);
      int num = 0;
      foreach (KeyValuePair<KeyValuePair<string[], FileInfo>, KeyValuePair<string, string>[]> keyValuePair in keyValuePairs)
      {
        string[] key = keyValuePair.Key.Key;
        try
        {
          byte[] numArray = Glob.InhaltAusDataiMitPfaad(keyValuePair.Key.Value.FullName);
          callbackDataiInhaltUndPfaadListeKomponenteUndMengeParam(key, numArray, keyValuePair.Value);
          ++num;
        }
        catch (Exception ex)
        {
          throw new ApplicationException("DataiPfaad=" + string.Join(Path.DirectorySeparatorChar.ToString(), key), ex);
        }
      }
      return num;
    }

    public static void ZuTestIdentMengeVersioonMengeDataiCallbackDataiPfaadListeKomponenteUndInhaltUndMengeParam(
      string verzaicnisBehältnisMengeVersioonPfaad,
      string testId,
      Action<string[], byte[], KeyValuePair<string, string>[]> callbackDataiPfaadListeKomponenteUndInhaltUndMengeParam,
      IEnumerable<KeyValuePair<string, int>> mengeZuVersioonIdentAnzaalScrankeMin = null)
    {
      DirectoryInfo directoryInfo1 = new DirectoryInfo(verzaicnisBehältnisMengeVersioonPfaad);
      DirectoryInfo[] directoryInfoArray = directoryInfo1.Exists ? directoryInfo1.GetDirectories() : throw new ArgumentOutOfRangeException("VerzaicnisBehältnisMengeVersioon.Exists");
      Dictionary<string, int> dict = new Dictionary<string, int>();
      foreach (DirectoryInfo directoryInfo2 in directoryInfoArray)
      {
        int num = TestDatumAusDatai.ZuVerzaicnisMengeDataiCallbackDataiPfaadListeKomponenteUndInhaltUndMengeParam(directoryInfo2.FullName + Path.DirectorySeparatorChar.ToString() + testId, callbackDataiPfaadListeKomponenteUndInhaltUndMengeParam);
        string lower = directoryInfo2.Name.ToLower();
        dict[lower] = num;
      }
      if (mengeZuVersioonIdentAnzaalScrankeMin == null)
        return;
      foreach (KeyValuePair<string, int> keyValuePair in mengeZuVersioonIdentAnzaalScrankeMin)
      {
        string lower = keyValuePair.Key.ToLower();
        int? valueNullable = dict.TryGetValueNullable<string, int>(lower);
        int num = keyValuePair.Value;
        int? nullable = valueNullable;
        int valueOrDefault = nullable.GetValueOrDefault();
        if ((num <= valueOrDefault ? (nullable.HasValue ? 1 : 0) : 0) == 0)
        {
          string[] strArray = new string[7];
          strArray[0] = "Anzaal (";
          nullable = valueNullable;
          strArray[1] = (nullable ?? 0).ToString();
          strArray[2] = ") Datum zu Test[";
          strArray[3] = testId;
          strArray[4] = "] in Versioon \"";
          strArray[5] = keyValuePair.Key;
          strArray[6] = "\" zu gering";
          throw new ApplicationException(string.Concat(strArray));
        }
      }
    }

    public static void ZuTestIdentMengeVersioonMengeDataiCallbackDataiSictCallbackTest<T>(
      string verzaicnisBehältnisMengeVersioonPfaad,
      string testId,
      Func<byte[], T> dataiSict,
      Action<T> test,
      IEnumerable<KeyValuePair<string, int>> mengeZuVersioonIdentAnzaalScrankeMin = null)
    {
      TestDatumAusDatai.ZuTestIdentMengeVersioonMengeDataiCallbackDataiPfaadListeKomponenteUndInhaltUndMengeParam(verzaicnisBehältnisMengeVersioonPfaad, testId, (Action<string[], byte[], KeyValuePair<string, string>[]>) ((dataiPfaadListeKomponente, dataiInhalt, listeParam) => test(dataiSict(dataiInhalt))), mengeZuVersioonIdentAnzaalScrankeMin);
    }
  }
}
