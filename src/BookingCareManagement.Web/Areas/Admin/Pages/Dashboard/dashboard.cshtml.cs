using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.Application.Abstractions; // ⭐️ Cần cho ICustomerRepository
using BookingCareManagement.Domain.Aggregates.Doctor; // ⭐️ Cần cho IDoctorRepository
using BookingCareManagement.Domain.Aggregates.Appointment; // ⭐️ Cần cho IAppointmentRepository
using Microsoft.EntityFrameworkCore; // ⭐️ Cần cho .CountAsync()

namespace BookingCareManagement.Web.Areas.Admin.Pages.Dashboard
{
    public class dashboardModel : PageModel
    {
        // --- Tiêm (Inject) các Repository ---
        private readonly UserManager<AppUser> _userManager;
        private readonly ICustomerRepository _customerRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly ISpecialtyRepository _specialtyRepository;

        // Định nghĩa CultureInfo để sử dụng trong toàn bộ class
        private static readonly CultureInfo viVNCulture = CultureInfo.GetCultureInfo("vi-VN");

        public dashboardModel(
            UserManager<AppUser> userManager,
            ICustomerRepository customerRepository,
            IAppointmentRepository appointmentRepository,
            IDoctorRepository doctorRepository,
            ISpecialtyRepository specialtyRepository)
        {
            _userManager = userManager;
            _customerRepository = customerRepository;
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _specialtyRepository = specialtyRepository;
        }

        // --- Thuộc tính (Properties) cho Giao diện ---
        public string AdminName { get; set; } = "Admin";
        // Filter selections (independent per widget)
        private Dictionary<string, string> _filterSelections = new(StringComparer.OrdinalIgnoreCase);
        public IReadOnlyDictionary<string, string> FilterSelections => _filterSelections;

        public string CustomerFilter { get; private set; } = "week";
        public string CustomerFilterLabel { get; private set; } = "Tuần này";

        public string RevenueFilter { get; private set; } = "week";
        public string RevenueFilterLabel { get; private set; } = "Tuần này";

        public string PendingFilter { get; private set; } = "week";
        public string PendingFilterLabel { get; private set; } = "Tuần này";

        public string ChartFilter { get; private set; } = "week";
        public string ChartFilterLabel { get; private set; } = "Tuần này";

        public string AppointmentListFilter { get; private set; } = "week";
        public string AppointmentListFilterLabel { get; private set; } = "Tuần này";

        public string HeatmapFilter { get; private set; } = "month";
        public string HeatmapFilterLabel { get; private set; } = "Tháng này";

        public string PerformanceFilter { get; private set; } = "week";
        public string PerformanceFilterLabel { get; private set; } = "Tuần này";

        // Thẻ thống kê
        public int NewCustomerCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingAppointmentCount { get; set; } // THAY THẾ "Sự chiếm dụng"
        public int BookedAppointmentCount { get; set; }
        public int CancelledAppointmentCount { get; set; }

        // % Tăng/Giảm
        public double BookedApptChangePercent { get; set; }
        public double CancelledApptChangePercent { get; set; }

        // Biểu đồ (JS)
        public List<int> NewCustomerSparkline { get; set; } = new();
        public List<decimal> RevenueSparkline { get; set; } = new();
        public List<int> PendingSparkline { get; set; } = new(); // THAY THẾ "Sự chiếm dụng"
        public List<int> MainAppointmentChart { get; set; } = new();
        public List<string> MainAppointmentLabels { get; set; } = new();
        public int DonutNewCustomerPercent { get; set; }
        public int DonutReturningCustomerPercent { get; set; }

        // Bảng (HTML)
        public List<AppointmentDto> RecentAppointments { get; set; } = new();
        public List<PerformanceDto> EmployeePerformance { get; set; } = new();
        public List<PerformanceDto> ServicePerformance { get; set; } = new();

        // Heatmap
        public List<HeatmapCellDto> HeatmapData { get; set; } = new();
        public string HeatmapMonthLabel { get; set; } = string.Empty;


