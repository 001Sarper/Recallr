using PdfSharp.Pdf.IO;

namespace Recallr.Models.Services;

public class PdfService
{
    public static int GetDocumentPageCount(string path)
    {
        using (PdfSharp.Pdf.PdfDocument document = PdfReader.Open(path, PdfDocumentOpenMode.InformationOnly))
        {
            return document.PageCount;
        }
    }
}