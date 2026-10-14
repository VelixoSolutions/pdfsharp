// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if WPF
using System.IO;
#endif
using System.Text;
using PdfSharp.Diagnostics;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using Xunit;
using FluentAssertions;

namespace PdfSharp.Tests.IO
{
    [Collection("PDFsharp")]
    public class ImportTests : IDisposable
    {
        public ImportTests()
        {
            PdfSharpCore.ResetAll();
        }

        public void Dispose()
        {
            PdfSharpCore.ResetAll();
        }

        [Fact]
        public void Import_page_with_references_to_undefined_objects()
        {
            // The page resources reference object 5, which the cross-reference table marks as free,
            // and object 99, which is beyond /Size. Such files are produced e.g. by HP Exstream.
            // Per ISO 32000-2 7.3.10 these references must be treated as references to the null object.
            var bytes = CreatePdfWithReferencesToUndefinedObjects();

            using var source = PdfReader.Open(new MemoryStream(bytes), PdfDocumentOpenMode.Import);
            using var target = new PdfDocument();

            var page = target.AddPage(source.Pages[0]);

            var resources = page.Elements.GetDictionary("/Resources")!;
            var fonts = resources.Elements.GetDictionary("/Font")!;
            fonts.Elements.ContainsKey("/F1").Should().BeTrue();
            fonts.Elements.ContainsKey("/F2").Should().BeFalse("a dictionary entry referencing an undefined object is equivalent to an absent entry");
            fonts.Elements.ContainsKey("/F3").Should().BeFalse("a dictionary entry referencing an undefined object is equivalent to an absent entry");

            var array = resources.Elements.GetArray("/ProcSet")!;
            array.Elements.Count.Should().Be(3);
            array.Elements[1].Should().BeOfType<PdfNull>();
            array.Elements[2].Should().BeOfType<PdfNull>();

            // The resulting document must be saveable and readable again.
            using var stream = new MemoryStream();
            target.Save(stream, false);
            stream.Position = 0;
            using var reloaded = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
            reloaded.PageCount.Should().Be(1);
        }

        static byte[] CreatePdfWithReferencesToUndefinedObjects()
        {
            var objects = new[]
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [ 3 0 R ] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [ 0 0 612 792 ] /Contents 6 0 R " +
                    "/Resources << /Font << /F1 4 0 R /F2 5 0 R /F3 99 0 R >> /ProcSet [ /PDF 5 0 R 99 0 R ] >> >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
                null, // Object 5 is free.
                "<< /Length 41 >>\nstream\nBT /F1 12 Tf 72 720 Td (Hello World) Tj ET\nendstream"
            };

            var pdf = new StringBuilder("%PDF-1.4\n");
            var offsets = new int[objects.Length];
            for (int idx = 0; idx < objects.Length; idx++)
            {
                if (objects[idx] == null)
                    continue;
                offsets[idx] = pdf.Length;
                pdf.Append($"{idx + 1} 0 obj\n{objects[idx]}\nendobj\n");
            }

            var xref = pdf.Length;
            pdf.Append($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
            for (int idx = 0; idx < objects.Length; idx++)
                pdf.Append(objects[idx] == null ? "0000000000 00000 f \n" : $"{offsets[idx]:D10} 00000 n \n");
            pdf.Append($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");

            return Encoding.ASCII.GetBytes(pdf.ToString());
        }
    }
}
