using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using DeviceManagement.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class ReportService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAssetService _assetService;

    public ReportService(UserManager<ApplicationUser> userManager, IAssetService assetService)
    {
        _userManager = userManager;
        _assetService = assetService;
    }

    public async Task<byte[]> GenerateUsersAssetsReportPdfAsync()
    {
        var users = _userManager.Users.ToList();
        var assets = (await _assetService.GetAllAsync()).ToList();

        using var stream = new MemoryStream();
        PdfDocument document = new PdfDocument();
        document.Info.Title = "Users and Assets Report";

        PdfPage page = document.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);

        XFont titleFont = new XFont("Verdana", 18, XFontStyle.Bold);
        XFont sectionFont = new XFont("Verdana", 14, XFontStyle.Bold);
        XFont textFont = new XFont("Verdana", 10, XFontStyle.Regular);
        XSolidBrush headerBrush = new XSolidBrush(XColor.FromArgb(30, 144, 255));
        XSolidBrush rowBrush = XBrushes.LightGray;
        XPen borderPen = new XPen(XColors.Black, 0.5);

        double yPoint = 40;
        double leftMargin = 40;
        double rightMargin = 40;
        double lineHeight = 20;

        double tableWidth = page.Width - leftMargin - rightMargin;

        // Column weights for proportional widths (sum = 1)
        double[] colWeights = { 0.25, 0.1, 0.12, 0.2, 0.18, 0.1 }; // Name, Tag, Room, Location, Condition, Verified
        string[] headers = { "Name", "Tag", "Room", "Location", "Condition", "Verified" };

        // Calculate column widths
        double[] colWidths = colWeights.Select(w => w * tableWidth).ToArray();

        // ===== Report Title =====
        gfx.DrawString("Users and Assets Report", titleFont, XBrushes.Black,
            new XRect(0, yPoint, page.Width, 30), XStringFormats.TopCenter);
        yPoint += 50;

        // ===== SECTION: Users =====
        gfx.DrawString("Users", sectionFont, XBrushes.DarkBlue,
            new XRect(leftMargin, yPoint, page.Width - leftMargin - rightMargin, lineHeight), XStringFormats.TopLeft);
        yPoint += 30;

        if (users.Any())
        {
            foreach (var user in users)
            {
                if (yPoint > page.Height - 80)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPoint = 40;
                }

                string userText = $"{user.FullName} | Email: {user.Email} | Gender: {user.Gender ?? "N/A"} | SA ID: {user.SAIdNumber}";
                gfx.DrawString(userText, textFont, XBrushes.Black,
                    new XRect(leftMargin, yPoint, page.Width - leftMargin - rightMargin, lineHeight), XStringFormats.TopLeft);
                yPoint += lineHeight;
            }
        }
        else
        {
            gfx.DrawString("No users found.", textFont, XBrushes.Gray,
                new XRect(leftMargin, yPoint, page.Width - leftMargin - rightMargin, lineHeight), XStringFormats.TopLeft);
            yPoint += lineHeight;
        }

        yPoint += 30; // Space before Assets section

        // ===== SECTION: Assets Table =====
        gfx.DrawString("Assets", sectionFont, XBrushes.DarkGreen,
            new XRect(leftMargin, yPoint, page.Width - leftMargin - rightMargin, lineHeight), XStringFormats.TopLeft);
        yPoint += 30;

        if (assets.Any())
        {
            // Draw headers
            double x = leftMargin;
            for (int i = 0; i < headers.Length; i++)
            {
                gfx.DrawRectangle(borderPen, XBrushes.LightGray, x, yPoint, colWidths[i], lineHeight);
                gfx.DrawString(Truncate(headers[i], (int)(colWidths[i] / 6)), textFont, XBrushes.Black,
                    new XRect(x + 2, yPoint + 2, colWidths[i] - 4, lineHeight), XStringFormats.TopLeft);
                x += colWidths[i];
            }
            yPoint += lineHeight;

            bool alternate = false;

            foreach (var asset in assets)
            {
                if (yPoint > page.Height - 50)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPoint = 40;

                    // Repeat headers on new page
                    x = leftMargin;
                    for (int i = 0; i < headers.Length; i++)
                    {
                        gfx.DrawRectangle(borderPen, XBrushes.LightGray, x, yPoint, colWidths[i], lineHeight);
                        gfx.DrawString(Truncate(headers[i], (int)(colWidths[i] / 6)), textFont, XBrushes.Black,
                            new XRect(x + 2, yPoint + 2, colWidths[i] - 4, lineHeight), XStringFormats.TopLeft);
                        x += colWidths[i];
                    }
                    yPoint += lineHeight;
                }

                // Alternate row shading
                if (alternate)
                    gfx.DrawRectangle(rowBrush, leftMargin, yPoint, colWidths.Sum(), lineHeight);
                alternate = !alternate;

                x = leftMargin;
                string[] row = {
                    Truncate(asset.AssetName, (int)(colWidths[0] / 6)),
                    Truncate(asset.AssetTag, (int)(colWidths[1] / 6)),
                    Truncate(asset.Room ?? "N/A", (int)(colWidths[2] / 6)),
                    Truncate(asset.Location ?? "N/A", (int)(colWidths[3] / 6)),
                    Truncate(asset.Condition ?? "N/A", (int)(colWidths[4] / 6)),
                    Truncate(asset.Verified ?? "N", (int)(colWidths[5] / 6))
                };

                for (int i = 0; i < row.Length; i++)
                {
                    gfx.DrawRectangle(borderPen, x, yPoint, colWidths[i], lineHeight);
                    gfx.DrawString(row[i], textFont, XBrushes.Black,
                        new XRect(x + 2, yPoint + 2, colWidths[i] - 4, lineHeight), XStringFormats.TopLeft);
                    x += colWidths[i];
                }

                yPoint += lineHeight;
            }
        }
        else
        {
            gfx.DrawString("No assets found.", textFont, XBrushes.Gray,
                new XRect(leftMargin, yPoint, page.Width - leftMargin - rightMargin, lineHeight), XStringFormats.TopLeft);
        }

        document.Save(stream);
        return stream.ToArray();
    }

    private string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
    }
}
