using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WopiHost.Dto;

namespace WopiHost.Common;

public static class PaginationExtensions
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 200;

    public static async Task<PagedResult<TDto>> ToPagedResultAsync<TEntity, TDto>(
        this IQueryable<TEntity> source, //specificerer at det er en helper med 'this'
        PageQuery q,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy, //modtager entitet, retunerer sorteret entitet
        Expression<Func<TEntity, TDto>> selector, //mapper entiteten til DTO
        CancellationToken ct = default)
    {
        var page     = Math.Max(1, q.Page);
        var pageSize = Math.Clamp(q.PageSize <= 0 ? DefaultPageSize : q.PageSize, 1, MaxPageSize);

        var sorted = orderBy(source);

        var total = await sorted.CountAsync(ct);

        var items = await sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(selector)
            .ToListAsync(ct);

        //sætter total pages til at være en integer
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling((double)total / pageSize);

        return new PagedResult<TDto>(items, page, pageSize, total, totalPages);
    }
}
