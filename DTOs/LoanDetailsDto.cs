namespace WebApplication3.DTOs;

public class LoanDetailsDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string Status { get; set; } = null!;
    public ReaderDto Reader { get; set; } = null!;
    public List<BookDto> Books { get; set; } = new();
}