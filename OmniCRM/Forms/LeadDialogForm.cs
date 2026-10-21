using OmniCRM.Models;
using OmniCRM.Services;

namespace OmniCRM.Forms;

/// <summary>
/// Modal dialog for adding or editing a sales lead in the CRM pipeline.
/// </summary>
public class LeadDialogForm : Form
{
    private readonly CustomerService _customerService;
    private readonly TextBox _txtTitle = new();
    private readonly ComboBox _cmbCustomer = new();
    private readonly ComboBox _cmbStatus = new();
    private readonly NumericUpDown _numValue = new();
    private readonly TextBox _txtNotes = new();
    private readonly Label _lblError = new();
    private readonly Button _btnSave = new();
    private readonly Button _btnCancel = new();

    public Lead Lead { get; private set; }
    public bool IsEditMode { get; }
    private List<Customer> _customers = new();

    public LeadDialogForm(CustomerService customerService, Lead? existingLead = null)
    {
        _customerService = customerService;
        IsEditMode = existingLead != null;
        Lead = existingLead ?? new Lead();

        InitializeComponent();
        _ = LoadCustomersAndPopulateAsync();
    }

    private void InitializeComponent()
    {
        Text = IsEditMode ? "Edit Sales Lead - OmniCRM" : "Create New Lead - OmniCRM";
        Size = new Size(520, 560);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;
        Font = UITheme.BodyRegular;

        // Header Panel
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = UITheme.ContentBg,
            Padding = new Padding(24, 16, 24, 12)
        };

        var lblTitle = new Label
        {
            Text = IsEditMode ? "Edit Lead Opportunity" : "New Sales Opportunity",
            Font = UITheme.HeaderMedium,
            ForeColor = UITheme.TextPrimary,
            AutoSize = true,
            Location = new Point(24, 14)
        };

        var lblSubtitle = new Label
        {
            Text = "Track prospective deals, customer associations, and expected values.",
            Font = UITheme.Subtext,
            ForeColor = UITheme.TextSecondary,
            AutoSize = true,
            Location = new Point(25, 42)
        };

        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(lblSubtitle);

