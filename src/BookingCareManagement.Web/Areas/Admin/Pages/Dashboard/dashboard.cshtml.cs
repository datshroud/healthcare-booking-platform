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
        public string FilterPeriodLabel { get; set; } = "Tuần này";

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


        public async Task OnGetAsync([FromQuery] string filter = "week", CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            AdminName = user?.GetFullName() ?? user?.Email ?? "Admin";

            // 1. Lấy khoảng thời gian dựa trên filter
            var (currentStart, currentEnd, previousStart, previousEnd, periodLabel, chartLabelFormat) = GetDateRangeFromFilter(filter);
            FilterPeriodLabel = periodLabel; // Gán nhãn cho dropdown (ví dụ: "Tuần này")
            HeatmapMonthLabel = currentStart.ToString("MMMM 'năm' yyyy", viVNCulture);

            // 2. Tải TẤT CẢ dữ liệu liên quan MỘT LẦN
            var allCustomers = await _customerRepository.GetAllCustomersAsync(cancellationToken);
            var allAppointments = await _appointmentRepository.GetAllAsync(cancellationToken);
            var allDoctors = await _doctorRepository.GetAllAsync(cancellationToken);
            var allSpecialties = await _specialtyRepository.GetAllAsync(cancellationToken);


            // --- 3. TÍNH TOÁN KHÁCH HÀNG ---
            // Lọc khách hàng trong khoảng thời gian hiện tại
            var customersInPeriod = allCustomers
                .Where(c => c.CreatedAt >= currentStart && c.CreatedAt < currentEnd)
                .ToList();
            NewCustomerCount = customersInPeriod.Count;

            // Nhóm khách hàng mới theo ngày để vẽ biểu đồ sparkline
            var newCustomersByDay = customersInPeriod
                .GroupBy(c => c.CreatedAt.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            int sparklineDays = (filter == "week" || filter == "lastweek" || filter == "nextweek") ? 7 : (currentEnd - currentStart).Days;
            if (sparklineDays <= 0) sparklineDays = 7;

            var sparklineCustomerData = new List<int>();
            for (int i = 0; i < sparklineDays; i++)
            {
                var day = currentStart.AddDays(i).Date;
                newCustomersByDay.TryGetValue(day, out int count);
                sparklineCustomerData.Add(count);
            }
            NewCustomerSparkline = sparklineCustomerData;

            // Donut Khách hàng
            int totalCustomerCount = allCustomers.Count();
            if (totalCustomerCount > 0)
            {
                var returningCustomerIds = allAppointments
                    .Where(a => !string.IsNullOrEmpty(a.PatientId) && a.Status == AppointmentStatus.Approved)
                    .Select(a => a.PatientId)
                    .Distinct()
                    .ToHashSet();

                int returningCount = allCustomers.Count(c => returningCustomerIds.Contains(c.Id));
                DonutReturningCustomerPercent = (int)Math.Round((double)returningCount / totalCustomerCount * 100);
                DonutNewCustomerPercent = 100 - DonutReturningCustomerPercent;
            }

            // --- 4. TÍNH TOÁN LỊCH HẸN & DOANH THU ---
            var apptsInPeriod = allAppointments
                .Where(a => a.StartUtc >= currentStart && a.StartUtc < currentEnd)
                .ToList();
            var apptsInPreviousPeriod = allAppointments
                .Where(a => a.StartUtc >= previousStart && a.StartUtc < previousEnd)
                .ToList();

            // Thẻ thống kê
            BookedAppointmentCount = apptsInPeriod.Count(a => a.Status == AppointmentStatus.Approved);
            CancelledAppointmentCount = apptsInPeriod.Count(a => a.Status == AppointmentStatus.Canceled);
            PendingAppointmentCount = apptsInPeriod.Count(a => a.Status == AppointmentStatus.Pending); // ⭐️ THAY THẾ
            TotalRevenue = 0; // Tạm thời là 0

            // Sparkline Doanh thu
            var revenueByDay = apptsInPeriod
                .Where(a => a.Status == AppointmentStatus.Approved)
                .GroupBy(a => a.StartUtc.Date)
                .ToDictionary(g => g.Key, g => g.Sum(a => 0m)); // TODO: Thay 0m bằng a.Price

            var revenueSparklineData = new List<decimal>();
            for (int i = 0; i < sparklineDays; i++)
            {
                var day = currentStart.AddDays(i).Date;
                revenueByDay.TryGetValue(day, out decimal sum);
                revenueSparklineData.Add(sum);
            }
            RevenueSparkline = revenueSparklineData;

            // ⭐️ Sparkline Lịch hẹn đang chờ (THAY THẾ)
            var pendingByDay = apptsInPeriod
                .Where(a => a.Status == AppointmentStatus.Pending)
                .GroupBy(a => a.StartUtc.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var pendingSparklineData = new List<int>();
            for (int i = 0; i < sparklineDays; i++)
            {
                var day = currentStart.AddDays(i).Date;
                pendingByDay.TryGetValue(day, out int count);
                pendingSparklineData.Add(count);
            }
            PendingSparkline = pendingSparklineData;

            // Tính % tăng/giảm so với kỳ trước
            int prevBookedCount = apptsInPreviousPeriod.Count(a => a.Status == AppointmentStatus.Approved);
            int prevCancelledCount = apptsInPreviousPeriod.Count(a => a.Status == AppointmentStatus.Canceled);
            BookedApptChangePercent = CalculatePercentageChange(BookedAppointmentCount, prevBookedCount);
            CancelledApptChangePercent = CalculatePercentageChange(CancelledAppointmentCount, prevCancelledCount);

            // Biểu đồ đường chính (Main Chart)
            var appointmentsByDay = apptsInPeriod
                .Where(a => a.Status == AppointmentStatus.Approved)
                .GroupBy(a => a.StartUtc.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            MainAppointmentChart.Clear();
            MainAppointmentLabels.Clear();

            int chartDays = (currentEnd - currentStart).Days;
            if (chartDays <= 0 || chartDays > 31) chartDays = (filter == "week" || filter == "lastweek" || filter == "nextweek") ? 7 : 30;

            for (var i = 0; i < chartDays; i++)
            {
                var day = currentStart.Date.AddDays(i);
                appointmentsByDay.TryGetValue(day, out int count);
                MainAppointmentChart.Add(count);
                MainAppointmentLabels.Add(day.ToString(chartLabelFormat, viVNCulture));
            }

            // Bảng "Cuộc hẹn gần đây" (Lấy 10 lịch hẹn, bất kể trạng thái)
            RecentAppointments = apptsInPeriod
                .OrderBy(a => a.StartUtc)
                .Take(10)
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
            var approvedApptsInPeriod = apptsInPeriod
                .Where(a => a.Status == AppointmentStatus.Approved)
                .ToList();

            EmployeePerformance = allDoctors
                .Select(doctor =>
                {
                    var appts = approvedApptsInPeriod.Where(a => a.DoctorId == doctor.Id).ToList();
                    return new PerformanceDto
                    {
                        Name = doctor.AppUser.GetFullName() ?? doctor.AppUser.Email,
                        AvatarUrl = GetAvatarUrl(doctor.AppUser),
                        Appointments = appts.Count,
                        Revenue = appts.Sum(a => 0m) // TODO: Thay 0m bằng a.Price
                    };
                })
                .ToList();

            ServicePerformance = allSpecialties
                .Select(spec =>
                {
                    var appts = approvedApptsInPeriod.Where(a => a.SpecialtyId == spec.Id).ToList();
                    return new PerformanceDto
                    {
                        Name = spec.Name,
                        AvatarUrl = GetAvatarUrl(spec.ImageUrl, spec.Name),
                        Appointments = appts.Count,
                        Revenue = appts.Sum(a => 0m) // TODO: Thay 0m bằng spec.Price
                    };
                })
                .ToList();

            // --- 6. TÍNH TOÁN HEATMAP ---
            // (Dùng apptsInPeriod để tính cho tháng hiện tại của filter)
            var appointmentsByDayInMonth = apptsInPeriod
                .Where(a => a.Status == AppointmentStatus.Approved)
                .GroupBy(a => TimeZoneInfo.ConvertTimeFromUtc(a.StartUtc, AppointmentStatusDisplay.DisplayTimeZone).Date)
                .ToDictionary(g => g.Key, g => g.Count());

            HeatmapData = GenerateHeatmapData(currentStart, currentEnd, appointmentsByDayInMonth);
        }

        #region Helper DTOs and Methods

        // DTO cho bảng "Cuộc hẹn gần đây"
        public class AppointmentDto
        {
            public Guid Id { get; set; }
            public DateTime Date { get; set; }
            public string SpecialtyName { get; set; }
            public string SpecialtyColor { get; set; }
            public string CustomerName { get; set; }
            public string Status { get; set; } // Tên mã (ví dụ: "Pending")
            public string AvatarUrl { get; set; }
            public string StatusLabel { get; set; } // Tên hiển thị (ví dụ: "Chờ xác nhận")
            public string StatusCssClass { get; set; } // CSS cho nút
            public string StatusIcon { get; set; } // Icon cho nút
        }

        // DTO cho bảng "Hiệu suất"
        public class PerformanceDto
        {
            public string Name { get; set; }
            public string AvatarUrl { get; set; }
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

        // Hàm helper lấy Avatar
        private string GetAvatarUrl(AppUser user)
        {
            if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
            {
                return user.AvatarUrl;
            }
            var name = user.GetFullName() ?? user.Email ?? "User";
            return $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(name)}&background=0D6EFD&color=fff";
        }

        private string GetAvatarUrl(string imageUrl, string fallbackName)
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