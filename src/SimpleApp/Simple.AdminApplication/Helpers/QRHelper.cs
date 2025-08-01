using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace Simple.AdminApplication.Helpers;

public class QRHelper
{
    /// <summary>
    /// 生成二维码并返回图像流
    /// </summary>
    /// <param name="content">要编码的内容</param>
    /// <param name="width">图像宽度，默认300</param>
    /// <param name="height">图像高度，默认300</param>
    /// <param name="margin">边距，默认1</param>
    /// <returns>包含二维码图像的内存流</returns>
    public static MemoryStream GenerateQrCodeStream(string content, int width = 300, int height = 300, int margin = 1)
    {
        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = width,
                Height = height,
                Margin = margin
            }
        };

        var pixelData = writer.Write(content);
        using var image = Image.LoadPixelData<Rgba32>(pixelData.Pixels, pixelData.Width, pixelData.Height);
        
        var ms = new MemoryStream();
        image.Save(ms, new PngEncoder());
        ms.Position = 0;
        
        return ms;
    }

    /// <summary>
    /// 生成条形码并返回图像流
    /// </summary>
    /// <param name="content">要编码的内容</param>
    /// <param name="format">条形码格式，默认CODE_128</param>
    /// <param name="width">图像宽度，默认500</param>
    /// <param name="height">图像高度，默认200</param>
    /// <param name="margin">边距，默认10</param>
    /// <returns>包含条形码图像的内存流</returns>
    public static MemoryStream GenerateBarcodeStream(
        string content, 
        BarcodeFormat format = BarcodeFormat.CODE_128, 
        int width = 500, 
        int height = 200, 
        int margin = 10)
    {
        var writer = new BarcodeWriterPixelData
        {
            Format = format,
            Options = new EncodingOptions
            {
                Width = width,
                Height = height,
                Margin = margin
            }
        };

        var pixelData = writer.Write(content);
        using var image = Image.LoadPixelData<Rgba32>(pixelData.Pixels, pixelData.Width, pixelData.Height);
        
        var ms = new MemoryStream();
        image.Save(ms, new PngEncoder());
        ms.Position = 0;
        
        return ms;
    }

    /// <summary>
    /// 生成二维码并保存为文件
    /// </summary>
    public static void GenerateQrCodeFile(string content, string filePath, int width = 300, int height = 300)
    {
        using var ms = GenerateQrCodeStream(content, width, height);
        using var fileStream = new FileStream(filePath, FileMode.Create);
        ms.CopyTo(fileStream);
    }

    /// <summary>
    /// 生成条形码并保存为文件
    /// </summary>
    public static void GenerateBarcodeFile(string content, string filePath, BarcodeFormat format = BarcodeFormat.CODE_128, int width = 500, int height = 200)
    {
        using var ms = GenerateBarcodeStream(content, format, width, height);
        using var fileStream = new FileStream(filePath, FileMode.Create);
        ms.CopyTo(fileStream);
    }
}