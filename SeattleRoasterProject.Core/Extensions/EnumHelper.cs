namespace SeattleRoasterProject.Core.Extensions;

public static class EnumHelper 
{
    public static List<T> GetEnumList<T>() where T : struct, IConvertible
    {
        var array = (T[])Enum.GetValues(typeof(T));
        var list = new List<T>(array);
        return list;
    }

    public static List<string> GetRandomEnums<T>(int numberOfItems) where T : struct, IConvertible
    {
        var random = new Random();
        var allItems = GetEnumList<T>()
            .OrderBy(_ => random.Next())
            .Take(numberOfItems);

        return allItems.Select(item => (item.ToString()?.Replace("_", " ") ?? string.Empty))
            .ToList();
    }
}