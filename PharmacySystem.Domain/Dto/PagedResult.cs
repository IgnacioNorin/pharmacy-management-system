using System;
using System.Collections.Generic;

namespace PharmacySystem.Model
{
    // One page of a larger list plus the total row count, so a grid can show "page X of Y"
    // and enable/disable its navigation without a second round trip. The repository fills this
    // from a single command (COUNT + the OFFSET/FETCH page).
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = DefaultPageSize;

        public const int DefaultPageSize = 50;

        public int TotalPages =>
            PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;

        public static PagedResult<T> Empty(int pageSize = DefaultPageSize) =>
            new PagedResult<T> { Items = new List<T>(), TotalCount = 0, PageNumber = 1, PageSize = pageSize };
    }
}
