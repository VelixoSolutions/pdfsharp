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
- **velixo.2** – `PdfReader.Open` threw `NotImplementedException: File streams are not yet implemented.` for any
  file that contains a stream with an `/F` entry, even one that nothing references (seen in InDesign invoices
  converted by Ghostscript, which leave a C2PA manifest object behind). Such a stream is now read like any other,
  from the bytes between `stream` and `endstream`, and its dictionary is kept unchanged. See `Parser.GetStreamLength`
  and `PdfSharp.Tests/IO/ImportTests.cs`.
- **velixo.3** – a stream with an `/F` entry is now read as an empty stream, and `/F`, `/FFilter`, `/FDecodeParms`,
  `/Filter`, `/DecodeParms` and `/DL` are removed from its dictionary, as ISO 32000-2 7.3.8.2 says to ignore the bytes
  in the file and `/Filter`. In velixo.2, decoding the stream applied `/Filter` to bytes it does not describe, and an
  imported page copied the reference to the external file, such as a network path, into the new document.
  See `Parser.ReadDictionaryStream` and `PdfSharp.Tests/IO/ImportTests.cs`.

## Building the binaries consumed by Velixo Reports Pro

```
dotnet build src/foundation/src/PDFsharp/src/PdfSharp-gdi/PdfSharp-gdi.csproj -c Release -f net462
dotnet build src/foundation/src/PDFsharp/src/PdfSharp.BarCodes-gdi/PdfSharp.BarCodes-gdi.csproj -c Release -f net462
dotnet build src/foundation/src/PDFsharp/src/PdfSharp.Charting-gdi/PdfSharp.Charting-gdi.csproj -c Release -f net462
dotnet build src/foundation/src/shared/src/PdfSharp.Quality-gdi/PdfSharp.Quality-gdi.csproj -c Release -f net462
dotnet build src/foundation/src/shared/src/PdfSharp.Snippets-gdi/PdfSharp.Snippets-gdi.csproj -c Release -f net462
```

Add `-p:FileVersion=6.2.4.77NN -p:InformationalVersion=6.2.4-velixo.N` to each command, and build on Windows with
the .NET 10 SDK (C# 14), like upstream 6.2.4, from a clone made with `git -c core.autocrlf=true clone`. Otherwise
GitVersion stamps `6.2.4-velixo` without the number, and on Linux the source has LF line endings, so the DLL differs
from the official build in more than the fixes.
Build from a checkout of the `v6.2.4-velixo.N` tag, on a `release/6.2.4-velixo.N` branch.
