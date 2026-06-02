Yes — on the test they may give you not only:

```http
GET api/loans/{id}
```

but also endpoints with **query parameters**, like:

```http
GET api/loans?bookId=1
GET api/books?title=clean
GET api/readers?lastName=Smith
```

Important difference:

```text
{id}      → route parameter
?bookId=1 → query parameter
```

---

# 1. Route parameter example

Endpoint:

```http
GET api/loans/1
```

Controller:

```csharp
[HttpGet("{id:int}")]
public async Task<IActionResult> GetLoan(int id)
{
    var loan = await _loanService.GetLoanByIdAsync(id);

    if (loan == null)
    {
        return NotFound();
    }

    return Ok(loan);
}
```

Service interface:

```csharp
Task<LoanDetailsDto?> GetLoanByIdAsync(int id);
```

Service:

```csharp
public async Task<LoanDetailsDto?> GetLoanByIdAsync(int id)
{
    return await _context.Loans
        .AsNoTracking()
        .Where(l => l.Id == id)
        .Select(l => new LoanDetailsDto
        {
            Id = l.Id,
            CreatedAt = l.CreatedAt,
            ReturnedAt = l.ReturnedAt,
            Status = l.LoanStatus.Name,
            Reader = new ReaderDto
            {
                FirstName = l.Reader.FirstName,
                LastName = l.Reader.LastName
            },
            Books = l.BookLoans.Select(bl => new BookDto
            {
                Title = bl.Book.Title,
                Price = bl.Book.Price,
                Quantity = bl.Quantity
            }).ToList()
        })
        .FirstOrDefaultAsync();
}
```

Use this when the endpoint has a specific ID in the URL.

---

# 2. Query parameter example: `?bookId=1`

Endpoint:

```http
GET api/loans?bookId=1
```

Meaning: return all loans that contain book with ID `1`.

Controller:

```csharp
[HttpGet]
public async Task<IActionResult> GetLoansByBook([FromQuery] int? bookId)
{
    var loans = await _loanService.GetLoansAsync(bookId);

    return Ok(loans);
}
```

Service interface:

```csharp
Task<List<LoanShortDto>> GetLoansAsync(int? bookId);
```

DTO:

```csharp
namespace WebApplication3.DTOs;

public class LoanShortDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string Status { get; set; } = null!;
    public string ReaderFullName { get; set; } = null!;
}
```

Service:

```csharp
public async Task<List<LoanShortDto>> GetLoansAsync(int? bookId)
{
    var query = _context.Loans
        .AsNoTracking()
        .AsQueryable();

    if (bookId.HasValue)
    {
        query = query.Where(l => l.BookLoans.Any(bl => bl.BookId == bookId.Value));
    }

    return await query
        .Select(l => new LoanShortDto
        {
            Id = l.Id,
            CreatedAt = l.CreatedAt,
            ReturnedAt = l.ReturnedAt,
            Status = l.LoanStatus.Name,
            ReaderFullName = l.Reader.FirstName + " " + l.Reader.LastName
        })
        .ToListAsync();
}
```

Important part:

```csharp
l.BookLoans.Any(bl => bl.BookId == bookId.Value)
```

This means:

```text
Give me loans where at least one related BookLoan row has this BookId.
```

---

# 3. Query parameter example: search books by title

Endpoint:

```http
GET api/books?title=code
```

Meaning: return books where title contains `code`.

Controller:

```csharp
[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks([FromQuery] string? title)
    {
        var books = await _bookService.GetBooksAsync(title);

        return Ok(books);
    }
}
```

Service interface:

```csharp
using WebApplication3.DTOs;

namespace WebApplication3.Services;

public interface IBookService
{
    Task<List<BookDto>> GetBooksAsync(string? title);
}
```

Service:

```csharp
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using WebApplication3.DTOs;

namespace WebApplication3.Services;

public class BookService : IBookService
{
    private readonly UniversityTasksDbContext _context;

    public BookService(UniversityTasksDbContext context)
    {
        _context = context;
    }

    public async Task<List<BookDto>> GetBooksAsync(string? title)
    {
        var query = _context.Books
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(b => b.Title.Contains(title));
        }

        return await query
            .Select(b => new BookDto
            {
                Title = b.Title,
                Price = b.Price
            })
            .ToListAsync();
    }
}
```

