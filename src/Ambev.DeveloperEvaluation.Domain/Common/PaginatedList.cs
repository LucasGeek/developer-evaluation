namespace Ambev.DeveloperEvaluation.Domain.Common;

public class PaginatedList<T> : List<T>
{
    public int Page { get; private set; }
    public int TotalPages { get; private set; }
    public int Size { get; private set; }
    public int TotalCount { get; private set; }

    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        Size = pageSize;
        Page = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);

        AddRange(items);
    }
}