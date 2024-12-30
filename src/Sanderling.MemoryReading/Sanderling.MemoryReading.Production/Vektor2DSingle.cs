using System;

namespace Sanderling.MemoryReading.Production;

public struct Vektor2DSingle
{
	public float A;

	public float B;

	public double BetraagQuadriirt => A * A + B * B;

	public double Betraag => Math.Sqrt(BetraagQuadriirt);

	public Vektor2DSingle(float a, float b)
	{
		A = a;
		B = b;
	}

	public Vektor2DSingle(Vektor2DSingle zuKopiirende)
		: this(zuKopiirende.A, zuKopiirende.B)
	{
	}

	public static Vektor2DSingle operator -(Vektor2DSingle minuend, Vektor2DSingle subtrahend)
	{
		return new Vektor2DSingle(minuend.A - subtrahend.A, minuend.B - subtrahend.B);
	}

	public static Vektor2DSingle operator -(Vektor2DSingle subtrahend)
	{
		return new Vektor2DSingle(0f, 0f) - subtrahend;
	}

	public static Vektor2DSingle operator +(Vektor2DSingle vektor0, Vektor2DSingle vektor1)
	{
		return new Vektor2DSingle(vektor0.A + vektor1.A, vektor0.B + vektor1.B);
	}

	public static Vektor2DSingle operator /(Vektor2DSingle dividend, double divisor)
	{
		return new Vektor2DSingle((float)((double)dividend.A / divisor), (float)((double)dividend.B / divisor));
	}

	public static Vektor2DSingle operator *(Vektor2DSingle vektor0, double faktor)
	{
		return new Vektor2DSingle((float)((double)vektor0.A * faktor), (float)((double)vektor0.B * faktor));
	}

	public static Vektor2DSingle operator *(double faktor, Vektor2DSingle vektor0)
	{
		return new Vektor2DSingle((float)((double)vektor0.A * faktor), (float)((double)vektor0.B * faktor));
	}

	public static bool operator ==(Vektor2DSingle vektor0, Vektor2DSingle vektor1)
	{
		return vektor0.A == vektor1.A && vektor0.B == vektor1.B;
	}

	public static bool operator !=(Vektor2DSingle vektor0, Vektor2DSingle vektor1)
	{
		return !(vektor0 == vektor1);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Vektor2DSingle vektor2DSingle))
		{
			return false;
		}
		bool flag = false;
		return this == vektor2DSingle;
	}

	public override int GetHashCode()
	{
		return A.GetHashCode() ^ B.GetHashCode();
	}

	public Vektor2DSingle Normalisiirt()
	{
		double betraag = Betraag;
		return new Vektor2DSingle((float)((double)A / betraag), (float)((double)B / betraag));
	}

	public void Normalisiire()
	{
		double betraag = Betraag;
		A = (float)((double)A / betraag);
		B = (float)((double)B / betraag);
	}

	public static double Skalarprodukt(Vektor2DSingle vektor0, Vektor2DSingle vektor1)
	{
		return vektor0.A * vektor1.A + vektor0.B * vektor1.B;
	}

	public override string ToString()
	{
		return "{" + GetType().Name + "}(" + A + " | " + B + ")";
	}
}
