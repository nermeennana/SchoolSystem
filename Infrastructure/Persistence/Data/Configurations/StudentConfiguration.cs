using System;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Address).IsRequired().HasMaxLength(200);
        builder.Property(s => s.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(s => s.ParentPhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(s => s.UserId).IsRequired().HasMaxLength(450);
    }
}
