Yes, now I understand. You need **not ZIP**, but a **real test simulation in chat**:

1. I create a **new mock test**.
2. I give you the **database diagram idea + SQL script**.
3. Then I solve it **step by step exactly like you should do during the test**.
4. With commands, files, code blocks, and the mental flow.

This mock is similar to your example test logic: **GET details + PUT action that changes status/date/removes related rows**, but with a different domain.

---

# Mock Test: Database First Practice

## Test description

You need to build an ASP.NET Core WebAPI using **Entity Framework Core Database First**.

The database represents a small **library loan system**.

## Database diagram in text

```text
Reader
------
ID int PK
FirstName nvarchar(50)
LastName nvarchar(100)

Book
----
ID int PK
Title nvarchar(100)
Price numeric(10,2)

LoanStatus
----------
ID int PK
Name nvarchar(50)

Loan
----
ID int PK
CreatedAt datetime
ReturnedAt datetime NULL
ReaderID int FK
LoanStatusID int FK

Book_Loan
---------
BookID int PK FK
LoanID int PK FK
Quantity int
```

Relationships:

```text
Reader 1 --- many Loan
LoanStatus 1 --- many Loan
Loan many --- many Book through Book_Loan
```

`Book_Loan` is a junction table with extra field `Quantity`.

---

# Required endpoints

## Endpoint 1

```http
GET api/loans/{id}
```

Should return:

```json
{
  "id": 1,
  "createdAt": "2026-06-01T10:00:00",
  "returnedAt": null,
  "status": "Borrowed",
  "reader": {
    "firstName": "John",
    "lastName": "Smith"
  },
  "books": [
    {
      "title": "Clean Code",
      "price": 120.00,
      "quantity": 1
    },
    {
      "title": "Database Systems",
      "price": 95.50,
      "quantity": 2
    }
  ]
}
```

Rules:

```text
If loan does not exist → 404 NotFound
If loan exists → 200 Ok with DTO
```

---

## Endpoint 2

```http
PUT api/loans/{id}/return
```

Request body:

```json
{
  "statusName": "Returned"
}
```

Logic:

```text
1. Check if loan exists.
2. Check if status exists.
3. Check if loan is already returned.
4. Set ReturnedAt to current date.
5. Change LoanStatusID.
6. Remove related rows from Book_Loan.
7. Save changes.
```

Rules:

```text
If loan does not exist → 404 NotFound
If status does not exist → 404 NotFound
If loan is already returned → 400 BadRequest
If request body is invalid → 400 BadRequest
If success → 204 NoContent
```

---

# Part 1: SQL database script

During the real test, they may give you a ready SQL script. Here we simulate that.

Create file:

```text
01_create_database.sql
```

Write this SQL:

```sql
CREATE DATABASE APBD_Mock_Library;
GO

USE APBD_Mock_Library;
GO

CREATE TABLE Reader (
    ID int IDENTITY(1,1) PRIMARY KEY,
    FirstName nvarchar(50) NOT NULL,
    LastName nvarchar(100) NOT NULL
);

CREATE TABLE Book (
    ID int IDENTITY(1,1) PRIMARY KEY,
    Title nvarchar(100) NOT NULL,
    Price numeric(10,2) NOT NULL
);

CREATE TABLE LoanStatus (
    ID int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(50) NOT NULL
);

CREATE TABLE Loan (
    ID int IDENTITY(1,1) PRIMARY KEY,
    CreatedAt datetime NOT NULL,
    ReturnedAt datetime NULL,
    ReaderID int NOT NULL,
    LoanStatusID int NOT NULL,

    CONSTRAINT FK_Loan_Reader
        FOREIGN KEY (ReaderID) REFERENCES Reader(ID),

    CONSTRAINT FK_Loan_LoanStatus
        FOREIGN KEY (LoanStatusID) REFERENCES LoanStatus(ID)
);

CREATE TABLE Book_Loan (
    BookID int NOT NULL,
    LoanID int NOT NULL,
    Quantity int NOT NULL,

    CONSTRAINT PK_Book_Loan
        PRIMARY KEY (BookID, LoanID),

    CONSTRAINT FK_BookLoan_Book
        FOREIGN KEY (BookID) REFERENCES Book(ID),

    CONSTRAINT FK_BookLoan_Loan
        FOREIGN KEY (LoanID) REFERENCES Loan(ID)
);
```

