namespace AppCore.Application.DTOs;

public class PaginationDto<E> {

    public int Count { get; set; }
    public int Pages { get; set; }
    public List<E>? Results { get; set; } = [];
}