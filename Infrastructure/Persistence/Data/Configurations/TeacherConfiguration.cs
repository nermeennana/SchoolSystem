using System;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Address).IsRequired().HasMaxLength(200);
        builder.Property(t => t.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(t => t.UserId).IsRequired().HasMaxLength(450);

        builder.HasOne(t => t.Subject)
               .WithMany()
               .HasForeignKey(t => t.SubjectId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.User)
               .WithOne()
               .HasForeignKey<Teacher>(t => t.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
