using System;
using System.Collections.Generic;

namespace WebApplication3.Models;

public partial class LoanStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
