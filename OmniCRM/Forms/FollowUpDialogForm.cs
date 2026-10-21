using OmniCRM.Models;
using OmniCRM.Services;

namespace OmniCRM.Forms;

/// <summary>
/// Modal dialog for scheduling customer follow-up tasks.
/// </summary>
public class FollowUpDialogForm : Form
{
    private readonly CustomerService _customerService;
    private readonly ComboBox _cmbCustomer = new();
    private readonly DateTimePicker _dtpDate = new();
    private readonly TextBox _txtNotes = new();
    private readonly Label _lblError = new();
    private readonly Button _btnSave = new();
    private readonly Button _btnCancel = new();

    public FollowUp FollowUp { get; private set; }
    private List<Customer> _customers = new();

    public FollowUpDialogForm(CustomerService customerService, FollowUp? existingFollowUp = null)
    {
        _customerService = customerService;
        FollowUp = existingFollowUp ?? new FollowUp();

        InitializeComponent();
        _ = LoadCustomersAndPopulateAsync();
    }

    private void InitializeComponent()
    {
        Text = "Schedule Customer Follow-Up - OmniCRM";
        Size = new Size(500, 480);
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
            Text = "Schedule Client Follow-Up",
            Font = UITheme.HeaderMedium,
            ForeColor = UITheme.TextPrimary,
            AutoSize = true,
            Location = new Point(24, 14)
        };

        var lblSubtitle = new Label
        {
            Text = "Record upcoming meetings, calls, and follow-up reminders.",
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
            Padding = new Padding(28, 20, 28, 20)
        };

        int top = 16;
        int fieldHeight = 32;
        int fieldSpacing = 58;
        int inputWidth = 420;

        // Customer Selection
        var lblCust = new Label
        {
            Text = "Customer Account *",
            Font = UITheme.BodyBold,
            ForeColor = UITheme.TextPrimary,
            Location = new Point(28, top),
            AutoSize = true
        };
        _cmbCustomer.Location = new Point(28, top + 22);
        _cmbCustomer.Size = new Size(inputWidth, fieldHeight);
        _cmbCustomer.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbCustomer.Font = UITheme.BodyRegular;
        pnlBody.Controls.Add(lblCust);
        pnlBody.Controls.Add(_cmbCustomer);
        top += fieldSpacing;

        // Scheduled Date
        var lblDate = new Label
        {
            Text = "Scheduled Date & Time *",
            Font = UITheme.BodyBold,
            ForeColor = UITheme.TextPrimary,
            Location = new Point(28, top),
            AutoSize = true
        };
        _dtpDate.Location = new Point(28, top + 22);
        _dtpDate.Size = new Size(inputWidth, fieldHeight);
        _dtpDate.Font = UITheme.BodyRegular;
        _dtpDate.Format = DateTimePickerFormat.Custom;
        _dtpDate.CustomFormat = "yyyy-MM-dd HH:mm";
        _dtpDate.Value = DateTime.Now.AddDays(1);
        pnlBody.Controls.Add(lblDate);
        pnlBody.Controls.Add(_dtpDate);
        top += fieldSpacing;

        // Notes
        var lblNotes = new Label
        {
            Text = "Follow-Up Objective / Notes *",
            Font = UITheme.BodyBold,
            ForeColor = UITheme.TextPrimary,
            Location = new Point(28, top),
            AutoSize = true
        };
        _txtNotes.Location = new Point(28, top + 22);
        _txtNotes.Size = new Size(inputWidth, 80);
        _txtNotes.Multiline = true;
        _txtNotes.Font = UITheme.BodyRegular;
        _txtNotes.BorderStyle = BorderStyle.FixedSingle;
        pnlBody.Controls.Add(lblNotes);
        pnlBody.Controls.Add(_txtNotes);
        top += 110;

        // Error message
        _lblError.Location = new Point(28, top);
        _lblError.Size = new Size(inputWidth, 30);
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

        _btnSave.Text = "Save Follow-Up";
        _btnSave.BackColor = UITheme.Primary;
        _btnSave.ForeColor = Color.White;
        _btnSave.FlatStyle = FlatStyle.Flat;
        _btnSave.FlatAppearance.BorderSize = 0;
        _btnSave.Font = UITheme.BodyBold;
        _btnSave.Size = new Size(140, 36);
        _btnSave.Location = new Point(314, 14);
        _btnSave.Cursor = Cursors.Hand;
        _btnSave.Click += BtnSave_Click;

        _btnCancel.Text = "Cancel";
        _btnCancel.BackColor = Color.White;
        _btnCancel.ForeColor = UITheme.TextSecondary;
        _btnCancel.FlatStyle = FlatStyle.Flat;
        _btnCancel.FlatAppearance.BorderColor = UITheme.CardBorder;
        _btnCancel.Font = UITheme.BodyRegular;
        _btnCancel.Size = new Size(90, 36);
        _btnCancel.Location = new Point(214, 14);
        _btnCancel.Cursor = Cursors.Hand;
        _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

        pnlFooter.Controls.Add(_btnCancel);
        pnlFooter.Controls.Add(_btnSave);

        Controls.Add(pnlBody);
        Controls.Add(pnlHeader);
        Controls.Add(pnlFooter);
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
            if (c.Id == FollowUp.CustomerId)
            {
                selectedIndex = i;
            }
        }

        if (_cmbCustomer.Items.Count > 0)
        {
            _cmbCustomer.SelectedIndex = selectedIndex;
        }

        if (FollowUp.Id > 0)
        {
            _dtpDate.Value = FollowUp.ScheduledDate;
            _txtNotes.Text = FollowUp.Notes;
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (_cmbCustomer.SelectedIndex < 0 || _customers.Count == 0)
        {
            _lblError.Text = "Please select a customer.";
            _lblError.Visible = true;
            return;
        }

        string notes = _txtNotes.Text.Trim();
        if (string.IsNullOrWhiteSpace(notes))
        {
            _lblError.Text = "Please write a brief note for this follow-up.";
            _lblError.Visible = true;
            _txtNotes.Focus();
            return;
        }

        FollowUp.CustomerId = _customers[_cmbCustomer.SelectedIndex].Id;
        FollowUp.ScheduledDate = _dtpDate.Value.ToUniversalTime();
        FollowUp.Notes = notes;

        DialogResult = DialogResult.OK;
        Close();
    }
}
