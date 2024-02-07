namespace Ticketing.Models;

public class Page
{
    public int Skip { get; set; } = 0;

    public int Take { get; set; } = 50;

    public int Count { get; set; }

    public int Total { get; set; }
}
