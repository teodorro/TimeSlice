using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace TimeSlice
{
    internal class ImageCreator
    {
        public BitmapImage GetImage(List<FileItem> bscans, int width, int height, int time)
        {
            var data = ConvertToArray(bscans, time);
            var bitmap = MakeBitmap(data);
            bitmap = ResizeBitmap(bitmap, width, height);
            return BitmapToImageSource(bitmap);
        }

        private int[,] ConvertToArray(List<FileItem> bscans, int time)
        {
            var maxLength = bscans.Select(x => x.Length).Max();

            var result = new int[bscans.Count, maxLength];
            for (int i = 0; i < bscans.Count; i++)
                for (int j = 0; j < bscans[i].Length; j++)
                    result[i, j] = bscans[i].Bscan[j][time];

            return result;
        }
        
        private Bitmap MakeBitmap(int[,] data)
        {
            int width = data.GetLength(1);
            int height = data.GetLength(0);
            int stride = width * 4;
            int[,] integers = new int[height, width];


            for (int y = 0; y < height; ++y)
                for (int x = 0; x < width; ++x)
                {
                    var val = GetValue(data[y, x]);
                    byte[] bgra = new byte[] { val, val, val, 255 };
                    integers[y, x] = BitConverter.ToInt32(bgra, 0);
                }

            // Copy into bitmap
            Bitmap bitmap;
            unsafe
            {
                fixed (int* intPtr = &integers[0, 0])
                {
                    bitmap = new Bitmap(width, height, stride, PixelFormat.Format32bppRgb, new IntPtr(intPtr));
                }
            }

            return bitmap;
        }

        private byte GetValue(int amp)
        {
            var val = Math.Abs(amp - FileItem.MaxAmp);
            var logValue = Math.Log(val + 1) * Math.Sign(amp);
            var valBalanced = logValue / Math.Log(FileItem.MaxAmp + 1) * 255;
            return Convert.ToByte(valBalanced);
        }
        
        //        private byte GetValue(int amp)
        //        {
        //            return Convert.ToByte(amp % 255);
        //        }

        private Bitmap ResizeBitmap(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public BitmapSource CreateBitmapSource(System.Windows.Media.Color color, int width, int height)
        {
            int stride = width;;
            byte[] pixels = new byte[height * stride];

            List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
            colors.Add(color);
            BitmapPalette myPalette = new BitmapPalette(colors);

            BitmapSource image = BitmapSource.Create(
                width,
                height,
                96,
                96,
                PixelFormats.Indexed1,
                myPalette,
                pixels,
                stride);

            return image;
        }
    }
}