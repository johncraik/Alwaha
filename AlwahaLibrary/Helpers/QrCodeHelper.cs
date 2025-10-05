using QRCoder;

namespace AlwahaLibrary.Helpers;

public class QrCodeHelper
{
    public static string GenerateQrCode(string content)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

        var pngBytes = new PngByteQRCode(data).GetGraphic(pixelsPerModule: 10);
        var base64 = Convert.ToBase64String(pngBytes);
        return $"data:image/png;base64,{base64}";
    }
}