Register in `Program.cs`:

```csharp
builder.Services.AddScoped<IBookService, BookService>();
```

---

# 4. Query parameter example with several filters

Endpoint:

```http
GET api/loans?bookId=1&statusName=Borrowed
```

Controller:

```csharp
[HttpGet]
public async Task<IActionResult> GetLoans(
    [FromQuery] int? bookId,
    [FromQuery] string? statusName)
{
    var loans = await _loanService.GetLoansAsync(bookId, statusName);

    return Ok(loans);
}
```

Service interface:

```csharp
Task<List<LoanShortDto>> GetLoansAsync(int? bookId, string? statusName);
```

Service:

```csharp
public async Task<List<LoanShortDto>> GetLoansAsync(int? bookId, string? statusName)
{
    var query = _context.Loans
        .AsNoTracking()
        .AsQueryable();

    if (bookId.HasValue)
    {
        query = query.Where(l => l.BookLoans.Any(bl => bl.BookId == bookId.Value));
    }

    if (!string.IsNullOrWhiteSpace(statusName))
    {
        query = query.Where(l => l.LoanStatus.Name == statusName);
    }

    return await query
        .Select(l => new LoanShortDto
        {
            Id = l.Id,
            CreatedAt = l.CreatedAt,
            ReturnedAt = l.ReturnedAt,
            Status = l.LoanStatus.Name,
            ReaderFullName = l.Reader.FirstName + " " + l.Reader.LastName
        })
        .ToListAsync();
}
```

This is very common on tests: optional filters.

Mental pattern:

```csharp
var query = _context.Table.AsQueryable();

if (filter exists)
{
    query = query.Where(...);
}

return await query.Select(...).ToListAsync();
```

---

# 5. Query parameter with required value

Endpoint:

```http
GET api/loans/by-book?bookId=1
```

Here `bookId` is required.

Controller:

```csharp
[HttpGet("by-book")]
public async Task<IActionResult> GetLoansByBook([FromQuery] int bookId)
{
    if (bookId <= 0)
    {
        return BadRequest("Invalid book id.");
    }

    var loans = await _loanService.GetLoansByRequiredBookAsync(bookId);

    return Ok(loans);
}
```

Service interface:

```csharp
Task<List<LoanShortDto>> GetLoansByRequiredBookAsync(int bookId);
```

Service:

```csharp
public async Task<List<LoanShortDto>> GetLoansByRequiredBookAsync(int bookId)
{
    return await _context.Loans
        .AsNoTracking()
        .Where(l => l.BookLoans.Any(bl => bl.BookId == bookId))
        .Select(l => new LoanShortDto
        {
            Id = l.Id,
            CreatedAt = l.CreatedAt,
            ReturnedAt = l.ReturnedAt,
            Status = l.LoanStatus.Name,
            ReaderFullName = l.Reader.FirstName + " " + l.Reader.LastName
        })
        .ToListAsync();
}
```

---

# 6. POST with body example

Endpoint:

```http
POST api/books
```

Body:

```json
{
  "title": "New Book",
  "price": 59.99
}
```

Request DTO:

```csharp
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.DTOs;

public class CreateBookRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = null!;

    [Range(0.01, 999999)]
    public decimal Price { get; set; }
}
```

Controller:

```csharp
[HttpPost]
public async Task<IActionResult> CreateBook(CreateBookRequestDto request)
{
    var id = await _bookService.CreateBookAsync(request);

    return Created($"api/books/{id}", new { id });
}
```

Service interface:

```csharp
Task<int> CreateBookAsync(CreateBookRequestDto request);
```

Service:

```csharp
using WebApplication3.Models;

public async Task<int> CreateBookAsync(CreateBookRequestDto request)
{
    var book = new Book
    {
        Title = request.Title,
        Price = request.Price
    };

    _context.Books.Add(book);

    await _context.SaveChangesAsync();

    return book.Id;
}
```

