# ?? H??NG D?N CH?Y API - ??Y ??

## ? B??c 1: Chu?n b?

### Ki?m tra .NET Runtime:
```cmd
dotnet --version
```

**K?t qu? mong ??i:** `9.0.x`

N?u không có, download t?i: https://dotnet.microsoft.com/download/dotnet/9.0

### Ki?m tra SQL Server:
- SQL Server ph?i ?ang ch?y
- Database `BookingCareDb` ph?i ?ã ???c t?o (migration)

---

## ?? B??c 2: Ch?y API

### Cách 1: Double-click file .bat (??n gi?n nh?t)

1. M? File Explorer
2. Navigate ??n: `C:\Users\COMP\Documents\GitHub\hospital-booking-system-web\src\BookingCareManagement.Web\publish\`
3. **Double-click** file: `START_API_SERVER.bat`

?? Console window s? m? ra

---

### Cách 2: Run t? Command Prompt

```cmd
cd C:\Users\COMP\Documents\GitHub\hospital-booking-system-web\src\BookingCareManagement.Web\publish
START_API_SERVER.bat
```

---

### Cách 3: Ch?y tr?c ti?p .exe

```cmd
cd C:\Users\COMP\Documents\GitHub\hospital-booking-system-web\src\BookingCareManagement.Web\publish
BookingCareManagement.exe --urls "https://localhost:7279;http://localhost:5088"
```

---

## ? B??c 3: Ki?m tra API ?ã ch?y

### Xem console output:
```
Now listening on: https://localhost:7279
Now listening on: http://localhost:5088
Application started. Press Ctrl+C to shut down.
```

### Test b?ng browser:
M? browser và truy c?p:
```
https://localhost:7279/api/Invoice
```

Ho?c:
```
https://localhost:7279/swagger
```

---

## ? X? lý l?i th??ng g?p:

### L?i 1: "Could not load file or assembly 'Swashbuckle...'"

**Nguyên nhân:** ?ã xóa nh?m Swagger DLL

**Gi?i pháp:** Re-publish
```cmd
cd C:\Users\COMP\Documents\GitHub\hospital-booking-system-web\src\BookingCareManagement.Web
dotnet publish -c Release -o .\publish --force
```

---

### L?i 2: "Port already in use"

**Nguyên nhân:** Port 7279 ho?c 5088 ?ang b? chi?m

**Gi?i pháp:**
```cmd
# Tìm process ?ang dùng port
netstat -ano | findstr :7279

# Kill process (thay <PID> b?ng s? ? c?t cu?i)
taskkill /PID <PID> /F
```

---

### L?i 3: Database connection failed

**Nguyên nhân:** SQL Server không ch?y ho?c connection string sai

**Gi?i pháp:**
1. Start SQL Server
2. Ki?m tra `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=BookingCareDb;User Id=sa;Password=BookingCare123!;TrustServerCertificate=True;Encrypt=False"
  }
}
```

3. Test connection:
```cmd
sqlcmd -S localhost -U sa -P BookingCare123!
```

---

### L?i 4: SSL Certificate error

**Nguyên nhân:** .NET dev certificate ch?a ???c trust

**Gi?i pháp:**
```cmd
dotnet dev-certs https --trust
```
Click "Yes" khi ???c h?i

---

### L?i 5: Console ?óng ngay l?p t?c

**Nguyên nhân:** API crash nh?ng l?i hi?n th? quá nhanh

**Gi?i pháp:** Dùng DEBUG mode
```cmd
cd C:\Users\COMP\Documents\GitHub\hospital-booking-system-web\src\BookingCareManagement.Web\publish
BookingCareManagement.exe --urls "https://localhost:7279;http://localhost:5088" 2>&1 | more
```

---

## ?? Workflow ??y ??:

```
1. ? Ch?y SQL Server
   ?
2. ? Ch?y API (double-click START_API_SERVER.bat)
   ?
3. ? ??i console hi?n th? "Now listening on..."
   ?
4. ? Test API: https://localhost:7279/api/Invoice
   ?
5. ? Ch?y WinForms app (Press F5 trong Visual Studio)
   ?
6. ? Click "?? Hóa ??n" trong sidebar
   ?
7. ? Data t? ??ng load t? API!
```

---

## ?? Ki?m tra nhanh:

### API ?ang ch?y?
```cmd
netstat -ano | findstr :7279
```

N?u th?y output ? **API ?ang ch?y** ?

### Process ?ang ch?y?
```cmd
tasklist | findstr BookingCareManagement
```

N?u th?y ? **API ?ang ch?y** ?

---

## ?? D?ng API:

1. Click vào console window c?a API
2. Press `Ctrl + C`
3. Ho?c ?óng console window

---

## ?? Re-publish khi có thay ??i code:

### Option 1: T? ??ng (Khuy?n ngh?)
```cmd
cd C:\Users\COMP\Documents\GitHub\hospital-booking-system-web\src\BookingCareManagement.Web
AUTO_PUBLISH_AND_CLEANUP.bat
```

### Option 2: Th? công
```cmd
cd C:\Users\COMP\Documents\GitHub\hospital-booking-system-web\src\BookingCareManagement.Web
dotnet publish -c Release -o .\publish --force
cd publish
CLEANUP_PUBLISH.bat
```

---

## ? Checklist tr??c khi ch?y:

- [ ] ? .NET 9 Runtime ?ã cài
- [ ] ? SQL Server ?ang ch?y
- [ ] ? Database `BookingCareDb` ?ã t?o
- [ ] ? Port 7279 & 5088 không b? chi?m
- [ ] ? `appsettings.json` có connection string ?úng
- [ ] ? SSL certificate ?ã trust

---

**N?u t?t c? OK, double-click `START_API_SERVER.bat` là xong! ??**

---

_Updated: 2025-01-18_
_Status: READY TO RUN ?_
