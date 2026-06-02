using APBDMockLibrary.Services;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using WebApplication3.DTOs;
using WebApplication3.Services;

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