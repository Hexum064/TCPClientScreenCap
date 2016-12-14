using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;


using System.Windows.Shell;
using System.Windows.Threading;

namespace ClientScreenCap
{
    /// <summary>
    /// Interaction logic for CapWindow.xaml
    /// </summary>
    public partial class CapWindow : Window
    {
        private Dispatcher _uiDispatcher = null;

        public CapWindow()
        {
            InitializeComponent();
            _uiDispatcher = Dispatcher.CurrentDispatcher;

            /* Set Borderless Chrome to this Window */
            WindowChrome Resizable_BorderLess_Chrome = new WindowChrome();
            Resizable_BorderLess_Chrome.GlassFrameThickness = new Thickness(-1);
            Resizable_BorderLess_Chrome.CornerRadius = new CornerRadius(0);
            Resizable_BorderLess_Chrome.ResizeBorderThickness = new Thickness(6);
            WindowChrome.SetWindowChrome(this, Resizable_BorderLess_Chrome);

            SizeChanged += (o, e) => Resizable_BorderLess_Chrome.CaptionHeight = ActualHeight;
        }

        public System.Windows.Size GetScreenSize()
        {
            return new System.Windows.Size(ActualWidth - 12, ActualHeight - 12);
        }

        public Bitmap GetScreenBitmap()
        {
            Bitmap bitmap = null;

            _uiDispatcher.Invoke(() =>
                {
                    bitmap = new Bitmap((int)(ActualWidth - 12), (int)(ActualHeight - 12), PixelFormat.Format24bppRgb);
                    // Create a graphics object from the bitmap
                    var gfxScreenshot = Graphics.FromImage(bitmap);
                    // Take the screenshot from the upper left corner to the right bottom corner
                    gfxScreenshot.CopyFromScreen((int)Left + 6, (int)Top + 6, 0, 0, new System.Drawing.Size((int)(ActualWidth - 12), (int)(ActualHeight - 12)), CopyPixelOperation.SourceCopy);


                });
            return bitmap;
        }
    }
}