---

# Part 2: Seed data script

Create file:

```text
02_seed_data.sql
```

Write this:

```sql
USE APBD_Mock_Library;
GO

INSERT INTO Reader (FirstName, LastName)
VALUES
('John', 'Smith'),
('Anna', 'Brown'),
('Michael', 'Johnson');

INSERT INTO Book (Title, Price)
VALUES
('Clean Code', 120.00),
('Database Systems', 95.50),
('ASP.NET Core Guide', 80.00),
('Algorithms', 150.00);

INSERT INTO LoanStatus (Name)
VALUES
('Borrowed'),
('Returned'),
('Cancelled');

INSERT INTO Loan (CreatedAt, ReturnedAt, ReaderID, LoanStatusID)
VALUES
('2026-06-01T10:00:00', NULL, 1, 1),
('2026-06-02T12:30:00', NULL, 2, 1),
('2026-05-20T09:15:00', '2026-05-25T15:00:00', 3, 2);

INSERT INTO Book_Loan (BookID, LoanID, Quantity)
VALUES
(1, 1, 1),
(2, 1, 2),
(3, 2, 1),
(4, 3, 1);
```

---

# Part 3: Student mindflow before coding

On the test, before writing code, think like this:

```text
Endpoint: GET api/loans/{id}

Main table:
Loan

Related tables needed:
Reader
LoanStatus
Book_Loan
Book

Returned JSON:
Loan fields + reader object + books list

Need DTO:
LoanDetailsDto
ReaderDto
BookDto
```

For PUT:

```text
Endpoint: PUT api/loans/{id}/return

Input:
id from route
statusName from body

Need checks:
Loan exists?
Status exists?
Already returned?

Need changes:
ReturnedAt = DateTime.Now
LoanStatusID = status.ID
Remove Book_Loan rows
SaveChangesAsync()
```

---

# Part 4: Create project

In terminal:

```bash
dotnet new webapi -n APBDMockLibrary
cd APBDMockLibrary
```

Install EF packages:

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

Install or update EF tool:

```bash
dotnet tool install --global dotnet-ef
```

If it is already installed:

```bash
dotnet tool update --global dotnet-ef
```

---

# Part 5: appsettings.json

Open:

```text
appsettings.json
```

Write connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=APBD_Mock_Library;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "AllowedHosts": "*"
}
```

If you use LocalDB:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=APBD_Mock_Library;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "AllowedHosts": "*"
}
```

---

# Part 6: Run scaffold command

This is the **Database First moment**.

Run:

```bash
dotnet ef dbcontext scaffold Name=ConnectionStrings:DefaultConnection Microsoft.EntityFrameworkCore.SqlServer --context-dir Data --output-dir Models --context UniversityTasksDbContext --no-onconfiguring --force
```

After this, EF generates:

```text
Data/
  UniversityTasksDbContext.cs

Models/
  Book.cs
  BookLoan.cs
  Loan.cs
  LoanStatus.cs
  Reader.cs
```

Important: check exact names. Maybe EF generates:

```text
BookLoan
```

instead of:

```text
Book_Loan
```

You must use generated C# names.

---

# Part 7: Check generated DbContext

Open:

```text
Data/UniversityTasksDbContext.cs
```

You should see something like:

```csharp
public virtual DbSet<Book> Books { get; set; }

public virtual DbSet<BookLoan> BookLoans { get; set; }

public virtual DbSet<Loan> Loans { get; set; }

public virtual DbSet<LoanStatus> LoanStatuses { get; set; }

public virtual DbSet<Reader> Readers { get; set; }
```

This tells you the names you will use in service:

```csharp
_context.Loans
_context.LoanStatuses
_context.BookLoans
```

---

# Part 8: Program.cs

Open:

```text
Program.cs
```

Write this:

```csharp
using APBDMockLibrary.Data;
using APBDMockLibrary.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<UniversityTasksDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ILoanService, LoanService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

At this point, `ILoanService` and `LoanService` do not exist yet. We will create them.

---

# Part 9: Create DTOs

Create folder:

```text
DTOs
```

Create file:

```text
DTOs/LoanDetailsDto.cs
```

Write:

```csharp
namespace APBDMockLibrary.DTOs;

