using BookingCareManagement.Domain.Aggregates.Doctor;
using BookingCareManagement.Domain.Aggregates.User; // Cần cho AppUser
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingCareManagement.Infrastructure.Persistence.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> e)
    {
        e.ToTable("Doctors");
        e.HasKey(x => x.Id);

        // 1. Xóa cấu hình cho FullName
        // e.Property(x => x.FullName)... (Dòng cũ đã bị xóa)

        // 2. Cấu hình quan hệ 1-1 với AppUser
        // Bảng Doctor sẽ có cột AppUserId
        e.HasOne(d => d.AppUser)
         .WithOne() // Không cần thuộc tính điều hướng ngược lại từ AppUser
         .HasForeignKey<Doctor>(d => d.AppUserId)
         .OnDelete(DeleteBehavior.Cascade); // Tùy chọn: Nếu xóa AppUser thì xóa Doctor

        // 3. Cấu hình quan hệ N-N với Specialty (giữ nguyên)
        e.HasMany(d => d.Specialties)
         .WithMany(s => s.Doctors)
         .UsingEntity(j => j.ToTable("DoctorSpecialties"));

        e.Property(d => d.LimitAppointments)
         .HasDefaultValue(false);

        e.HasMany(d => d.WorkingHours)
         .WithOne(h => h.Doctor)
         .HasForeignKey(h => h.DoctorId)
         .OnDelete(DeleteBehavior.Cascade);

        e.Navigation(d => d.WorkingHours)
         .UsePropertyAccessMode(PropertyAccessMode.Field);

        e.HasMany(d => d.DaysOff)
         .WithOne(o => o.Doctor)
         .HasForeignKey(o => o.DoctorId)
         .OnDelete(DeleteBehavior.Cascade);

        e.Navigation(d => d.DaysOff)
         .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}