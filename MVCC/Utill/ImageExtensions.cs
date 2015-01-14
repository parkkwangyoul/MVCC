﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.IO;
using Media = System.Windows.Media;

namespace MVCC.Utill
{
    public static class ImageExtensions
    {
        public static Bitmap ToBitmap(this byte[] data, int width, int height
            , PixelFormat format)
        {
            var bitmap = new Bitmap(width, height, format);

            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat);
            Marshal.Copy(data, 0, bitmapData.Scan0, data.Length);
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        public static Bitmap ToBitmap(this short[] data, int width, int height
            , PixelFormat format)
        {
            var bitmap = new Bitmap(width, height, format);

            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat);
            Marshal.Copy(data, 0, bitmapData.Scan0, data.Length);
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        public static Media.Imaging.BitmapSource ToBitmapSource(this byte[] data
            , Media.PixelFormat format, int width, int height)
        {
            return Media.Imaging.BitmapSource.Create(width, height, 96, 96
                , format, null, data, width * format.BitsPerPixel / 8);
        }

        public static Media.Imaging.BitmapSource ToBitmapSource(this short[] data
    , Media.PixelFormat format, int width, int height)
        {
            return Media.Imaging.BitmapSource.Create(width, height, 96, 96
                , format, null, data, width * format.BitsPerPixel / 8);
        }

        public static Media.Imaging.BitmapSource ToTransparentBitmapSource(this byte[] data
            , int width, int height)
        {
            return data.ToBitmapSource(Media.PixelFormats.Bgra32, width, height);
        }



        // conversion between bitmapsource and bitmap

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static Media.Imaging.BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            if (bitmap == null) return null;
            IntPtr ptr = bitmap.GetHbitmap();
            var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            ptr,
            IntPtr.Zero,
            Int32Rect.Empty,
            Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ptr);
            return source;
        }

        public static Bitmap ToBitmap(this Media.Imaging.BitmapSource source)
        {
            Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                var enc = new Media.Imaging.PngBitmapEncoder();
                enc.Frames.Add(Media.Imaging.BitmapFrame.Create(source));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        public static Bitmap Flip(this Bitmap bitmap)
        {
            if (bitmap == null)
                return null;
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
            return bitmap;
        }
    }
}
