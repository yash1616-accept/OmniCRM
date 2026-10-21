using System.Data;
using OmniCRM.Forms;
using OmniCRM.Models;
using OmniCRM.Services;

namespace OmniCRM.UserControls;

/// <summary>
/// UserControl implementing customer management operations (List, Search, Create, Edit, Delete).
/// </summary>
public class CustomerManagementControl : UserControl
{
    private readonly CustomerService _customerService;
    private readonly DataGridView _grid = new();
    private readonly TextBox _txtSearch = new();
    private readonly Label _lblCount = new();
    private readonly Label _lblStatus = new();
    private List<Customer> _currentCustomers = new();

    public CustomerManagementControl(CustomerService customerService)
    {
        _customerService = customerService;
        InitializeComponent();
        _ = LoadCustomersAsync();
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;
        BackColor = UITheme.ContentBg;
        Padding = new Padding(24);

        // Header and Actions Panel
        var pnlTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            BackColor = Color.Transparent
        };

        var lblTitle = new Label
        {
            Text = "Customer Directory",
            Font = UITheme.HeaderLarge,
            ForeColor = UITheme.TextPrimary,
            AutoSize = true,
            Location = new Point(0, 0)
        };

        var lblSub = new Label
        {
            Text = "Manage customer profiles, contact info, and associated sales pipelines.",
            Font = UITheme.Subtext,
            ForeColor = UITheme.TextSecondary,
            AutoSize = true,
            Location = new Point(0, 32)
        };

        // Actions Row (Search + Buttons)
        var pnlActions = new Panel
        {
            Location = new Point(0, 60),
            Size = new Size(1100, 42),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        _txtSearch.Location = new Point(0, 6);
        _txtSearch.Size = new Size(260, 32);
        _txtSearch.Font = UITheme.BodyRegular;
        _txtSearch.PlaceholderText = "Search by name, email, company...";
        _txtSearch.KeyDown += async (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await LoadCustomersAsync(_txtSearch.Text);
            }
        };

