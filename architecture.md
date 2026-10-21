# OmniCRM — System Architecture & Design Documentation

This document presents the detailed architectural blueprint of the **OmniCRM** enterprise application, including multi-tier architectural diagrams, Entity-Relationship (ER) schemas, component navigation hierarchies, and asynchronous data flows.

---

## 1. High-Level Multi-Tier Architecture

OmniCRM is architected as a clean, decoupled **Four-Tier Desktop System**:

```mermaid
graph TB
    subgraph Presentation_Layer["1. PRESENTATION LAYER (WinForms UI)"]
        direction TB
        MainShell["MainForm Shell (Navigation Sidebar, Header, Status)"]
        DashCtrl["DashboardControl (KPIs, Charts, Feeds)"]
        CustCtrl["CustomerManagementControl (Data Grid, Filter, Actions)"]
        LeadCtrl["LeadManagementControl (Pipeline Funnel, Stages)"]
        FollowCtrl["FollowUpManagementControl (Scheduler, Reminders)"]
        Dialogs["Modal Dialogs (CustomerDialog, LeadDialog, FollowUpDialog)"]
        Theme["UITheme Token Provider (Colors, Fonts, Grid Styles)"]

        MainShell --> DashCtrl
        MainShell --> CustCtrl
        MainShell --> LeadCtrl
        MainShell --> FollowCtrl
        CustCtrl -.-> Dialogs
        LeadCtrl -.-> Dialogs
        FollowCtrl -.-> Dialogs
    end

    subgraph Service_Layer["2. SERVICE & BUSINESS LOGIC LAYER"]
        direction TB
        CustSvc["CustomerService (CRUD, Unique Email Validation, Full-Text Search)"]
        LeadSvc["LeadService (Deal Stages, Status Filter, Value Forecasts)"]
        FollowSvc["FollowUpService (Schedule Tasks, Status Toggle)"]
        DashSvc["DashboardService (Real-Time Aggregations, Top Activities)"]
    end

    subgraph Data_Layer["3. DATA ACCESS LAYER (Entity Framework Core 8)"]
        direction TB
        DbContext["OmniCrmDbContext (DbSet Mapping, Fluent API Rules, Indexes)"]
        DbInit["DbInitializer (EnsureCreatedAsync, Sample Seed Data)"]
        Config["appsettings.json (SQL Server Connection Strings)"]
    end

    subgraph Database_Layer["4. PERSISTENCE LAYER (Microsoft SQL Server)"]
        direction TB
        SqlServer[("Microsoft SQL Server: OmniCrmDb")]
        TblCust[("Customers Table")]
        TblLead[("Leads Table")]
        TblFollow[("FollowUps Table")]

        SqlServer --> TblCust
        SqlServer --> TblLead
        SqlServer --> TblFollow
    end

    CustCtrl ==> CustSvc
    LeadCtrl ==> LeadSvc
    FollowCtrl ==> FollowSvc
    DashCtrl ==> DashSvc

    CustSvc ==> DbContext
    LeadSvc ==> DbContext
    FollowSvc ==> DbContext
    DashSvc ==> DbContext

    Config -.-> DbContext
    DbInit -.-> DbContext
    DbContext ==> SqlServer
```

---

## 2. Entity-Relationship Diagram (ERD)

The database schema enforces strict relational modeling, 1-to-Many cardinality, and cascading referential integrity:

```mermaid
erDiagram
    CUSTOMERS ||--o{ LEADS : "has / creates"
    CUSTOMERS ||--o{ FOLLOWUPS : "scheduled for"

    CUSTOMERS {
        int Id PK "Identity (1,1)"
        string Name "Required, Max 100"
        string Email "Required, Unique Index, Max 120"
        string Phone "Optional, Max 25"
        string CompanyName "Optional, Max 100"
        string Address "Optional, Max 250"
        datetime CreatedAt "Default: GETUTCDATE()"
    }

    LEADS {
        int Id PK "Identity (1,1)"
        int CustomerId FK "Foreign Key -> Customers.Id (Cascade Delete)"
        string Title "Required, Max 150"
        int Status "Enum: 0=New, 1=Contacted, 2=Qualified, 3=Converted, 4=Lost"
        decimal EstimatedValue "decimal(18,2)"
        string Notes "Optional, Max 500"
        datetime CreatedAt "Default: GETUTCDATE()"
    }

    FOLLOWUPS {
        int Id PK "Identity (1,1)"
        int CustomerId FK "Foreign Key -> Customers.Id (Cascade Delete)"
        datetime ScheduledDate "Scheduled interaction time"
        string Notes "Required, Max 1000"
        bit IsCompleted "0=Pending, 1=Completed"
        datetime CreatedAt "Default: GETUTCDATE()"
    }
```

---

## 3. Asynchronous Execution Lifecycle

To guarantee that the desktop user interface remains fluid and non-blocking, all database operations employ the **Task-based Asynchronous Pattern (TAP)**:

```mermaid
sequenceDiagram
    autonumber
    actor User as User / Sales Rep
    participant Form as CustomerManagementControl (UI Thread)
    participant Svc as CustomerService (Async)
    participant EF as OmniCrmDbContext (EF Core)
    participant SQL as Microsoft SQL Server

    User->>Form: Types "Apex" in Search Box & Clicks "Search"
    Form->>Form: Set Status: "Loading customers from SQL Server..."
    Form->>Svc: await GetAllAsync("Apex")
    Note over Form: UI Thread remains interactive (User can resize, type, click other tabs)
    Svc->>EF: Customers.Where(...).AsNoTracking().ToListAsync()
    EF->>SQL: Execute Parameterized SQL Query
    SQL-->>EF: Return Record Rows
    EF-->>Svc: Hydrate List<Customer>
    Svc-->>Form: Return List<Customer>
    Form->>Form: Populate DataGridView Rows on UI Thread
    Form->>Form: Set Status: "Showing 1 customer(s)."
```

---

## 4. Layer-by-Layer Architectural Breakdown

### 1. Presentation Layer (Desktop UI)
* **Single Form Shell (`MainForm`):** Acts as the parent container. Contains the dark sidebar navigation, dynamic section title, and real-time database connection status badge.
* **Modular View Injection:** Instead of launching multiple separate windows, `MainForm` swaps lightweight `UserControl` instances (`DashboardControl`, `CustomerManagementControl`, `LeadManagementControl`, `FollowUpManagementControl`) into a central panel.
* **Design System (`UITheme`):** Centralizes colors (Dark Navy `#0F172A`, Royal Indigo `#2563EB`, Canvas `#F8FAFC`), fonts, and grid rendering to maintain a consistent enterprise aesthetic.

### 2. Service Layer (Domain Logic)
* Encapsulates all query composition, business rules, calculations, and data validations.
* **Email Uniqueness:** Prevents duplicate accounts by querying existing emails before saving.
* **Status Filtering & Valuation:** Sums active deals while excluding discarded leads.

### 3. Data Access Layer (Entity Framework Core)
* **`OmniCrmDbContext`:** Manages entity sets, connection lifetimes, and mapping rules.
* **Performance Optimizations:** Queries that are read-only use `.AsNoTracking()` to reduce memory footprint and avoid change-tracker overhead.
* **Resilient Startup (`DbInitializer`):** Automatically initializes the database catalog and seeds enterprise demonstration records if tables are empty.

### 4. Database Layer (SQL Server)
* Relational tables with clustered primary keys, foreign key constraints with cascade deletes, and non-clustered indexes on search/filter columns (`Email`, `Name`, `Status`, `ScheduledDate`).
