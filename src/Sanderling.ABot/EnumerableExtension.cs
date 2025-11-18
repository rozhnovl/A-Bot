namespace Sanderling.ABot;

public static class EnumerableExtension
{
	public static IEnumerable<IEnumerable<T>> EnumerateSubsequencesStartingWithFirstElement<T>(
		this IEnumerable<T> sequence)
	{
		List<T> subsequence = [];

		foreach (var element in sequence)
		{
			subsequence.Add(element);
			yield return subsequence.ToArray();
		}
	}
}
