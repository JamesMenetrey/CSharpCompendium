namespace ConcurrencyInCSharpCookbook;

public static class UtilityExtensions
{
    public static IEnumerable<IEnumerable<T>> SplitIntoChunks<T>(this IEnumerable<T> enumerable, int numberOfChunks)
    {
        var array = enumerable.ToArray();
        var chunkSize = array.Length / numberOfChunks;
        var remainder = array.Length % numberOfChunks;

        for (var i = 0; i < numberOfChunks; i++)
        {
            var startIndex = i * chunkSize + Math.Min(i, remainder);
            var endIndex = startIndex + chunkSize + (i < remainder ? 1 : 0);

            yield return array.Skip(startIndex).Take(endIndex - startIndex);
        }
    }
}