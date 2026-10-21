using OmniCRM.Data;
using OmniCRM.Models;
using OmniCRM.Services;
using OmniCRM.UserControls;

namespace OmniCRM.Forms;

/// <summary>
/// Main application shell for OmniCRM featuring enterprise sidebar navigation and modular content views.
/// </summary>
public class MainForm : Form
{
    private readonly OmniCrmDbContext _dbContext;
    private readonly CustomerService _customerService;
    private readonly LeadService _leadService;
    private readonly FollowUpService _followUpService;
    private readonly DashboardService _dashboardService;

    // UI Components
    private readonly Panel _pnlSidebar = new();
    private readonly Panel _pnlTopBar = new();
    private readonly Panel _pnlContent = new();
    private readonly Label _lblActiveSection = new();
    private readonly Label _lblDbStatus = new();

    private readonly List<Button> _navButtons = new();
    private DashboardControl? _dashboardView;
    private CustomerManagementControl? _customersView;
    private LeadManagementControl? _leadsView;
    private FollowUpManagementControl? _followUpsView;

    public MainForm()
    {
        // Initialize services and EF Core DB Context
        _dbContext = new OmniCrmDbContext();
        _customerService = new CustomerService(_dbContext);
        _leadService = new LeadService(_dbContext);
        _followUpService = new FollowUpService(_dbContext);
        _dashboardService = new DashboardService(_dbContext);

        InitializeComponent();
        NavigateTo("Dashboard");

        // Asynchronously check and initialize database
        _ = InitializeDatabaseAsync();
    }

    private void InitializeComponent()
    {
        Text = "OmniCRM — Enterprise Customer Relationship Management";
        Size = new Size(1280, 800);
        MinimumSize = new Size(1024, 650);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UITheme.ContentBg;
        Font = UITheme.BodyRegular;

        // 1. Sidebar Panel (Left)
        _pnlSidebar.Dock = DockStyle.Left;
        _pnlSidebar.Width = 240;
        _pnlSidebar.BackColor = UITheme.SidebarBg;
        _pnlSidebar.Padding = new Padding(16, 20, 16, 20);

        // Brand Logo & Title
        var pnlBrand = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Color.Transparent
        };

        var lblBrandName = new Label
        {
            Text = "OmniCRM",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(4, 4),
            AutoSize = true
        };

        var lblBrandSub = new Label
        {
            Text = "Universal Enterprise CRM",
            Font = new Font("Segoe UI", 8F, FontStyle.Regular),
            ForeColor = UITheme.TextMuted,
            Location = new Point(6, 36),
            AutoSize = true
        };

        pnlBrand.Controls.Add(lblBrandName);
        pnlBrand.Controls.Add(lblBrandSub);

