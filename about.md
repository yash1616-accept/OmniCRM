# OmniCRM — Technology Stack & Specifications

This document outlines the complete technology stack, frameworks, libraries, database engines, and software architecture patterns implemented in **OmniCRM**.

---

## 1. Core Technology Stack

| Category | Technology | Version | Purpose in OmniCRM |
| :--- | :--- | :--- | :--- |
| **Language** | **C#** | C# 12 | Core programming language; modern language features (nullable types, string interpolation, record/struct efficiency). |
| **Platform / Runtime** | **.NET** | .NET 8.0 (LTS) | High-performance, cross-architecture managed runtime environment. |
| **UI Framework** | **Windows Forms (WinForms)** | .NET Desktop App | Native Windows desktop GUI framework providing responsive desktop client capabilities. |
| **ORM** | **Entity Framework Core** | 8.0.11 | Object-Relational Mapper translating C# LINQ queries into optimized SQL Server commands. |
| **Database Engine** | **Microsoft SQL Server** | 2019 / 2022 / Express | Enterprise relational database management system for persistent, transactional storage. |
| **Configuration** | **Microsoft Extensions Configuration** | 8.0.1 | Modern JSON-based configuration management (`appsettings.json`). |

---

## 2. NuGet Packages & Dependencies

All dependencies are pinned to stable releases and referenced in [OmniCRM.csproj](file:///c:/Users/vivek%20halnure/Desktop/asp.project/OmniCRM/OmniCRM.csproj):

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.11" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.11">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.11">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
  <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.1" />
</ItemGroup>
```

### Detailed Purpose of Each Package:

1. **`Microsoft.EntityFrameworkCore.SqlServer` (v8.0.11)**
   - The primary SQL Server database provider for EF Core.
   - Translates LINQ expressions into SQL Server dialect (`SELECT`, `INSERT`, `UPDATE`, `DELETE`, `MERGE`).
   - Supports connection pooling, transactional commit/rollback, and SQL Server-specific types.

2. **`Microsoft.EntityFrameworkCore.Design` (v8.0.11)**
   - Provides design-time components for EF Core tooling.
   - Powers Code-First schema generation, model snapshot generation, and migration compilation.

3. **`Microsoft.EntityFrameworkCore.Tools` (v8.0.11)**
   - Adds MSBuild targets and design-time hooks for Visual Studio and CLI tooling.

4. **`Microsoft.Extensions.Configuration.Json` (v8.0.1)**
   - Enables parsing of `appsettings.json` configuration files into structured C# key-value pairs.
   - Provides hot-reload capabilities and environmental overrides without recompiling binaries.

---

## 3. Database Technologies & Features

* **Database Name:** `OmniCrmDb`
* **Instance Compatibility:**
  * `MSSQLSERVER` (Standard default local instance on `localhost`)
  * `SQLEXPRESS` (Microsoft SQL Server Express instance on `.\SQLEXPRESS`)
  * `(localdb)\mssqllocaldb` (Lightweight SQL Server LocalDB)
* **SQL Server Specific Features Implemented:**
  * **Automatic Default Values:** `GETUTCDATE()` for record timestamping.
  * **Precision Modeling:** `decimal(18,2)` data types for financial values (`EstimatedValue` in sales leads).
  * **Index Optimization:**
    * Unique Index on `Customers(Email)` preventing duplicate customer email entries.
    * Non-Clustered Index on `Customers(Name)` for rapid full-text lookup.
    * Non-Clustered Index on `Leads(Status)` for fast pipeline filtering.
    * Non-Clustered Index on `FollowUps(ScheduledDate, IsCompleted)` for rapid reminder polling.
  * **Referential Integrity:** Foreign keys with `ON DELETE CASCADE` between Customer and associated child entities.

---

## 4. UI Design System & Theming Architecture

Windows Forms has been customized with an enterprise design system via [UITheme.cs](file:///c:/Users/vivek%20halnure/Desktop/asp.project/OmniCRM/UITheme.cs):

* **Color Palette (Corporate Modern Slate & Indigo):**
  * `SidebarBg`: `#0F172A` (Dark Slate 900)
  * `SidebarActive`: `#334155` (Slate 700)
  * `Primary`: `#2563EB` (Modern Blue)
  * `PrimaryDark`: `#1D4ED8`
  * `PrimaryLight`: `#EFF6FF` (Soft Blue Hover Tint)
  * `ContentBg`: `#F8FAFC` (Canvas Gray)
  * `CardBorder`: `#E2E8F0` (Subtle Grid Border)
  * `Success`: `#10B981` (Emerald Green)
  * `Warning`: `#F59E0B` (Amber Orange)
  * `Danger`: `#EF4444` (Crimson Red)
* **Typography:** `Segoe UI` font family with designated hierarchy (`HeaderLarge`, `HeaderMedium`, `HeaderSmall`, `BodyRegular`, `BodyBold`, `Subtext`).
* **Custom Grid Styler:** Flat single-horizontal cell border lines, colored row headers, and alternating row background highlights.

---

## 5. Software Engineering Patterns & Principles

1. **Separation of Concerns (SoC):** Distinct separation between Presentation (`Forms` & `UserControls`), Business Logic (`Services`), Entity Models (`Models`), and Data Persistence (`Data`).
2. **Repository / Service Pattern:** Business rules, queries, and entity modifications are isolated inside service classes (`CustomerService`, `LeadService`, `FollowUpService`, `DashboardService`).
3. **Single Responsibility Principle (SRP):** Each form, control, and service class performs a well-defined domain responsibility.
4. **Task-based Asynchronous Pattern (TAP):** Extensive use of `async`/`await` across all database I/O operations, ensuring high UI responsiveness and non-blocking background execution.
5. **Defensive Programming:** Nullable reference types (`<Nullable>enable</Nullable>`), null-conditional operators (`?.`), and try-catch blocks with graceful UI status feedback.
