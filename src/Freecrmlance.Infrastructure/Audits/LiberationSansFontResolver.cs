using PdfSharp.Fonts;
namespace Freecrmlance.Infrastructure.Audits;
internal sealed class LiberationSansFontResolver : IFontResolver
{
 public FontResolverInfo? ResolveTypeface(string familyName,bool isBold,bool isItalic)=>new(isBold?"LiberationSans-Bold":"LiberationSans-Regular");
 public byte[]? GetFont(string faceName)
 {
  var file=faceName=="LiberationSans-Bold"?"LiberationSans-Bold.ttf":"LiberationSans-Regular.ttf";
  foreach(var root in new[]{"/usr/share/fonts/truetype/liberation2","/usr/share/fonts/truetype/liberation","/usr/share/fonts/truetype/dejavu"}){var path=Path.Combine(root,file);if(File.Exists(path))return File.ReadAllBytes(path);}
  throw new PlatformNotSupportedException("A compatible Liberation Sans font is required for PDF generation.");
 }
}