public class LoanDetailsDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string Status { get; set; } = null!;
    public ReaderDto Reader { get; set; } = null!;
    public List<BookDto> Books { get; set; } = new();
}
```

Create file:

```text
DTOs/ReaderDto.cs
```

Write:

```csharp
namespace APBDMockLibrary.DTOs;

public class ReaderDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}
```

Create file:

```text
DTOs/BookDto.cs
```

Write:

```csharp
namespace APBDMockLibrary.DTOs;

public class BookDto
{
    public string Title { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
```

Create file:

```text
DTOs/ReturnLoanRequestDto.cs
```

Write:

```csharp
using System.ComponentModel.DataAnnotations;

namespace APBDMockLibrary.DTOs;

public class ReturnLoanRequestDto
{
    [Required]
    public string StatusName { get; set; } = null!;
}
```

Why `[Required]`?

Because if request body is:

```json
{}
```

then API should return `400 BadRequest`.

---

# Part 10: Create service result enum

Create folder:

```text
Services
```

Create file:

```text
Services/ReturnLoanResult.cs
```

Write:

```csharp
namespace APBDMockLibrary.Services;

public enum ReturnLoanResult
{
    Success,
    LoanNotFound,
    StatusNotFound,
    AlreadyReturned,
    InvalidData
}
```

This helps service return a clean result to controller.

---

# Part 11: Create service interface

Create file:

```text
Services/ILoanService.cs
```

Write:

```csharp
using APBDMockLibrary.DTOs;

namespace APBDMockLibrary.Services;

public interface ILoanService
{
    Task<LoanDetailsDto?> GetLoanByIdAsync(int id);
    Task<ReturnLoanResult> ReturnLoanAsync(int id, ReturnLoanRequestDto request);
}
```

Meaning:

```text
GetLoanByIdAsync:
returns loan DTO or null

ReturnLoanAsync:
returns result enum, controller converts it to HTTP response
```

---

# Part 12: Create service implementation

Create file:

```text
Services/LoanService.cs
```

Write:

```csharp
using APBDMockLibrary.Data;
using APBDMockLibrary.DTOs;
using Microsoft.EntityFrameworkCore;

namespace APBDMockLibrary.Services;

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

Important note: generated property names may be slightly different.

For example, EF might generate:

```csharp
LoanStatusId
```

or:

```csharp
LoanStatusid
```

Usually it is:

```csharp
LoanStatusId
```

But always check generated model.

---

# Part 13: Create controller

Create folder:

```text
Controllers
```

Create file:

```text
Controllers/LoansController.cs
```

Write:

```csharp
using APBDMockLibrary.DTOs;
using APBDMockLibrary.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBDMockLibrary.Controllers;

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

Route logic:

```text
Controller route:
api/loans

GET method:
{id:int}

Final:
GET api/loans/1

PUT method:
{id:int}/return

Final:
PUT api/loans/1/return
```

---

# Part 14: Build project

Run:

```bash
dotnet build
```

If you get errors, check:

```text
1. Namespace is correct?
2. DbContext name is correct?
3. Generated entity property names are correct?
4. Did you register service in Program.cs?
5. Did you install EF Core packages?
```

Common fixes:

If service cannot find `LoanStatus`, open generated `Loan.cs` and check the property. It may be:

```csharp
public virtual LoanStatus LoanStatus { get; set; } = null!;
```

If it is different, use the generated name.

If service cannot find `BookLoans`, open generated `Loan.cs`. It should have:

```csharp
public virtual ICollection<BookLoan> BookLoans { get; set; } = new List<BookLoan>();
```

If name is different, adapt.

---

# Part 15: Run project

Run:

```bash
dotnet run
```

Open Swagger:

```text
https://localhost:xxxx/swagger
```

or:

```text
http://localhost:xxxx/swagger
```

---

# Part 16: Test GET endpoint

Request:

```http
GET api/loans/1
```

Expected response:

```json
{
  "id": 1,
  "createdAt": "2026-06-01T10:00:00",
  "returnedAt": null,
  "status": "Borrowed",
  "reader": {
    "firstName": "John",
    "lastName": "Smith"
  },
  "books": [
    {
      "title": "Clean Code",
      "price": 120.00,
      "quantity": 1
    },
    {
      "title": "Database Systems",
      "price": 95.50,
      "quantity": 2
    }
  ]
}
```

Test not found:

```http
GET api/loans/999
```

Expected:

```text
404 NotFound
```

---

# Part 17: Test PUT endpoint

Request:

```http
PUT api/loans/1/return
```

Body:

```json
{
  "statusName": "Returned"
}
```

Expected:

```text
204 NoContent
```

Then test again:

```http
GET api/loans/1
```

Now it should have:

```json
{
  "returnedAt": "some current date",
  "status": "Returned",
  "books": []
}
```

Because related `Book_Loan` rows were removed.

Test already returned:

```http
PUT api/loans/1/return
```

Expected:

```text
400 BadRequest
```

Test wrong status:

```http
PUT api/loans/2/return
```

Body:

```json
{
  "statusName": "Finished"
}
```

Expected:

```text
404 NotFound
```

Because status `Finished` does not exist.

---

# Full real-test mindflow

This is the exact order you should follow during the real test.

## Step 1: Read task

Write mentally:

```text
What endpoints?
What main entity?
What related data?
What validation?
What response status codes?
```

For this mock:

```text
GET api/loans/{id}
PUT api/loans/{id}/return
```

---

## Step 2: Read diagram

Find:

```text
Main table:
Loan

Parent tables:
Reader
LoanStatus

Junction table:
Book_Loan

Many-to-many target:
Book
```

---

## Step 3: Create database

Run SQL:

```sql
CREATE DATABASE ...
CREATE TABLE ...
INSERT ...
```

---

## Step 4: Create WebAPI

```bash
dotnet new webapi -n APBDMockLibrary
cd APBDMockLibrary
```

---

## Step 5: Install packages

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

---

## Step 6: Add connection string

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=APBD_Mock_Library;Trusted_Connection=True;TrustServerCertificate=True"
}
```

---

## Step 7: Scaffold

```bash
dotnet ef dbcontext scaffold Name=ConnectionStrings:DefaultConnection Microsoft.EntityFrameworkCore.SqlServer --context-dir Data --output-dir Models --context UniversityTasksDbContext --no-onconfiguring --force
```

---

## Step 8: Check generated names

Open:

```text
Data/UniversityTasksDbContext.cs
Models/Loan.cs
Models/BookLoan.cs
```

Check:

```text
DbSet names
Navigation property names
Foreign key property names
```

---

## Step 9: Create DTOs

From expected JSON:

```text
LoanDetailsDto
ReaderDto
BookDto
ReturnLoanRequestDto
```

---

## Step 10: Create service

```text
ILoanService
LoanService
ReturnLoanResult
```

Business logic lives here.

---

## Step 11: Create controller

```text
LoansController
```

Controller only maps:

```text
Service result → HTTP response
```

---

## Step 12: Register everything

In `Program.cs`:

```csharp
builder.Services.AddDbContext<UniversityTasksDbContext>(...);
builder.Services.AddScoped<ILoanService, LoanService>();
```

---

## Step 13: Build

```bash
dotnet build
```

Never skip this. In your teacher’s rules, program that does not compile = 0.

---

## Step 14: Run and test

```bash
dotnet run
```

Test:

```text
GET existing ID
GET missing ID
PUT valid body
PUT already returned
PUT wrong status
PUT empty body
```

---

# What you must memorize

For Database First:

```text
Database creates:
Models + DbContext

You write:
DTOs
Services
Controllers
Program.cs registration
appsettings connection string
```

The most important command:

```bash
dotnet ef dbcontext scaffold Name=ConnectionStrings:DefaultConnection Microsoft.EntityFrameworkCore.SqlServer --context-dir Data --output-dir Models --context UniversityTasksDbContext --no-onconfiguring --force
```

The most important GET pattern:

```csharp
return await _context.Loans
    .AsNoTracking()
    .Where(l => l.Id == id)
    .Select(l => new LoanDetailsDto
    {
        // map fields here
    })
    .FirstOrDefaultAsync();
```

The most important PUT pattern:

```csharp
var entity = await _context.Loans
    .Include(l => l.BookLoans)
    .FirstOrDefaultAsync(l => l.Id == id);

if (entity == null)
{
    return NotFound;
}

entity.SomeField = request.SomeField;

await _context.SaveChangesAsync();
```

For this exact mock, the key logic is:

```csharp
loan.LoanStatusId = status.Id;
loan.ReturnedAt = DateTime.Now;
_context.BookLoans.RemoveRange(loan.BookLoans);
await _context.SaveChangesAsync();
```

That is the whole Database First test logic.
# Mocktest2apbd
