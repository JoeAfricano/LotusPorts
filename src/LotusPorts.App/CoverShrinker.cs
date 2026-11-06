using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace LotusPorts.App;

/// <summary>Turns downloaded box art into a 400 px wide JPEG with the Windows image codecs (about 10x smaller).</summary>
public static class CoverShrinker
{
    private const uint TargetWidth = 400;

    public static async Task<byte[]?> ShrinkAsync(byte[] png)
    {
        try
        {
            using var input = new InMemoryRandomAccessStream();
            await input.WriteAsync(png.AsBuffer());
            input.Seek(0);
            var decoder = await BitmapDecoder.CreateAsync(input);
            var width = Math.Min(TargetWidth, decoder.PixelWidth);
            var height = (uint)Math.Max(1, Math.Round(decoder.PixelHeight * (double)width / decoder.PixelWidth));
            using var pixels = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                new BitmapTransform { ScaledWidth = width, ScaledHeight = height, InterpolationMode = BitmapInterpolationMode.Fant },
                ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);

            using var output = new InMemoryRandomAccessStream();
            var options = new BitmapPropertySet { ["ImageQuality"] = new BitmapTypedValue(0.85, Windows.Foundation.PropertyType.Single) };
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, output, options);
            using var opaque = SoftwareBitmap.Convert(pixels, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore);
            encoder.SetSoftwareBitmap(opaque);
            await encoder.FlushAsync();

            var bytes = new byte[output.Size];
            output.Seek(0);
            await output.ReadAsync(bytes.AsBuffer(), (uint)bytes.Length, InputStreamOptions.None);
            return bytes;
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or ArgumentException)
        {
            // A file the codecs can't read is treated as no cover.
            return null;
        }
    }
}
