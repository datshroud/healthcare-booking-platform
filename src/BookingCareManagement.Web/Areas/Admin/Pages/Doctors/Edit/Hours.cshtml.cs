using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Doctors.Commands;
using BookingCareManagement.Application.Features.Doctors.Queries;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookingCareManagement.Web.Areas.Admin.Pages.Doctors.Edit;

public class HoursModel : PageModel
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly GetDoctorWorkingHoursQueryHandler _getHoursHandler;
    private readonly UpdateDoctorHoursCommandHandler _updateHoursHandler;

    private static readonly DayOfWeek[] OrderedDays =
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday,
        DayOfWeek.Sunday
    };

    private static readonly IReadOnlyDictionary<DayOfWeek, string> DayDisplayNames = new Dictionary<DayOfWeek, string>
    {
        { DayOfWeek.Monday, "Thứ Hai" },
        { DayOfWeek.Tuesday, "Thứ Ba" },
        { DayOfWeek.Wednesday, "Thứ Tư" },
        { DayOfWeek.Thursday, "Thứ Năm" },
        { DayOfWeek.Friday, "Thứ Sáu" },
        { DayOfWeek.Saturday, "Thứ Bảy" },
        { DayOfWeek.Sunday, "Chủ Nhật" }
    };

    public HoursModel(
        IDoctorRepository doctorRepository,
        UserManager<AppUser> userManager,
        GetDoctorWorkingHoursQueryHandler getHoursHandler,
        UpdateDoctorHoursCommandHandler updateHoursHandler)
    {
        _doctorRepository = doctorRepository;
        _userManager = userManager;
        _getHoursHandler = getHoursHandler;
        _updateHoursHandler = updateHoursHandler;
    }

    [BindProperty(SupportsGet = true)]
    public Guid DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    [BindProperty]
    public WorkingHoursForm Form { get; set; } = WorkingHoursForm.CreateDefault();

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public ModalSlotInput ModalInput { get; set; } = new();

    public string GetDayDisplayName(int dayOfWeek)
    {
        var day = (DayOfWeek)dayOfWeek;
        return DayDisplayNames.TryGetValue(day, out var display) ? display : day.ToString();
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!await LoadDoctorAsync(cancellationToken))
        {
            return NotFound();
        }

        var dto = await _getHoursHandler.Handle(new GetDoctorWorkingHoursQuery(DoctorId), cancellationToken);

        Form = WorkingHoursForm.CreateDefault();
        Form.LimitAppointments = dto.LimitAppointments;

        foreach (var day in Form.Days)
        {
            var matches = dto.Hours
                .Where(h => h.DayOfWeek == day.DayOfWeek)
                .OrderBy(h => h.StartTime)
                .ToList();

            if (matches.Count == 0)
            {
                day.Slots.Clear();
                continue;
            }

            day.IsEnabled = true;
            day.Slots = matches
                .Select(m => new WorkingHourSlotInput
                {
                    Id = m.Id,
                    StartTime = m.StartTime,
                    EndTime = m.EndTime,
                    Location = m.Location
                })
                .ToList();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? command, CancellationToken cancellationToken)
    {
        if (!await LoadDoctorAsync(cancellationToken))
        {
            return NotFound();
        }

        Form ??= WorkingHoursForm.CreateDefault();
        Form.Normalize(OrderedDays);
        ModalInput ??= new ModalSlotInput();

        if (string.IsNullOrWhiteSpace(command))
        {
            ModelState.AddModelError(string.Empty, "Không xác định hành động.");
            return Page();
        }

        var sections = command.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (sections.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Không xác định hành động.");
            return Page();
        }

        switch (sections[0])
        {
            case "add-slot":
                return HandleAddSlot();
            case "delete-slot":
                return HandleDeleteSlot(sections);
            case "edit-slot":
                return HandleEditSlot();
            case "copy-day":
                return HandleCopyDay(sections);
            case "save":
                return await HandleSaveAsync(cancellationToken);
            default:
                ModelState.AddModelError(string.Empty, "Hành động không được hỗ trợ.");
                return Page();
        }
    }

    private async Task<IActionResult> HandleSaveAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < Form.Days.Count; i++)
        {
            var day = Form.Days[i];
            day.Trim();

            if (!OrderedDays.Contains((DayOfWeek)day.DayOfWeek))
            {
                ModelState.AddModelError(string.Empty, $"Ngày không hợp lệ tại vị trí {i + 1}.");
                continue;
            }

            if (!day.IsEnabled)
            {
                day.Slots.Clear();
                continue;
            }

            if (day.Slots.Count == 0)
            {
                ModelState.AddModelError(string.Empty, $"Vui lòng thêm ít nhất một khung giờ cho {GetDayDisplayName(day.DayOfWeek)}.");
                continue;
            }

            var parsedSlots = new List<(TimeSpan Start, TimeSpan End, int Index)>();

            for (var j = 0; j < day.Slots.Count; j++)
            {
                var slot = day.Slots[j];
                slot.Trim();

                if (!TimeSpan.TryParse(slot.StartTime, out var start))
                {
                    ModelState.AddModelError($"Form.Days[{i}].Slots[{j}].StartTime", "Thời gian bắt đầu không hợp lệ (định dạng HH:mm).");
                    continue;
                }

                if (!TimeSpan.TryParse(slot.EndTime, out var end))
                {
                    ModelState.AddModelError($"Form.Days[{i}].Slots[{j}].EndTime", "Thời gian kết thúc không hợp lệ (định dạng HH:mm).");
                    continue;
                }

                if (start >= end)
                {
                    ModelState.AddModelError($"Form.Days[{i}].Slots[{j}].StartTime", "Thời gian bắt đầu phải sớm hơn thời gian kết thúc.");
                    continue;
                }

                parsedSlots.Add((start, end, j));
            }

            var ordered = parsedSlots.OrderBy(p => p.Start).ToList();
            for (var j = 1; j < ordered.Count; j++)
            {
                var previous = ordered[j - 1];
                var current = ordered[j];
                if (current.Start < previous.End)
                {
                    ModelState.AddModelError(string.Empty, $"Các khung giờ của {GetDayDisplayName(day.DayOfWeek)} không được chồng lấn.");
                    break;
                }
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var command = new UpdateDoctorHoursCommand
        {
            DoctorId = DoctorId,
            LimitAppointments = Form.LimitAppointments,
            Hours = Form.Days
                .Where(d => d.IsEnabled)
                .SelectMany(d => d.Slots.Select(s => new UpdateDoctorHoursSlotRequest
                {
                    DayOfWeek = d.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Location = s.Location
                }))
                .ToArray()
        };

        try
        {
            await _updateHoursHandler.Handle(command, cancellationToken);
            StatusMessage = "Cập nhật giờ làm việc thành công.";
            return RedirectToPage(new { doctorId = DoctorId });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }

    private IActionResult HandleAddSlot()
    {
        if (ModalInput.DayIndex < 0 || ModalInput.DayIndex >= Form.Days.Count)
        {
            ModelState.AddModelError(string.Empty, "Chỉ số ngày không hợp lệ.");
            return Page();
        }

        var startRaw = ModalInput.StartTime?.Trim();
        var endRaw = ModalInput.EndTime?.Trim();

        if (string.IsNullOrWhiteSpace(startRaw) || string.IsNullOrWhiteSpace(endRaw))
        {
            ModelState.AddModelError(string.Empty, "Vui lòng chọn đầy đủ thời gian bắt đầu và kết thúc.");
            return Page();
        }

        if (!TimeSpan.TryParse(startRaw, out var start) || !TimeSpan.TryParse(endRaw, out var end))
        {
            ModelState.AddModelError(string.Empty, "Thời gian không hợp lệ (định dạng HH:mm).");
            return Page();
        }

        if (start >= end)
        {
            ModelState.AddModelError(string.Empty, "Thời gian bắt đầu phải sớm hơn thời gian kết thúc.");
            return Page();
        }

        if (start.Minutes % 30 != 0 || end.Minutes % 30 != 0)
        {
            ModelState.AddModelError(string.Empty, "Thời gian chỉ hỗ trợ bước 30 phút.");
            return Page();
        }

        ModalInput.SlotIndex = null;

        var day = Form.Days[ModalInput.DayIndex];

        var startString = start.ToString("hh\\:mm");
        var endString = end.ToString("hh\\:mm");

        if (day.Slots.Any(s => string.Equals(s.StartTime, startString, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(string.Empty, $"Khung giờ bắt đầu {startString} đã tồn tại trong {GetDayDisplayName(day.DayOfWeek)}.");
            return Page();
        }

        if (day.Slots.Any(s => string.Equals(s.EndTime, endString, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(string.Empty, $"Khung giờ kết thúc {endString} đã tồn tại trong {GetDayDisplayName(day.DayOfWeek)}.");
            return Page();
        }

        var location = string.IsNullOrWhiteSpace(ModalInput.Location) ? null : ModalInput.Location.Trim();

        day.IsEnabled = true;
        day.Slots.Add(new WorkingHourSlotInput
        {
            StartTime = startString,
            EndTime = endString,
            Location = location
        });

        ModelState.Clear();
        return Page();
    }

    private IActionResult HandleEditSlot()
    {
        if (ModalInput.DayIndex < 0 || ModalInput.DayIndex >= Form.Days.Count)
        {
            ModelState.AddModelError(string.Empty, "Chỉ số ngày không hợp lệ.");
            return Page();
        }

        if (ModalInput.SlotIndex is null)
        {
            ModelState.AddModelError(string.Empty, "Không xác định được khung giờ để chỉnh sửa.");
            return Page();
        }

        var slotIndex = ModalInput.SlotIndex.Value;
        var day = Form.Days[ModalInput.DayIndex];

        if (slotIndex < 0 || slotIndex >= day.Slots.Count)
        {
            ModelState.AddModelError(string.Empty, "Chỉ số khung giờ không hợp lệ.");
            return Page();
        }

        var startRaw = ModalInput.StartTime?.Trim();
        var endRaw = ModalInput.EndTime?.Trim();

        if (string.IsNullOrWhiteSpace(startRaw) || string.IsNullOrWhiteSpace(endRaw))
        {
            ModelState.AddModelError(string.Empty, "Vui lòng chọn đầy đủ thời gian bắt đầu và kết thúc.");
            return Page();
        }

        if (!TimeSpan.TryParse(startRaw, out var start) || !TimeSpan.TryParse(endRaw, out var end))
        {
            ModelState.AddModelError(string.Empty, "Thời gian không hợp lệ (định dạng HH:mm).");
            return Page();
        }

        if (start >= end)
        {
            ModelState.AddModelError(string.Empty, "Thời gian bắt đầu phải sớm hơn thời gian kết thúc.");
            return Page();
        }

        if (start.Minutes % 30 != 0 || end.Minutes % 30 != 0)
        {
            ModelState.AddModelError(string.Empty, "Thời gian chỉ hỗ trợ bước 30 phút.");
            return Page();
        }

        var startString = start.ToString("hh\\:mm");
        var endString = end.ToString("hh\\:mm");

        if (day.Slots.Where((_, index) => index != slotIndex).Any(s => string.Equals(s.StartTime, startString, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(string.Empty, $"Khung giờ bắt đầu {startString} đã tồn tại trong {GetDayDisplayName(day.DayOfWeek)}.");
            return Page();
        }

        if (day.Slots.Where((_, index) => index != slotIndex).Any(s => string.Equals(s.EndTime, endString, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(string.Empty, $"Khung giờ kết thúc {endString} đã tồn tại trong {GetDayDisplayName(day.DayOfWeek)}.");
            return Page();
        }

        var location = string.IsNullOrWhiteSpace(ModalInput.Location) ? null : ModalInput.Location.Trim();
        var slot = day.Slots[slotIndex];
        slot.StartTime = startString;
        slot.EndTime = endString;
        slot.Location = location;

        day.IsEnabled = true;

        ModelState.Clear();
        return Page();
    }

    private IActionResult HandleDeleteSlot(IReadOnlyList<string> sections)
    {
        if (sections.Count < 3 ||
            !int.TryParse(sections[1], out var dayIndex) ||
            !int.TryParse(sections[2], out var slotIndex))
        {
            ModelState.AddModelError(string.Empty, "Không xác định được khung giờ cần xóa.");
            return Page();
        }

        if (dayIndex < 0 || dayIndex >= Form.Days.Count)
        {
            ModelState.AddModelError(string.Empty, "Chỉ số ngày không hợp lệ.");
            return Page();
        }

        var day = Form.Days[dayIndex];

        if (slotIndex < 0 || slotIndex >= day.Slots.Count)
        {
            ModelState.AddModelError(string.Empty, "Chỉ số khung giờ không hợp lệ.");
            return Page();
        }

        day.Slots.RemoveAt(slotIndex);
        if (day.Slots.Count == 0)
        {
            day.IsEnabled = false;
        }

        ModelState.Clear();
        return Page();
    }

    private IActionResult HandleCopyDay(IReadOnlyList<string> sections)
    {
        if (sections.Count < 2 || !int.TryParse(sections[1], out var sourceIndex))
        {
            ModelState.AddModelError(string.Empty, "Không xác định được ngày nguồn để sao chép.");
            return Page();
        }

        if (sourceIndex < 0 || sourceIndex >= Form.Days.Count)
        {
            ModelState.AddModelError(string.Empty, "Chỉ số ngày không hợp lệ.");
            return Page();
        }

        var source = Form.Days[sourceIndex];
        source.Trim();

        if (!source.IsEnabled || source.Slots.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Ngày nguồn phải có ít nhất một khung giờ để sao chép.");
            return Page();
        }

        for (var i = 0; i < Form.Days.Count; i++)
        {
            if (i == sourceIndex)
            {
                continue;
            }

            var target = Form.Days[i];
            target.IsEnabled = true;
            target.Slots = source.Slots
                .Select(s => new WorkingHourSlotInput
                {
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Location = s.Location
                })
                .ToList();
        }

        ModelState.Clear();
        return Page();
    }

    private async Task<bool> LoadDoctorAsync(CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(DoctorId, cancellationToken);
        if (doctor is null)
        {
            return false;
        }

        var user = await _userManager.FindByIdAsync(doctor.AppUserId);
        var displayName = user?.GetFullName();
        DoctorName = string.IsNullOrWhiteSpace(displayName) ? (user?.Email ?? "Bác sĩ") : displayName;
        return true;
    }

    public class WorkingHoursForm
    {
        public bool LimitAppointments { get; set; }
        public List<DayScheduleInput> Days { get; set; } = new();

        public static WorkingHoursForm CreateDefault()
        {
            var form = new WorkingHoursForm();
            foreach (var day in OrderedDays)
            {
                form.Days.Add(new DayScheduleInput
                {
                    DayOfWeek = (int)day,
                    IsEnabled = false,
                    Slots = new List<WorkingHourSlotInput>()
                });
            }

            return form;
        }

        public void Normalize(IEnumerable<DayOfWeek> orderedDays)
        {
            var lookup = Days
                .GroupBy(d => d.DayOfWeek)
                .ToDictionary(g => g.Key, g => g.First());

            Days = orderedDays
                .Select(d =>
                {
                    var key = (int)d;
                    if (!lookup.TryGetValue(key, out var existing))
                    {
                        existing = new DayScheduleInput
                        {
                            DayOfWeek = key,
                            IsEnabled = false,
                            Slots = new List<WorkingHourSlotInput>()
                        };
                    }
                    else if (existing.Slots == null)
                    {
                        existing.Slots = new List<WorkingHourSlotInput>();
                    }

                    return existing;
                })
                .ToList();
        }
    }

    public class DayScheduleInput
    {
        public int DayOfWeek { get; set; }
        public bool IsEnabled { get; set; }
        public List<WorkingHourSlotInput> Slots { get; set; } = new();

        public void Trim()
        {
            foreach (var slot in Slots)
            {
                slot.Trim();
            }
        }
    }

    public class WorkingHourSlotInput
    {
        public Guid? Id { get; set; }

        [Display(Name = "Giờ bắt đầu")]
        [Required(ErrorMessage = "Vui lòng nhập giờ bắt đầu.")]
        public string StartTime { get; set; } = string.Empty;

        [Display(Name = "Giờ kết thúc")]
        [Required(ErrorMessage = "Vui lòng nhập giờ kết thúc.")]
        public string EndTime { get; set; } = string.Empty;

        [Display(Name = "Địa điểm")]
        [StringLength(128, ErrorMessage = "Địa điểm không được vượt quá 128 ký tự.")]
        public string? Location { get; set; }

        public void Trim()
        {
            StartTime = StartTime?.Trim() ?? string.Empty;
            EndTime = EndTime?.Trim() ?? string.Empty;
            Location = string.IsNullOrWhiteSpace(Location) ? null : Location.Trim();
        }
    }

    public class ModalSlotInput
    {
        public int DayIndex { get; set; }
        public int? SlotIndex { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string? Location { get; set; }
    }
}
