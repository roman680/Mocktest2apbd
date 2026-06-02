using System;
using System.Collections.Generic;

namespace WebApplication3.Models;

public partial class Loan
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReturnedAt { get; set; }

    public int ReaderId { get; set; }

    public int LoanStatusId { get; set; }

    public virtual ICollection<BookLoan> BookLoans { get; set; } = new List<BookLoan>();

    public virtual LoanStatus LoanStatus { get; set; } = null!;

    public virtual Reader Reader { get; set; } = null!;
}
