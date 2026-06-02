using System;
using System.Collections.Generic;

namespace WebApplication3.Models;

public partial class Reader
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
