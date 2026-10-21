using System.Drawing;
using System.Windows.Forms;

namespace OmniCRM;

/// <summary>
/// Modern UI Theme styling definitions for OmniCRM Windows Forms.
/// </summary>
public static class UITheme
{
    // Color Palette
    public static readonly Color Primary = Color.FromArgb(37, 99, 235);         // Modern Blue (#2563EB)
    public static readonly Color PrimaryDark = Color.FromArgb(29, 78, 216);     // Darker Blue (#1D4ED8)
    public static readonly Color PrimaryLight = Color.FromArgb(239, 246, 255);  // Light Blue Tint (#EFF6FF)

    public static readonly Color SidebarBg = Color.FromArgb(15, 23, 42);        // Dark Slate (#0F172A)
    public static readonly Color SidebarHover = Color.FromArgb(30, 41, 59);     // Slate 800 (#1E293B)
    public static readonly Color SidebarActive = Color.FromArgb(51, 65, 85);    // Slate 700 (#334155)

    public static readonly Color ContentBg = Color.FromArgb(248, 250, 252);     // Canvas Gray (#F8FAFC)
    public static readonly Color CardBg = Color.White;
    public static readonly Color CardBorder = Color.FromArgb(226, 232, 240);    // Border Gray (#E2E8F0)

    public static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);      // Slate 900
    public static readonly Color TextSecondary = Color.FromArgb(100, 116, 139); // Slate 500
    public static readonly Color TextMuted = Color.FromArgb(148, 163, 184);     // Slate 400

    public static readonly Color Success = Color.FromArgb(16, 185, 129);        // Emerald (#10B981)
    public static readonly Color Warning = Color.FromArgb(245, 158, 11);        // Amber (#F59E0B)
    public static readonly Color Danger = Color.FromArgb(239, 68, 68);          // Red (#EF4444)

    // Typography
    public static readonly Font HeaderLarge = new Font("Segoe UI", 16F, FontStyle.Bold);
    public static readonly Font HeaderMedium = new Font("Segoe UI", 13F, FontStyle.Bold);
    public static readonly Font HeaderSmall = new Font("Segoe UI", 11F, FontStyle.Bold);
    public static readonly Font BodyRegular = new Font("Segoe UI", 9.5F, FontStyle.Regular);
    public static readonly Font BodyBold = new Font("Segoe UI", 9.5F, FontStyle.Bold);
    public static readonly Font Subtext = new Font("Segoe UI", 8.5F, FontStyle.Regular);

    public static void ApplyModernGridStyle(DataGridView grid)
    {
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = Color.FromArgb(241, 245, 249);
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowTemplate.Height = 38;
        grid.EnableHeadersVisualStyles = false;

        // Header style
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = TextSecondary;
        grid.ColumnHeadersDefaultCellStyle.Font = BodyBold;
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 10, 12, 10);
        grid.ColumnHeadersHeight = 44;

        // Default cell style
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.ForeColor = TextPrimary;
        grid.DefaultCellStyle.Font = BodyRegular;
        grid.DefaultCellStyle.Padding = new Padding(12, 6, 12, 6);
        grid.DefaultCellStyle.SelectionBackColor = PrimaryLight;
        grid.DefaultCellStyle.SelectionForeColor = PrimaryDark;

        // Alternating row style
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
        grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = PrimaryLight;
        grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = PrimaryDark;
    }

    public static Button CreatePrimaryButton(string text, int width = 120, int height = 36)
    {
        return new Button
        {
            Text = text,
            Width = width,
            Height = height,
            BackColor = Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = BodyBold,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 0 }
        };
    }
}
