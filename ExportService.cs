using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace AvApp1.Services;

public interface IExportService
{
    void RenderAndSave(Control control, string path);
}

public class ExportService : IExportService
{
    public void RenderAndSave(Control control, string path)
    {

        if (control == null || control.Bounds.Width <= 0 || control.Bounds.Height <= 0) return;

        double scaleFactor = 3.0;


        var pixelWidth = (int)(control.Bounds.Width * scaleFactor);
        var pixelHeight = (int)(control.Bounds.Height * scaleFactor);
        var pixelSize = new PixelSize(pixelWidth, pixelHeight);


        var dpiVector = new Vector(96 * scaleFactor, 96 * scaleFactor);
        
        using (var bitmap = new RenderTargetBitmap(pixelSize, dpiVector))
        {
            bitmap.Render(control);
            bitmap.Save(path);
        }

    }
}