        // Body Panel
        var pnlBody = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28, 20, 28, 20),
            AutoScroll = true
        };

        int top = 16;
        int fieldHeight = 32;
        int fieldSpacing = 58;
        int inputWidth = 440;

        // Lead Title
        AddFormField(pnlBody, "Lead Title / Subject *", _txtTitle, ref top, fieldHeight, fieldSpacing, inputWidth);

        // Customer Selection
        var lblCustomer = new Label
        {
            Text = "Associated Customer *",
            Font = UITheme.BodyBold,
            ForeColor = UITheme.TextPrimary,
            Location = new Point(28, top),
            AutoSize = true
        };
        _cmbCustomer.Location = new Point(28, top + 22);
        _cmbCustomer.Size = new Size(inputWidth, fieldHeight);
        _cmbCustomer.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbCustomer.Font = UITheme.BodyRegular;
        pnlBody.Controls.Add(lblCustomer);
        pnlBody.Controls.Add(_cmbCustomer);
        top += fieldSpacing;

        // Status Selection
        var lblStatus = new Label
        {
            Text = "Pipeline Status *",
            Font = UITheme.BodyBold,
            ForeColor = UITheme.TextPrimary,
            Location = new Point(28, top),
            AutoSize = true
        };
        _cmbStatus.Location = new Point(28, top + 22);
        _cmbStatus.Size = new Size(inputWidth, fieldHeight);
        _cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbStatus.Font = UITheme.BodyRegular;
        _cmbStatus.Items.AddRange(Enum.GetNames<LeadStatus>());
        _cmbStatus.SelectedIndex = 0;
        pnlBody.Controls.Add(lblStatus);
        pnlBody.Controls.Add(_cmbStatus);
        top += fieldSpacing;

        // Estimated Value
        var lblValue = new Label
        {
            Text = "Estimated Value ($)",
            Font = UITheme.BodyBold,
            ForeColor = UITheme.TextPrimary,
            Location = new Point(28, top),
            AutoSize = true
        };
        _numValue.Location = new Point(28, top + 22);
        _numValue.Size = new Size(inputWidth, fieldHeight);
        _numValue.Font = UITheme.BodyRegular;
        _numValue.DecimalPlaces = 2;
        _numValue.Maximum = 10000000m;
        _numValue.ThousandsSeparator = true;
        pnlBody.Controls.Add(lblValue);
        pnlBody.Controls.Add(_numValue);
        top += fieldSpacing;

        // Notes
        AddFormField(pnlBody, "Deal Notes", _txtNotes, ref top, fieldHeight, fieldSpacing, inputWidth);

        // Error message
        _lblError.Location = new Point(28, top);
        _lblError.Size = new Size(inputWidth, 36);
        _lblError.ForeColor = UITheme.Danger;
        _lblError.Font = UITheme.Subtext;
        _lblError.Visible = false;
        pnlBody.Controls.Add(_lblError);

        // Footer Actions Panel
        var pnlFooter = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 65,
            BackColor = UITheme.ContentBg,
            Padding = new Padding(24, 14, 24, 14)
        };

        _btnSave.Text = IsEditMode ? "Save Changes" : "Create Lead";
        _btnSave.BackColor = UITheme.Primary;
        _btnSave.ForeColor = Color.White;
        _btnSave.FlatStyle = FlatStyle.Flat;
        _btnSave.FlatAppearance.BorderSize = 0;
        _btnSave.Font = UITheme.BodyBold;
        _btnSave.Size = new Size(130, 36);
        _btnSave.Location = new Point(344, 14);
        _btnSave.Cursor = Cursors.Hand;
        _btnSave.Click += BtnSave_Click;

        _btnCancel.Text = "Cancel";
        _btnCancel.BackColor = Color.White;
        _btnCancel.ForeColor = UITheme.TextSecondary;
        _btnCancel.FlatStyle = FlatStyle.Flat;
        _btnCancel.FlatAppearance.BorderColor = UITheme.CardBorder;
        _btnCancel.Font = UITheme.BodyRegular;
        _btnCancel.Size = new Size(90, 36);
        _btnCancel.Location = new Point(244, 14);
        _btnCancel.Cursor = Cursors.Hand;
        _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

        pnlFooter.Controls.Add(_btnCancel);
        pnlFooter.Controls.Add(_btnSave);

        Controls.Add(pnlBody);
        Controls.Add(pnlHeader);
        Controls.Add(pnlFooter);
    }

    private void AddFormField(Panel parent, string labelText, TextBox textBox, ref int top, int fieldHeight, int spacing, int width)
    {
        var lbl = new Label
        {
            Text = labelText,
            Font = UITheme.BodyBold,
            ForeColor = UITheme.TextPrimary,
            Location = new Point(28, top),
            AutoSize = true
        };

        textBox.Location = new Point(28, top + 22);
        textBox.Size = new Size(width, fieldHeight);
        textBox.Font = UITheme.BodyRegular;
        textBox.BorderStyle = BorderStyle.FixedSingle;

        parent.Controls.Add(lbl);
        parent.Controls.Add(textBox);

        top += spacing;
    }

    private async Task LoadCustomersAndPopulateAsync()
    {
        _customers = await _customerService.GetAllAsync();
        _cmbCustomer.Items.Clear();

        int selectedIndex = 0;
        for (int i = 0; i < _customers.Count; i++)
        {
            var c = _customers[i];
            _cmbCustomer.Items.Add($"{c.Name} ({c.CompanyName})");
            if (c.Id == Lead.CustomerId)
            {
                selectedIndex = i;
            }
        }

        if (_cmbCustomer.Items.Count > 0)
        {
            _cmbCustomer.SelectedIndex = selectedIndex;
        }

        if (IsEditMode)
        {
            _txtTitle.Text = Lead.Title;
            _cmbStatus.SelectedItem = Lead.Status.ToString();
            _numValue.Value = Lead.EstimatedValue;
            _txtNotes.Text = Lead.Notes;
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        string title = _txtTitle.Text.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            _lblError.Text = "Please enter a lead title.";
            _lblError.Visible = true;
            _txtTitle.Focus();
            return;
        }

        if (_cmbCustomer.SelectedIndex < 0 || _customers.Count == 0)
        {
            _lblError.Text = "Please select an associated customer.";
            _lblError.Visible = true;
            return;
        }

        Lead.Title = title;
        Lead.CustomerId = _customers[_cmbCustomer.SelectedIndex].Id;
        Lead.Status = Enum.Parse<LeadStatus>(_cmbStatus.SelectedItem?.ToString() ?? "New");
        Lead.EstimatedValue = _numValue.Value;
        Lead.Notes = _txtNotes.Text.Trim();

        DialogResult = DialogResult.OK;
        Close();
    }
}
