using System.Drawing.Drawing2D;
using OmniCRM.Services;

namespace OmniCRM.UserControls;

/// <summary>
/// Modern Executive Dashboard displaying real-time CRM KPIs, pipeline statistics, and recent activities.
/// </summary>
public class DashboardControl : UserControl
{
    private readonly DashboardService _dashboardService;
    private readonly Action<string> _navigateToTab;

    private readonly Label _lblTotalCustomers = new();
    private readonly Label _lblActiveLeads = new();
    private readonly Label _lblConvertedLeads = new();
    private readonly Label _lblPipelineValue = new();
    private readonly Label _lblPendingFollowUps = new();

    private readonly ListBox _lstRecentCustomers = new();
    private readonly ListBox _lstRecentLeads = new();
    private readonly ListBox _lstUpcomingFollowUps = new();

    public DashboardControl(DashboardService dashboardService, Action<string> navigateToTab)
    {
        _dashboardService = dashboardService;
        _navigateToTab = navigateToTab;

        InitializeComponent();
        _ = LoadMetricsAsync();
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;
        BackColor = UITheme.ContentBg;
        AutoScroll = true;
        Padding = new Padding(28, 24, 28, 24);

        // Header
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Color.Transparent
        };

        var lblWelcome = new Label
        {
            Text = "Executive Overview",
            Font = UITheme.HeaderLarge,
            ForeColor = UITheme.TextPrimary,
            AutoSize = true,
            Location = new Point(0, 0)
        };

        var lblSubtitle = new Label
        {
            Text = "Live operational performance, conversion pipeline, and customer relationships.",
            Font = UITheme.Subtext,
            ForeColor = UITheme.TextSecondary,
            AutoSize = true,
            Location = new Point(0, 32)
        };

