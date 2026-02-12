namespace AppCore.Application.DTOs;

public sealed class PaginationDto<E> {

    public int Count { get; set; }
    public int Pages { get; set; }
    public List<E>? Results { get; set; } = [];
}
