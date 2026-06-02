using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Models;

namespace WebApplication3.Data;

public partial class UniversityTasksDbContext : DbContext
{
    public UniversityTasksDbContext(DbContextOptions<UniversityTasksDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookLoan> BookLoans { get; set; }

    public virtual DbSet<Loan> Loans { get; set; }

    public virtual DbSet<LoanStatus> LoanStatuses { get; set; }

    public virtual DbSet<Reader> Readers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Book__3214EC27093F89A5");

            entity.ToTable("Book");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Price).HasColumnType("numeric(10, 2)");
            entity.Property(e => e.Title).HasMaxLength(100);
        });

        modelBuilder.Entity<BookLoan>(entity =>
        {
            entity.HasKey(e => new { e.BookId, e.LoanId });

            entity.ToTable("Book_Loan");

            entity.Property(e => e.BookId).HasColumnName("BookID");
            entity.Property(e => e.LoanId).HasColumnName("LoanID");

            entity.HasOne(d => d.Book).WithMany(p => p.BookLoans)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookLoan_Book");

            entity.HasOne(d => d.Loan).WithMany(p => p.BookLoans)
                .HasForeignKey(d => d.LoanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookLoan_Loan");
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Loan__3214EC27E0AA82D1");

            entity.ToTable("Loan");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.LoanStatusId).HasColumnName("LoanStatusID");
            entity.Property(e => e.ReaderId).HasColumnName("ReaderID");
            entity.Property(e => e.ReturnedAt).HasColumnType("datetime");

            entity.HasOne(d => d.LoanStatus).WithMany(p => p.Loans)
                .HasForeignKey(d => d.LoanStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Loan_LoanStatus");

            entity.HasOne(d => d.Reader).WithMany(p => p.Loans)
                .HasForeignKey(d => d.ReaderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Loan_Reader");
        });

        modelBuilder.Entity<LoanStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LoanStat__3214EC27F7D64138");

            entity.ToTable("LoanStatus");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Reader>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reader__3214EC274B474B20");

            entity.ToTable("Reader");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
