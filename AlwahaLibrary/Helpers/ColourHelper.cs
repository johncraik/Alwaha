namespace AlwahaLibrary.Helpers;

public static class ColourHelper
{
    public static string HoverColour(string col)
    {
        var rgb = ExtractRGB(col);
        
        rgb.R = (int)(rgb.R + (255 - rgb.R) * 0.4);
        rgb.R = Math.Min(255, Math.Max(0, rgb.R));
        
        rgb.G = (int)(rgb.G + (255 - rgb.G) * 0.4);
        rgb.G = Math.Min(255, Math.Max(0, rgb.G));
        
        rgb.B = (int)(rgb.B + (255 - rgb.B) * 0.4);
        rgb.B = Math.Min(255, Math.Max(0, rgb.B));

        return $"#{rgb.R:X2}{rgb.G:X2}{rgb.B:X2}";
    }

    public static string FontColour(string col)
    {
        var rgb = ExtractRGB(col);

        var rNormalised = rgb.R / 255.0;
        var gNormalised = rgb.G / 255.0;
        var bNormalised = rgb.B / 255.0;

        var luminance = 0.2126 * rNormalised + 0.7152 * gNormalised + 0.0722 * bNormalised;
        return luminance > 0.5 ? "#000000" : "#ffffff";
    }

    private static (int R, int G, int B) ExtractRGB(string colour)
    {
        var hex = colour[1..];
        
        var r = Convert.ToInt32(hex[..2], 16);
        var g = Convert.ToInt32(hex[2..4], 16);
        var b = Convert.ToInt32(hex[4..6], 16);

        return (r, g, b);
    }
}