        // Navigation Buttons
        var pnlNav = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 16, 0, 0)
        };

        int navTop = 0;
        int btnHeight = 44;
        int spacing = 6;

        AddNavButton(pnlNav, "Dashboard", "📊   Dashboard", ref navTop, btnHeight, spacing);
        AddNavButton(pnlNav, "Customers", "👥   Customers", ref navTop, btnHeight, spacing);
        AddNavButton(pnlNav, "Leads", "💼   Sales Leads", ref navTop, btnHeight, spacing);
        AddNavButton(pnlNav, "FollowUps", "📅   Follow-Ups", ref navTop, btnHeight, spacing);

        // Sidebar Footer - DB Status
        var pnlSidebarFooter = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 65,
            BackColor = Color.FromArgb(20, 30, 50),
            Padding = new Padding(12)
        };

        var lblDbTitle = new Label
        {
            Text = "DATABASE STATUS",
            Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
            ForeColor = UITheme.TextMuted,
            Location = new Point(12, 10),
            AutoSize = true
        };

        _lblDbStatus.Text = "● Connecting to SQL Server...";
        _lblDbStatus.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
        _lblDbStatus.ForeColor = UITheme.Warning;
        _lblDbStatus.Location = new Point(12, 28);
        _lblDbStatus.Size = new Size(200, 24);

        pnlSidebarFooter.Controls.Add(lblDbTitle);
        pnlSidebarFooter.Controls.Add(_lblDbStatus);

        _pnlSidebar.Controls.Add(pnlNav);
        _pnlSidebar.Controls.Add(pnlBrand);
        _pnlSidebar.Controls.Add(pnlSidebarFooter);

        // 2. Top Header Bar
        _pnlTopBar.Dock = DockStyle.Top;
        _pnlTopBar.Height = 60;
        _pnlTopBar.BackColor = Color.White;
        _pnlTopBar.Padding = new Padding(28, 12, 28, 12);
        _pnlTopBar.Paint += (s, e) =>
        {
            using var pen = new Pen(UITheme.CardBorder);
            e.Graphics.DrawLine(pen, 0, _pnlTopBar.Height - 1, _pnlTopBar.Width, _pnlTopBar.Height - 1);
        };

        _lblActiveSection.Text = "Dashboard";
        _lblActiveSection.Font = UITheme.HeaderMedium;
        _lblActiveSection.ForeColor = UITheme.TextPrimary;
        _lblActiveSection.Dock = DockStyle.Left;
        _lblActiveSection.TextAlign = ContentAlignment.MiddleLeft;
        _lblActiveSection.AutoSize = true;

        var btnQuickCustomer = new Button
        {
            Text = "+ Quick Customer",
            Size = new Size(150, 36),
            Dock = DockStyle.Right,
            BackColor = UITheme.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = UITheme.BodyBold,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 0 }
        };
        btnQuickCustomer.Click += BtnQuickCustomer_Click;

        _pnlTopBar.Controls.Add(_lblActiveSection);
        _pnlTopBar.Controls.Add(btnQuickCustomer);

        // 3. Central Content Panel
        _pnlContent.Dock = DockStyle.Fill;
        _pnlContent.BackColor = UITheme.ContentBg;

        Controls.Add(_pnlContent);
        Controls.Add(_pnlTopBar);
        Controls.Add(_pnlSidebar);
    }

    private void AddNavButton(Panel parent, string tag, string title, ref int top, int height, int spacing)
    {
        var btn = new Button
        {
            Text = title,
            Tag = tag,
            Location = new Point(0, top),
            Size = new Size(208, height),
            BackColor = Color.Transparent,
            ForeColor = UITheme.TextMuted,
            FlatStyle = FlatStyle.Flat,
            Font = UITheme.BodyBold,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 0, MouseOverBackColor = UITheme.SidebarHover }
        };

        btn.Click += (s, e) => NavigateTo(tag);

        _navButtons.Add(btn);
        parent.Controls.Add(btn);

        top += height + spacing;
    }

    public void NavigateTo(string tabName)
    {
        // Update navigation button active state
        foreach (var btn in _navButtons)
        {
            bool isActive = string.Equals(btn.Tag?.ToString(), tabName, StringComparison.OrdinalIgnoreCase);
            btn.BackColor = isActive ? UITheme.SidebarActive : Color.Transparent;
            btn.ForeColor = isActive ? Color.White : UITheme.TextMuted;
        }

        _pnlContent.Controls.Clear();

        switch (tabName.ToLowerInvariant())
        {
            case "dashboard":
                _lblActiveSection.Text = "Dashboard & KPIs";
                _dashboardView ??= new DashboardControl(_dashboardService, NavigateTo);
                _pnlContent.Controls.Add(_dashboardView);
                _ = _dashboardView.LoadMetricsAsync();
                break;

            case "customers":
                _lblActiveSection.Text = "Customer Relationship Directory";
                _customersView ??= new CustomerManagementControl(_customerService);
                _pnlContent.Controls.Add(_customersView);
                _ = _customersView.LoadCustomersAsync();
                break;

            case "leads":
                _lblActiveSection.Text = "Sales Leads & Opportunities";
                _leadsView ??= new LeadManagementControl(_leadService, _customerService);
                _pnlContent.Controls.Add(_leadsView);
                _ = _leadsView.LoadLeadsAsync();
                break;

            case "followups":
                _lblActiveSection.Text = "Customer Follow-Up Scheduler";
                _followUpsView ??= new FollowUpManagementControl(_followUpService, _customerService);
                _pnlContent.Controls.Add(_followUpsView);
                _ = _followUpsView.LoadFollowUpsAsync();
                break;
        }
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            bool success = await DbInitializer.InitializeAsync(_dbContext);
            if (success)
            {
                _lblDbStatus.Text = "● SQL Server Connected";
                _lblDbStatus.ForeColor = UITheme.Success;

                // Refresh active view
                if (_dashboardView != null)
                {
                    await _dashboardView.LoadMetricsAsync();
                }
            }
            else
            {
                _lblDbStatus.Text = "● SQL Server Standby / Local";
                _lblDbStatus.ForeColor = UITheme.Warning;
            }
        }
        catch (Exception ex)
        {
            _lblDbStatus.Text = "● Offline / Installing SQL";
            _lblDbStatus.ForeColor = UITheme.Danger;
            System.Diagnostics.Debug.WriteLine($"Db Init Exception: {ex.Message}");
        }
    }

    private async void BtnQuickCustomer_Click(object? sender, EventArgs e)
    {
        using var dialog = new CustomerDialogForm();
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            var result = await _customerService.CreateAsync(dialog.Customer);
            if (result.Success)
            {
                MessageBox.Show("Customer created successfully!", "OmniCRM",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (_customersView != null) await _customersView.LoadCustomersAsync();
                if (_dashboardView != null) await _dashboardView.LoadMetricsAsync();
            }
            else
            {
                MessageBox.Show(result.ErrorMessage ?? "Failed to create customer.", "Notice",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dbContext.Dispose();
        }
        base.Dispose(disposing);
    }
}
