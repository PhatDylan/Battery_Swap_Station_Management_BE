namespace BusinessObject.DTOs;

public class ResponseObject <T>
{
    public T? Content { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool Success { get; set; } = false;
    public PaginationResponse? Pagination { get; set; }

    public ResponseObject<T> UnwrapPagination<TList, TItem>(PaginationWrapper<TList, TItem> response)
        where TList : IList<TItem>
    {
        Content = (T)(object)response.Items!;
        Pagination = new PaginationResponse
        {
            Page = response.Page,
            TotalCount = response.TotalCount,
            PageSize = response.PageSize
        };
        return this;
    }
}

public class PaginationResponse
{
    public int Page { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
}

public class PaginationWrapper<T, TU>(T items, int page, int totalCount, int pageSize)
    where T : IList<TU>
{
    public T Items { get; set; } = items;
    public int Page { get; set; } = page;
    public int TotalCount { get; set; } = totalCount;
    public int PageSize { get; set; } = pageSize;
}