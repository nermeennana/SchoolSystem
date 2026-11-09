using System;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data.Configurations;

namespace Persistence.Data;

public class SchoolDbContext : DbContext
{
    public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options)
    {
        
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Catch all configurations in the assembly and apply them
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchoolDbContext).Assembly);

        // modelBuilder.ApplyConfiguration(new SubjectConfiguration());
    }
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Teacher> Teachers { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;
    public DbSet<Enrollment> Enrollments { get; set; } = null!;
    public DbSet<Attendance> Attendances { get; set; } = null!;

}
