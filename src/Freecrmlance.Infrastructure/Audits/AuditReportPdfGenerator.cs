using Freecrmlance.Application.Audits;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Fonts;

namespace Freecrmlance.Infrastructure.Audits;

public sealed class AuditReportPdfGenerator : IAuditReportPdfGenerator
{
    private static readonly object FontLock = new();
    public byte[] Generate(AuditReportPdfModel model)
    {
        EnsureFonts();
        var document=new Document(); document.Info.Title=$"Rapport d'audit {model.Reference} V{model.VersionNumber}";
        var normal=document.Styles[StyleNames.Normal]!; normal.Font.Name="Arial"; normal.Font.Size=9;
        var section=document.AddSection(); section.PageSetup.TopMargin=Unit.FromCentimeter(1.6);section.PageSetup.BottomMargin=Unit.FromCentimeter(1.6);section.PageSetup.LeftMargin=Unit.FromCentimeter(1.7);section.PageSetup.RightMargin=Unit.FromCentimeter(1.7);
        var title=section.AddParagraph("Rapport d'audit");title.Format.Font.Size=20;title.Format.Font.Bold=true;
        var meta=section.AddParagraph();meta.AddFormattedText($"{model.Reference} — Version {model.VersionNumber}",TextFormat.Bold);meta.AddLineBreak();meta.AddText(model.Title);meta.AddLineBreak();meta.AddText($"Finalisé le {model.FinalizedAtUtc:dd/MM/yyyy HH:mm} UTC");meta.Format.SpaceAfter=Unit.FromCentimeter(.5);
        if(!string.IsNullOrWhiteSpace(model.Description)){var p=section.AddParagraph(model.Description);p.Format.SpaceAfter=Unit.FromCentimeter(.5);}
        foreach(var s in model.Sections){var h=section.AddParagraph(s.Title);h.Format.Font.Size=14;h.Format.Font.Bold=true;h.Format.SpaceBefore=Unit.FromCentimeter(.5);if(!string.IsNullOrWhiteSpace(s.Description))section.AddParagraph(s.Description);foreach(var i in s.Items){var p=section.AddParagraph();p.Format.KeepTogether=true;p.Format.SpaceBefore=Unit.FromCentimeter(.25);p.AddFormattedText(i.Label,TextFormat.Bold);if(!string.IsNullOrWhiteSpace(i.Description)){p.AddLineBreak();p.AddText(i.Description);}p.AddLineBreak();p.AddFormattedText("Réponse : ",TextFormat.Bold);p.AddText(i.Value??"Non renseigné");if(!string.IsNullOrWhiteSpace(i.Observation)){p.AddLineBreak();p.AddFormattedText("Observation : ",TextFormat.Bold);p.AddText(i.Observation);}if(!string.IsNullOrWhiteSpace(i.Recommendation)){p.AddLineBreak();p.AddFormattedText("Recommandation : ",TextFormat.Bold);p.AddText(i.Recommendation);}if(i.Evidence.Count>0){p.AddLineBreak();p.AddFormattedText("Preuves : ",TextFormat.Bold);p.AddText(string.Join(", ",i.Evidence));}}}
        var integrity=section.AddParagraph();integrity.Format.SpaceBefore=Unit.FromCentimeter(.8);integrity.AddFormattedText("Empreinte d'intégrité technique SHA-256",TextFormat.Bold);integrity.AddLineBreak();integrity.AddText(model.Hash);integrity.AddLineBreak();integrity.AddText("Cette empreinte permet de vérifier l'intégrité du snapshot et ne constitue pas une certification juridique.");
        var renderer=new PdfDocumentRenderer{Document=document};renderer.RenderDocument();using var stream=new MemoryStream();renderer.PdfDocument.Save(stream,false);return stream.ToArray();
    }
    private static void EnsureFonts(){lock(FontLock){if(OperatingSystem.IsWindows()){GlobalFontSettings.UseWindowsFontsUnderWindows=true;return;}if(GlobalFontSettings.FontResolver is null)GlobalFontSettings.FontResolver=new LiberationSansFontResolver();}}
}
