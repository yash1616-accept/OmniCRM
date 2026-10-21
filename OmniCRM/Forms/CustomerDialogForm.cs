using System.Text.RegularExpressions;
using OmniCRM.Models;

namespace OmniCRM.Forms;

/// <summary>
/// Modal dialog for adding or editing a Customer with field validation.
/// </summary>
public class CustomerDialogForm : Form
{
    private readonly TextBox _txtName = new();
    private readonly TextBox _txtEmail = new();
    private readonly TextBox _txtPhone = new();
    private readonly TextBox _txtCompany = new();
    private readonly TextBox _txtAddress = new();
    private readonly Label _lblError = new();
    private readonly Button _btnSave = new();
    private readonly Button _btnCancel = new();

    public Customer Customer { get; private set; }
    public bool IsEditMode { get; }

    public CustomerDialogForm(Customer? existingCustomer = null)
    {
        IsEditMode = existingCustomer != null;
        Customer = existingCustomer ?? new Customer();

        InitializeComponent();
        PopulateForm();
    }

    private void InitializeComponent()
    {
        Text = IsEditMode ? "Edit Customer - OmniCRM" : "New Customer - OmniCRM";
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
            Text = IsEditMode ? "Edit Customer Details" : "Create New Customer",
            Font = UITheme.HeaderMedium,
            ForeColor = UITheme.TextPrimary,
            AutoSize = true,
            Location = new Point(24, 14)
        };

        var lblSubtitle = new Label
        {
            Text = "Fill in the required customer details below to save into SQL Server.",
            Font = UITheme.Subtext,
            ForeColor = UITheme.TextSecondary,
            AutoSize = true,
            Location = new Point(25, 42)
        };

        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(lblSubtitle);

        // Form Fields Container
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

        // Name
        AddFormField(pnlBody, "Customer Name *", _txtName, ref top, fieldHeight, fieldSpacing, inputWidth);
        // Email
        AddFormField(pnlBody, "Email Address *", _txtEmail, ref top, fieldHeight, fieldSpacing, inputWidth);
        // Phone
        AddFormField(pnlBody, "Phone Number", _txtPhone, ref top, fieldHeight, fieldSpacing, inputWidth);
        // Company
        AddFormField(pnlBody, "Company Name", _txtCompany, ref top, fieldHeight, fieldSpacing, inputWidth);
        // Address
        AddFormField(pnlBody, "Address", _txtAddress, ref top, fieldHeight, fieldSpacing, inputWidth);

        // Error message label
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

        _btnSave.Text = IsEditMode ? "Save Changes" : "Create Customer";
        _btnSave.BackColor = UITheme.Primary;
        _btnSave.ForeColor = Color.White;
        _btnSave.FlatStyle = FlatStyle.Flat;
        _btnSave.FlatAppearance.BorderSize = 0;
        _btnSave.Font = UITheme.BodyBold;
        _btnSave.Size = new Size(140, 36);
        _btnSave.Location = new Point(334, 14);
        _btnSave.Cursor = Cursors.Hand;
        _btnSave.Click += BtnSave_Click;

        _btnCancel.Text = "Cancel";
        _btnCancel.BackColor = Color.White;
        _btnCancel.ForeColor = UITheme.TextSecondary;
        _btnCancel.FlatStyle = FlatStyle.Flat;
        _btnCancel.FlatAppearance.BorderColor = UITheme.CardBorder;
        _btnCancel.Font = UITheme.BodyRegular;
        _btnCancel.Size = new Size(90, 36);
        _btnCancel.Location = new Point(236, 14);
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

    private void PopulateForm()
    {
        if (IsEditMode)
        {
            _txtName.Text = Customer.Name;
            _txtEmail.Text = Customer.Email;
            _txtPhone.Text = Customer.Phone;
            _txtCompany.Text = Customer.CompanyName;
            _txtAddress.Text = Customer.Address;
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        // Validation checks
        string name = _txtName.Text.Trim();
        string email = _txtEmail.Text.Trim();
        string phone = _txtPhone.Text.Trim();
        string company = _txtCompany.Text.Trim();
        string address = _txtAddress.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            ShowError("Please enter the customer's full name.");
            _txtName.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            ShowError("Please enter an email address.");
            _txtEmail.Focus();
            return;
        }

        // Validate email format
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
        if (!emailRegex.IsMatch(email))
        {
            ShowError("Please enter a valid email address (e.g., name@company.com).");
            _txtEmail.Focus();
            return;
        }

        // Assign to model
        Customer.Name = name;
        Customer.Email = email;
        Customer.Phone = phone;
        Customer.CompanyName = company;
        Customer.Address = address;

        DialogResult = DialogResult.OK;
        Close();
    }

    private void ShowError(string message)
    {
        _lblError.Text = message;
        _lblError.Visible = true;
    }
}
