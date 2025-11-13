using BookingCareManagement.Domain.Aggregates.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingCareManagement.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> e)
    {
        e.ToTable("Services");
        e.HasKey(x => x.Id);
        e.Property(x => x.Name).IsRequired().HasMaxLength(200);
        e.Property(s => s.Price).HasColumnType("decimal(18,2)");

        // THÊM 2 DÒNG NÀY:
        e.Property(x => x.Color).HasMaxLength(10); // Cho mã hex #RRGGBB
        e.Property(x => x.ImageUrl).HasMaxLength(500);

        // Cấu hình quan hệ 1-N với Specialty (Danh mục) (CÓ THỂ BẠN ĐÃ CÓ)
        e.HasOne(s => s.Specialty)
         .WithMany() // Nếu Specialty không cần list Services
         .HasForeignKey(s => s.SpecialtyId);

        // THÊM CẤU HÌNH M-N VỚI DOCTOR:
        // Tự động tạo bảng "DoctorServices"
        e.HasMany(s => s.Doctors)
         .WithMany(d => d.Services)
         .UsingEntity(j => j.ToTable("DoctorServices"));
    }
}