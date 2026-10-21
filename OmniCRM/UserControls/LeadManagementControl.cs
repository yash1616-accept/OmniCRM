using System.Data;
using OmniCRM.Forms;
using OmniCRM.Models;
using OmniCRM.Services;

namespace OmniCRM.UserControls;

/// <summary>
/// UserControl implementing pipeline tracking and lead management.
/// </summary>
public class LeadManagementControl : UserControl
{
    private readonly LeadService _leadService;
    private readonly CustomerService _customerService;
    private readonly DataGridView _grid = new();
    private readonly ComboBox _cmbStatusFilter = new();
    private readonly TextBox _txtSearch = new();
    private readonly Label _lblCount = new();
    private readonly Label _lblTotalPipeline = new();
    private List<Lead> _currentLeads = new();

    public LeadManagementControl(LeadService leadService, CustomerService customerService)
    {
        _leadService = leadService;
        _customerService = customerService;
        InitializeComponent();
        _ = LoadLeadsAsync();
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
            Text = "Sales Pipeline & Leads",
            Font = UITheme.HeaderLarge,
            ForeColor = UITheme.TextPrimary,
            AutoSize = true,
            Location = new Point(0, 0)
        };

        var lblSub = new Label
        {
            Text = "Monitor deal stages, forecasted deal values, and customer conversions.",
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

        _txtSearch.Location = new Point(0, 6);
        _txtSearch.Size = new Size(220, 32);
        _txtSearch.Font = UITheme.BodyRegular;
        _txtSearch.PlaceholderText = "Search leads or client...";
        _txtSearch.KeyDown += async (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await LoadLeadsAsync();
            }
        };

        // Status Filter
        var lblFilter = new Label
        {
            Text = "Status:",
            Location = new Point(230, 10),
            AutoSize = true,
            Font = UITheme.BodyBold,
            ForeColor = UITheme.TextSecondary
        };

        _cmbStatusFilter.Location = new Point(285, 6);
        _cmbStatusFilter.Size = new Size(140, 32);
        _cmbStatusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbStatusFilter.Font = UITheme.BodyRegular;
        _cmbStatusFilter.Items.Add("All Statuses");
        _cmbStatusFilter.Items.AddRange(Enum.GetNames<LeadStatus>());
        _cmbStatusFilter.SelectedIndex = 0;
        _cmbStatusFilter.SelectedIndexChanged += async (s, e) => await LoadLeadsAsync();