        public async Task OnGetAsync(
            [FromQuery] string customerFilter = "week",
            [FromQuery] string revenueFilter = "week",
            [FromQuery] string pendingFilter = "week",
            [FromQuery] string chartFilter = "week",
            [FromQuery] string heatmapFilter = "month",
            [FromQuery] string listFilter = "week",
            [FromQuery] string performanceFilter = "week",
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            AdminName = user?.GetFullName() ?? user?.Email ?? "Admin";

            CustomerFilter = NormalizeFilterKey(customerFilter);
            RevenueFilter = NormalizeFilterKey(revenueFilter);
            PendingFilter = NormalizeFilterKey(pendingFilter);
            ChartFilter = NormalizeFilterKey(chartFilter);
            HeatmapFilter = NormalizeFilterKey(heatmapFilter, "month");
            AppointmentListFilter = NormalizeFilterKey(listFilter);
            PerformanceFilter = NormalizeFilterKey(performanceFilter);

            _filterSelections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["customerFilter"] = CustomerFilter,
                ["revenueFilter"] = RevenueFilter,
                ["pendingFilter"] = PendingFilter,
                ["chartFilter"] = ChartFilter,
                ["heatmapFilter"] = HeatmapFilter,
                ["listFilter"] = AppointmentListFilter,
                ["performanceFilter"] = PerformanceFilter
            };

            var customerRange = GetDateRangeFromFilter(CustomerFilter);
            CustomerFilterLabel = customerRange.Label;

            var revenueRange = GetDateRangeFromFilter(RevenueFilter);
            RevenueFilterLabel = revenueRange.Label;

            var pendingRange = GetDateRangeFromFilter(PendingFilter);
            PendingFilterLabel = pendingRange.Label;

            var chartRange = GetDateRangeFromFilter(ChartFilter);
            ChartFilterLabel = chartRange.Label;

            var heatmapRange = GetDateRangeFromFilter(HeatmapFilter);
            HeatmapFilterLabel = heatmapRange.Label;
            HeatmapMonthLabel = heatmapRange.Start.ToString("MMMM 'năm' yyyy", viVNCulture);

            var listRange = GetDateRangeFromFilter(AppointmentListFilter);
            AppointmentListFilterLabel = listRange.Label;

            var performanceRange = GetDateRangeFromFilter(PerformanceFilter);
            PerformanceFilterLabel = performanceRange.Label;

            // 2. Tải TẤT CẢ dữ liệu liên quan MỘT LẦN
            var allCustomers = await _customerRepository.GetAllCustomersAsync(cancellationToken);
            var allAppointments = await _appointmentRepository.GetAllAsync(cancellationToken);
            var allDoctors = await _doctorRepository.GetAllAsync(cancellationToken);
            var allSpecialties = await _specialtyRepository.GetAllAsync(cancellationToken);


            // --- 3. TÍNH TOÁN KHÁCH HÀNG ---
            var customersInSelectedRange = allCustomers
                .Where(c => c.CreatedAt >= customerRange.Start && c.CreatedAt < customerRange.End)
                .ToList();
            NewCustomerCount = customersInSelectedRange.Count;

