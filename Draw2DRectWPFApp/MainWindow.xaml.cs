using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Draw2DRectWPFApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Rectangle DragRectangle = null;
        private Point StartPoint, LastPoint;

        private void canDraw_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StartPoint = Mouse.GetPosition(canDraw);
            LastPoint = StartPoint;
            DragRectangle = new Rectangle();
            DragRectangle.Width = 1;
            DragRectangle.Height = 1;
            DragRectangle.Stroke = Brushes.Red;
            DragRectangle.StrokeThickness = 1;
            DragRectangle.Cursor = Cursors.Cross;

            canDraw.Children.Add(DragRectangle);
            Canvas.SetLeft(DragRectangle, StartPoint.X);
            Canvas.SetTop(DragRectangle, StartPoint.Y);

            canDraw.MouseMove += canDraw_MouseMove;
            canDraw.MouseUp += canDraw_MouseUp;
            canDraw.CaptureMouse();
        }

        private void canDraw_MouseMove(object sender, MouseEventArgs e)
        {
            LastPoint = Mouse.GetPosition(canDraw);
            DragRectangle.Width = Math.Abs(LastPoint.X - StartPoint.X);
            DragRectangle.Height = Math.Abs(LastPoint.Y - StartPoint.Y);
            Canvas.SetLeft(DragRectangle, Math.Min(LastPoint.X, StartPoint.X));
            Canvas.SetTop(DragRectangle, Math.Min(LastPoint.Y, StartPoint.Y));
        }

        private void canDraw_MouseUp(object sender, MouseButtonEventArgs e)
        {
            canDraw.ReleaseMouseCapture();
            canDraw.MouseMove -= canDraw_MouseMove;
            canDraw.MouseUp -= canDraw_MouseUp;
            canDraw.Children.Remove(DragRectangle);

            if (LastPoint.X < 0) LastPoint.X = 0;
            if (LastPoint.X >= canDraw.Width) LastPoint.X = canDraw.Width - 1;
            if (LastPoint.Y < 0) LastPoint.Y = 0;
            if (LastPoint.Y >= canDraw.Height) LastPoint.Y = canDraw.Height - 1;

            int x = (int)Math.Min(LastPoint.X, StartPoint.X);
            int y = (int)Math.Min(LastPoint.Y, StartPoint.Y);
            int width = (int)Math.Abs(LastPoint.X - StartPoint.X) + 1;
            int height = (int)Math.Abs(LastPoint.Y - StartPoint.Y) + 1;

            BitmapSource bms = (BitmapSource)imgOriginal.Source;
            CroppedBitmap cropped_bitmap = new CroppedBitmap(bms, new Int32Rect(x, y, width, height));
            imgResult.Source = cropped_bitmap;

            DragRectangle = null;
        }

        private void SaveAsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog sfdImage = new SaveFileDialog();
            sfdImage.Filter = "Image Files|*.bmp;*.gif;*.jpg;*.png;*.tif|All files (*.*)|*.*";
            sfdImage.DefaultExt = "png";
            if (sfdImage.ShowDialog().Value)
            {
                (imgResult.Source as CroppedBitmap)?.SaveImage(sfdImage.FileName);
            }
        }
    }

    public static class ImageExtensions
    {
        public static void SaveImage(this CroppedBitmap cropped_bitmap, string filename)
        {
            FileInfo file_info = new FileInfo(filename);
            BitmapEncoder encoder = null;
            switch (file_info.Extension.ToLower())
            {
                case ".bmp":
                    encoder = new BmpBitmapEncoder();
                    break;
                case ".gif":
                    encoder = new GifBitmapEncoder();
                    break;
                case ".jpg":
                case ".jpeg":
                    encoder = new JpegBitmapEncoder();
                    break;
                case ".png":
                    encoder = new PngBitmapEncoder();
                    break;
                case ".tif":
                    encoder = new TiffBitmapEncoder();
                    break;
            }

            if (encoder != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(cropped_bitmap));

                using (FileStream stream = new FileStream(filename, FileMode.Create))
                {
                    encoder.Save(stream);
                }
            }
        }
    }
}
