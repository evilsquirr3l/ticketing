namespace Ticketing.Models;

public record PaginatedResult<T> where T : class
{
    public PaginatedResult(IEnumerable<T> data, Page page)
    {
        Data = data;
        Page = page;
    }

    public IEnumerable<T> Data { get; set; } = default!;
    
    public Page Page { get; set; }
}
