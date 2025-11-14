using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookingCareManagement.WinForms.Clients;
using BookingCareManagement.WinForms.Models;

namespace BookingCareManagement.WinForms;

public partial class Form1 : Form
{
    private readonly DoctorApiClient _doctorApiClient;

    public Form1(DoctorApiClient doctorApiClient)
    {
        _doctorApiClient = doctorApiClient;
        InitializeComponent();
        Load += async (_, _) => await LoadDoctorsAsync();
        refreshButton.Click += async (_, _) => await LoadDoctorsAsync();
    }

    private async Task LoadDoctorsAsync()
    {
        try
        {
            ToggleLoading(true);
            var doctors = await _doctorApiClient.GetDoctorsAsync();
            var rows = doctors
                .Select(d => new DoctorDisplayRow(d.Id, d.FullName, string.Join(", ", d.Specialties)))
                .ToList();

            doctorsGrid.DataSource = new BindingList<DoctorDisplayRow>(rows);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show(this, $"Không thể tải danh sách bác sĩ. Vui lòng kiểm tra API: {ex.Message}", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ToggleLoading(false);
        }
    }

    private void ToggleLoading(bool isLoading)
    {
        refreshButton.Enabled = !isLoading;
        loadingIndicator.Visible = isLoading;
    }

    private void button1_Click(object sender, EventArgs e)
    {

    }

    private sealed record DoctorDisplayRow(Guid Id, string FullName, string Specialties);
}