        var btnAdd = new Button
        {
            Text = "+ New Lead",
            Location = new Point(440, 4),
            Size = new Size(110, 34),
            BackColor = UITheme.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = UITheme.BodyBold,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 0 }
        };
        btnAdd.Click += BtnAdd_Click;

        var btnEdit = new Button
        {
            Text = "Edit",
            Location = new Point(560, 4),
            Size = new Size(75, 34),
            BackColor = Color.White,
            ForeColor = UITheme.TextPrimary,
            FlatStyle = FlatStyle.Flat,
            Font = UITheme.BodyBold,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderColor = UITheme.CardBorder }
        };
        btnEdit.Click += BtnEdit_Click;

        var btnDelete = new Button
        {
            Text = "Delete",
            Location = new Point(645, 4),
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
            Location = new Point(735, 4),
            Size = new Size(95, 34),
            BackColor = Color.White,
            ForeColor = UITheme.TextSecondary,
            FlatStyle = FlatStyle.Flat,
            Font = UITheme.BodyRegular,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderColor = UITheme.CardBorder }
        };
        btnRefresh.Click += async (s, e) => await LoadLeadsAsync();

        pnlActions.Controls.Add(_txtSearch);
        pnlActions.Controls.Add(lblFilter);
        pnlActions.Controls.Add(_cmbStatusFilter);
        pnlActions.Controls.Add(btnAdd);
        pnlActions.Controls.Add(btnEdit);
        pnlActions.Controls.Add(btnDelete);
        pnlActions.Controls.Add(btnRefresh);

        pnlTop.Controls.Add(lblTitle);
        pnlTop.Controls.Add(lblSub);
        pnlTop.Controls.Add(pnlActions);

        // Bottom Strip
        var pnlBottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 32,
            BackColor = Color.Transparent
        };

        _lblCount.Dock = DockStyle.Left;
        _lblCount.Font = UITheme.Subtext;
        _lblCount.ForeColor = UITheme.TextSecondary;
        _lblCount.TextAlign = ContentAlignment.MiddleLeft;

        _lblTotalPipeline.Dock = DockStyle.Right;
        _lblTotalPipeline.Font = UITheme.BodyBold;
        _lblTotalPipeline.ForeColor = UITheme.PrimaryDark;
        _lblTotalPipeline.TextAlign = ContentAlignment.MiddleRight;

        pnlBottom.Controls.Add(_lblCount);
        pnlBottom.Controls.Add(_lblTotalPipeline);

        // Grid Container Card
        var pnlGridCard = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(1)
        };

        UITheme.ApplyModernGridStyle(_grid);
        ConfigureGridColumns();

        _grid.Dock = DockStyle.Fill;
        _grid.CellDoubleClick += (s, e) =>
        {
            if (e.RowIndex >= 0) BtnEdit_Click(s, e);
        };

        pnlGridCard.Controls.Add(_grid);

        Controls.Add(pnlGridCard);
        Controls.Add(pnlBottom);
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
            Name = "Title",
            HeaderText = "Opportunity Title",
            FillWeight = 130,
            ReadOnly = true
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Customer",
            HeaderText = "Customer / Account",
            FillWeight = 110,
            ReadOnly = true
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Status",
            HeaderText = "Stage",
            FillWeight = 70,
            ReadOnly = true
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Value",
            HeaderText = "Est. Value",
            FillWeight = 70,
            ReadOnly = true
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Notes",
            HeaderText = "Notes",
            FillWeight = 120,
            ReadOnly = true
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Date",
            HeaderText = "Created",
            FillWeight = 60,
            ReadOnly = true
        });
    }

    public async Task LoadLeadsAsync()
    {
        try
        {
            LeadStatus? statusFilter = null;
            if (_cmbStatusFilter.SelectedIndex > 0)
            {
                statusFilter = Enum.Parse<LeadStatus>(_cmbStatusFilter.SelectedItem?.ToString() ?? "New");
            }

            _currentLeads = await _leadService.GetAllAsync(statusFilter, _txtSearch.Text);

            _grid.Rows.Clear();
            decimal totalVal = 0m;

            foreach (var l in _currentLeads)
            {
                totalVal += l.EstimatedValue;
                _grid.Rows.Add(
                    l.Id,
                    l.Title,
                    l.Customer?.Name ?? "—",
                    l.Status.ToString(),
                    l.EstimatedValue.ToString("C0"),
                    string.IsNullOrWhiteSpace(l.Notes) ? "—" : l.Notes,
                    l.CreatedAt.ToString("MMM dd, yyyy")
                );
            }

            _lblCount.Text = $"Showing {_currentLeads.Count} lead opportunity record(s).";
            _lblTotalPipeline.Text = $"Total Active Pipeline Value: {totalVal:C0}";
        }
        catch (Exception ex)
        {
            _lblCount.Text = $"Error: {ex.Message}";
        }
    }

    private Lead? GetSelectedLead()
    {
        if (_grid.CurrentRow == null || _grid.CurrentRow.Index < 0) return null;
        int id = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
        return _currentLeads.FirstOrDefault(l => l.Id == id);
    }

    private async void BtnAdd_Click(object? sender, EventArgs e)
    {
        using var dialog = new LeadDialogForm(_customerService);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            await _leadService.CreateAsync(dialog.Lead);
            MessageBox.Show("Lead opportunity recorded successfully!", "OmniCRM",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadLeadsAsync();
        }
    }

    private async void BtnEdit_Click(object? sender, EventArgs e)
    {
        var selected = GetSelectedLead();
        if (selected == null)
        {
            MessageBox.Show("Please select a lead to edit.", "Selection Required",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new LeadDialogForm(_customerService, new Lead
        {
            Id = selected.Id,
            Title = selected.Title,
            CustomerId = selected.CustomerId,
            Status = selected.Status,
            EstimatedValue = selected.EstimatedValue,
            Notes = selected.Notes,
            CreatedAt = selected.CreatedAt
        });

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            await _leadService.UpdateAsync(dialog.Lead);
            MessageBox.Show("Lead updated successfully!", "OmniCRM",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadLeadsAsync();
        }
    }

    private async void BtnDelete_Click(object? sender, EventArgs e)
    {
        var selected = GetSelectedLead();
        if (selected == null)
        {
            MessageBox.Show("Please select a lead to delete.", "Selection Required",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"Delete lead opportunity '{selected.Title}'?",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm == DialogResult.Yes)
        {
            await _leadService.DeleteAsync(selected.Id);
            await LoadLeadsAsync();
        }
    }
}
