namespace OrionSoft.AppCore.Domain.Common;

public sealed class PaginationResponse<T> {

    public int Count { get; set; }

    public int Pages { get; set; }

    public List<T> Results { get; set; } = [];

    public PaginationResponse(List<T>? data = null) {
        Results = data ?? [];
        Count = Results.Count;
    }

    public static PaginationResponse<T> Create(List<T> data)
        => new PaginationResponse<T>(data);

}
