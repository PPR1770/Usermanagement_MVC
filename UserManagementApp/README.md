# UserManagementApp - ASP.NET Core MVC

A complete User Management System built with ASP.NET Core 8 MVC + Bootstrap 5.

## Features
- ✅ Login / Logout (Cookie Authentication)
- ✅ Registration (auto-assigned "User" role)
- ✅ Forgot Password (email token link)
- ✅ Reset Password (token validated)
- ✅ Change Password (logged-in users)
- ✅ User Management (list, edit, delete, toggle active, reset password)
- ✅ Role Management (create, edit, delete)
- ✅ Permission Management (assign permissions per role)
- ✅ Global Exception Handling Middleware
- ✅ Serilog File Logging
- ✅ Bootstrap 5 UI (no IdentityUI)
- ✅ Admin & Role seeding on startup

## Default Admin Credentials
- **Email:** admin@app.com
- **Password:** Admin@123

---

## Setup Instructions

### 1. Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server or LocalDB
- Visual Studio 2022 / VS Code

### 2. Install Dependencies
```bash
dotnet restore
```

### 3. Configure Database
Edit `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=UserManagementDb;Trusted_Connection=True;"
}
```

### 4. Configure Email (for Forgot Password)
Edit `appsettings.json`:
```json
"Email": {
  "Host": "smtp.gmail.com",
  "Port": "587",
  "From": "your-email@gmail.com",
  "Username": "your-email@gmail.com",
  "Password": "your-app-password"
}
```
> For Gmail, use an [App Password](https://support.google.com/accounts/answer/185833).

### 5. Run Migrations
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 6. Run the App
```bash
dotnet run
```
Navigate to `https://localhost:5001` and login with the admin credentials above.

---

## Project Structure
```
UserManagementApp/
├── Controllers/
│   ├── AccountController.cs   # Login, Register, Forgot/Reset Password
│   ├── HomeController.cs      # Dashboard
│   ├── UserController.cs      # User CRUD
│   └── RoleController.cs      # Role CRUD + Permissions
├── Data/
│   ├── ApplicationDbContext.cs
│   └── SeedData.cs
├── Middleware/
│   └── GlobalExceptionMiddleware.cs
├── Models/
│   ├── ApplicationUser.cs
│   ├── ApplicationRole.cs
│   └── Permission.cs
├── Services/
│   └── EmailService.cs
├── ViewModels/
│   └── AccountViewModels.cs
├── Views/
│   ├── Account/    # Login, Register, ForgotPassword, ResetPassword, ChangePassword
│   ├── Home/       # Dashboard, Error
│   ├── User/       # Index, Details, Edit
│   ├── Role/       # Index, Edit, ManagePermissions
│   └── Shared/     # _Layout, _AuthLayout, _ValidationScriptsPartial
├── appsettings.json
└── Program.cs
```

## Roles
| Role    | Access |
|---------|--------|
| Admin   | Full access: users, roles, permissions |
| Manager | View/manage users |
| User    | Dashboard + change password only |
