static class EnumerableExtensions
{
    public static void Apply<TElement>(this IEnumerable<TElement> elements, Action<TElement> processElement)
    {
        foreach(var element in elements)
        {
            processElement(element);
        }
    }

    public static string JoinedBy<TElement>(this IEnumerable<TElement> elements, string separator) => 
        string.Join(separator, elements);
}