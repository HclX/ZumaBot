﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;

namespace Zuma
{
    public class GameInfo
    {
        private string _zumaDir;

        public ImageData BallMask
        {
            get;
            private set;
        }

        public GameInfo(string zumaDir)
        {
            _zumaDir = zumaDir;
            BallMask = ImageData.FromFile(Path.Combine(_zumaDir, "levels\\triangle\\ballmask.gif")).GrayScale().Threshold(0, 0, 0);
        }

        public static readonly Rectangle LifeRect = new Rectangle(new Point(24, 0), new Size(80, 30));

        public static readonly Rectangle LevelRect = new Rectangle(new Point(176, 4), new Size(50, 12));

        public static readonly Rectangle ScoreRect = new Rectangle(new Point(270, 0), new Size(94, 22));

        public static readonly Rectangle ZumaRect = new Rectangle(new Point(410, 6), new Size(64, 10));
    }

    public static class Game
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetDesktopWindow();

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT pt);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetClientRect(IntPtr hWnd, ref RECT rect);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetForgroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string className, string windowName);

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern void mouse_event(UInt32 dwFlags, UInt32 dx, UInt32 dy, UInt32 cButtons, IntPtr extraInfo);
        private const UInt32 MOUSEEVENTF_LEFTDOWN = 0x02;
        private const UInt32 MOUSEEVENTF_LEFTUP = 0x04;
        private const UInt32 MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const UInt32 MOUSEEVENTF_RIGHTUP = 0x010;
        private const UInt32 MOUSEEVENTF_MOVE = 0x0001;

        private static void DoMouseLClick()
        {
            //perform click            
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
        }

        private static void DoMouseRClick()
        {
            //perform click            
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, IntPtr.Zero);
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, IntPtr.Zero);
        }

        private static IntPtr FindMainWnd()
        {
            IntPtr hwnd = GetForegroundWindow();

            IntPtr hZumaWnd = FindWindow("MainWindow", "×æÂêºÀ»ªÍêÈ«°æ1.30");
            if (!IsWindow(hZumaWnd))
            {
                System.Diagnostics.Debug.WriteLine("Zuma window not found!");
                return IntPtr.Zero;
            }

            if (hZumaWnd != GetForegroundWindow())
            {
                System.Diagnostics.Debug.WriteLine("Zuma window not active!");
                return IntPtr.Zero;
            }

            RECT r = new RECT();
            GetClientRect(hZumaWnd, ref r);

            if (r.Right != 640 || r.Bottom != 480)
            {
                System.Diagnostics.Debug.WriteLine("Zuma window size incorrect:{0}x{1}", r.Right, r.Bottom);
                return IntPtr.Zero;
            }

            return hZumaWnd;
        }

        private static Bitmap CaptureScreen(IntPtr hwnd)
        {
            var r = new RECT();
            GetWindowRect(hwnd, ref r);

            if (r.Right == 0 && r.Bottom == 0)
            {
                // full screen
                Bitmap bit = new Bitmap(r.Right, r.Bottom);
                Graphics gs = Graphics.FromImage(bit);
                gs.CopyFromScreen(new Point(0, 0), new Point(0, 0), bit.Size);

                return bit;
            }
            else
            {
                GetClientRect(hwnd, ref r);
                var bounds = new Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
                var result = new Bitmap(bounds.Width, bounds.Height);

                using (var graphics = Graphics.FromImage(result))
                {
                    IntPtr hdc = graphics.GetHdc();
                    PrintWindow(hwnd, hdc, 1);
                    graphics.ReleaseHdc(hdc);
                }

                return result;
            }
        }

        private static IntPtr _hZumaWnd = IntPtr.Zero;
        public static IntPtr MainWindow
        {
            get
            {
                if (!IsWindow(_hZumaWnd))
                {
                    _hZumaWnd = FindMainWnd();
                }

                return _hZumaWnd;
            }
        }

        public static Bitmap Screenshot
        {
            get
            {
                var hwnd = MainWindow;

                if (hwnd != IntPtr.Zero)
                {
                    return CaptureScreen(hwnd);
                }
                else
                {
                    return null;
                }
            }
        }

        public static bool Active
        {
            get
            {
                return MainWindow == GetForegroundWindow();
            }
        }

        public static bool Shoot(Int32 x, Int32 y)
        {
            var hwnd = MainWindow;
            if (hwnd != IntPtr.Zero)
            {
                POINT pt = new POINT() { X = x, Y = y };
                ClientToScreen(hwnd, ref pt);
                Cursor.Position = new Point(pt.X, pt.Y);

                DoMouseLClick();
                Thread.Sleep(500);

                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Swap()
        {
            var hwnd = MainWindow;
            if (hwnd != IntPtr.Zero)
            {
                DoMouseRClick();
                Thread.Sleep(200);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public struct Point3D
    {
        public Int32 X { get; private set; }
        public Int32 Y { get; private set; }
        public Int32 Z { get; private set; }

        public Point3D(double x, double y, double z = 0.0)
            : this((Int32)x, (Int32)y, (Int32)z)
        {
        }

        public Point3D(Int32 x, Int32 y, Int32 z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3D Offset2D(Int32 x, Int32 y)
        {
            return new Point3D(X - x, Y - y);
        }

        public Point3D Offset2D(Point3D pt)
        {
            return Offset2D(pt.X, pt.Y);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point3D))
            {
                return false;
            }

            var pt = (Point3D)obj;
            return (pt.X == X && pt.Y == Y && pt.Z == Z);
        }

        public Int32 Angle2D(Point3D from)
        {
            var pt = Offset2D(from);
            return (Int32)(Math.Atan2(pt.Y, pt.X) * 1800 / Math.PI + 1800);
        }

        public Int32 Distance2D(Point3D from)
        {
            var pt = Offset2D(from);
            return (Int32)(Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y));
        }
    }

    struct ColorRange
    {
        public UInt32 MaxTotal;
        public UInt32 MinTotal;
        public UInt32 MaxR;
        public UInt32 MinR;
        public UInt32 MaxG;
        public UInt32 MinG;
        public UInt32 MaxB;
        public UInt32 MinB;

        public ColorRange(UInt32 t1, UInt32 t2, UInt32 r1, UInt32 r2, UInt32 g1, UInt32 g2, UInt32 b1, UInt32 b2)
        {
            MaxTotal = t1;
            MinTotal = t2;
            MaxR = r1;
            MinR = r2;
            MaxG = g1;
            MinG = g2;
            MaxB = b1;
            MinB = b2;
        }

        public bool Satisfy(int t, int r, int g, int b)
        {
            return
                //t <= MaxTotal && t >= MinTotal &&
                r <= MaxR && r >= MinR &&
                g <= MaxG && g >= MinG &&
                b <= MaxB && b >= MinB;
        }
    };

    public class Curve
    {
        private Point3D[] _lines;
        private Point3D[] _points;

        public Point3D this[int index]
        {
            get
            {
                return _points[index];
            }
        }

        public int Length
        {
            get
            {
                return _points.Length;
            }
        }

        public Curve(string file)
        {
            using (var br = new BinaryReader(File.OpenRead(file)))
            {
                var signature = br.ReadUInt32();
                var versionMajor = br.ReadUInt32();
                var versionMinor = br.ReadUInt32();

                UInt32 sectionSize = br.ReadUInt32();

                br.BaseStream.Seek(4 * 4, SeekOrigin.Begin);
                UInt32 count = br.ReadUInt32();

                var lines = new List<Point3D>();
                for (var i = 0; i < count; i++)
                {
                    lines.Add(new Point3D(br.ReadInt32(), br.ReadInt32(), br.ReadUInt16()));
                }
                lines.Reverse();

                _lines = lines.ToArray();

                br.BaseStream.Seek(4 * 4 + sectionSize, SeekOrigin.Begin);
                count = br.ReadUInt32();

                var x = (double)br.ReadSingle();
                var y = (double)br.ReadSingle();

                var pts = new List<Point3D>();

                pts.Add(new Point3D(x, y));
                for (var i = 1; i < count; i++)
                {
                    var z = br.ReadUInt16();
                    x += br.ReadSByte() / 100.0;
                    y += br.ReadSByte() / 100.0;

                    pts.Add(new Point3D(x, y, z));
                }

                pts.Reverse();
                _points = pts.ToArray();
            }
        }

        public IEnumerable<Point3D> Points
        {
            get
            {
                return _points;
            }
        }
    }

    public class Ball
    {
        static public int LastID = 0;

        Point3D _center;

        public int Position
        {
            get;
            private set;
        }

        public Curve Curve
        {
            get;
            private set;
        }

        public Color Color
        {
            get;
            private set;
        }

        public Point3D XY
        {
            get
            {
                return Curve[Position];
            }
        }

        public Int32 Distance
        {
            get;
            private set;
        }

        public Int32 Angle
        {
            get;
            private set;
        }

        public Int32 AngleSpan
        {
            get;
            private set;
        }

        public Int32 ID
        {
            get;
            private set;
        }

        public bool Visible
        {
            get;
            set;
        }

        public Ball(Curve curve, int pos, Color color, Point3D frogXY)
        {
            _center = frogXY;

            Curve = curve;
            Position = pos;
            Color = color;

            Angle = XY.Angle2D(_center);
            Distance = XY.Distance2D(_center);
            AngleSpan = (Int32)(1800 * 32 / (Distance * Math.PI));

            ID = LastID++;

            // System.Diagnostics.Debug.WriteLine("ID={6}, Color={0}, Angle={1}, AngleSpan={2}, Distance={3}, XY=({4}, {5})", Color, Angle, AngleSpan, Distance, XY.X, XY.Y, ID);
        }

        public void Draw(Graphics g)
        {
            g.DrawEllipse(new Pen(Color, 3), XY.X - 16, XY.Y - 16, 32, 32);
            g.DrawEllipse(new Pen(Color.Black, 3), XY.X - 1, XY.Y - 1, 2, 2);

            StringFormat sf = new StringFormat();
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;

            if (Visible)
            {
                g.DrawLine(new Pen(Color, 1), _center.X, _center.Y, XY.X, XY.Y);
            }

            g.DrawString(ID.ToString(), new Font("Arial", 16), Brushes.White, new PointF(XY.X, XY.Y), sf);
        }
    }

    public class Frog
    {
        static readonly Dictionary<Color, (int r, int g, int b)> largeRanges = new Dictionary<Color, (int r, int g, int b)>()
        {
            {Color.Blue,    (25,   25,   50)},
            {Color.Green,   (25,   50,   25)},
            {Color.Red,     (50,   25,   25)},
            {Color.Yellow,  (45,   45,   10)},
            {Color.Purple,  (40,   20,   40)},
            {Color.White,   (33,   33,   33)},
        };

        static readonly Dictionary<Color, (int r, int g, int b)> smallRanges = new Dictionary<Color, (int r, int g, int b)>()
        {
            {Color.Blue,    (25,   25,   50)},
            {Color.Green,   (25,   50,   25)},
            {Color.Red,     (50,   25,   25)},
            {Color.Yellow,  (45,   45,   10)},
            {Color.Purple,  (40,   20,   40)},
            {Color.White,   (33,   33,   33)},
        };


        class DetectState
        {
            public Point3D C1;
            public Point3D C2;

            public ImageData S1;
            public ImageData S2;

            public Int32 Angle;

            public Int32 AbsDev;

            public DetectState(ImageData screen, Point3D c, Int32 angle)
            {
                Int32 x1 = (Int32)(Math.Sin(angle * Math.PI / 180) * 26 + c.X);
                Int32 y1 = (Int32)(Math.Cos(angle * Math.PI / 180) * 26 + c.Y);
                C1 = new Point3D(x1, y1);

                Int32 x2 = (Int32)(Math.Sin((angle + 180) * Math.PI / 180) * 26 + c.X);
                Int32 y2 = (Int32)(Math.Cos((angle + 180) * Math.PI / 180) * 26 + c.Y);
                C2 = new Point3D(x2, y2);

                S1 = screen.SubImage(x1 - 16, y1 - 16, 32, 32);
                S2 = screen.SubImage(x2 - 6, y2 - 6, 12, 12);

                Angle = angle;

                var (r1, g1, b1) = S1.SumColors();
                var avg1 = (r1 + g1 + b1) / 3;
                var dev1 = Math.Abs(r1 - avg1) + Math.Abs(g1 - avg1) + Math.Abs(b1 - avg1);

                var (r2, g2, b2) = S2.SumColors();
                var avg2 = (r2 + g2 + b2) / 3;
                var dev2 = Math.Abs(r2 - avg2) + Math.Abs(g2 - avg2) + Math.Abs(b2 - avg2);

                AbsDev = dev1 + dev2;
            }
        };

        DetectState _state;

        public Color First
        {
            get;
            private set;
        }

        public Color Next
        {
            get;
            private set;
        }

        public Int32 Angle
        {
            get { return _state != null ? _state.Angle : 0; }
        }

        public bool Ready
        {
            get { return _state != null; }
        }

        public Point3D XY
        {
            get;
            private set;
        }

        public Frog(Point3D xy)
        {
            XY = xy;
        }

        public void Process(ImageData image)
        {
            DetectState maxState = null;

            for (var angle = 0; angle < 360; angle++)
            {
                var s = new DetectState(image, XY, angle);
                if (maxState == null || maxState.AbsDev < s.AbsDev)
                {
                    maxState = s;
                }
            }

            if (maxState != null && maxState.AbsDev > 30000)
            {
                _state = maxState;

                Color first, next;
                if (!_state.S1.ClassifyColor(largeRanges, 10, out first))
                {
                    System.Diagnostics.Debug.WriteLine("Large ball classify failed");
                }

                if (!_state.S2.ClassifyColor(smallRanges, 10, out next))
                {
                    System.Diagnostics.Debug.WriteLine("Small ball classify failed");
                }

                First = first;
                Next = next;
            }
            else
            {
                _state = null;
            }
        }

        public void Draw(Graphics g)
        {
            g.DrawEllipse(new Pen(Color.Black, 3), XY.X - 54, XY.Y - 54, 108, 108);
            if (_state != null)
            {
                g.DrawEllipse(new Pen(First, 3), _state.C1.X - 16, _state.C1.Y - 16, 32, 32);
                g.DrawEllipse(new Pen(Next, 3), _state.C2.X - 6, _state.C2.Y - 6, 12, 12);
                g.DrawLine(new Pen(Color.Black, 3), _state.C1.X, _state.C1.Y, _state.C2.X, _state.C2.Y);
            }
        }

        public void Shoot(Ball b)
        {
            /*
            NativeHelper.ClientToScreen()
            var rand = new Random();
            Cursor.Position = new Point(rand.Next(640), rand.Next(480));
            NativeHelper.DoMouseClick();
            */
        }
    }

    public class ImageData
    {
        private byte[] rawData;

        private ImageData(UInt16 width, UInt16 height, byte[] data)
        {
            Width = width;
            Height = height;
            rawData = data;
        }

        public UInt16 Width { get; private set; }
        public UInt16 Height { get; private set; }

        public static ImageData FromBitmap(Bitmap image)
        {
            var bits = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            System.Diagnostics.Debug.Assert(bits.Stride == image.Width * 4);
            System.Diagnostics.Debug.Assert(bits.Height == image.Height);

            var size = bits.Stride * bits.Height;
            byte[] s = new byte[size];
            Marshal.Copy(bits.Scan0, s, 0, size);

            image.UnlockBits(bits);

            return new ImageData((UInt16)image.Width, (UInt16)image.Height, s);
        }

        public static ImageData FromFile(string fname)
        {
            return FromBitmap(new Bitmap(fname));
        }

        public ImageData SubImage(Rectangle rect)
        {
            return SubImage((UInt16)rect.Left, (UInt16)rect.Top, (UInt16)rect.Width, (UInt16)rect.Height);
        }

        public ImageData SubImage(int x, int y, UInt16 width, UInt16 height)
        {
            var size = width * height * 4;
            byte[] s = new byte[size];

            for (int i = y; i < y + height; i++)
            {
                Array.Copy(this.rawData, (i * this.Width + x) * 4, s, (i - y) * width * 4, width * 4);
            }

            return new ImageData(width, height, s);
        }

        public int Match(ImageData[] targets, UInt32 threshold)
        {
            var min = int.MaxValue;
            var minIndex = -1;
            for (int i = 0; i < targets.Length; i++)
            {
                var s = Diff(targets[i]);
                var v = s.SumPixels();

                if (min > v)
                {
                    minIndex = i;
                    min = v;
                }
            }

            return min < threshold ? minIndex : -1;
        }

        public ImageData AlphaBlend(ImageData topLayer, ImageData alphaLayer)
        {
            System.Diagnostics.Debug.Assert(this.Width == alphaLayer.Width);
            System.Diagnostics.Debug.Assert(this.Height == alphaLayer.Height);

            var size = this.Width * this.Height * 4;
            byte[] s = new byte[size];

            for (int i = 0; i < this.rawData.Length; i++)
            {
                s[i] = (byte)((this.rawData[i] * (byte.MaxValue - alphaLayer.rawData[i]) + topLayer.rawData[i] * alphaLayer.rawData[i]) / byte.MaxValue);
            }

            return new ImageData(this.Width, this.Height, s);
        }

        public ImageData Diff(ImageData other)
        {
            System.Diagnostics.Debug.Assert(this.Width == other.Width);
            System.Diagnostics.Debug.Assert(this.Height == other.Height);

            var size = this.Width * this.Height * 4;
            byte[] s = new byte[size];

            for (int i = 0; i < this.rawData.Length; i++)
            {
                s[i] = (byte)Math.Abs(this.rawData[i] - other.rawData[i]);
            }

            return new ImageData(this.Width, this.Height, s);
        }

        public ImageData Diff(ImageData background, UInt32 threshold)
        {
            System.Diagnostics.Debug.Assert(this.Width == background.Width);
            System.Diagnostics.Debug.Assert(this.Height == background.Height);

            var size = this.Width * this.Height * 4;
            byte[] s = new byte[size];

            for (int i = 0; i < this.rawData.Length; i += 4)
            {
                var b = Math.Abs(this.rawData[i] - background.rawData[i]);
                var g = Math.Abs(this.rawData[i + 1] - background.rawData[i + 1]);
                var r = Math.Abs(this.rawData[i + 2] - background.rawData[i + 2]);

                if (b + g + r > threshold)
                {
                    s[i] = this.rawData[i];
                    s[i + 1] = this.rawData[i + 1];
                    s[i + 2] = this.rawData[i + 2];
                }
            }

            return new ImageData(this.Width, this.Height, s);
        }

        public ImageData GrayScale()
        {
            var size = this.Width * this.Height * 4;
            byte[] s = new byte[size];

            for (int i = 0; i < this.rawData.Length; i += 4)
            {
                var v = (byte)(this.rawData[i] * 0.2989 + 0.5870 * this.rawData[i + 1] + 0.1140 * this.rawData[i + 2]);
                s[i] = v;
                s[i + 1] = v;
                s[i + 2] = v;
            }

            return new ImageData(this.Width, this.Height, s);
        }

        public ImageData Threshold(byte r, byte g, byte b)
        {
            var size = this.Width * this.Height * 4;
            byte[] s = new byte[size];

            for (int i = 0; i < this.rawData.Length; i += 4)
            {
                s[i] = (byte)(this.rawData[i] > r ? 255 : 0);
                s[i + 1] = (byte)(this.rawData[i + 1] > g ? 255 : 0);
                s[i + 2] = (byte)(this.rawData[i + 2] > b ? 255 : 0);
            }

            return new ImageData(this.Width, this.Height, s);
        }

        public ImageData Mask(ImageData mask)
        {
            System.Diagnostics.Debug.Assert(this.Width == mask.Width);
            System.Diagnostics.Debug.Assert(this.Height == mask.Height);

            var size = this.Width * this.Height * 4;
            byte[] s = new byte[size];

            for (int i = 0; i < this.rawData.Length; i++)
            {
                s[i] = (mask.rawData[i] > 0) ? this.rawData[i] : (byte)0;
            }

            return new ImageData(this.Width, this.Height, s);
        }

        public void Dump()
        {
            for (int i = 0; i < this.Height; i++)
            {
                System.Diagnostics.Debug.Write(" ");
                for (int j = 0; j < this.Width; j++)
                {
                    System.Diagnostics.Debug.Write(string.Format("{0,3} ", this.rawData[(i * this.Width + j) * 4]));
                }
                System.Diagnostics.Debug.WriteLine("");
            }

            System.Diagnostics.Debug.WriteLine("");
            System.Diagnostics.Debug.WriteLine("");
        }

        public void Foo(int d)
        {
            for (int i = 0; i < this.Height; i++)
            {
                int lastX = -1;
                for (int j = 0; j < this.Width; j++)
                {
                    if (this.rawData[(i * this.Width + j) * 4] == 255)
                    {
                        if (lastX == -1 && j - lastX < d)
                        {
                            for (int k = lastX; k < j; k++)
                            {
                                this.rawData[(i * this.Width + k) * 4 + 1] = 255;
                                this.rawData[(i * this.Width + k) * 4 + 2] = 255;
                                this.rawData[(i * this.Width + k) * 4 + 2] = 255;
                            }
                        }
                        lastX = j;
                    }
                }
            }
        }

        public Bitmap GetImage()
        {
            Bitmap image = new Bitmap(this.Width, this.Height);

            var bits = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            System.Diagnostics.Debug.Assert(bits.Stride == image.Width * 4);
            System.Diagnostics.Debug.Assert(bits.Height == image.Height);

            var size = bits.Stride * bits.Height;
            Marshal.Copy(this.rawData, 0, bits.Scan0, size);

            image.UnlockBits(bits);
            return image;
        }

        public (int r, int g, int b) SumColors()
        {
            int r = 0;
            int g = 0;
            int b = 0;

            for (int i = 0; i < this.rawData.Length; i += 4)
            {
                b += this.rawData[i];
                g += this.rawData[i + 1];
                r += this.rawData[i + 2];
            }
            return (r, g, b);
        }

        public int SumPixels()
        {
            var (r, g, b) = SumColors();
            return r + g + b;
        }

        public bool ClassifyColor(Dictionary<Color, (int r, int g, int b)> categories, int threshold, out Color color)
        {
            var (rr, gg, bb) = SumColors();
            var t = rr + gg + bb;

            rr = rr * 100 / t;
            gg = gg * 100 / t;
            bb = bb * 100 / t;

            var result = categories.Select(x => (x.Key, Math.Abs(rr - x.Value.r) + Math.Abs(gg - x.Value.g) + Math.Abs(bb - x.Value.b))).OrderBy(x => x.Item2);
            var sum = result.Sum(x => x.Item2);

            if (result.First().Item2 * 100 / sum < threshold)
            {
                color = result.First().Item1;
                return true;
            }
            else
            {
                color = Color.Empty;
                return false;
            }
        }
    }

    public class Level
    {
        struct LevelInfo
        {
            public string Name;
            public string[] CurveFiles;
            public Point3D FrogXY;

            public LevelInfo(string name, string[] curveFiles, Point3D frogXY)
            {
                Name = name;
                CurveFiles = curveFiles;
                FrogXY = frogXY;
            }
        }

        static readonly Dictionary<string, LevelInfo> knownLevels = new Dictionary<string, LevelInfo>()
        {
            {"blackswirley", new LevelInfo("blackswirley", new string[] {"BlackSwirley-1", "BlackSwirley-2"}, new Point3D(320, 240)) },
            {"claw",        new LevelInfo("claw", new string[] {"claw"}, new Point3D(333, 235)) },
            {"coaster",     new LevelInfo("coaster", new string[] {"coaster"}, new Point3D(325, 240)) },
            {"groovefest",  new LevelInfo("groovefest", new string[] {"Groovefest"}, new Point3D(227, 236)) },
            {"inversespiral", new LevelInfo("inversespiral", new string[] {"inversespiral"}, new Point3D(424, 263)) },
            {"longrange", new LevelInfo("longrange", new string[] {"longrange"}, new Point3D(450, 240)) },
            {"loopy", new LevelInfo("loopy", new string[] {"loopy"}, new Point3D(325, 114)) },
            {"overunder", new LevelInfo("overunder", new string[] {"overunder"}, new Point3D(307, 278)) },
            {"riverbed", new LevelInfo("riverbed", new string[] {"riverbed"}, new Point3D(274, 264)) },
            {"serpents", new LevelInfo("serpents", new string[] {"serpents-1", "serpents-2"}, new Point3D(339, 271)) },
            {"snakepit", new LevelInfo("snakepit", new string[] {"snakepit-1", "snakepit-2"}, new Point3D(318, 240)) },
            {"space", new LevelInfo("space", new string[] {"space"}, new Point3D(246, 188)) },
            {"spaceinvaders", new LevelInfo("spaceinvaders", new string[] {"spaceinvaders"}, new Point3D(271, 358)) },
            {"spiral", new LevelInfo("spiral", new string[] {"spiral"}, new Point3D(327, 233)) },
            {"squaresville", new LevelInfo("squaresville", new string[] {"squaresville"}, new Point3D(324, 284)) },
            {"targetglyph", new LevelInfo("targetglyph", new string[] {"targetglyph"}, new Point3D(369, 222)) },
            {"tiltspiral", new LevelInfo("tiltspiral", new string[] {"tiltspiral"}, new Point3D(242, 248)) },
            {"triangle", new LevelInfo("triangle", new string[] {"triangle"}, new Point3D(369, 277)) },
            {"tunnellevel", new LevelInfo("tunnellevel", new string[] {"tunnellevel"}, new Point3D(215, 240)) },
            {"turnaround", new LevelInfo("turnaround", new string[] {"turnaround"}, new Point3D(300, 230)) },
            {"underover", new LevelInfo("underover", new string[] {"underover"}, new Point3D(334, 247)) },
            {"warshak", new LevelInfo("warshak", new string[] {"warshak"}, new Point3D(305, 338)) },
        };

        static readonly string[][] levelMap = new string[][]
        {
            new string[] {"spiral", "claw", "riverbed", "targetglyph", "blackswirley"},
            new string[] {"tiltspiral", "underover", "warshak", "loopy", "snakepit"},
            new string[] {"triangle", "coaster", "squaresville", "tunnellevel", "serpents"},
            new string[] {"spiral", "claw", "riverbed", "targetglyph", "blackswirley", "turnaround"},
            new string[] {"tiltspiral", "underover", "warshak", "loopy", "snakepit", "groovefest"},
            new string[] {"triangle", "coaster", "squaresville", "tunnellevel", "serpents", "overunder"},
            new string[] {"spiral", "claw", "riverbed", "targetglyph", "blackswirley", "turnaround", "longrange"},
            new string[] {"tiltspiral", "underover", "warshak", "loopy", "snakepit", "groovefest", "spaceinvaders"},
            new string[] {"triangle", "coaster", "squaresville", "tunnellevel", "serpents", "overunder", "inversespiral"},
            new string[] {"spiral", "claw", "riverbed", "targetglyph", "blackswirley", "turnaround", "longrange"},
            new string[] {"tiltspiral", "underover", "warshak", "loopy", "snakepit", "groovefest", "spaceinvaders"},
            new string[] {"triangle", "coaster", "squaresville", "tunnellevel", "serpents", "overunder", "inversespiral"},
        };

        static readonly string zumaDir = "D:\\Games\\Zuma";

        static readonly ImageData menuImage = ImageData.FromFile(Path.Combine(zumaDir, "images\\Menubar.jpg"));
        static readonly ImageData menuMask = ImageData.FromFile(Path.Combine(zumaDir, "images\\_Menubar.gif"));

        static Dictionary<string, Level> allLevels = new Dictionary<string, Level>();

        private Level(string id)
        {
            var v = id.Split('-');
            var stage = int.Parse(v[0]);
            var subStage = int.Parse(v[1]);

            var name = levelMap[stage - 1][subStage - 1];

            var bkgnd = ImageData.FromFile(Path.Combine(zumaDir, "levels", name, name + ".jpg"));
            Background = bkgnd.AlphaBlend(menuImage, menuMask);
            Curves = knownLevels[name].CurveFiles.Select(x => new Curve(Path.Combine(zumaDir, "levels", name, x + ".dat"))).ToArray();
            FrogXY = knownLevels[name].FrogXY;
        }

        public ImageData Background;
        public Curve[] Curves;
        public Point3D FrogXY;

        public static Level FromId(string id)
        {
            Level lvl;
            if (!allLevels.TryGetValue(id, out lvl))
            {
                try
                {
                    lvl = new Level(id);
                }
                catch (Exception)
                {
                    lvl = null;
                }

                allLevels.Add(id, lvl);
            }

            return lvl;
        }
    }

    public class Target
    {
        public Point3D XY;
        public Int32 Angle;
        public Int32 Span;
        public Int32 Score;

        public void Draw(Graphics g)
        {
            g.FillEllipse(new SolidBrush(Color.Black), XY.X - 10, XY.Y - 10, 20, 20);
        }
    }

    public class Track
    {
        GameInfo _info;
        Curve _curve;
        Frog _frog;
        Ball[] _balls;

        public IEnumerable<Ball> Balls { get { return _balls; } }

        private Color DetectBall(ImageData screen, int pos)
        {
            Dictionary<Color, ColorRange> ranges = new Dictionary<Color, ColorRange>()
            {
                {Color.Blue,    new ColorRange(32, 30,   25, 14,   35, 27,   53, 40)},
                {Color.Green,   new ColorRange(25, 23,   26, 20,   54, 48,   28, 23)},
                {Color.Red,     new ColorRange(24, 22,   69, 48,   25, 14,   36, 15)},
                {Color.Yellow,  new ColorRange(32, 31,   49, 45,   45, 39,   15,  6)},
                {Color.Purple,  new ColorRange(35, 34,   43, 35,   25, 17,   41, 35)},
                {Color.White,   new ColorRange(38, 34,   39, 33,   36, 30,   35, 27)},
            };

            var pt = _curve[pos];
            var s = screen.SubImage((UInt16)(pt.X - 16), (UInt16)(pt.Y - 16), 32, 32).Mask(_info.BallMask);

            var (r, g, b) = s.SumColors();
            var t = r + g + b;

            if (t > 200000)
            {
                r = r * 100 / t;
                g = g * 100 / t;
                b = b * 100 / t;

                t /= 10000;
                foreach (var x in ranges)
                {
                    if (x.Value.Satisfy(t, r, g, b))
                    {
                        //System.Diagnostics.Debug.WriteLine("pos = {0}, XY = ({1}, {2}), t = {3}, r = {4}, g = {5}, b = {6}, {7}", pos, pt.X, pt.Y, t, r, g, b, x.Key);
                        return x.Key;
                    }
                }
                System.Diagnostics.Debug.WriteLine("pos = {0}, XY = ({1}, {2}), t = {3}, r = {4}, g = {5}, b = {6}, Unknown", pos, pt.X, pt.Y, t, r, g, b);
            }

            return Color.Black;
        }

        public Track(GameInfo info, Curve c, Frog f)
        {
            _info = info;
            _curve = c;
            _frog = f;
        }

        public void Process(ImageData screen)
        {
            var v1 = _curve.Points.Select(pt =>
            {
                if (pt.X < 16 || pt.Y < 16 || pt.X > 640 - 16 || pt.Y > 480 - 16)
                {
                    return 0;
                }
                else
                {
                    var ss = screen.SubImage((UInt16)(pt.X - 16), (UInt16)(pt.Y - 16), 32, 32).Mask(_info.BallMask).Threshold(4, 4, 4);
                    return (int)ss.SumPixels() / 3;
                }
            }).ToArray();

            var v2 = new int[v1.Length];
            var sum = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                sum += v1[i];
                if (i >= 11)
                {
                    sum -= v1[i - 11];
                    v2[i - 5] = sum / 11;
                }
                else
                {
                    v2[i] = v1[i];
                }
            }

            var result = new List<Ball>();
            var pos = 50;

            while (pos < v2.Length - 100)
            {
                if (v2[pos] < 1000)
                {
                    pos++;
                    continue;
                }

                if ((v2[pos] < v2[pos - 1] || v2[pos] < v2[pos + 1]))
                {
                    pos++;
                    continue;
                }

                var color = DetectBall(screen, pos);

                if (color == Color.Black)
                {
                    pos++;
                    continue;
                }

                result.Add(new Ball(_curve, pos, color, _frog.XY));
                pos += 24;
            }

            _balls = result.ToArray();
        }

        class VisibleSequence
        {
            ColorSequence _parent;
            List<Ball> _balls;

            public int Length
            {
                get { return _balls.Count; }
            }

            public int RunLength
            {
                get { return _parent.Length; }
            }

            public Color Color
            {
                get { return _parent.Color; }
            }

            public VisibleSequence(ColorSequence cs)
            {
                _parent = cs;
                _balls = new List<Ball>();
            }

            public void Add(Ball b)
            {
                _balls.Add(b);
            }

            public Target GetTarget()
            {
                Ball b = null;
                if (this.Length == this.RunLength)
                {
                    b = _balls[0];
                }
                else
                {
                    b = _balls[Length / 2];
                }

                return new Target() { Angle = b.Angle, Score = RunLength * Length, Span = 0, XY = b.XY };
            }
        }

        class ColorSequence
        {
            List<Ball> _balls;

            public Color Color
            {
                get { return _balls[0].Color; }
            }

            public int Length
            {
                get { return _balls.Count; }
            }

            public int Score
            {
                get;
                private set;
            }

            public ColorSequence(Ball b)
            {
                _balls = new List<Ball>();
                _balls.Add(b);
            }

            public bool Append(Ball b)
            {
                if (b.Color == Color)
                {
                    _balls.Add(b);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public VisibleSequence[] GetVisibleSequences()
            {
                var vss = new List<VisibleSequence>();
                VisibleSequence vs = null;

                foreach (var b in _balls)
                {
                    if (!b.Visible)
                    {
                        if (vs != null)
                        {
                            vss.Add(vs);
                            vs = null;
                        }
                        continue;
                    }

                    if (vs == null)
                    {
                        vs = new VisibleSequence(this);
                    }

                    vs.Add(b);
                }

                if (vs != null)
                {
                    vss.Add(vs);
                }

                return vss.ToArray();
            }
        }

        public IEnumerable<Target> FindTargets(Color color)
        {
            var cs = new List<ColorSequence>();
            if (_balls.Length > 0)
            {
                // all color sequence
                var s = new ColorSequence(_balls[0]);
                foreach (var b in _balls.Skip(1))
                {
                    if (!s.Append(b))
                    {
                        cs.Add(s);
                        s = new ColorSequence(b);
                    }
                }
                cs.Add(s);
            }

            return cs.Where(x => x.Color == color).SelectMany(x => x.GetVisibleSequences()).Select(x => x.GetTarget());
        }

        public void Draw(Graphics g)
        {
            foreach (var b in _balls)
            {
                b.Draw(g);
            }
        }
    }

    public class Player
    {
        private GameInfo _info;

        private void UpdateVisibility(IEnumerable<Ball> all)
        {
            var sorted = all.OrderBy(x => x.Distance).ToArray();
            for (var i = 0; i < sorted.Length; i++)
            {
                var target = sorted[i];
                if (target.XY.Z == 1)
                {
                    target.Visible = false;
                    continue;
                }

                target.Visible = true;
                for (var j = 0; j < i; j++)
                {
                    var x = sorted[j];

                    if (x.XY.Z == 1)
                    {
                        continue;
                    }

                    if (target.Distance - x.Distance < 16)
                    {
                        continue;
                    }

                    var aDist = Math.Abs(x.Angle - target.Angle);
                    if (aDist > 1800)
                    {
                        aDist = 3600 - aDist;
                    }

                    if (aDist > (x.AngleSpan + target.AngleSpan) / 2)
                    {
                        continue;
                    }

                    target.Visible = false;
                    break;
                }
            }
        }

        private Dictionary<string, ImageData> LoadSubImages(string folder, Byte threshold)
        {
            Directory.CreateDirectory(folder);

            var result = new Dictionary<string, ImageData>();
            foreach (var f in Directory.GetFiles(folder, "*.jpg", SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileNameWithoutExtension(f).ToLower();
                result[name] = ImageData.FromFile(f).GrayScale().Threshold(threshold, threshold, threshold);
            }

            return result;
        }

        private string MatchImage(ImageData target, Dictionary<string, ImageData> candidates, Byte threshold1, UInt32 threshold2, string folder)
        {
            var ll = candidates.ToArray();
            var tt = target.GrayScale().Threshold(threshold1, threshold1, threshold1);

            var idx = tt.Match(ll.Select(x => x.Value).ToArray(), threshold2);
            if (idx != -1)
            {
                return ll[idx].Key;
            }
            else
            {
                var name = string.Format("unk_{0}", ll.Length);
                var fname = Path.Combine(folder, name + ".jpg");
                target.GetImage().Save(fname);
                candidates.Add(name, tt);
                return name;
            }
        }

        private Target[] Process(Bitmap screen)
        {
            var s = ImageData.FromBitmap(screen);

            var levelIdImages = LoadSubImages("level", 32);
            var lifeCountImages = LoadSubImages("life", 32);

            var levelIdImage = s.SubImage(GameInfo.LevelRect);
            LevelId = MatchImage(levelIdImage, levelIdImages, 32, 5000, "level");

            var lifeCountImage = s.SubImage(GameInfo.LifeRect);
            LifeCount = MatchImage(lifeCountImage, lifeCountImages, 32, 5000, "life");

            var state = s.GetImage();
            var targets = new Target[2];

            var level = Level.FromId(LevelId);
            if (level == null)
            {
                State = state;
                return targets;
            }

            var f = new Frog(level.FrogXY);
            f.Process(s);

            if (!f.Ready)
            {
                State = state;
                return targets;
            }

            var r = s.Diff(level.Background, 80);

            Ball.LastID = 0;
            var tracks = new List<Track>();

            foreach (var c in level.Curves)
            {
                var t = new Track(_info, c, f);
                t.Process(r);
                tracks.Add(t);
            }

            UpdateVisibility(tracks.SelectMany(x => x.Balls));

            if (f.First == f.Next)
            {
                var rs = tracks.SelectMany(x => x.FindTargets(f.First)).OrderByDescending(x => x.Score).Take(2).ToArray();

                if (rs.Length > 0)
                {
                    targets[0] = rs[0];
                    if (rs.Length > 1)
                    {
                        targets[1] = rs[1];
                    }
                }
            }
            else
            {
                targets[0] = tracks.SelectMany(x => x.FindTargets(f.First)).OrderByDescending(x => x.Score).FirstOrDefault();
                targets[1] = tracks.SelectMany(x => x.FindTargets(f.Next)).OrderByDescending(x => x.Score).FirstOrDefault();
            }

            state = r.GetImage();
            using (var g = Graphics.FromImage(state))
            {
                f.Draw(g);

                foreach (var t in tracks)
                {
                    t.Draw(g);
                }

                foreach (var t in targets)
                {
                    if (t != null)
                    {
                        t.Draw(g);
                    }
                }
            }

            State = state;

            return targets;
        }

        public Player(GameInfo info)
        {
            _info = info;
        }

        public string LevelId
        {
            get;
            private set;
        }

        public string LifeCount
        {
            get;
            private set;
        }

        public Bitmap State
        {
            get;
            private set;
        }


        public void Play(PictureBox result)
        {
            if (Game.Active)
            {
                var targets = Process(Game.Screenshot);

                result.Image = this.State;
                result.Refresh();

                if (targets[0] != null)
                {
                    Game.Shoot(targets[0].XY.X, targets[0].XY.Y);

                    if (targets[1] != null)
                    {
                        Game.Shoot(targets[1].XY.X, targets[1].XY.Y);
                    }
                }
                else
                {
                    if (targets[1] != null)
                    {
                        Game.Swap();
                        Game.Shoot(targets[1].XY.X, targets[1].XY.Y);
                    }
                }
            }
        }

        public void Simulate(PictureBox result, Bitmap screenshot)
        {
            var targets = Process(screenshot);
            result.Image = this.State;
            result.Refresh();
        }
    }
}
