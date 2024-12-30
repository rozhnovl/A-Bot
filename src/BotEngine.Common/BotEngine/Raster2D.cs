namespace BotEngine;

public class Raster2D<TPixel>
{
	public readonly TPixel[] ListePixel;

	public readonly int Length;

	public Raster2D()
	{
	}

	public Raster2D(TPixel[] listePixel, int length)
	{
		ListePixel = listePixel;
		Length = length;
	}
}
