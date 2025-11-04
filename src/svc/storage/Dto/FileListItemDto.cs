namespace Storage.Dto;

public sealed class PageQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

public sealed record PagedResult<T>(
    IReadOnlyList<T> items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
);