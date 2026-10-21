using System.Data;
using OmniCRM.Forms;
using OmniCRM.Models;
using OmniCRM.Services;

namespace OmniCRM.UserControls;

/// <summary>
/// UserControl implementing scheduled follow-up reminders and completion status.
/// </summary>
public class FollowUpManagementControl : UserControl
{
    private readonly FollowUpService _followUpService;
    private readonly CustomerService _customerService;
    private readonly DataGridView _grid = new();
    private readonly CheckBox _chkOnlyPending = new();
    private readonly Label _lblCount = new();
    private List<FollowUp> _currentFollowUps = new();

    public FollowUpManagementControl(FollowUpService followUpService, CustomerService customerService)
    {
        _followUpService = followUpService;
        _customerService = customerService;
        InitializeComponent();
        _ = LoadFollowUpsAsync();
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;
        BackColor = UITheme.ContentBg;
        Padding = new Padding(24);

        // Header Panel
        var pnlTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            BackColor = Color.Transparent
        };

        var lblTitle = new Label
        {
            Text = "Customer Follow-Up Scheduler",
            Font = UITheme.HeaderLarge,
            ForeColor = UITheme.TextPrimary,
            AutoSize = true,
            Location = new Point(0, 0)
        };

        var lblSub = new Label
        {
            Text = "Schedule calls, send quotes, and keep customer relationship commitments on track.",
            Font = UITheme.Subtext,
            ForeColor = UITheme.TextSecondary,
            AutoSize = true,
            Location = new Point(0, 32)
        };

        // Actions Row
        var pnlActions = new Panel
        {
            Location = new Point(0, 60),
            Size = new Size(1100, 42),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        _chkOnlyPending.Text = "Show Pending Tasks Only";
        _chkOnlyPending.Font = UITheme.BodyBold;
        _chkOnlyPending.ForeColor = UITheme.TextPrimary;
        _chkOnlyPending.Location = new Point(0, 8);
        _chkOnlyPending.AutoSize = true;
        _chkOnlyPending.Checked = true;
        _chkOnlyPending.CheckedChanged += async (s, e) => await LoadFollowUpsAsync();

        var btnAdd = new Button
        {
            Text = "+ Schedule Follow-Up",
            Location = new Point(220, 4),
            Size = new Size(170, 34),
            BackColor = UITheme.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = UITheme.BodyBold,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 0 }
        };
        btnAdd.Click += BtnAdd_Click;

        var btnToggle = new Button
        {
            Text = "✓ Toggle Completed",
            Location = new Point(400, 4),
            Size = new Size(160, 34),
            BackColor = Color.White,
            ForeColor = UITheme.Success,
            FlatStyle = FlatStyle.Flat,
            Font = UITheme.BodyBold,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderColor = Color.FromArgb(167, 243, 208) }
        };
        btnToggle.Click += BtnToggle_Click;

        var btnDelete = new Button
        {
            Text = "Delete",
            Location = new Point(570, 4),
            Size = new Size(80, 34),
            BackColor = Color.White,
            ForeColor = UITheme.Danger,
            FlatStyle = FlatStyle.Flat,
            Font = UITheme.BodyBold,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderColor = Color.FromArgb(254, 202, 202) }
        };
        btnDelete.Click += BtnDelete_Click;

        var btnRefresh = new Button
        {
            Text = "🔄 Refresh",
            Location = new Point(660, 4),
            Size = new Size(95, 34),
            BackColor = Color.White,
            ForeColor = UITheme.TextSecondary,
            FlatStyle = FlatStyle.Flat,
            Font = UITheme.BodyRegular,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderColor = UITheme.CardBorder }
        };
        btnRefresh.Click += async (s, e) => await LoadFollowUpsAsync();

        pnlActions.Controls.Add(_chkOnlyPending);
        pnlActions.Controls.Add(btnAdd);
        pnlActions.Controls.Add(btnToggle);
        pnlActions.Controls.Add(btnDelete);
        pnlActions.Controls.Add(btnRefresh);

        pnlTop.Controls.Add(lblTitle);
        pnlTop.Controls.Add(lblSub);
        pnlTop.Controls.Add(pnlActions);

        // Bottom Strip
        _lblCount.Dock = DockStyle.Bottom;
        _lblCount.Height = 28;
        _lblCount.Font = UITheme.Subtext;
        _lblCount.ForeColor = UITheme.TextSecondary;
        _lblCount.TextAlign = ContentAlignment.MiddleLeft;

        // Grid Container
        var pnlGridCard = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(1)
        };

        UITheme.ApplyModernGridStyle(_grid);
        ConfigureGridColumns();

        _grid.Dock = DockStyle.Fill;
        pnlGridCard.Controls.Add(_grid);

        Controls.Add(pnlGridCard);
        Controls.Add(_lblCount);
        Controls.Add(pnlTop);
    }

    private void ConfigureGridColumns()
    {
        _grid.Columns.Clear();

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Id",
            HeaderText = "ID",
            FillWeight = 30,
            ReadOnly = true
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Customer",
            HeaderText = "Customer Name",
            FillWeight = 110,
            ReadOnly = true
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "DueDate",
            HeaderText = "Scheduled Due Date",
            FillWeight = 85,
            ReadOnly = true
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Notes",
            HeaderText = "Action Items / Notes",
            FillWeight = 180,
            ReadOnly = true
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Status",
            HeaderText = "Status",
            FillWeight = 60,
            ReadOnly = true
        });
    }

    public async Task LoadFollowUpsAsync()
    {
        try
        {
            _currentFollowUps = await _followUpService.GetAllAsync(_chkOnlyPending.Checked);

            _grid.Rows.Clear();
            foreach (var f in _currentFollowUps)
            {
                string statusText = f.IsCompleted ? "Completed" : "Pending";
                _grid.Rows.Add(
                    f.Id,
                    f.Customer?.Name ?? "—",
                    f.ScheduledDate.ToLocalTime().ToString("MMM dd, yyyy HH:mm"),
                    f.Notes,
                    statusText
                );
            }

            _lblCount.Text = $"Showing {_currentFollowUps.Count} follow-up interaction task(s).";
        }
        catch (Exception ex)
        {
            _lblCount.Text = $"Error: {ex.Message}";
        }
    }

    private FollowUp? GetSelectedFollowUp()
    {
        if (_grid.CurrentRow == null || _grid.CurrentRow.Index < 0) return null;
        int id = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
        return _currentFollowUps.FirstOrDefault(f => f.Id == id);
    }

    private async void BtnAdd_Click(object? sender, EventArgs e)
    {
        using var dialog = new FollowUpDialogForm(_customerService);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            await _followUpService.CreateAsync(dialog.FollowUp);
            MessageBox.Show("Follow-up scheduled successfully!", "OmniCRM",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadFollowUpsAsync();
        }
    }

    private async void BtnToggle_Click(object? sender, EventArgs e)
    {
        var selected = GetSelectedFollowUp();
        if (selected == null)
        {
            MessageBox.Show("Please select a follow-up record to update.", "Selection Required",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        await _followUpService.ToggleCompletedAsync(selected.Id);
        await LoadFollowUpsAsync();
    }

    private async void BtnDelete_Click(object? sender, EventArgs e)
    {
        var selected = GetSelectedFollowUp();
        if (selected == null)
        {
            MessageBox.Show("Please select a follow-up record to delete.", "Selection Required",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show("Delete this follow-up reminder?", "Confirm",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (confirm == DialogResult.Yes)
        {
            await _followUpService.DeleteAsync(selected.Id);
            await LoadFollowUpsAsync();
        }
    }
}
