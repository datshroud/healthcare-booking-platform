# BookingCare API Server - Production Ready

## ??	CÁCH CH?Y NHANH

### Option 1: Double-click (Khuy?n ngh?)
```cmd
START_API_SERVER.bat
```
?ây là cách ??n gi?n nh?t! Ch? c?n double-click file này.

### Option 2: PowerShell
```powershell
.\start-api.ps1
```

### Option 3: Command Line
```cmd
BookingCareManagement.exe --urls "https://localhost:7279;http://localhost:5088"
```

---

## ??	API Endpoints

- **HTTPS**: https://localhost:7279
- **HTTP**: http://localhost:5088

---

## ?	Test API ho?t ??ng

M? browser và truy c?p:
```
https://localhost:7279/api/Invoice
```

N?u th?y JSON data ho?c error 401 (Unauthorized) ? **API ?ang ch?y OK** ?

---

## ??	D?ng Server

Press `Ctrl + C` trong console window

---

## ??	Ch?y t? ??ng khi kh?i ??ng Windows

1. Press `Win + R`
2. Type: `shell:startup`
3. Copy file `START_API_SERVER.bat` vào folder này
4. Restart máy ? API s? t? ??ng ch?y

---

## ??	Configuration

### Database Connection

File: `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=BookingCareDb;User Id=sa;Password=BookingCare123!;TrustServerCertificate=True;Encrypt=False"
  }
}
```

**L?u ý**: ??i connection string n?u dùng SQL Server khác

### ??i Port

S?a file `START_API_SERVER.bat`:
```cmd
BookingCareManagement.exe --urls "https://localhost:YOUR_PORT"
```

---

## ??	Troubleshooting

### L?i "Port already in use"
```powershell
# Tìm process ?ang dùng port
netstat -ano | findstr :7279

# Kill process
taskkill /PID <PID_NUMBER> /F
```

### L?i SSL Certificate
```powershell
dotnet dev-certs https --trust
```

### L?i Database Connection
1. Ki?m tra SQL Server ?ã ch?y ch?a
2. Ki?m tra connection string trong `appsettings.json`
3. Test connection:
   ```cmd
   sqlcmd -S localhost -U sa -P BookingCare123!
   ```

### L?i "Missing DLL"
Re-publish l?i:
```powershell
cd ..\
dotnet publish -c Release -o .\publish
```

---

## ??	Publish l?i khi có thay ??i code

```powershell
# T? folder BookingCareManagement.Web
dotnet publish -c Release -o .\publish

# Copy l?i config
Copy-Item "appsettings.Development.json" "publish\appsettings.json" -Force
```

---

## ??	Deploy lên Server Production

### Option A: Windows Service (Khuy?n ngh?)

Install as Windows Service:
```powershell
# Install service
sc.exe create "BookingCareAPI" binpath="C:\Path\To\publish\BookingCareManagement.exe"

# Start service
sc.exe start "BookingCareAPI"

# Stop service
sc.exe stop "BookingCareAPI"

# Delete service
sc.exe delete "BookingCareAPI"
```

### Option B: IIS

1. Install IIS v?i .NET Hosting Bundle
2. T?o Application Pool (.NET CLR Version: No Managed Code)
3. T?o Website, tr? physical path ??n folder `publish`
4. Set bindings: https://localhost:7279

### Option C: Docker (Advanced)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY publish/ .
EXPOSE 7279
ENTRYPOINT ["dotnet", "BookingCareManagement.dll"]
```

---

## ??	Logs

Logs s? ???c ghi vào:
- Console output (khi ch?y b?ng .bat file)
- Windows Event Viewer (khi ch?y as service)

---

## ??	Security Notes

**?? QUAN TR?NG**: Tr??c khi deploy production:

1. **??i JWT Secret Key**:
   ```json
   "Jwt": {
     "Key": "YOUR_SUPER_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG"
   }
   ```

2. **B?t HTTPS redirect** trong `Program.cs`

3. **T?t detailed errors**:
   ```json
   "DetailedErrors": false
   ```

4. **H?n ch? CORS** n?u c?n

---

## ??	Support

N?u g?p v?n ??, ki?m tra:
1. SQL Server ?ã ch?y ch?a
2. Port 7279 có b? chi?m ch?a
3. Firewall có block không
4. appsettings.json có ?úng không

---

## ??	Next Steps

Sau khi API ch?y thành công:
1. Ch?y WinForms app
2. Click "Hóa ??n" trong sidebar
3. Data s? t? ??ng load t? API

**Happy Coding! ??**