            var newCustomersByDay = customersInSelectedRange
                .GroupBy(c => c.CreatedAt.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            int customerSparklineDays = ResolveSparklineLength(CustomerFilter, customerRange.Start, customerRange.End);
            var sparklineCustomerData = new List<int>(customerSparklineDays);
            for (int i = 0; i < customerSparklineDays; i++)
            {
                var day = customerRange.Start.AddDays(i).Date;
                newCustomersByDay.TryGetValue(day, out int count);
                sparklineCustomerData.Add(count);
            }
            NewCustomerSparkline = sparklineCustomerData;

            // Donut Khách hàng (dựa trên khoảng thời gian của biểu đồ chính)
            var approvedAppointments = allAppointments
                .Where(a => a.Status == AppointmentStatus.Approved && !string.IsNullOrWhiteSpace(a.PatientId))
                .ToList();

            var firstAppointmentByPatient = approvedAppointments
                .GroupBy(a => a.PatientId!)
                .ToDictionary(g => g.Key, g => g.Min(a => a.StartUtc));

            var approvedInChartRange = approvedAppointments
                .Where(a => a.StartUtc >= chartRange.Start && a.StartUtc < chartRange.End)
                .ToList();

            var patientIdsInChartRange = approvedInChartRange
                .Select(a => a.PatientId!)
                .Distinct()
                .ToList();

            if (patientIdsInChartRange.Count > 0)
            {
                int newPatientCount = patientIdsInChartRange.Count(id =>
                    firstAppointmentByPatient.TryGetValue(id, out var firstSeen)
                        && firstSeen >= chartRange.Start && firstSeen < chartRange.End);

                newPatientCount = Math.Clamp(newPatientCount, 0, patientIdsInChartRange.Count);
                int returningPatientCount = Math.Max(patientIdsInChartRange.Count - newPatientCount, 0);
                int totalPatients = Math.Max(newPatientCount + returningPatientCount, 1);

                DonutNewCustomerPercent = (int)Math.Round((double)newPatientCount / totalPatients * 100);
                DonutReturningCustomerPercent = 100 - DonutNewCustomerPercent;
            }
            else
            {
                DonutNewCustomerPercent = 100;
                DonutReturningCustomerPercent = 0;
            }

            // --- 4. TÍNH TOÁN LỊCH HẸN & DOANH THU ---
            var chartAppointments = allAppointments
                .Where(a => a.StartUtc >= chartRange.Start && a.StartUtc < chartRange.End)
                .ToList();

            var chartAppointmentsPrevious = allAppointments
                .Where(a => a.StartUtc >= chartRange.PrevStart && a.StartUtc < chartRange.PrevEnd)
                .ToList();

            BookedAppointmentCount = chartAppointments.Count(a => a.Status == AppointmentStatus.Approved);
            CancelledAppointmentCount = chartAppointments.Count(a => a.Status == AppointmentStatus.Canceled);

            var revenueAppointments = allAppointments
                .Where(a => a.StartUtc >= revenueRange.Start && a.StartUtc < revenueRange.End)
                .ToList();
            TotalRevenue = revenueAppointments
                .Where(a => a.Status == AppointmentStatus.Approved)
                .Sum(a => a.Price);

            var pendingAppointments = allAppointments
                .Where(a => a.StartUtc >= pendingRange.Start && a.StartUtc < pendingRange.End)
                .ToList();
            PendingAppointmentCount = pendingAppointments.Count(a => a.Status == AppointmentStatus.Pending);

            // Sparkline Doanh thu
            var revenueByDay = revenueAppointments
                .Where(a => a.Status == AppointmentStatus.Approved)
                .GroupBy(a => a.StartUtc.Date)
                .ToDictionary(g => g.Key, g => g.Sum(a => a.Price));

            int revenueSparklineDays = ResolveSparklineLength(RevenueFilter, revenueRange.Start, revenueRange.End);
            var revenueSparklineData = new List<decimal>(revenueSparklineDays);
            for (int i = 0; i < revenueSparklineDays; i++)
            {
                var day = revenueRange.Start.AddDays(i).Date;
                revenueByDay.TryGetValue(day, out var sum);
                revenueSparklineData.Add(sum);
            }
            RevenueSparkline = revenueSparklineData;

            // Sparkline lịch hẹn đang chờ
            var pendingByDay = pendingAppointments
                .Where(a => a.Status == AppointmentStatus.Pending)
                .GroupBy(a => a.StartUtc.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            int pendingSparklineDays = ResolveSparklineLength(PendingFilter, pendingRange.Start, pendingRange.End);
            var pendingSparklineData = new List<int>(pendingSparklineDays);
            for (int i = 0; i < pendingSparklineDays; i++)
            {
                var day = pendingRange.Start.AddDays(i).Date;
                pendingByDay.TryGetValue(day, out var count);
                pendingSparklineData.Add(count);
            }
            PendingSparkline = pendingSparklineData;

            // % thay đổi so với kỳ trước
            int prevBookedCount = chartAppointmentsPrevious.Count(a => a.Status == AppointmentStatus.Approved);
            int prevCancelledCount = chartAppointmentsPrevious.Count(a => a.Status == AppointmentStatus.Canceled);
            BookedApptChangePercent = CalculatePercentageChange(BookedAppointmentCount, prevBookedCount);
            CancelledApptChangePercent = CalculatePercentageChange(CancelledAppointmentCount, prevCancelledCount);

            // Biểu đồ đường chính
            var appointmentsByDay = chartAppointments
                .Where(a => a.Status == AppointmentStatus.Approved)
                .GroupBy(a => a.StartUtc.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            MainAppointmentChart.Clear();
            MainAppointmentLabels.Clear();

            int chartDays = ResolveChartDayCount(ChartFilter, chartRange.Start, chartRange.End);
            for (int i = 0; i < chartDays; i++)
            {
                var day = chartRange.Start.Date.AddDays(i);
                appointmentsByDay.TryGetValue(day, out var count);
                MainAppointmentChart.Add(count);
                MainAppointmentLabels.Add(day.ToString(chartRange.ChartLabel, viVNCulture));
            }

            // Bảng "Cuộc hẹn gần đây"
            var appointmentsForList = allAppointments
                .Where(a => a.StartUtc >= listRange.Start && a.StartUtc < listRange.End)
                .OrderBy(a => a.StartUtc)
                .Take(10)
                .ToList();

            RecentAppointments = appointmentsForList
                .Join(allSpecialties,
                      app => app.SpecialtyId,
                      spec => spec.Id,
                      (app, spec) => new { app, spec })
                .Select(joined => new AppointmentDto
                {
                    Id = joined.app.Id,
                    Date = TimeZoneInfo.ConvertTimeFromUtc(joined.app.StartUtc, AppointmentStatusDisplay.DisplayTimeZone),
                    SpecialtyName = joined.spec.Name,
                    SpecialtyColor = string.IsNullOrWhiteSpace(joined.spec.Color) ? "#6c757d" : joined.spec.Color,
                    CustomerName = joined.app.PatientName,
                    Status = joined.app.Status,
                    AvatarUrl = $"https://i.pravatar.cc/150?u={joined.app.PatientId ?? joined.app.PatientName}",
                    StatusLabel = AppointmentStatusDisplay.GetLabel(joined.app.Status),
                    StatusCssClass = AppointmentStatusDisplay.GetCssClass(joined.app.Status),
                    StatusIcon = AppointmentStatusDisplay.GetIcon(joined.app.Status)
                })
                .ToList();

            // --- 5. TÍNH TOÁN HIỆU SUẤT ---
            var approvedApptsInPerformanceRange = allAppointments
                .Where(a => a.StartUtc >= performanceRange.Start && a.StartUtc < performanceRange.End && a.Status == AppointmentStatus.Approved)
                .ToList();

            EmployeePerformance = allDoctors
                .Select(doctor =>
                {
                    var doctorUser = doctor.AppUser;
                    var doctorName = doctorUser?.GetFullName() ?? doctorUser?.Email ?? "Bác sĩ";
                    var appts = approvedApptsInPerformanceRange.Where(a => a.DoctorId == doctor.Id).ToList();
                    return new PerformanceDto
                    {
                        Name = doctorName,
                        AvatarUrl = doctorUser is null ? GetAvatarUrl((string?)null, doctorName) : GetAvatarUrl(doctorUser),
                        Appointments = appts.Count,
                        Revenue = appts.Sum(a => a.Price)
                    };
                })
                .ToList();

            ServicePerformance = allSpecialties
                .Select(spec =>
                {
                    var serviceName = string.IsNullOrWhiteSpace(spec.Name) ? "Chuyên khoa" : spec.Name;
                    var appts = approvedApptsInPerformanceRange.Where(a => a.SpecialtyId == spec.Id).ToList();
                    return new PerformanceDto
                    {
                        Name = serviceName,
                        AvatarUrl = GetAvatarUrl(spec.ImageUrl, serviceName),
                        Appointments = appts.Count,
                        Revenue = appts.Sum(a => a.Price)
                    };
                })
                .ToList();

            // --- 6. TÍNH TOÁN HEATMAP ---
            var heatmapAppointments = allAppointments
                .Where(a => a.StartUtc >= heatmapRange.Start && a.StartUtc < heatmapRange.End && a.Status == AppointmentStatus.Approved)
                .ToList();

            var appointmentsByDayInMonth = heatmapAppointments
                .GroupBy(a => TimeZoneInfo.ConvertTimeFromUtc(a.StartUtc, AppointmentStatusDisplay.DisplayTimeZone).Date)
                .ToDictionary(g => g.Key, g => g.Count());

            HeatmapData = GenerateHeatmapData(heatmapRange.Start, heatmapRange.End, appointmentsByDayInMonth);
        }

        #region Helper DTOs and Methods

        // DTO cho bảng "Cuộc hẹn gần đây"
        public class AppointmentDto
        {
            public Guid Id { get; set; }
            public DateTime Date { get; set; }
            public string SpecialtyName { get; set; } = string.Empty;
            public string SpecialtyColor { get; set; } = "#6c757d";
            public string CustomerName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty; // Tên mã (ví dụ: "Pending")
            public string AvatarUrl { get; set; } = string.Empty;
            public string StatusLabel { get; set; } = string.Empty; // Tên hiển thị (ví dụ: "Chờ xác nhận")
            public string StatusCssClass { get; set; } = string.Empty; // CSS cho nút
            public string StatusIcon { get; set; } = string.Empty; // Icon cho nút
        }

        // DTO cho bảng "Hiệu suất"
        public class PerformanceDto
        {
            public string Name { get; set; } = string.Empty;
            public string AvatarUrl { get; set; } = string.Empty;
            public int Appointments { get; set; } // Tổng số lịch hẹn
            public decimal Revenue { get; set; } // Tổng doanh thu
            public double Occupancy { get; set; } // Tỷ lệ chiếm dụng (tạm thời = 0)
        }

        // DTO cho Heatmap
        public class HeatmapCellDto
        {
            public bool IsEmpty { get; set; } = false;
            public string DateIso { get; set; } = string.Empty; // "2025-11-25"
            public string PercentageLabel { get; set; } = "0%";
            public string CssLevel { get; set; } = "level-0";
        }

        // Lớp helper để quản lý hiển thị Status
        public static class AppointmentStatusDisplay
        {
            public static readonly TimeZoneInfo DisplayTimeZone = ResolveVietnamTimeZone();

            private static readonly Dictionary<string, (string Label, string CssClass, string Icon)> Map = new()
            {
                [AppointmentStatus.Approved] = ("Đã xác nhận", "btn-status-approved", "bi bi-check-circle-fill"),
                [AppointmentStatus.Pending] = ("Chờ xác nhận", "btn-status-pending", "bi bi-clock-fill"),
                [AppointmentStatus.Canceled] = ("Đã hủy", "btn-status-canceled", "bi bi-x-circle-fill"),
                [AppointmentStatus.Rejected] = ("Đã từ chối", "btn-status-rejected", "bi bi-x-circle-fill"),
                [AppointmentStatus.NoShow] = ("Vắng mặt", "btn-status-noshow", "bi bi-eye-slash-fill")
            };
            private static readonly (string Label, string CssClass, string Icon) Default = ("Không rõ", "btn-status-secondary", "bi bi-question-circle-fill");

            public static string GetLabel(string status) => Map.TryGetValue(status, out var val) ? val.Label : Default.Label;
            public static string GetCssClass(string status) => Map.TryGetValue(status, out var val) ? val.CssClass : Default.CssClass;
            public static string GetIcon(string status) => Map.TryGetValue(status, out var val) ? val.Icon : Default.Icon;

            private static TimeZoneInfo ResolveVietnamTimeZone()
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
                catch
                {
                    try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
                    catch { return TimeZoneInfo.Local; }
                }
            }
        }

        public string BuildFilterUrl(string targetKey, string targetValue)
        {
            if (string.IsNullOrWhiteSpace(targetKey))
            {
                return Request.Path;
            }

            var normalizedTarget = targetKey.Trim();
            var normalizedValue = string.IsNullOrWhiteSpace(targetValue)
                ? (normalizedTarget.Equals("heatmapFilter", StringComparison.OrdinalIgnoreCase) ? "month" : "week")
                : targetValue.Trim();

            var builder = new List<string>();
            if (_filterSelections == null || _filterSelections.Count == 0)
            {
                builder.Add($"{Uri.EscapeDataString(normalizedTarget)}={Uri.EscapeDataString(normalizedValue)}");
            }
            else
            {
                foreach (var kvp in _filterSelections)
                {
                    var value = kvp.Key.Equals(normalizedTarget, StringComparison.OrdinalIgnoreCase)
                        ? normalizedValue
                        : kvp.Value;
                    builder.Add($"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(value)}");
                }

                if (!_filterSelections.ContainsKey(normalizedTarget))
                {
                    builder.Add($"{Uri.EscapeDataString(normalizedTarget)}={Uri.EscapeDataString(normalizedValue)}");
                }
            }

            var query = builder.Count > 0 ? "?" + string.Join("&", builder) : string.Empty;
            return $"{Request.Path}{query}";
        }

        // Hàm helper lấy Avatar
        private string GetAvatarUrl(AppUser? user)
        {
            if (user is null)
            {
                return GetAvatarUrl((string?)null, "User");
            }

            if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
            {
                return user.AvatarUrl;
            }
            var name = user.GetFullName() ?? user.Email ?? "User";
            return $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(name)}&background=0D6EFD&color=fff";
        }

