using APBDMockLibrary.Services;
using WebApplication3.DTOs;
using WebApplication3.Services;
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