        var btnSearch = new Button
        {
            Text = "Search",
            Location = new Point(270, 4),
            Size = new Size(80, 34),
            BackColor = UITheme.PrimaryLight,
            ForeColor = UITheme.PrimaryDark,
            FlatStyle = FlatStyle.Flat,
            Font = UITheme.BodyBold,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderColor = UITheme.Primary }
        };
        btnSearch.Click += async (s, e) => await LoadCustomersAsync(_txtSearch.Text);

        var btnClear = new Button
        {
            Text = "Clear",
            Location = new Point(356, 4),
            Size = new Size(70, 34),
            BackColor = Color.White,
            ForeColor = UITheme.TextSecondary,
            FlatStyle = FlatStyle.Flat,
            Font = UITheme.BodyRegular,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderColor = UITheme.CardBorder }
        };
        btnClear.Click += async (s, e) =>
        {
            _txtSearch.Clear();
            await LoadCustomersAsync();
        };

        var btnAdd = new Button
        {
            Text = "+ Add Customer",
            Location = new Point(440, 4),
            Size = new Size(130, 34),
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
            Location = new Point(576, 4),
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
            Location = new Point(657, 4),
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
            Location = new Point(743, 4),
            Size = new Size(95, 34),
            BackColor = Color.White,
            ForeColor = UITheme.TextSecondary,
            FlatStyle = FlatStyle.Flat,
            Font = UITheme.BodyRegular,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderColor = UITheme.CardBorder }
        };
        btnRefresh.Click += async (s, e) => await LoadCustomersAsync(_txtSearch.Text);

        pnlActions.Controls.Add(_txtSearch);
        pnlActions.Controls.Add(btnSearch);
        pnlActions.Controls.Add(btnClear);
        pnlActions.Controls.Add(btnAdd);
        pnlActions.Controls.Add(btnEdit);
        pnlActions.Controls.Add(btnDelete);
        pnlActions.Controls.Add(btnRefresh);

        pnlTop.Controls.Add(lblTitle);
        pnlTop.Controls.Add(lblSub);
        pnlTop.Controls.Add(pnlActions);

        // Status Label for error / notification
        _lblStatus.Dock = DockStyle.Bottom;
        _lblStatus.Height = 28;
        _lblStatus.Font = UITheme.Subtext;
        _lblStatus.ForeColor = UITheme.TextSecondary;
        _lblStatus.TextAlign = ContentAlignment.MiddleLeft;

        // Grid Container Panel (Card)
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
        Controls.Add(_lblStatus);
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
            Name = "Name",
            HeaderText = "Customer Name",
            FillWeight = 110,
            ReadOnly = true
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Email",
            HeaderText = "Email Address",
            FillWeight = 120,
            ReadOnly = true
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Phone",
            HeaderText = "Phone",
            FillWeight = 80,
            ReadOnly = true
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Company",
            HeaderText = "Company",
            FillWeight = 110,
            ReadOnly = true
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Address",
            HeaderText = "Address",
            FillWeight = 120,
            ReadOnly = true
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Created",
            HeaderText = "Joined Date",
            FillWeight = 70,
            ReadOnly = true
        });
    }

    public async Task LoadCustomersAsync(string? search = null)
    {
        try
        {
            _lblStatus.Text = "Loading customers from SQL Server...";
            _lblStatus.ForeColor = UITheme.TextSecondary;

            _currentCustomers = await _customerService.GetAllAsync(search);

            _grid.Rows.Clear();
            foreach (var c in _currentCustomers)
            {
                _grid.Rows.Add(
                    c.Id,
                    c.Name,
                    c.Email,
                    string.IsNullOrWhiteSpace(c.Phone) ? "—" : c.Phone,
                    string.IsNullOrWhiteSpace(c.CompanyName) ? "—" : c.CompanyName,
                    string.IsNullOrWhiteSpace(c.Address) ? "—" : c.Address,
                    c.CreatedAt.ToString("MMM dd, yyyy")
                );
            }

            _lblStatus.Text = $"Showing {_currentCustomers.Count} customer(s).";
        }
        catch (Exception ex)
        {
            _lblStatus.Text = $"Database notice: {ex.Message} (Check SQL Server service status)";
            _lblStatus.ForeColor = UITheme.Danger;
        }
    }

    private Customer? GetSelectedCustomer()
    {
        if (_grid.CurrentRow == null || _grid.CurrentRow.Index < 0) return null;
        int id = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
        return _currentCustomers.FirstOrDefault(c => c.Id == id);
    }

    private async void BtnAdd_Click(object? sender, EventArgs e)
    {
        using var dialog = new CustomerDialogForm();
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            var result = await _customerService.CreateAsync(dialog.Customer);
            if (result.Success)
            {
                MessageBox.Show("Customer created successfully!", "OmniCRM Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadCustomersAsync(_txtSearch.Text);
            }
            else
            {
                MessageBox.Show(result.ErrorMessage ?? "Failed to create customer.", "Validation Notice",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private async void BtnEdit_Click(object? sender, EventArgs e)
    {
        var selected = GetSelectedCustomer();
        if (selected == null)
        {
            MessageBox.Show("Please select a customer from the table to edit.", "Selection Required",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new CustomerDialogForm(new Customer
        {
            Id = selected.Id,
            Name = selected.Name,
            Email = selected.Email,
            Phone = selected.Phone,
            CompanyName = selected.CompanyName,
            Address = selected.Address,
            CreatedAt = selected.CreatedAt
        });

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            var result = await _customerService.UpdateAsync(dialog.Customer);
            if (result.Success)
            {
                MessageBox.Show("Customer updated successfully!", "OmniCRM Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadCustomersAsync(_txtSearch.Text);
            }
            else
            {
                MessageBox.Show(result.ErrorMessage ?? "Failed to update customer.", "Update Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private async void BtnDelete_Click(object? sender, EventArgs e)
    {
        var selected = GetSelectedCustomer();
        if (selected == null)
        {
            MessageBox.Show("Please select a customer to delete.", "Selection Required",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"Are you sure you want to permanently delete customer '{selected.Name}'?\n\nThis will also remove associated leads and follow-up activities.",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm == DialogResult.Yes)
        {
            var result = await _customerService.DeleteAsync(selected.Id);
            if (result.Success)
            {
                MessageBox.Show("Customer deleted successfully.", "OmniCRM",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadCustomersAsync(_txtSearch.Text);
            }
            else
            {
                MessageBox.Show(result.ErrorMessage ?? "Failed to delete customer.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
