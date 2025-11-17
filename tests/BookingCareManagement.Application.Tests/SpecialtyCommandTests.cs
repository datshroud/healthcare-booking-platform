using System;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Specialties.Commands;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Infrastructure.Persistence;
using BookingCareManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookingCareManagement.Application.Tests
{
    public class SpecialtyCommandTests : IDisposable
    {
        private readonly ApplicationDBContext _context;
        private readonly ISpecialtyRepository _specialtyRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SpecialtyCommandTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDBContext(options);
            _specialtyRepository = new SpecialtyRepository(_context);
            _doctorRepository = new DoctorRepository(_context);
            _unitOfWork = new UnitOfWork(_context);
        }

        [Fact]
        public async Task Create_Update_Delete_Specialty_Flow_WritesToDatabase()
        {
            var createHandler = new CreateSpecialtyCommandHandler(_specialtyRepository, _doctorRepository, _unitOfWork);
            var createCommand = new CreateSpecialtyCommand
            {
                Name = "Chấn thương",
                Description = "Kiểm tra cơ xương khớp",
                Color = "#123456",
                Price = 150000
            };

            var created = await createHandler.Handle(createCommand, CancellationToken.None);
            Assert.NotEqual(Guid.Empty, created.Id);
            Assert.Equal(150000, created.Price);

            var updateHandler = new UpdateSpecialtyCommandHandler(_specialtyRepository, _doctorRepository, _unitOfWork);
            var updateCommand = new UpdateSpecialtyCommand
            {
                Id = created.Id,
                Name = "Chấn thương chỉnh hình",
                Slug = "chan-thuong-chinh-hinh",
                Description = "Điều trị nâng cao",
                Color = "#654321",
                Price = 200000,
                DoctorIds = Array.Empty<Guid>()
            };

            var updated = await updateHandler.Handle(updateCommand, CancellationToken.None);
            Assert.Equal("Chấn thương chỉnh hình", updated.Name);
            Assert.Equal("#654321", updated.Color);
            Assert.Equal(200000, updated.Price);

            var deleteHandler = new DeleteSpecialtyCommandHandler(_specialtyRepository, _unitOfWork);
            await deleteHandler.Handle(new DeleteSpecialtyCommand { Id = created.Id }, CancellationToken.None);

            var dbSpecialty = await _context.Specialties.FindAsync(created.Id);
            Assert.Null(dbSpecialty);
        }

        [Fact]
        public async Task UpdateSpecialty_Throws_WhenDoctorDoesNotExist()
        {
            var createHandler = new CreateSpecialtyCommandHandler(_specialtyRepository, _doctorRepository, _unitOfWork);
            var created = await createHandler.Handle(new CreateSpecialtyCommand
            {
                Name = "Cấp cứu",
                Description = "24/7",
                Price = 100000
            }, CancellationToken.None);

            var updateHandler = new UpdateSpecialtyCommandHandler(_specialtyRepository, _doctorRepository, _unitOfWork);
            var invalidDoctorId = Guid.NewGuid();

            await Assert.ThrowsAsync<ValidationException>(() => updateHandler.Handle(new UpdateSpecialtyCommand
            {
                Id = created.Id,
                Name = "Cấp cứu tổng hợp",
                DoctorIds = new[] { invalidDoctorId }
            }, CancellationToken.None));
        }

        [Fact]
        public async Task UpdateSpecialtyStatus_TogglesActiveFlag()
        {
            var createHandler = new CreateSpecialtyCommandHandler(_specialtyRepository, _doctorRepository, _unitOfWork);
            var created = await createHandler.Handle(new CreateSpecialtyCommand
            {
                Name = "Tai mũi họng",
                Price = 90000
            }, CancellationToken.None);

            var statusHandler = new UpdateSpecialtyStatusCommandHandler(_specialtyRepository, _unitOfWork);

            var deactivated = await statusHandler.Handle(new UpdateSpecialtyStatusCommand
            {
                Id = created.Id,
                Active = false
            }, CancellationToken.None);

            Assert.False(deactivated.Active);

            var reactivated = await statusHandler.Handle(new UpdateSpecialtyStatusCommand
            {
                Id = created.Id,
                Active = true
            }, CancellationToken.None);

            Assert.True(reactivated.Active);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
