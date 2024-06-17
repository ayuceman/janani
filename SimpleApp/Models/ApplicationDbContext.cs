using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SimpleApp.Constants;

namespace SimpleApp.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<ImageCategory> ImageCategories { get; set; }

    public virtual DbSet<ImageDetail> ImageDetails { get; set; }

    public virtual DbSet<IncomeExpense> IncomeExpenses { get; set; }

    public virtual DbSet<IncomeExpensesLog> IncomeExpensesLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(ConnectionConstants.ConnectionStr);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.Property(e => e.RoleId).IsRequired();

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.Property(e => e.UserId).IsRequired();

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);
            entity.Property(e => e.UserId).IsRequired();

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC074B1F866F");

            entity.ToTable("Category");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
        });

        modelBuilder.Entity<ImageCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ImageCat__3214EC07663916AD");

            entity.ToTable("ImageCategory");

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
        });

        modelBuilder.Entity<ImageDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ImageDet__3214EC078D13E6C6");

            entity.Property(e => e.AlternativeText).HasMaxLength(500);
            entity.Property(e => e.ImageName)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.UploadedTs)
                .HasColumnType("datetime")
                .HasColumnName("UploadedTS");

            entity.HasOne(d => d.ImageCategory).WithMany(p => p.ImageDetails)
                .HasForeignKey(d => d.ImageCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ImageDeta__Image__2D27B809");
        });

        modelBuilder.Entity<IncomeExpense>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__IncomeEx__3214EC0703C75A40");

            entity.Property(e => e.CurrentMonthAmount).HasColumnType("money");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.NextMonthAmount).HasColumnType("money");

            entity.HasOne(d => d.Category).WithMany(p => p.IncomeExpenses)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__IncomeExp__Categ__2E1BDC42");

            entity.HasOne(d => d.IncomeExpensesLog).WithMany(p => p.IncomeExpenses)
                .HasForeignKey(d => d.IncomeExpensesLogId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__IncomeExp__Incom__2F10007B");
        });

        modelBuilder.Entity<IncomeExpensesLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__IncomeEx__3214EC073A60944C");

            entity.ToTable("IncomeExpensesLog");

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(e => e.CreatedTs)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("CreatedTS");
            entity.Property(e => e.Date).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
