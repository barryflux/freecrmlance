using Freecrmlance.Application.Crm;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Fonts;

namespace Freecrmlance.Infrastructure.Documents;

public sealed class MigraDocPdfGenerator : IPdfGenerator
{
    private static readonly object FontLock = new();

    public byte[] GenerateQuote(QuotePdfModel model)
    {
        EnsureFontsConfigured();

        var document = new Document();
        document.Info.Title = $"Quote {model.Number}";

        var normal = document.Styles[StyleNames.Normal]!;
        normal.Font.Name = "Arial";
        normal.Font.Size = 10;

        var section = document.AddSection();
        section.PageSetup.TopMargin = Unit.FromCentimeter(1.8);
        section.PageSetup.BottomMargin = Unit.FromCentimeter(1.8);
        section.PageSetup.LeftMargin = Unit.FromCentimeter(1.8);
        section.PageSetup.RightMargin = Unit.FromCentimeter(1.8);

        var title = section.AddParagraph($"Quote {model.Number}");
        title.Format.Font.Size = 20;
        title.Format.Font.Bold = true;
        title.Format.SpaceAfter = Unit.FromCentimeter(0.3);

        var summary = section.AddParagraph();
        summary.AddFormattedText("Customer: ", TextFormat.Bold);
        summary.AddText(model.CustomerName);
        summary.AddLineBreak();
        summary.AddFormattedText("Status: ", TextFormat.Bold);
        summary.AddText(model.Status);
        summary.AddLineBreak();
        summary.AddFormattedText("Created: ", TextFormat.Bold);
        summary.AddText($"{model.CreatedAtUtc:yyyy-MM-dd HH:mm} UTC");
        summary.AddLineBreak();
        summary.AddFormattedText("Updated: ", TextFormat.Bold);
        summary.AddText($"{model.UpdatedAtUtc:yyyy-MM-dd HH:mm} UTC");
        summary.Format.SpaceAfter = Unit.FromCentimeter(0.6);

        var table = section.AddTable();
        table.Borders.Width = 0.5;
        table.AddColumn(Unit.FromCentimeter(8.2));
        table.AddColumn(Unit.FromCentimeter(2.2));
        table.AddColumn(Unit.FromCentimeter(3.0));
        table.AddColumn(Unit.FromCentimeter(3.0));

        var header = table.AddRow();
        header.Format.Font.Bold = true;
        header.Cells[0].AddParagraph("Description");
        header.Cells[1].AddParagraph("Quantity");
        header.Cells[2].AddParagraph("Unit price");
        header.Cells[3].AddParagraph("Total");

        foreach (var line in model.Lines)
        {
            var row = table.AddRow();
            row.Cells[0].AddParagraph(line.Description);
            row.Cells[1].AddParagraph(line.Quantity.ToString("0.####"));
            row.Cells[2].AddParagraph(line.UnitPrice.ToString("0.00"));
            row.Cells[3].AddParagraph(line.Total.ToString("0.00"));
        }

        var total = section.AddParagraph();
        total.Format.Alignment = ParagraphAlignment.Right;
        total.Format.SpaceBefore = Unit.FromCentimeter(0.5);
        total.Format.Font.Size = 13;
        total.Format.Font.Bold = true;
        total.AddText($"Total: {model.Total:0.00}");

        var renderer = new PdfDocumentRenderer { Document = document };
        renderer.RenderDocument();

        using var stream = new MemoryStream();
        renderer.PdfDocument.Save(stream, false);
        return stream.ToArray();
    }

    private static void EnsureFontsConfigured()
    {
        lock (FontLock)
        {
            if (OperatingSystem.IsWindows())
            {
                GlobalFontSettings.UseWindowsFontsUnderWindows = true;
                return;
            }

            throw new PlatformNotSupportedException(
                "Quote PDF generation requires an explicit packaged font resolver on non-Windows hosts.");
        }
    }
}
