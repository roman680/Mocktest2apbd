using WebApplication3.DTOs;
using WebApplication3.Services;

namespace APBDMockLibrary.Services;

public interface ILoanService
{
    Task<LoanDetailsDto?> GetLoanByIdAsync(int id);
    Task<ReturnLoanResult> ReturnLoanAsync(int id, ReturnLoanRequestDto request);
}