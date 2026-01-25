namespace AppCore.Application.Wrappers;

public class PageResult<T> where T : class {
    public List<T> Items { get; set; } = [];
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int Count { get; set; }
    public bool IsFirstPage => CurrentPage == 1;
    public bool IsLastPage => CurrentPage == TotalPages;
}
