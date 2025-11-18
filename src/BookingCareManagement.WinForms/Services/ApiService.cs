using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BookingCareManagement.WinForms.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        // Chạy Localhost:
        private readonly string _baseUrl = "https://localhost:7279";
        // Chạy Online:
        // Thay bằng đường dẫn trang web API thực tế của bạn
        //private readonly string _baseUrl = "https://ten-mien-website-cua-ban.com";
        //Khi đang viết code sửa lỗi: Bạn nên bật START_API_SERVER và dùng localhost để test cho nhanh, không sợ làm hỏng dữ liệu thật trên web.
        //Khi muốn kiểm tra thực tế: Bạn sửa lại _baseUrl thành link web thật, build ra file .exe rồi gửi cho người khác dùng thử.

        private static ApiService _instance;
        public static ApiService Instance => _instance ??= new ApiService();

        private ApiService()
        {
            var handler = new HttpClientHandler();
            // Bỏ qua lỗi SSL certificate khi chạy localhost
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            _httpClient = new HttpClient(handler);
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        // 1. GET: Lấy dữ liệu
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return await response.Content.ReadFromJsonAsync<T>(options);
                }
                return default;
            }
            catch (Exception ex)
            {
                ShowError(ex);
                return default;
            }
        }

        // 2. POST: Thêm mới
        public async Task<TResponse?> PostAsync<TResponse, TRequest>(string endpoint, TRequest data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data);
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return await response.Content.ReadFromJsonAsync<TResponse>(options);
                }
                else
                {
                    await ShowApiError(response);
                    return default;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
                return default;
            }
        }

        // 3. PUT: Cập nhật
        public async Task<bool> PutAsync<TRequest>(string endpoint, TRequest data)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(endpoint, data);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    await ShowApiError(response);
                    return false;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
                return false;
            }
        }

        // 4. DELETE: Xóa
        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    await ShowApiError(response);
                    return false;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
                return false;
            }
        }

        // --- Helper hiển thị lỗi ---
        private async Task ShowApiError(HttpResponseMessage response)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            MessageBox.Show($"Lỗi từ Server ({response.StatusCode}):\n{errorContent}", "Lỗi API", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ShowError(Exception ex)
        {
            MessageBox.Show($"Không kết nối được Server: {ex.Message}", "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}