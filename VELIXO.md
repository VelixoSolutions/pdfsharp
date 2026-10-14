# Velixo fork of PDFsharp 6.2.4

This branch is PDFsharp 6.2.4 (tag `v6.2.4`, the last MIT-licensed release) plus fixes Velixo needs that are
not available upstream. Upstream releases from 6.5.0 on are AGPL / commercially dual-licensed, so Velixo stays on 6.2.4.

Assembly identity is unchanged (`Version=6.2.4.0, PublicKeyToken=f94615aa0424f9eb`, signed with the
public `src/StrongnameKey.snk`), so the binaries are drop-in replacements for the PDFsharp-GDI 6.2.4 NuGet package.
The informational version is `6.2.4-velixo.N`, and the file version is `6.2.4.77NN` (official 6.2.4: `6.2.4.7675`).

## Fixes

- **velixo.1** – `PdfDocument.AddPage(importedPage)` threw `KeyNotFoundException: The given key 'N 0' was not
  present in the dictionary` when the page references objects that do not exist in the source file
  (seen in HP Exstream invoices). Such references are now treated as null (ISO 32000-2 7.3.10).
  See `PdfObject.FixUpObject` and `PdfSharp.Tests/IO/ImportTests.cs`.

## Building the binaries consumed by Velixo Reports Pro

```
dotnet build src/foundation/src/PDFsharp/src/PdfSharp-gdi/PdfSharp-gdi.csproj -c Release -f net462
dotnet build src/foundation/src/PDFsharp/src/PdfSharp.BarCodes-gdi/PdfSharp.BarCodes-gdi.csproj -c Release -f net462
dotnet build src/foundation/src/PDFsharp/src/PdfSharp.Charting-gdi/PdfSharp.Charting-gdi.csproj -c Release -f net462
dotnet build src/foundation/src/shared/src/PdfSharp.Quality-gdi/PdfSharp.Quality-gdi.csproj -c Release -f net462
dotnet build src/foundation/src/shared/src/PdfSharp.Snippets-gdi/PdfSharp.Snippets-gdi.csproj -c Release -f net462
```

Add `-p:FileVersion=6.2.4.77NN` to each command (and build with the .NET 10 SDK, i.e. C# 14, like upstream 6.2.4).
Build from a checkout of the `v6.2.4-velixo.N` tag so GitVersion stamps the right version.
