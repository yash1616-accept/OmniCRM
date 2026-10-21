# OmniCRM — Enterprise Customer Relationship Management

[![.NET Version](https://img.shields.io/badge/.NET-8.0%20LTS-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp)](https://docs.microsoft.com/dotnet/csharp/)
[![Database](https://img.shields.io/badge/Database-SQL%20Server-CC292B?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![UI Framework](https://img.shields.io/badge/UI-Windows%20Forms-0078D4?logo=windows)](https://learn.microsoft.com/dotnet/desktop/winforms/)
[![ORM](https://img.shields.io/badge/ORM-Entity%20Framework%20Core%208-68217A)](https://learn.microsoft.com/ef/core/)

**OmniCRM** is an enterprise-inspired Universal Customer Relationship Management and Sales Pipeline desktop application built with modern **C#**, **.NET 8 (LTS)**, **Windows Forms (WinForms)**, **Entity Framework Core 8**, and **Microsoft SQL Server**.

Designed for manufacturers, IT solution providers, distributors, and sales teams, OmniCRM centralizes customer accounts, tracks sales pipeline opportunities across conversion stages, schedules client interaction reminders, and delivers real-time executive KPI metrics.

---

## 🌟 Key Features

* **📊 Executive KPI Dashboard:**
  * Real-time metrics for Total Customers, Active Leads, Converted Deals, and Total Pipeline Valuation.
  * Live activity feeds displaying recently acquired customers and active deals.
  * Live SQL Server connection health badge.

* **👥 Customer Relationship Directory:**
  * Complete CRUD (Create, Read, Update, Delete) management.
  * Asynchronous real-time search across names, emails, companies, and phone numbers.
  * Robust client-side and server-side data validation (format verification, required fields, and unique email constraints).

* **💼 Sales Pipeline & Lead Stages:**
  * Track opportunities through standard pipeline stages: `New` → `Contacted` → `Qualified` → `Converted` → `Lost`.
  * Dynamic filtering by deal stage and instant total active pipeline valuation calculations.

* **📅 Scheduled Follow-Up System:**
  * Date-and-time interaction scheduling for client calls, meetings, and quote delivery.
  * One-click task completion toggling with pending-only view filters.

* **🗄️ Relational Database & Auto-Seeding:**
  * Code-First Entity Framework Core 8 with SQL Server.
  * Referential integrity with cascading deletes and optimized indexing.
  * Automatic schema generation and demo enterprise data seeding on first launch.

* **🎨 Modern Corporate UI Theme:**
  * Professional enterprise design system (`#0F172A` Dark Slate sidebar, `#2563EB` Royal Indigo accents, Canvas `#F8FAFC`).
  * Custom flat DataGridView styling with alternating row highlights.

---

## 🏛️ System Architecture

OmniCRM is designed following a **Four-Tier Clean Architecture**:

```
[ Presentation Layer ]  -->  MainForm Shell + Modular UserControls (Dashboard, Customers, Leads, FollowUps)
          ↓
[ Business / Service ]  -->  CustomerService, LeadService, FollowUpService, DashboardService (Async TAP)
          ↓
[ Data Access (ORM) ]   -->  OmniCrmDbContext (EF Core 8 Fluent API + Indexes + DbInitializer)
          ↓
[ Persistence Layer ]   -->  Microsoft SQL Server (OmniCrmDb)
```

For complete architectural diagrams and entity-relationship specifications, refer to [architecture.md](architecture.md).

---

## 📂 Project Structure

```text
asp.project/
├── OmniCRM/                          # Main Windows Forms Application Project
│   ├── Data/
│   │   ├── OmniCrmDbContext.cs       # EF Core Context & Fluent API Configurations
│   │   └── DbInitializer.cs          # Schema check & demo data seeder
│   ├── Forms/
│   │   ├── MainForm.cs               # Main shell with sidebar navigation
│   │   ├── CustomerDialogForm.cs     # Customer add/edit modal dialog
│   │   ├── LeadDialogForm.cs         # Sales lead modal dialog
│   │   └── FollowUpDialogForm.cs     # Follow-up task modal dialog
│   ├── Models/
│   │   ├── Customer.cs               # Customer entity model
│   │   ├── Lead.cs                   # Sales lead entity model
│   │   └── FollowUp.cs               # Scheduled follow-up model
│   ├── Services/
│   │   ├── CustomerService.cs        # Customer CRUD & duplicate validation
│   │   ├── LeadService.cs            # Pipeline management & status filtering
│   │   ├── FollowUpService.cs        # Scheduled interaction reminders
│   │   └── DashboardService.cs       # Aggregated KPI metrics
│   ├── UserControls/
│   │   ├── DashboardControl.cs       # Executive KPI cards & activity feed
│   │   ├── CustomerManagementControl.cs # Customer directory & search grid
│   │   ├── LeadManagementControl.cs  # Deal pipeline table & status filter
│   │   └── FollowUpManagementControl.cs # Task scheduler & completion toggle
│   ├── appsettings.json              # SQL Server connection string
│   ├── OmniCRM.csproj                # .NET 8 Project file with EF Core packages
│   ├── Program.cs                    # Application entry point
│   └── UITheme.cs                    # Colors, typography, and grid styles
├── schema.sql                        # Standalone SQL Server migration & seed script
├── start.md                          # Quick start and 5-Act live demo script
├── explain.md                        # Interview guide (elevator pitch + top 10 Q&A)
├── about.md                          # Detailed technology specifications
├── architecture.md                   # Mermaid architecture & ER diagrams
├── migrate.md                        # Moving to another PC & standalone deployment
├── .gitignore                        # Git ignore rules for .NET and Visual Studio
└── README.md                         # Project documentation overview
```

---

## 🚀 Quick Start Guide

### 1. Prerequisites
* **Windows 10 / 11** (64-bit)
* **[.NET 8.0 SDK](https://dotnet.microsoft.com/download)** (or newer)
* **[Microsoft SQL Server](https://www.microsoft.com/sql-server)** (Developer, Standard, or SQL Express)

### 2. Start SQL Server Service
Open PowerShell as Administrator:
```powershell
Start-Service MSSQLSERVER
```
*(Or for SQL Express: `Start-Service "MSSQL$SQLEXPRESS"`)*

### 3. Build & Run
Open a terminal in the project directory:
```powershell
# Navigate into the OmniCRM folder
cd OmniCRM

# Restore dependencies
dotnet restore

# Run the desktop application
dotnet run
```

---

## 📚 Documentation Index

| Guide | Description |
| :--- | :--- |
| **[start.md](start.md)** | Step-by-step launch instructions and 5-Act evaluator demo script. |
| **[explain.md](explain.md)** | Interview preparation guide: 30-second pitch, design choices, and top 10 technical Q&As. |
| **[about.md](about.md)** | Exhaustive breakdown of frameworks, NuGet packages, and design system tokens. |
| **[architecture.md](architecture.md)** | Mermaid multi-tier diagrams, Entity-Relationship schemas, and sequence flows. |
| **[migrate.md](migrate.md)** | System transfer guide, connection string options, and portable standalone publishing. |
| **[schema.sql](schema.sql)** | 1-click database schema creation and seed script for SQL Server. |

---

## 🛠️ Technology Stack & Dependencies

* **Language:** C# 12
* **Framework:** .NET 8.0-windows (WinForms)
* **Database Engine:** Microsoft SQL Server
* **ORM:** Entity Framework Core 8.0.11
* **Key NuGet Packages:**
  * `Microsoft.EntityFrameworkCore.SqlServer` (v8.0.11)
  * `Microsoft.EntityFrameworkCore.Design` (v8.0.11)
  * `Microsoft.EntityFrameworkCore.Tools` (v8.0.11)
  * `Microsoft.Extensions.Configuration.Json` (v8.0.1)

---

## 👤 Author & Versioning

* **Author:** Yash Gaikwad ([@yash1616-accept](https://github.com/yash1616-accept))
* **Version:** 1.0.0 (Enterprise Edition)
* **Target Framework:** `net8.0-windows`