        var btnRefresh = new Button
        {
            Text = "🔄 Refresh Metrics",
            Size = new Size(140, 36),
            Location = new Point(880, 8),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            BackColor = Color.White,
            ForeColor = UITheme.Primary,
            FlatStyle = FlatStyle.Flat,
            Font = UITheme.BodyBold,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderColor = UITheme.CardBorder }
        };
        btnRefresh.Click += async (s, e) => await LoadMetricsAsync();

        pnlHeader.Controls.Add(lblWelcome);
        pnlHeader.Controls.Add(lblSubtitle);
        pnlHeader.Controls.Add(btnRefresh);

        // Metric Cards Container
        var pnlCards = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 135,
            BackColor = Color.Transparent,
            WrapContents = false,
            AutoScroll = false,
            Padding = new Padding(0, 6, 0, 10)
        };

        // 4 KPI Cards
        pnlCards.Controls.Add(CreateKpiCard("TOTAL CUSTOMERS", _lblTotalCustomers, "👥 Active Clients", UITheme.Primary, "Customers"));
        pnlCards.Controls.Add(CreateKpiCard("ACTIVE LEADS", _lblActiveLeads, "💼 Pipeline Opportunities", UITheme.Warning, "Leads"));
        pnlCards.Controls.Add(CreateKpiCard("CONVERTED DEALS", _lblConvertedLeads, "🏆 Won Deals", UITheme.Success, "Leads"));
        pnlCards.Controls.Add(CreateKpiCard("PIPELINE VALUE", _lblPipelineValue, "💰 Total Deal Forecast", Color.FromArgb(124, 58, 237), "Leads"));

        // Activity Feeds Container (2 columns)
        var pnlFeeds = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 360,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 16, 0, 0)
        };
        pnlFeeds.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        pnlFeeds.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

        // Column 1: Recent Customers
        var pnlCol1 = CreateActivityCard("Recently Acquired Customers", _lstRecentCustomers, "Customers");
        // Column 2: Recent Pipeline Deals
        var pnlCol2 = CreateActivityCard("Recent Sales Pipeline Activity", _lstRecentLeads, "Leads");

        pnlFeeds.Controls.Add(pnlCol1, 0, 0);
        pnlFeeds.Controls.Add(pnlCol2, 1, 0);

        Controls.Add(pnlFeeds);
        Controls.Add(pnlCards);
        Controls.Add(pnlHeader);
    }

    private Panel CreateKpiCard(string caption, Label valueLabel, string subCaption, Color accentColor, string targetTab)
    {
        var card = new Panel
        {
            Width = 245,
            Height = 115,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 20, 0),
            Cursor = Cursors.Hand
        };
        card.Paint += (s, e) =>
        {
            // Top accent color bar
            using var brush = new SolidBrush(accentColor);
            e.Graphics.FillRectangle(brush, 0, 0, card.Width, 4);

            // Subtle border
            using var pen = new Pen(UITheme.CardBorder);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };
        card.Click += (s, e) => _navigateToTab(targetTab);

        var lblCaption = new Label
        {
            Text = caption,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            ForeColor = UITheme.TextSecondary,
            Location = new Point(18, 16),
            AutoSize = true
        };

        valueLabel.Text = "—";
        valueLabel.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        valueLabel.ForeColor = UITheme.TextPrimary;
        valueLabel.Location = new Point(16, 36);
        valueLabel.AutoSize = true;

        var lblSub = new Label
        {
            Text = subCaption,
            Font = UITheme.Subtext,
            ForeColor = UITheme.TextMuted,
            Location = new Point(18, 82),
            AutoSize = true
        };

        card.Controls.Add(lblCaption);
        card.Controls.Add(valueLabel);
        card.Controls.Add(lblSub);

        return card;
    }

    private Panel CreateActivityCard(string title, ListBox listBox, string targetTab)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20),
            Margin = new Padding(0, 0, 16, 0)
        };
        card.Paint += (s, e) =>
        {
            using var pen = new Pen(UITheme.CardBorder);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 36,
            BackColor = Color.Transparent
        };

        var lblTitle = new Label
        {
            Text = title,
            Font = UITheme.HeaderSmall,
            ForeColor = UITheme.TextPrimary,
            Dock = DockStyle.Left,
            AutoSize = true
        };

        var btnViewAll = new Button
        {
            Text = "View All →",
            Dock = DockStyle.Right,
            FlatStyle = FlatStyle.Flat,
            ForeColor = UITheme.Primary,
            Font = UITheme.Subtext,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 0 }
        };
        btnViewAll.Click += (s, e) => _navigateToTab(targetTab);

        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(btnViewAll);

        listBox.Dock = DockStyle.Fill;
        listBox.BorderStyle = BorderStyle.None;
        listBox.Font = UITheme.BodyRegular;
        listBox.ItemHeight = 34;
        listBox.DrawMode = DrawMode.OwnerDrawFixed;
        listBox.DrawItem += (s, e) =>
        {
            if (e.Index < 0 || e.Index >= listBox.Items.Count) return;

            e.DrawBackground();
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            using var brush = new SolidBrush(isSelected ? UITheme.PrimaryDark : UITheme.TextPrimary);
            string itemText = listBox.Items[e.Index].ToString() ?? "";
            e.Graphics.DrawString(itemText, UITheme.BodyRegular, brush, e.Bounds.X + 8, e.Bounds.Y + 8);
            e.DrawFocusRectangle();
        };

        card.Controls.Add(listBox);
        card.Controls.Add(pnlHeader);

        return card;
    }

    public async Task LoadMetricsAsync()
    {
        try
        {
            var metrics = await _dashboardService.GetMetricsAsync();

            _lblTotalCustomers.Text = metrics.TotalCustomers.ToString("N0");
            _lblActiveLeads.Text = metrics.ActiveLeads.ToString("N0");
            _lblConvertedLeads.Text = metrics.ConvertedLeads.ToString("N0");
            _lblPipelineValue.Text = metrics.TotalPipelineValue.ToString("C0");

            // Populate recent customers
            _lstRecentCustomers.Items.Clear();
            if (metrics.RecentCustomers.Count == 0)
            {
                _lstRecentCustomers.Items.Add("No customer records found yet.");
            }
            else
            {
                foreach (var c in metrics.RecentCustomers)
                {
                    string company = string.IsNullOrWhiteSpace(c.CompanyName) ? "Individual" : c.CompanyName;
                    _lstRecentCustomers.Items.Add($"•  {c.Name}  —  {company} ({c.Email})");
                }
            }

            // Populate recent leads
            _lstRecentLeads.Items.Clear();
            if (metrics.RecentLeads.Count == 0)
            {
                _lstRecentLeads.Items.Add("No active sales leads found yet.");
            }
            else
            {
                foreach (var l in metrics.RecentLeads)
                {
                    _lstRecentLeads.Items.Add($"•  [{l.Status}] {l.Title} — {l.EstimatedValue:C0} ({l.Customer?.Name ?? "General"})");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dashboard loading error: {ex.Message}");
        }
    }
}
