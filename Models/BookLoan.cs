using System;
using System.Collections.Generic;

namespace WebApplication3.Models;

public partial class BookLoan
{
    public int BookId { get; set; }

    public int LoanId { get; set; }

    public int Quantity { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual Loan Loan { get; set; } = null!;
}
