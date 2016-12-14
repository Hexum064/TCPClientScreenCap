using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ClientScreenCap
{
    public class TcpClientVm : INotifyPropertyChanged
    {
        private const byte MAX_BRIGHT = 64;
        
        private TcpClient _client = null;
        private bool _isReady = false;
        private CapWindow _win = null;
        private Dispatcher _uiDispatcher = null;
        private byte[] _gammaLut = new byte[256];
        private Timer _fpsTimer = new Timer(500);
        private int _frames = 0;


        public TcpClientVm()
        {
            _uiDispatcher = Dispatcher.CurrentDispatcher;

            _client = new TcpClient();
            IpAddress = "192.168.4.1";
            Port = 6001;
            ConnectCommand = new SimpleCommand(SetConnect);
            RunCommand = new SimpleCommand(RunCap);
            Rows = 48;
            Cols = 148;
            Gamma = 18;

            _gammaLut = GenerateGammaTable(2.8, MAX_BRIGHT);

            _fpsTimer.Elapsed += _fpsTimer_Elapsed;
        }

        private void _fpsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _uiDispatcher.Invoke(() => Status = (_frames * 2).ToString());
            _frames = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SimpleCommand ConnectCommand { get; private set; }
        public SimpleCommand RunCommand { get; private set; }

        private double _gamma;

        public double Gamma
        {
            get { return _gamma; }
            set { _gamma = value; RaisePropertyChanged(); _gammaLut = GenerateGammaTable((_gamma / 10d), MAX_BRIGHT); }
        }

        private string _ipAddress;

        public string IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; RaisePropertyChanged(); }
        }

        private int _port;

        public int Port
        {
            get { return _port; }
            set { _port = value; RaisePropertyChanged(); }
        }


        private bool _isConnected;

        public bool IsConnected
        {
            get { return _isConnected; }
            private set { _isConnected = value; RaisePropertyChanged(); }
        }


        private bool _isRunning;

        public bool IsRunning
        {
            get { return _isRunning; }
            private set { _isRunning = value; RaisePropertyChanged(); }
        }

        private Bitmap _bmp;

        public Bitmap Bmp
        {
            get { return _bmp; }
            private set { _bmp = value; RaisePropertyChanged("ImageSource"); RaisePropertyChanged(); }
        }

        private byte _rows;

        public byte Rows
        {
            get { return _rows; }
            private set { _rows = value; RaisePropertyChanged(); }
        }

        private UInt16 _cols;

        public UInt16 Cols
        {
            get { return _cols; }
            private set { _cols = value; RaisePropertyChanged(); }
        }

        private string _status;

        public string Status
        {
            get { return _status; }
            private set { _status = value; RaisePropertyChanged(); }
        }


        public BitmapImage ImageSource
        {
            get { return BitmapToImageSource(Bmp); }
        }


        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            BitmapImage bitmapimage = null;
            if (bitmap == null)
                return null;
            _uiDispatcher.Invoke(() =>
                {
                    using (MemoryStream memory = new MemoryStream())
                    {
                        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                        memory.Position = 0;
                        bitmapimage = new BitmapImage();
                        bitmapimage.BeginInit();
                        bitmapimage.StreamSource = memory;
                        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapimage.EndInit();

                        
                    }
                });

            return bitmapimage;
        }

        private byte[] GenerateGammaTable(double gamma, byte maxOut)
        {
            byte[] table = new byte[256];
            for (int i = 0; i <= 255; i++)
                table[i] = (byte)(Math.Pow((double)i / (double)255, gamma) * maxOut + 0.5);

            return table;
        }

        public void Dispose()
        {
            _client.Close();

            if (_win != null && !_win.IsLoaded)
                _win.Close();

            if (IsRunning)
                RunCap();
        }

        protected void SetConnect()
        {
            IPAddress address;

            return;

            try
            {
                if (_client.Client == null)
                    _client = new TcpClient();

                if (_client.Connected)
                {

                    Dispose();
                    return;
                }

                _isReady = false;


                if (!IPAddress.TryParse(IpAddress, out address))
                    MessageBox.Show("The address '" + IpAddress + "' is not valid.");

                _client.Connect(address, Port);
                _isReady = true;

                Task.Run(() =>
                {
                    while (_client.Client != null && _client.Connected)
                    {
                        byte[] rcvData = new byte[1];
                        //if (!IsRunning)
                        //    break;
                        _client.Client.Receive(rcvData);
                        System.Diagnostics.Debug.WriteLine(rcvData[0].ToString());
                        if (rcvData[0] == 128)
                            _isReady = true;
                    };
                });
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
            finally
            {
                if (_client.Client == null)
                    IsConnected = false;
                else
                    IsConnected = _client.Connected;
            }

        }

        private void RunCap()
        {
            byte[] buff;

            if (IsRunning)
            {
                IsRunning = false;
                _win.Close();
                _fpsTimer.Enabled = false;
                _fpsTimer.Stop();
                return;
            }

            _fpsTimer.Enabled = true;
            _fpsTimer.Start();

            _win = new CapWindow();
            _win.Show();


            IsRunning = true;
            _client.Client.Send(new byte[] { Rows, 0, 0, 129, (byte)(Cols & 0x00FF), (byte)(Cols >> 8), 0, 130 });
            buff = GenerateDefaultLut();

            _client.Client.Send(buff);

            Task.Run(() => SendCap());
        }

        private void SendCap()
        {
            //Bitmap bmp;
            byte[] buff;
            //int frames = 0;

            while(IsRunning)
            {
                
                Bmp = new Bitmap(_win.GetScreenBitmap(), Cols, Rows);
                buff = GetBitmapBytes(Bmp);
                while (!_isReady) { }
                _isReady = false;
                if (!IsRunning)
                    break;
                 _client.Client.Send(buff);
                 _frames++;
                //System.Diagnostics.Debug.WriteLine(frames++);

            }

        }

        private byte[] GetBitmapBytes(Bitmap bmp)
        {
            byte[] buff = null;
    
            _uiDispatcher.Invoke(() =>
                {

                    buff = new byte[bmp.Width * bmp.Height * 2];
                    Color pixelColor = default(Color);
                    UInt16 pixel;
                    int index = 0;

                    for (int x = 0; x < bmp.Width; x++ )
                    {
                        for(int y = 0; y < bmp.Height; y++)
                        {

                                    pixelColor = bmp.GetPixel(x, y);


                            pixel = (UInt16)(((_gammaLut[pixelColor.G] >> 3) << 10) + ((_gammaLut[pixelColor.R] >> 3) << 5) + (_gammaLut[pixelColor.B] >> 3));
                            buff[(index * 2) + 1] = (byte)(pixel >> 8);
                            buff[(index * 2) + 0] = (byte)(pixel & 0x00FF);
                            index++;
                        }
                    }
                });
            return buff;
        }

        private byte[] GenerateDefaultLut()
        {
            byte len = (byte)Math.Pow(2, 5);
            byte[] lut = new byte[12 * len];

            for (byte c = 0; c < 3; c++)
            {



                for (byte i = 0; i < Math.Pow(2, 5); i++)
                {
                    lut[(c * len * 4) + (i * 4) + 3] = (byte)(131 + c);
                    lut[(c * len * 4) + (i * 4) + 2] = 0;
                    lut[(c * len * 4) + (i * 4) + 1] = i;
                    lut[(c * len * 4) + (i * 4) + 0] = (byte)(i << 3);
                }
            }

            return lut;
        }


        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
