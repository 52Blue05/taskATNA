
namespace Sale_Saas.Application.Common.Models;

public class GoalPaginatedList<T>
{
    public IReadOnlyCollection<T> Items { get; }
    public int PageIndex { get; }
    public int TotalPages { get; }
    public int TotalRecords { get; }
    public int TotalExtend { get; set; }
    public int TotalTargetPoint { get; set; }
    public int TotalActualPoint { get; set; }

    public GoalPaginatedList(IReadOnlyCollection<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalRecords = count;
        Items = items;
    }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public static async Task<GoalPaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new GoalPaginatedList<T>(items, count, pageNumber, pageSize);
    }

    public static GoalPaginatedList<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new GoalPaginatedList<T>(items, count, pageNumber, pageSize);
    }

    public static GoalPaginatedList<T> CreateFromList(List<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count;
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new GoalPaginatedList<T>(items, count, pageNumber, pageSize);
    }
}
