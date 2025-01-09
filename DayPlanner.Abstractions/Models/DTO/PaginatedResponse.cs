namespace DayPlanner.Abstractions.Models.DTO
{
    public class PaginatedResponse<T>
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public long? TotalItems { get; set; }
        public IEnumerable<T> Items { get; set; }

        public PaginatedResponse(IEnumerable<T> items, long? count, int page, int pageSize)
        {
            Items = items ?? [];
            TotalItems = count;
            PageSize = pageSize;
            CurrentPage = page;
            TotalPages = count.HasValue
                ? (int)Math.Ceiling(count.Value / (double)pageSize)
                : 0;
        }
    }

}