---

# 7. DELETE with route parameter

Endpoint:

```http
DELETE api/books/1
```

Controller:

```csharp
[HttpDelete("{id:int}")]
public async Task<IActionResult> DeleteBook(int id)
{
    var result = await _bookService.DeleteBookAsync(id);

    if (!result)
    {
        return NotFound();
    }

    return NoContent();
}
```

Service interface:

```csharp
Task<bool> DeleteBookAsync(int id);
```

Service:

```csharp
public async Task<bool> DeleteBookAsync(int id)
{
    var book = await _context.Books
        .FirstOrDefaultAsync(b => b.Id == id);

    if (book == null)
    {
        return false;
    }

    _context.Books.Remove(book);

    await _context.SaveChangesAsync();

    return true;
}
```

---

# 8. Most common endpoint forms

Remember these:

```text
GET api/loans/1
```

```csharp
[HttpGet("{id:int}")]
public async Task<IActionResult> GetLoan(int id)
```

---

```text
GET api/loans?bookId=1
```

```csharp
[HttpGet]
public async Task<IActionResult> GetLoans([FromQuery] int? bookId)
```

---

```text
GET api/loans/by-book?bookId=1
```

```csharp
[HttpGet("by-book")]
public async Task<IActionResult> GetLoansByBook([FromQuery] int bookId)
```

---

```text
PUT api/loans/1/return
```

```csharp
[HttpPut("{id:int}/return")]
public async Task<IActionResult> ReturnLoan(int id, ReturnLoanRequestDto request)
```

---

```text
POST api/books
```

```csharp
[HttpPost]
public async Task<IActionResult> CreateBook(CreateBookRequestDto request)
```

---

```text
DELETE api/books/1
```

```csharp
[HttpDelete("{id:int}")]
public async Task<IActionResult> DeleteBook(int id)
```

---

# 9. Full controller with different endpoint types

Example `LoansController`:

```csharp
using Microsoft.AspNetCore.Mvc;
using WebApplication3.DTOs;
using WebApplication3.Services;

namespace WebApplication3.Controllers;

[ApiController]
[Route("api/loans")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetLoan(int id)
    {
        var loan = await _loanService.GetLoanByIdAsync(id);

        if (loan == null)
        {
            return NotFound();
        }

        return Ok(loan);
    }

    [HttpGet]
    public async Task<IActionResult> GetLoans(
        [FromQuery] int? bookId,
        [FromQuery] string? statusName)
    {
        var loans = await _loanService.GetLoansAsync(bookId, statusName);

        return Ok(loans);
    }

    [HttpGet("by-book")]
    public async Task<IActionResult> GetLoansByBook([FromQuery] int bookId)
    {
        if (bookId <= 0)
        {
            return BadRequest("Invalid book id.");
        }

        var loans = await _loanService.GetLoansByRequiredBookAsync(bookId);

        return Ok(loans);
    }

    [HttpPut("{id:int}/return")]
    public async Task<IActionResult> ReturnLoan(int id, ReturnLoanRequestDto request)
    {
        var result = await _loanService.ReturnLoanAsync(id, request);

        return result switch
        {
            ReturnLoanResult.Success => NoContent(),
            ReturnLoanResult.LoanNotFound => NotFound("Loan not found."),
            ReturnLoanResult.StatusNotFound => NotFound("Status not found."),
            ReturnLoanResult.AlreadyReturned => BadRequest("Loan is already returned."),
            ReturnLoanResult.InvalidData => BadRequest("Invalid request data."),
            _ => BadRequest()
        };
    }
}
```

---

# 10. Full service interface

```csharp
using WebApplication3.DTOs;

namespace WebApplication3.Services;

public interface ILoanService
{
    Task<LoanDetailsDto?> GetLoanByIdAsync(int id);

    Task<List<LoanShortDto>> GetLoansAsync(int? bookId, string? statusName);

    Task<List<LoanShortDto>> GetLoansByRequiredBookAsync(int bookId);

    Task<ReturnLoanResult> ReturnLoanAsync(int id, ReturnLoanRequestDto request);
}
```

---

# 11. Full service implementation