        private string GetAvatarUrl(string? imageUrl, string fallbackName)
        {
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                return imageUrl;
            }
            var name = string.IsNullOrWhiteSpace(fallbackName) ? "Service" : fallbackName;
            return $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(name)}&background=198754&color=fff";
        }

        // Hàm helper tính % thay đổi
        private double CalculatePercentageChange(int current, int previous)
        {
            if (previous == 0)
            {
                return current > 0 ? 100.0 : 0.0;
            }
            return Math.Round(((double)(current - previous) / previous) * 100.0);
        }

        private static int ResolveSparklineLength(string filterKey, DateTime start, DateTime end)
        {
            if (IsWeekFilter(filterKey))
            {
                return 7;
            }

            var span = (int)(end - start).TotalDays;
            return span <= 0 ? 7 : span;
        }

        private static int ResolveChartDayCount(string filterKey, DateTime start, DateTime end)
        {
            if (IsWeekFilter(filterKey))
            {
                return 7;
            }

            var span = (int)(end - start).TotalDays;
            if (span <= 0)
            {
                return 30;
            }

            return Math.Min(span, 31);
        }

        private static bool IsWeekFilter(string filterKey)
        {
            return filterKey is "week" or "lastweek" or "nextweek";
        }

        private static string NormalizeFilterKey(string? filter, string defaultValue = "week")
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return defaultValue;
            }

            var normalized = filter.Trim().ToLowerInvariant();
            return normalized switch
            {
                "week" or "lastweek" or "nextweek" or "month" or "lastmonth" or "nextmonth" => normalized,
                _ => defaultValue
            };
        }

        // Hàm helper xử lý filter thời gian
        private (DateTime Start, DateTime End, DateTime PrevStart, DateTime PrevEnd, string Label, string ChartLabel) GetDateRangeFromFilter(string filter)
        {
            DateTime start, end, prevStart, prevEnd;
            string label, chartLabel;

            var todayInVietnam = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, AppointmentStatusDisplay.DisplayTimeZone).Date;
            int diffCurrent = (7 + (int)todayInVietnam.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            var thisWeekStart = todayInVietnam.AddDays(-diffCurrent); // Thứ 2 tuần này
            var thisMonthStart = new DateTime(todayInVietnam.Year, todayInVietnam.Month, 1);

            switch (filter?.ToLower())
            {
                case "month":
                    start = thisMonthStart;
                    end = start.AddMonths(1);
                    prevStart = start.AddMonths(-1);
                    prevEnd = start;
                    label = "Tháng này";
                    chartLabel = "d";
                    break;

                case "lastmonth":
                    start = thisMonthStart.AddMonths(-1);
                    end = start.AddMonths(1);
                    prevStart = start.AddMonths(-1);
                    prevEnd = start;
                    label = "Tháng trước";
                    chartLabel = "d";
                    break;

                case "lastweek":
                    start = thisWeekStart.AddDays(-7);
                    end = thisWeekStart;
                    prevStart = start.AddDays(-7);
                    prevEnd = start;
                    label = "Tuần trước";
                    chartLabel = "ddd";
                    break;

                case "nextweek":
                    start = thisWeekStart.AddDays(7);
                    end = start.AddDays(7);
                    prevStart = thisWeekStart;
                    prevEnd = start;
                    label = "Tuần sau";
                    chartLabel = "ddd";
                    break;

                case "nextmonth":
                    start = thisMonthStart.AddMonths(1);
                    end = start.AddMonths(1);
                    prevStart = thisMonthStart;
                    prevEnd = start;
                    label = "Tháng sau";
                    chartLabel = "d";
                    break;

                case "week":
                default:
                    start = thisWeekStart;
                    end = start.AddDays(7);
                    prevStart = start.AddDays(-7);
                    prevEnd = start;
                    label = "Tuần này";
                    chartLabel = "ddd";
                    break;
            }

            return (
                TimeZoneInfo.ConvertTimeToUtc(start, AppointmentStatusDisplay.DisplayTimeZone),
                TimeZoneInfo.ConvertTimeToUtc(end, AppointmentStatusDisplay.DisplayTimeZone),
                TimeZoneInfo.ConvertTimeToUtc(prevStart, AppointmentStatusDisplay.DisplayTimeZone),
                TimeZoneInfo.ConvertTimeToUtc(prevEnd, AppointmentStatusDisplay.DisplayTimeZone),
                label,
                chartLabel
            );
        }

        // Hàm helper tính toán Heatmap
        private List<HeatmapCellDto> GenerateHeatmapData(DateTime startDateUtc, DateTime endDateUtc, Dictionary<DateTime, int> appointmentsByDay)
        {
            var cells = new List<HeatmapCellDto>();
            var startDateLocal = TimeZoneInfo.ConvertTimeFromUtc(startDateUtc, AppointmentStatusDisplay.DisplayTimeZone).Date;
            var endDateLocal = TimeZoneInfo.ConvertTimeFromUtc(endDateUtc, AppointmentStatusDisplay.DisplayTimeZone).Date;

            // Đảm bảo chúng ta bắt đầu từ ngày 1 của tháng
            var firstDayOfMonth = new DateTime(startDateLocal.Year, startDateLocal.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Tìm ngày Tối đa (Max) của các cuộc hẹn để chia tỷ lệ
            int maxAppointments = appointmentsByDay.Count > 0 ? appointmentsByDay.Values.Max() : 1;
            if (maxAppointments == 0) maxAppointments = 1; // Tránh chia cho 0

            // 1. Thêm các ô trống (padding) cho đến Thứ 2 đầu tiên của tháng
            int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek; // Chủ nhật = 0, Thứ 2 = 1...
            int paddingDays = (startDayOfWeek == 0) ? 6 : startDayOfWeek - 1; // Tính số ô trống trước ngày 1

            for (int i = 0; i < paddingDays; i++)
            {
                cells.Add(new HeatmapCellDto { IsEmpty = true });
            }

            // 2. Lặp qua các ngày trong tháng
            for (DateTime day = firstDayOfMonth; day <= lastDayOfMonth; day = day.AddDays(1))
            {
                appointmentsByDay.TryGetValue(day, out int count);

                // Tính toán %
                double percentage = Math.Round((double)count / maxAppointments * 100.0);
                string cssLevel = "level-0";

                if (percentage >= 81) cssLevel = "level-4";
                else if (percentage >= 41) cssLevel = "level-3";
                else if (percentage >= 21) cssLevel = "level-2";
                else if (percentage > 0) cssLevel = "level-1";

                cells.Add(new HeatmapCellDto
                {
                    IsEmpty = false,
                    DateIso = day.ToString("yyyy-MM-dd"),
                    PercentageLabel = $"{percentage.ToString("F0")}%",
                    CssLevel = cssLevel
                });
            }

            return cells;
        }

        #endregion
    }
}