using System;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.HasOne(a => a.Enrollment)
               .WithMany(e => e.Attendances)
               .HasForeignKey(a => a.EnrollmentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