```csharp
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using WebApplication3.DTOs;

namespace WebApplication3.Services;

public class LoanService : ILoanService
{
    private readonly UniversityTasksDbContext _context;

    public LoanService(UniversityTasksDbContext context)
    {
        _context = context;
    }

    public async Task<LoanDetailsDto?> GetLoanByIdAsync(int id)
    {
        return await _context.Loans
            .AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => new LoanDetailsDto
            {
                Id = l.Id,
                CreatedAt = l.CreatedAt,
                ReturnedAt = l.ReturnedAt,
                Status = l.LoanStatus.Name,
                Reader = new ReaderDto
                {
                    FirstName = l.Reader.FirstName,
                    LastName = l.Reader.LastName
                },
                Books = l.BookLoans.Select(bl => new BookDto
                {
                    Title = bl.Book.Title,
                    Price = bl.Book.Price,
                    Quantity = bl.Quantity
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<LoanShortDto>> GetLoansAsync(int? bookId, string? statusName)
    {
        var query = _context.Loans
            .AsNoTracking()
            .AsQueryable();

        if (bookId.HasValue)
        {
            query = query.Where(l => l.BookLoans.Any(bl => bl.BookId == bookId.Value));
        }

        if (!string.IsNullOrWhiteSpace(statusName))
        {
            query = query.Where(l => l.LoanStatus.Name == statusName);
        }

        return await query
            .Select(l => new LoanShortDto
            {
                Id = l.Id,
                CreatedAt = l.CreatedAt,
                ReturnedAt = l.ReturnedAt,
                Status = l.LoanStatus.Name,
                ReaderFullName = l.Reader.FirstName + " " + l.Reader.LastName
            })
            .ToListAsync();
    }

    public async Task<List<LoanShortDto>> GetLoansByRequiredBookAsync(int bookId)
    {
        return await _context.Loans
            .AsNoTracking()
            .Where(l => l.BookLoans.Any(bl => bl.BookId == bookId))
            .Select(l => new LoanShortDto
            {
                Id = l.Id,
                CreatedAt = l.CreatedAt,
                ReturnedAt = l.ReturnedAt,
                Status = l.LoanStatus.Name,
                ReaderFullName = l.Reader.FirstName + " " + l.Reader.LastName
            })
            .ToListAsync();
    }

    public async Task<ReturnLoanResult> ReturnLoanAsync(int id, ReturnLoanRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.StatusName))
        {
            return ReturnLoanResult.InvalidData;
        }

        var loan = await _context.Loans
            .Include(l => l.BookLoans)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null)
        {
            return ReturnLoanResult.LoanNotFound;
        }

        if (loan.ReturnedAt != null)
        {
            return ReturnLoanResult.AlreadyReturned;
        }

        var status = await _context.LoanStatuses
            .FirstOrDefaultAsync(s => s.Name == request.StatusName);

        if (status == null)
        {
            return ReturnLoanResult.StatusNotFound;
        }

        loan.LoanStatusId = status.Id;
        loan.ReturnedAt = DateTime.Now;

        _context.BookLoans.RemoveRange(loan.BookLoans);

        await _context.SaveChangesAsync();

        return ReturnLoanResult.Success;
    }
}
```

---

# 12. How to think on the test

When you see:

```http
GET api/something?book=1
```

Think:

```text
This is query parameter.
I need [FromQuery].
Probably optional filter.
Use IQueryable.
If parameter exists, add Where.
Return ToListAsync.
```

Template:

```csharp
[HttpGet]
public async Task<IActionResult> GetSomething([FromQuery] int? book)
{
    var result = await _service.GetSomethingAsync(book);
    return Ok(result);
}
```

Service template:

```csharp
public async Task<List<SomeDto>> GetSomethingAsync(int? book)
{
    var query = _context.SomeTable
        .AsNoTracking()
        .AsQueryable();

    if (book.HasValue)
    {
        query = query.Where(x => x.SomeRelation.Any(r => r.BookId == book.Value));
    }

    return await query
        .Select(x => new SomeDto
        {
            // mapping
        })
        .ToListAsync();
}
```
