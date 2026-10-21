# OmniCRM — System Migration & Setup Guide

This guide provides instructions for transferring, configuring, and running **OmniCRM** on another laptop, PC, or virtual machine from scratch.

---

## 1. Prerequisites on the Target Machine

Before running the project on the new system, ensure the following software is installed:

| Component | Recommended Version | Download Link / Notes |
| :--- | :--- | :--- |
| **Operating System** | Windows 10 or Windows 11 (64-bit) | Windows Desktop runtime required for WinForms |
| **.NET SDK** | **.NET 8.0 SDK** (or .NET 10) | [Download .NET SDK](https://dotnet.microsoft.com/download) |
| **SQL Server** | SQL Server 2019/2022 (Developer / Express) | [Download SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) *(Express is free and lightweight)* |
| **SQL Client (Optional)** | SQL Server Management Studio (SSMS) or Azure Data Studio | [Download SSMS](https://aka.ms/ssmsfullsetup) |

---

## 2. Packaging the Project for Transfer

When copying the project to a USB drive, cloud storage (Google Drive / OneDrive), or Git repository, **exclude temporary build folders** (`bin/` and `obj/`) to keep the file size minimal and prevent architecture conflicts:

### What to Copy:
```text
asp.project/
├── OmniCRM/                # All C# source code and project files
│   ├── Data/
│   ├── Forms/
│   ├── Models/
│   ├── Services/
│   ├── UserControls/
│   ├── appsettings.json
│   ├── OmniCRM.csproj
│   ├── Program.cs
│   └── UITheme.cs
├── schema.sql              # Database creation and seeding script
├── start.md                # Launch and live demo guide
├── explain.md              # Interview preparation guide
├── about.md                # Tech stack documentation
├── architecture.md         # System diagrams
└── migrate.md              # This migration guide
```

> [!TIP]
> **Exclude these directories before copying:**
> * `OmniCRM\bin\`
> * `OmniCRM\obj\`
> * `.vs\`

---

## 3. Database Setup on the New Machine

### Step 3.1: Start the SQL Server Service
On the new machine, open **PowerShell as Administrator** and ensure the SQL Server service is active:

```powershell
# For default instance (MSSQLSERVER):
Start-Service MSSQLSERVER

# OR for SQL Server Express instance:
Start-Service "MSSQL`$SQLEXPRESS"
```

### Step 3.2: Create the Database & Seed Data
You have two simple ways to set up the database on the new system:

#### Method A: Using SQL Server Management Studio (SSMS)
1. Open SSMS on the new computer and connect to `localhost` (or `.\SQLEXPRESS`).
2. Open the included [schema.sql](file:///c:/Users/vivek%20halnure/Desktop/asp.project/schema.sql) file.
3. Click **Execute** (or press <kbd>F5</kbd>).
4. The database `OmniCrmDb`, all tables, foreign keys, unique indexes, and 10 demonstration records will be created in seconds.

#### Method B: Using PowerShell / sqlcmd (Automated)
Open PowerShell in the project directory and run:

```powershell
# For default instance:
sqlcmd -S localhost -E -i "schema.sql"

# OR for SQL Express instance:
sqlcmd -S .\SQLEXPRESS -E -i "schema.sql"
```

---

## 4. Connection String Configuration

Check the SQL Server instance type installed on the new machine and adjust [appsettings.json](file:///c:/Users/vivek%20halnure/Desktop/asp.project/OmniCRM/appsettings.json) accordingly:

### Scenario A: Standard / Developer SQL Server (Default)
If the new PC has standard SQL Server (`MSSQLSERVER`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OmniCrmDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### Scenario B: SQL Server Express Edition
If the new PC has SQL Server Express installed (`.\SQLEXPRESS`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=OmniCrmDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

---

## 5. Building & Running on the New System

1. Open PowerShell or Command Prompt on the new computer.
2. Navigate to the `OmniCRM` folder:
   ```powershell
   cd "path\to\asp.project\OmniCRM"
   ```
3. Restore packages, build, and launch:
   ```powershell
   dotnet restore
   dotnet build
   dotnet run
   ```

The application window will open, connect to the new machine's SQL Server, and display the dashboard.

---

## 6. Pro Tip: Portable Self-Contained Deployment

If you want to run OmniCRM on another computer **without installing the .NET SDK**, you can publish a self-contained portable build:

Run this command on your development PC:

```powershell
cd "c:\Users\vivek halnure\Desktop\asp.project\OmniCRM"
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "c:\Users\vivek halnure\Desktop\OmniCRM_Portable"
```

### What this produces:
* A standalone folder containing `OmniCRM.exe` that bundles the entire .NET runtime inside it.
* The target computer only needs Windows and SQL Server — **no .NET SDK installation required**!

---

## 7. Migration Troubleshooting Checklist

| Issue / Error | Cause | Solution |
| :--- | :--- | :--- |
| `Cannot open database "OmniCrmDb"` | Database not created yet | Run `schema.sql` using SSMS or sqlcmd as described in Step 3. |
| `A network-related or instance-specific error occurred` | SQL Server service is stopped or wrong instance name | Verify with `Get-Service *sql*`. If using Express, change connection string to `Server=.\\SQLEXPRESS`. |
| `Certificate chain was issued by an untrusted authority` | SQL Server 2022+ default encryption enforcement | Ensure `TrustServerCertificate=True;` is present in the connection string in `appsettings.json`. |
| `The process cannot access the file ... because it is being used` | A previous instance of `OmniCRM.exe` is still running | Run `Stop-Process -Name OmniCRM -Force` in PowerShell. |
