using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics;
using SignalProcessor;
namespace ShowWave
{
    public partial class Form1 : Form
    {
        int bottom = 20, top = 15, left = 35, right = 15;
        int scale = 1;
        int MainFrequency = 25;
        double[] time,wavelet;
        double t_start, t_end, t_sample;
        double curxmin=0, curxmax=1, curymin=-1, curymax=1;
        Font fnt_tick = new Font("Times New Roman", 18.0f);
        Pen Pen_wave = new Pen(Color.Red,2);
        public List<Reflectivity> reflect = new   List<Reflectivity>();
        public Reflectivity reflect_select = new  Reflectivity(1,1);
        int Index_reflect_select;
        bool IsSelect = false;
        bool IsMultSelect = false;
        List<Reflectivity> List_multSelect = new List<Reflectivity>();
        List<Reflectivity> List_Last_multSelect = new List<Reflectivity>();
        Point ValuedlgLoc;
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            //Console.WriteLine(e.ClipRectangle);
            //pictureBox1.Invalidate();
            DrawAxis(e.Graphics);
            
        }

        public Form1()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
            Init();
        }

        private void Init()
        {
            t_start = 0;
            t_end = 1;
            t_sample = 0.002F;
            int samples = (int)Math.Ceiling((t_end - t_start) / 0.002);
            time = new double[samples];
            int len_wavelet = 120;
            wavelet = new double[len_wavelet];
            reflect_select.IsSelected = false;
            unsafe
            {
                //记录的时间序列
                fixed(double* p_time = time)
                {
                    for (int i = 0; i < samples; i++)
                        p_time[i] = t_start + i * t_sample;
                }
                //子波的时间序列
                fixed(double* p_wavelet = wavelet)
                {            //Ricker = lambda t,f:(1-2*np.pi**2*f**2*t**2)*np.exp(-2*np.pi**2*f**2*t**2)
                    double[] t_wave = Generate.LinearSpaced(len_wavelet,-len_wavelet/2*0.002,len_wavelet/2*0.002);
                    for (int i = 0; i < len_wavelet; i++)
                        p_wavelet[i] = System.Convert.ToSingle((1 - 2 * Math.Pow(Math.PI * MainFrequency * t_wave[i], 2))*
                            Math.Exp(-2*Math.Pow(Math.PI * MainFrequency * t_wave[i], 2)));
                }
            }
        }
        private void DrawAxis(Graphics g)
        {
            Bitmap bt = new Bitmap(pictureBox1.Width*scale, pictureBox1.Height*scale);
            Graphics bg = Graphics.FromImage(bt);
            DrawWave(bg);
            //x axis
            bg.DrawLine(Pens.Black, left*scale, (pictureBox1.Height - bottom) * scale, (pictureBox1.Width - right) * scale, (pictureBox1.Height - bottom) * scale);
            //y axis
            bg.DrawLine(Pens.Black, left * scale, (pictureBox1.Height - bottom) * scale, left * scale, top * scale);
            //x arrow
            Point[] xarrow ={new Point((pictureBox1.Width - right)*scale, (pictureBox1.Height - bottom - 5)*scale),
                new Point((pictureBox1.Width - right)*scale, (pictureBox1.Height - bottom + 5)*scale),
                new Point((pictureBox1.Width - right + 5)*scale, (pictureBox1.Height - bottom)*scale)};
            //y arrow
            Point[] yarrow ={new Point(left*scale, (top-5)*scale),
                new Point((left-5)*scale, top*scale),
                new Point((left+5)*scale, top*scale)};
            Drawtick(bg,curxmin, curxmax, curymin, curymax);
            bg.FillPolygon(Brushes.Black, yarrow);
            bg.FillPolygon(Brushes.Black, xarrow);
            DrawReflct(bg, curxmin, curxmax, curymin, curymax);
            
            g.DrawImage(bt,pictureBox1.DisplayRectangle, 0,0, pictureBox1.Width * scale, pictureBox1.Height * scale,GraphicsUnit.Pixel);
            bt.Dispose();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IsSelect)
            {
                reflect.Remove(reflect_select);
                IsSelect = false;
            }
            else
                reflect.Clear();
            pictureBox1.Refresh();
        }
        private Reflectivity Last = null;

        private void setValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            if (IsSelect)
            {
                myDialog mydlg = new myDialog(UpdateReflect);
                Last = reflect_select;
                mydlg.SetValue(reflect_select.x, reflect_select.strength);
                Point pt = PointToScreen(ValuedlgLoc);
                mydlg.Location = new Point(pt.X,pt.Y);
                mydlg.Show();
                IsSelect = false;
            }
        }
        //只选中一个反射系数 量，Isupdate同步更新，上一次更新要删除
        public void UpdateReflect(Reflectivity select,bool IsUpdate)
        {
            if (!IsUpdate)
            {
                reflect.Remove(reflect_select);
                reflect.Add(new Reflectivity(select.x, select.strength));
                pictureBox1.Refresh();
            }
            else
            {
                foreach(Reflectivity i in reflect)
                {
                    if (i.IsSelected) { i.x = select.x; i.strength = select.strength; }                        
                }
                pictureBox1.Refresh();
            }
        }

        //同时选中多个反射系数更新时
        public void UpdateReflectMult(List<Reflectivity> selects, bool IsUpdate)
        {
            //更新反射系数表
            if (!IsUpdate)
            {
                foreach (Reflectivity i in List_multSelect)
                {
                    reflect.Remove(i);
                }
                foreach (Reflectivity i in selects)
                {
                    i.IsSelected = false;
                    reflect.Add(new Reflectivity(i.x, i.strength));
                }
                List_multSelect.Clear();
                pictureBox1.Refresh();
            }
            else
            {
                int k = 0;
                    foreach (Reflectivity i in reflect)
                    {
                        if (i.IsSelected)
                        {
                            i.x = selects[k].x; i.strength = selects[k].strength;
                            k++;

                        }

                    }
                pictureBox1.Refresh();
            }
        }

        //打开多项移动窗口
        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IsMultSelect)
            {
                myDialog2 mydlg = new myDialog2(List_multSelect,UpdateReflectMult);
                
                Point pt = PointToScreen(ValuedlgLoc);
                mydlg.Location = new Point(pt.X, pt.Y);
                mydlg.Show();
                IsMultSelect = false;

            }
        }

        private void Drawtick(Graphics g, double xmin = -1, double xmax = 1, double ymin = -1, double ymax = 1)
        {
            GraphicsPath blackfont = new GraphicsPath();

            g.SmoothingMode = SmoothingMode.HighQuality;
            double xlim = xmax - xmin;
            double ylim = ymax - ymin;
            int nSpan = 10;
            double xspan = CalRationalNum(xlim / nSpan);//判断span必须为有效位数可数的有理数
            double yspan = CalRationalNum(ylim / nSpan);//判断span必须为有效位数可数的有理数
            for (int i = 0; i < nSpan+1; i++)
            {
                //画x轴的Tick
                g.DrawLine(Pens.Black,((pictureBox1.Width-left-right)*i/nSpan+left) * scale,
                    (pictureBox1.Height-bottom-5) * scale,
                    ((pictureBox1.Width - left - right)*i / nSpan + left) * scale,
                    (pictureBox1.Height - bottom) * scale
                    );
                //画xtick
                double x_f_tick = Math.Round(xmin + xspan * i,2);
                if (Math.Abs(x_f_tick / xspan) < 1e-6) x_f_tick = 0;
                SizeF sz = g.MeasureString(x_f_tick.ToString(), fnt_tick);
                blackfont.AddString(x_f_tick.ToString(), fnt_tick.FontFamily, (int)fnt_tick.Style, fnt_tick.Size,
                    new PointF(((pictureBox1.Width - left - right) * i / nSpan + left - sz.Width / 3) * scale,
                    (pictureBox1.Height - bottom) * scale), StringFormat.GenericDefault);
                //g.DrawString(x_f_tick.ToString(), fnt_tick,Brushes.Black, ((pictureBox1.Width - left - right)*i / nSpan  + left-sz.Width/2) * scale, (pictureBox1.Height - bottom )*scale);
                //现y tick
                g.DrawLine(Pens.Black, left * scale, 
                    -(pictureBox1.Height - top - bottom)*i / nSpan   * scale + pictureBox1.Height * scale - bottom * scale ,
                     (left+5) * scale,
                    -(pictureBox1.Height - top - bottom)*i / nSpan  * scale + pictureBox1.Height * scale - bottom * scale
                     );
                double y_f_tick = Math.Round(ymin + yspan * i,2);
                if (Math.Abs(y_f_tick / yspan) < 1e-6) y_f_tick = 0;
                SizeF ysz = g.MeasureString(y_f_tick.ToString(), fnt_tick);
                blackfont.AddString(y_f_tick.ToString(), fnt_tick.FontFamily, (int)fnt_tick.Style, fnt_tick.Size,
                new PointF(0,-(pictureBox1.Height - top - bottom) * i / nSpan * scale + pictureBox1.Height * scale - bottom * scale - 1*ysz.Height / 3 * scale), 
                StringFormat.GenericDefault);

                //g.DrawString(y_f_tick.ToString(), fnt_tick, Brushes.Black, 0, -(pictureBox1.Height - top - bottom)*i / nSpan  * scale + pictureBox1.Height * scale - bottom * scale - ysz.Height/2 * scale);

            }
            g.FillPath(Brushes.Black, blackfont);    //Fill the font with White brush

        }

        private void DrawWave(Graphics g)
        {
            //Ricker = lambda t,f:(1-2*np.pi**2*f**2*t**2)*np.exp(-2*np.pi**2*f**2*t**2)
            reflect.Sort(CompareDinosByLength);
            g.SmoothingMode = SmoothingMode.HighQuality;  
            unsafe
            {
                double[] vector = new double[time.Length];
                for(int i =0; i < time.Length; i++)
                {
                    vector[i] = 0;
                }
                foreach(Reflectivity i in reflect)
                {
                    vector[(int)Math.Floor((i.x - curxmin) / 0.002)] += i.strength;
                }
                List<double> list_reflect = vector.OfType<double>().ToList();
                List<double> list_wavelet = wavelet.OfType<double>().ToList();
                DSP.Convolution conv = new DSP.Convolution();
                List<double> conv_result = conv.Convolve(list_reflect,list_wavelet,DSP.ConvolutionType.INPUTSIDE);
                conv_result.RemoveRange(0,list_wavelet.Count/2);
                if (conv_result.Min() < conv_result.Max())
                {
                    curymin = Math.Round( conv_result.Min(),1);
                    curymax = Math.Round(Math.Abs( conv_result.Max()),1)*Math.Sign(conv_result.Max());
                }
                PointF[] points = new PointF[time.Length];
                for(int i = 0; i < time.Length; i++)
                {
                    points[i] = Loc2Screen(time[i], conv_result[i], curxmin, curxmax, curymin, curymax);
                }
                g.DrawCurve(Pen_wave, points);
            }
        }


        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                double x = (double)(e.Location.X - left) / (pictureBox1.Width - left - right) * (curxmax - curxmin) + curxmin;
                x = Math.Round(Math.Floor( x / t_sample),3) * t_sample;
                double y = -(double)(e.Location.Y - pictureBox1.Height + bottom) / (pictureBox1.Height - top - bottom) * (curymax - curymin) + curymin;
                foreach (Reflectivity i in reflect)
                {
                    if (Math.Abs(i.x - x) < 0.002)
                        if (Math.Sign(i.strength) == -1*Math.Sign(y))
                        {
                            reflect.Remove(i);
                            reflect.Add(new Reflectivity(x, 0));
                            pictureBox1.Refresh();
                            return;
                        }
                    else if(Math.Sign(i.strength) ==  Math.Sign(y))
                        {
                            return;
                        }
                }
                if (y >= 0)
                    reflect.Add(new Reflectivity(x, 1));
                else reflect.Add(new Reflectivity(x, -1));
                pictureBox1.Invalidate();
                pictureBox1.Update();
            }
        }

        //选择反射系数 
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {                        
            foreach (Reflectivity i in reflect)
            {
                Rectangle rct = new Rectangle((int)Loc2Screen(i.x,i.strength).X-2, 
                    (int)Math.Ceiling(Loc2Screen(i.x, i.strength).Y),
                    2, 
                    (int)Math.Ceiling(Loc2Screen(i.x, 0).Y- (int)Math.Ceiling(Loc2Screen(i.x, i.strength).Y)));
                //判断点击点的位置附近 是否有反射点
                if (rct.Contains(e.Location))
                {
                    Bitmap bt = new Bitmap(rct.Width,rct.Height);
                    Graphics bg = Graphics.FromImage(bt);
                    //画强调圆
                    //bg.DrawEllipse(Pens.Black, (int)Loc2Screen(i.x, i.strength).X - 2,
                    //    (int)Math.Ceiling(Loc2Screen(i.x, i.strength).Y)+2,
                        //2,2);
                    Pen blackPen = new Pen(Color.Black, 3);
                    //画线
                    bg.DrawLine(blackPen, 0,0,0,rct.Height);
                    bg.DrawEllipse(blackPen, 0,0,2, 2);
                    pictureBox1.CreateGraphics().DrawImage(bt,rct.X,rct.Y);
                    //bg.Dispose();
                    //bt.Dispose();
                    break;
                }
            }
            if (e.Button == MouseButtons.Right)
            {
                if (!IsMultSelect)
                {
                    int count = 0;
                    foreach (Reflectivity i in reflect)
                    {

                        double y = Loc2Screen(i.x, i.strength, curxmin, curxmax, curymin, curymax).Y;
                        double z = Loc2Screen(i.x, 0, curxmin, curxmax, curymin, curymax).Y;
                        double height, top;
                        if (y > z)
                        {
                            height = y - z;
                            top = (int)Math.Ceiling(z);
                        }
                        else
                        {
                            height = z - y;
                            top = (int)Math.Ceiling(y);
                        }

                        Rectangle rct = new Rectangle((int)Loc2Screen(i.x, i.strength, curxmin, curxmax, curymin, curymax).X - 5,
                            (int)top, 10, (int)height);
                        //判断点击点的位置附近 是否有反射点
                        if (rct.Contains(e.Location))
                        {
                            if(reflect_select.IsSelected )
                            {
                                Reflectivity find =  reflect.Find(x => (x.x - reflect_select.x)<=0.002);
                                find.IsSelected = false;

                            }
                            
                            i.IsSelected = true;
                            reflect_select = i;
                            IsSelect = true;
                            Index_reflect_select = count;

                            //Console.WriteLine(IsSelect+"_"+ DateTime.Now);
                            pictureBox1.CreateGraphics().DrawEllipse(Pens.Red, rct.X + 2, rct.Y -3, 6, 6);
                            break;
                        }
                        count += 1;
                    }
                    
                    contextMenuStrip2.Show(PointToScreen(e.Location));
                    ValuedlgLoc = e.Location;
                }
                else
                {
                    contextMenuStrip1.Show(PointToScreen(e.Location));
                    ValuedlgLoc = e.Location;

                }
            }
            if (e.Button == MouseButtons.Left && (System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                int count = 0;
                foreach (Reflectivity i in reflect)
                {
                    double y = Loc2Screen(i.x, i.strength, curxmin, curxmax, curymin, curymax).Y;
                    double z = Loc2Screen(i.x, 0, curxmin, curxmax, curymin, curymax).Y;
                    double height, top;
                    if (y > z)
                    {
                        height = y - z;
                        top = (int)Math.Ceiling(z);
                    }
                    else
                    {
                        height = z - y;
                        top = (int)Math.Ceiling(y);
                    }

                    Rectangle rct = new Rectangle((int)Loc2Screen(i.x, i.strength, curxmin, curxmax, curymin, curymax).X - 5,
                        (int)top, 20, 100);
                    //判断点击点的位置附近 是否有反射点
                    if (rct.Contains(e.Location))
                    {
                        //画强调圆
                        //bg.DrawEllipse(Pens.Black, (int)Loc2Screen(i.x, i.strength).X - 2,
                        //    (int)Math.Ceiling(Loc2Screen(i.x, i.strength).Y)+2,2,2);
                        pictureBox1.CreateGraphics().DrawEllipse(Pens.Red, rct.X + 2, rct.Y - 3, 6, 6);
                        i.IsSelected = true;
                        List_multSelect.Add(i);
                        IsMultSelect = true;
                        //Index_reflect_select = count;

                    }
                    count += 1;
                }

                ValuedlgLoc = e.Location;
            }
            if (e.Button == MouseButtons.Left && (System.Windows.Forms.Control.ModifierKeys & Keys.Control) != Keys.Control)
            {
                List_multSelect.Clear();
                pictureBox1.Invalidate();
            }

        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void DrawReflct(Graphics g, double xmin = -1, double xmax = 1, double ymin = -1, double ymax = 1)
        {
            g.DrawLine(Pen_wave, Loc2Screen(xmin,0,xmin,xmax,ymin,ymax), Loc2Screen(xmax, 0, xmin, xmax, ymin, ymax));
            g.SmoothingMode = SmoothingMode.HighSpeed;
            Pen scaledPen = new Pen(Brushes.Black, 2);
            foreach (Reflectivity i in reflect)
            {

                g.DrawLine(scaledPen, Loc2Screen(i.x, 0, xmin, xmax, ymin, ymax).X,
                    Loc2Screen(i.x, 0, xmin, xmax, ymin, ymax).Y,
                    Loc2Screen(i.x, 0, xmin, xmax, ymin, ymax).X,
                    Loc2Screen(i.x, i.strength, xmin, xmax, ymin, ymax).Y);
                //Console.WriteLine("loc"+Loc2Screen(i.x, 0, xmin, xmax, ymin, ymax).X + ',' + Loc2Screen(i.x, i.strength, xmin, xmax, ymin, ymax).X);
            }
            g.SmoothingMode = SmoothingMode.HighQuality;
        }
        //坐标值转换为窗口坐标
        private PointF Loc2Screen(double x, double y, double xmin = -1, double xmax = 1, double ymin = -1, double ymax = 1)
        {
            return new PointF(Convert.ToSingle((x - xmin) / (xmax - xmin) * (pictureBox1.Width - left - right) * scale + left * scale),
                Convert.ToSingle(-(y - ymin) / (ymax - ymin) * (pictureBox1.Height - top - bottom) * scale + pictureBox1.Height * scale - bottom * scale));
        }

        //计算间隔
        private double CalRationalNum(double value)
        {
            double tmp = 1;
            if (value > 1)
            {
                while (value>1)
                {
                    value /= 10;
                    tmp *= 10;
                }
                return (double)( tmp*value );
            }
            else
            {
                while(value < 1)
                {
                    value *= 10;
                    tmp /= 10;
                }
                return (double)(tmp * value);
            }
        }

        //反射序列的比较器
        private static int CompareDinosByLength(Reflectivity x, Reflectivity y)
        {
            if (x.x > y.x)
                return 1;
            else return -1;
        }
        public static void PrintPropreties(object obj)
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(obj);
                Console.WriteLine("{0}={1}", name, value);
            }
        }


    }
}

public class Reflectivity:Object
{
    public double x;
    public double strength;
    public bool IsSelected;
    public Reflectivity(double x1, double strength1)
    {
        x = x1;
        strength = strength1;
        IsSelected = false;
    }
}

