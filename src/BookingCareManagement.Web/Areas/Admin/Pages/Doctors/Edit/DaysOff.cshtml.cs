using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Doctors.Commands;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DoctorAggregate = BookingCareManagement.Domain.Aggregates.Doctor.Doctor;
using DoctorDayOffAggregate = BookingCareManagement.Domain.Aggregates.Doctor.DoctorDayOff;

namespace BookingCareManagement.Web.Areas.Admin.Pages.Doctors.Edit;

public class DaysOffModel : PageModel
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly CreateDoctorDayOffCommandHandler _createHandler;
    private readonly UpdateDoctorDayOffCommandHandler _updateHandler;
    private readonly DeleteDoctorDayOffCommandHandler _deleteHandler;

    private DoctorAggregate? _doctor;

    public DaysOffModel(
        IDoctorRepository doctorRepository,
        UserManager<AppUser> userManager,
        CreateDoctorDayOffCommandHandler createHandler,
        UpdateDoctorDayOffCommandHandler updateHandler,
        DeleteDoctorDayOffCommandHandler deleteHandler)
    {
        _doctorRepository = doctorRepository;
        _userManager = userManager;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
    }

    [BindProperty(SupportsGet = true)]
    public Guid DoctorId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Year { get; set; }

    public int SelectedYear { get; private set; }

    public string DoctorName { get; private set; } = string.Empty;

    public List<DoctorDayOffViewModel> DayOffs { get; private set; } = new();

    public IReadOnlyList<int> YearOptions { get; private set; } = Array.Empty<int>();

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public DayOffModalInput AddInput { get; set; } = new();

    [BindProperty]
    public DayOffModalInput EditInput { get; set; } = new();

    [BindProperty]
    public Guid? EditingId { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        SelectedYear = ResolveYear();

        if (!await LoadDoctorAsync(cancellationToken))
        {
            return NotFound();
        }

        PopulateDayOffs();
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(CancellationToken cancellationToken)
    {
        SelectedYear = ResolveYear();

        if (!await LoadDoctorAsync(cancellationToken))
        {
            return NotFound();
        }

        var validation = ValidateModalInput(AddInput, nameof(AddInput));
        if (!validation.Success)
        {
            PopulateDayOffs();
            ViewData["OpenAddModal"] = true;
            return Page();
        }

        try
        {
            await _createHandler.Handle(
                new CreateDoctorDayOffCommand(DoctorId, validation.Name!, validation.Start, validation.End, AddInput.RepeatYearly),
                cancellationToken);

            StatusMessage = "Thêm ngày nghỉ thành công.";
            return RedirectToPage(new { doctorId = DoctorId, year = SelectedYear });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError($"{nameof(AddInput)}.{nameof(DayOffModalInput.Name)}", ex.Message);
            PopulateDayOffs();
            ViewData["OpenAddModal"] = true;
            return Page();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError($"{nameof(AddInput)}.{nameof(DayOffModalInput.Name)}", ex.Message);
            PopulateDayOffs();
            ViewData["OpenAddModal"] = true;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostEditAsync(CancellationToken cancellationToken)
    {
        SelectedYear = ResolveYear();

        if (!await LoadDoctorAsync(cancellationToken))
        {
            return NotFound();
        }

        if (EditingId is null || EditingId == Guid.Empty)
        {
            ModelState.AddModelError(string.Empty, "Không xác định được ngày nghỉ để cập nhật.");
            PopulateDayOffs();
            ViewData["OpenEditModal"] = true;
            return Page();
        }

        var validation = ValidateModalInput(EditInput, nameof(EditInput));
        if (!validation.Success)
        {
            PopulateDayOffs();
            ViewData["OpenEditModal"] = true;
            return Page();
        }

        try
        {
            await _updateHandler.Handle(
                new UpdateDoctorDayOffCommand(DoctorId, EditingId.Value, validation.Name!, validation.Start, validation.End, EditInput.RepeatYearly),
                cancellationToken);

            StatusMessage = "Cập nhật ngày nghỉ thành công.";
            return RedirectToPage(new { doctorId = DoctorId, year = SelectedYear });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError($"{nameof(EditInput)}.{nameof(DayOffModalInput.Name)}", ex.Message);
            PopulateDayOffs();
            ViewData["OpenEditModal"] = true;
            return Page();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError($"{nameof(EditInput)}.{nameof(DayOffModalInput.Name)}", ex.Message);
            PopulateDayOffs();
            ViewData["OpenEditModal"] = true;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid dayOffId, CancellationToken cancellationToken)
    {
        SelectedYear = ResolveYear();

        if (!await LoadDoctorAsync(cancellationToken))
        {
            return NotFound();
        }

        try
        {
            await _deleteHandler.Handle(new DeleteDoctorDayOffCommand(DoctorId, dayOffId), cancellationToken);
            StatusMessage = "Đã xóa ngày nghỉ.";
            return RedirectToPage(new { doctorId = DoctorId, year = SelectedYear });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    private async Task<bool> LoadDoctorAsync(CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(DoctorId, cancellationToken);
        if (doctor == null)
        {
            return false;
        }

        _doctor = doctor;

        var user = await _userManager.FindByIdAsync(doctor.AppUserId);
        var displayName = user?.GetFullName();
        DoctorName = string.IsNullOrWhiteSpace(displayName) ? (user?.Email ?? "Bác sĩ") : displayName!;
        return true;
    }

    private void PopulateDayOffs()
    {
        DayOffs = new List<DoctorDayOffViewModel>();
        if (_doctor == null)
        {
            return;
        }

        var items = new List<DoctorDayOffViewModel>();

        foreach (var dayOff in _doctor.DaysOff.OrderBy(d => d.StartDate))
        {
            if (TryProjectDayOff(dayOff, SelectedYear, out var projected))
            {
                items.Add(projected);
            }
        }

        DayOffs = items
            .OrderBy(d => d.DisplayStart)
            .ThenBy(d => d.Name)
            .ToList();

        YearOptions = BuildYearOptions();
    }

    private bool TryProjectDayOff(DoctorDayOffAggregate dayOff, int year, out DoctorDayOffViewModel projected)
    {
        projected = null!;

        if (dayOff.RepeatYearly)
        {
            var start = CreateDateForYear(year, dayOff.StartDate);
            var end = CreateDateForYear(year, dayOff.EndDate);
            if (end < start)
            {
                end = start;
            }

            projected = new DoctorDayOffViewModel(dayOff.Id, dayOff.Name, dayOff.StartDate, dayOff.EndDate, true, start, end);
            return true;
        }

        if (year < dayOff.StartDate.Year || year > dayOff.EndDate.Year)
        {
            return false;
        }

        projected = new DoctorDayOffViewModel(dayOff.Id, dayOff.Name, dayOff.StartDate, dayOff.EndDate, false, dayOff.StartDate, dayOff.EndDate);
        return true;
    }

    private IReadOnlyList<int> BuildYearOptions()
    {
        var baseYear = SelectedYear;
        var start = (baseYear / 10) * 10;
        var years = new List<int>(10);
        for (var i = 0; i < 10; i++)
        {
            years.Add(start + i);
        }

        return years;
    }

    private static DateOnly CreateDateForYear(int year, DateOnly source)
    {
        var day = Math.Min(source.Day, DateTime.DaysInMonth(year, source.Month));
        return new DateOnly(year, source.Month, day);
    }

    private (bool Success, string? Name, DateOnly Start, DateOnly End) ValidateModalInput(DayOffModalInput input, string prefix)
    {
        var name = input.Name?.Trim();
        var valid = true;

        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError($"{prefix}.{nameof(DayOffModalInput.Name)}", "Vui lòng nhập tên ngày nghỉ.");
            valid = false;
        }
        else
        {
            input.Name = name;
        }

        var startParsed = TryParseDate(input.StartDate, out var startDate);
        if (!startParsed)
        {
            ModelState.AddModelError($"{prefix}.{nameof(DayOffModalInput.StartDate)}", "Ngày bắt đầu không hợp lệ.");
            valid = false;
        }

        var endParsed = TryParseDate(input.EndDate, out var endDate);
        if (!endParsed)
        {
            ModelState.AddModelError($"{prefix}.{nameof(DayOffModalInput.EndDate)}", "Ngày kết thúc không hợp lệ.");
            valid = false;
        }

        if (startParsed && endParsed && startDate > endDate)
        {
            ModelState.AddModelError($"{prefix}.{nameof(DayOffModalInput.StartDate)}", "Ngày bắt đầu phải trước hoặc bằng ngày kết thúc.");
            valid = false;
        }

        return (valid, name, startDate, endDate);
    }

    private static bool TryParseDate(string? value, out DateOnly date)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            date = default;
            return false;
        }

        return DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    private int ResolveYear()
    {
        if (Year.HasValue && Year.Value > 0)
        {
            return Year.Value;
        }

        return DateTime.UtcNow.Year;
    }

    public record DoctorDayOffViewModel(Guid Id, string Name, DateOnly StartDate, DateOnly EndDate, bool RepeatYearly, DateOnly DisplayStart, DateOnly DisplayEnd)
    {
        public string DisplayRange => $"{DisplayStart.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} - {DisplayEnd.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}";
        public string StartForEdit => StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        public string EndForEdit => EndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public class DayOffModalInput
    {
        [Required(ErrorMessage = "Vui lòng nhập tên ngày nghỉ.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
        public string? StartDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc.")]
        public string? EndDate { get; set; }

        public bool RepeatYearly { get; set; }
    }
}
