using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WkHtmlToPdfDotNet;

public class PDFConverterManager
{

    public static void ConvertToPDF(string path, string html)
    {
        var converter = new BasicConverter(new PdfTools());

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4Plus,
            },
            Objects =
            {
                new ObjectSettings() {
                    PagesCount = true,
                    HtmlContent = html,
                    WebSettings = { DefaultEncoding = "utf-8" },
                    HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 }
                }
            }
        };

        byte[] pdfBytes = converter.Convert(doc);

        try
        {
            File.WriteAllBytes(path, pdfBytes);
            Debug.Log($"Archivo guardado en: {path}");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }


}
