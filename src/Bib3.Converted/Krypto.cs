// Decompiled with JetBrains decompiler
// Type: Bib3.Krypto
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace Bib3
{
  public class Krypto
  {
    public static ulong ZuufalUInt64(RandomNumberGenerator source, ulong maximum = 18446744073709551615)
    {
      byte[] data = new byte[8];
      source.GetBytes(data);
      return BitConverter.ToUInt64(data, 0) % maximum;
    }

    public static uint FindFaktor(ulong produkt, uint suuceBegin = 0)
    {
      uint num = (uint) Math.Sqrt((double) produkt);
      if (produkt % 2UL == 0UL)
        return 2;
      if (produkt % 3UL == 0UL)
        return 3;
      uint faktor1;
      for (uint faktor2 = (uint) (((int) (suuceBegin / 6U) + 1) * 6 - 1); faktor2 < num; faktor2 = faktor1 + 4U)
      {
        if (produkt % (ulong) faktor2 == 0UL)
          return faktor2;
        faktor1 = faktor2 + 2U;
        if (produkt % (ulong) faktor1 == 0UL)
          return faktor1;
      }
      return 1;
    }

    public static bool MillerRabinZusamegesezt(
      BigInteger kandidaat,
      IEnumerable<BigInteger> listeZaalTest)
    {
      if (kandidaat < 4L)
        return false;
      BigInteger[] array = listeZaalTest.ToArray<BigInteger>();
      BigInteger bigInteger1 = kandidaat - (BigInteger) 1;
      int num = 0;
      BigInteger exponent = bigInteger1;
      while (true)
      {
        if (!(0L < (exponent & (BigInteger) 1)))
        {
          ++num;
          exponent >>= 1;
        }
        else
          break;
      }
      if (num == 0)
        return true;
      Random random = new Random();
      foreach (BigInteger bigInteger2 in array)
      {
        BigInteger bigInteger3 = BigInteger.ModPow(bigInteger2, exponent, kandidaat);
        if (!(bigInteger3 == 1L) && !(bigInteger3 == bigInteger1))
        {
          for (int index = 0; index < num; ++index)
          {
            bigInteger3 = BigInteger.ModPow(bigInteger3, (BigInteger) 2, kandidaat);
            if (bigInteger3 == 1L)
              return true;
            if (bigInteger3 == bigInteger1)
              break;
          }
          if (bigInteger3 != bigInteger1)
            return true;
        }
      }
      return false;
    }
  }
}
