using BookingCareManagement.Application.Features.Doctors.Dtos;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Doctor;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;       // Cần cho CancellationToken
using System.Threading.Tasks;   // Cần cho Task

namespace BookingCareManagement.Application.Features.Doctors.Commands
{
    // 1. Command DTO: Định nghĩa dữ liệu
    public class CreateDoctorCommand
    {
        // Thông tin cho AppUser
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // ⭐️ THÊM THUỘC TÍNH NÀY ⭐️
        public string? AvatarUrl { get; set; } // Thuộc tính để nhận link ảnh

        // Thông tin cho Doctor
        public IEnumerable<Guid> SpecialtyIds { get; set; } = new List<Guid>();
    }

    // 2. Handler: Logic nghiệp vụ
    public class CreateDoctorCommandHandler
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly ISpecialtyRepository _specialtyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public CreateDoctorCommandHandler(
            IDoctorRepository doctorRepository,
            ISpecialtyRepository specialtyRepository,
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager)
        {
            _doctorRepository = doctorRepository;
            _specialtyRepository = specialtyRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<DoctorDto> Handle(CreateDoctorCommand command, CancellationToken cancellationToken)
        {
            // 1. Tạo AppUser trước
            var appUser = new AppUser
            {
                FirstName = command.FirstName,
                LastName = command.LastName,
                Email = command.Email,
                UserName = command.Email,
                PhoneNumber = command.PhoneNumber,
                EmailConfirmed = true, // Tạm thời để true cho dễ test

                // ⭐️ THAY ĐỔI Ở ĐÂY ⭐️
                AvatarUrl = command.AvatarUrl // Gán AvatarUrl từ command
            };

            // Tạo mật khẩu tự động
            var emailLocalPart = command.Email.Split('@')[0];
            var defaultPassword = $"{emailLocalPart}@123";

            var result = await _userManager.CreateAsync(appUser, defaultPassword);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // 2. Gán Role "Doctor"
            await _userManager.AddToRoleAsync(appUser, "Doctor");

            // 3. Tạo Doctor Entity
            var doctor = new Doctor(appUser.Id);

            // 4. Tìm và thêm chuyên khoa (chỉ 1)
            if (command.SpecialtyIds.Any())
            {
                var specialties = await _specialtyRepository.GetByIdsAsync(command.SpecialtyIds, cancellationToken);
                foreach (var specialty in specialties)
                {
                    doctor.AddSpecialty(specialty);
                }
            }

            // 5. Thêm Doctor vào Repository
            _doctorRepository.Add(doctor);

            // 6. Lưu tất cả thay đổi
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 7. Map sang DTO để trả về
            return new DoctorDto
            {
                Id = doctor.Id,
                AppUserId = doctor.AppUserId,
                FirstName = appUser.FirstName ?? string.Empty,
                LastName = appUser.LastName ?? string.Empty,
                FullName = appUser.GetFullName(), // Cần using BookingCareManagement.Domain.Aggregates.User
                Email = appUser.Email ?? string.Empty,
                PhoneNumber = appUser.PhoneNumber ?? string.Empty,
                Active = doctor.Active,
                AvatarUrl = appUser.AvatarUrl ?? string.Empty, // ⭐️ SẼ HIỂN THỊ ẢNH MỚI ⭐️
                Specialties = doctor.Specialties.Select(s => s.Name)
            };
        }
    }
}