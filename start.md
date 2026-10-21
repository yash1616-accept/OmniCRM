# OmniCRM — Getting Started & Live Demo Guide

This guide provides step-by-step instructions on prerequisites, configuring SQL Server, launching the project, and executing an end-to-end live demonstration.

---

## 1. Prerequisites & Environment

Ensure the following tools are installed on your Windows machine:

| Component | Minimum Version | Notes |
| :--- | :--- | :--- |
| **Operating System** | Windows 10 / 11 | Required for Windows Forms runtime |
| **.NET SDK** | .NET 8.0 SDK (or .NET 10) | Verified with SDK `10.0.402` and .NET 8 runtime |
| **Database** | Microsoft SQL Server | `MSSQLSERVER` (Local instance), SQL Express, or LocalDB |
| **IDE / Editor** | Visual Studio Code or Visual Studio 2022 | C# Dev Kit extension recommended |

---

## 2. Setting Up SQL Server

OmniCRM is configured by default to connect to the local SQL Server instance (`Server=localhost;Database=OmniCrmDb`).

### Starting the SQL Server Service
On Windows, check and start the `MSSQLSERVER` service:

1. Press <kbd>Win</kbd> + <kbd>X</kbd> and select **Terminal (Admin)** or **PowerShell (Admin)**.
2. Run the following command:
   ```powershell
   Start-Service MSSQLSERVER
   ```
3. Verify that the service is running:
   ```powershell
   Get-Service MSSQLSERVER
   ```
   *Expected status: `Running`.*

> [!TIP]
> **Alternative using GUI:** Press <kbd>Win</kbd> + <kbd>R</kbd>, type `services.msc`, locate **SQL Server (MSSQLSERVER)**, right-click, and select **Start**.

---

## 3. How to Build & Run the Application

Navigate to the project directory in PowerShell or Command Prompt:

```powershell
# Navigate into the OmniCRM folder
cd "c:\Users\vivek halnure\Desktop\asp.project\OmniCRM"

# Clean and restore dependencies
dotnet restore

# Build the solution
dotnet build

# Launch the desktop application
dotnet run
```

> [!NOTE]
> On the first startup, `DbInitializer` automatically checks SQL Server, creates the `OmniCrmDb` database if it does not already exist, creates all relational tables, and loads initial sample data.

---

## 4. Live Demo Walkthrough (Step-by-Step Script)

Follow this structured script when demonstrating the application to evaluators or stakeholders:

### Act 1: Executive Dashboard (Real-Time CRM KPIs)
1. Launch the application. The **Executive Overview** screen opens with a dark navy sidebar and enterprise card layouts.
2. **Observe KPI Cards:**
   - **Total Customers:** Shows active customer accounts.
   - **Active Leads:** Pipeline opportunities currently in progress.
   - **Converted Deals:** Successfully closed opportunities.
   - **Pipeline Value:** Dollar value of deals in the pipeline.
3. **Recent Activity Feeds:** Point out the live lists displaying the newest customers and latest sales opportunities.
4. **Database Connection Status:** Direct attention to the bottom of the sidebar showing `● SQL Server Connected`.

---

### Act 2: Customer Directory & Validation (CRUD Operations)
1. Click **`👥 Customers`** in the left sidebar navigation.
2. **Live Search Demonstration:**
   - In the search bar, type `Apex` and press <kbd>Enter</kbd> (or click **Search**).
   - Notice the table filters instantly.
   - Click **Clear** to restore all records.
3. **Create Customer & Validation:**
   - Click **`+ Add Customer`**. The modal dialog appears.
   - Click **Save Changes** without filling anything: notice the red validation cue: *"Please enter the customer's full name."*
   - Enter invalid email format (e.g. `test@invalid`): notice email validation trigger.
   - Fill in valid test details:
     - **Name:** `Global Dynamics Corp`
     - **Email:** `contact@globaldynamics.com`
     - **Phone:** `+1 (555) 345-6789`
     - **Company:** `Global Dynamics`
     - **Address:** `742 Evergreen Terrace, Springfield`
   - Click **Create Customer**: Success notification pops up, and the new customer appears in the directory table and in SQL Server.
4. **Edit Customer:**
   - Select the newly created customer row and click **Edit** (or double-click the row).
   - Change the phone number or company name, then click **Save Changes**.
5. **Delete Customer:**
   - Select a row and click **Delete**. Confirm the prompt to remove the record.

---

### Act 3: Sales Pipeline & Deal Stages
1. Click **`💼 Sales Leads`** in the left sidebar.
2. **Filter by Pipeline Stage:**
   - Use the **Status** dropdown to switch between `New`, `Contacted`, `Qualified`, `Converted`, and `Lost`.
   - Point out the total active pipeline sum calculated at the bottom right.
3. **Add Sales Lead:**
   - Click **`+ New Lead`**.
   - Enter Title: `Warehouse Automation System`.
   - Select the Associated Customer from the dropdown.
   - Choose Stage: `Qualified`.
   - Enter Estimated Value: `$75,000`.
   - Click **Create Lead**. The pipeline immediately refreshes and updates total valuation.

---

### Act 4: Customer Follow-Up Scheduler
1. Click **`📅 Follow-Ups`** in the left sidebar.
2. **Show Scheduled Tasks:**
   - Review pending follow-up meetings and call reminders.
3. **Schedule New Follow-Up:**
   - Click **`+ Schedule Follow-Up`**.
   - Select a Customer, pick a target date and time using the custom calendar picker, and add meeting notes.
   - Click **Save Follow-Up**.
4. **Toggle Completion:**
   - Select a pending task and click **`✓ Toggle Completed`**.
   - Watch the status switch between `Pending` and `Completed`.
   - Toggle the checkbox **Show Pending Tasks Only** to filter completed tasks out of view.

---

### Act 5: SQL Server Database Verification (Behind the Scenes)
Open SQL Server Management Studio (SSMS), Azure Data Studio, or PowerShell to verify persistence:

```powershell
# Query SQL Server via sqlcmd or PowerShell
sqlcmd -S localhost -E -Q "SELECT Id, Name, Email, CompanyName, CreatedAt FROM OmniCrmDb.dbo.Customers;"
```
Demonstrate that all records created in the UI are physically committed to SQL Server with indexes, foreign keys, and constraints.

---

## 5. Troubleshooting & FAQs

### Problem: "Database notice: Cannot open database 'OmniCrmDb'" or "Connecting to SQL Server..."
* **Cause:** The SQL Server Windows service is stopped.
* **Fix:** Open PowerShell as Administrator and run:
  ```powershell
  Start-Service MSSQLSERVER
  ```

### Problem: SQL Server instance is named SQLEXPRESS instead of default MSSQLSERVER
* **Fix:** Open [appsettings.json](file:///c:/Users/vivek%20halnure/Desktop/asp.project/OmniCRM/appsettings.json) and set `DefaultConnection` to:
  ```json
  "Server=.\\SQLEXPRESS;Database=OmniCrmDb;Trusted_Connection=True;TrustServerCertificate=True"
  ```
