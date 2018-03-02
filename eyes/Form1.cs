using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using bearing;
using EyeDection;
using SomeCalibrations;

namespace eyes
{
    public partial class Form1 : Form
    {
        bool flowon = true;

        public Image<Gray, Byte> My_Image1;
        public Image<Bgr, Byte> My_Image2;
        Image<Bgr, Byte> Ori_Image;

        public Image<Bgr, Byte> facecut;
        public Image<Bgr, Byte> facecutori;
        Image<Bgr, Byte> eyecutleft;
        Image<Bgr, Byte> eyecutright;
        Image<Bgr, Byte> eyebrowimg;

        Image<Gray, Byte> eyecutleftgray;
        Image<Gray, Byte> eyecutrightgray;
        public Image<Gray, Byte> facecutorigray;

        Image<Bgr, Byte> mouthimg;

        Image<Gray, byte> binImg;
        public Bitmap camera;
        
        /// ////////////////////////////柏洧
        List<CircleF> fortestcyl = new List<CircleF>();
        List<CircleF> fortestcyr = new List<CircleF>();
        Dictionary<CircleF, double> lvalver = new Dictionary<CircleF, double>();
        Dictionary<CircleF, double> rvalver = new Dictionary<CircleF, double>();
        Dictionary<CircleF, double> lvalbla = new Dictionary<CircleF, double>();
        Dictionary<CircleF, double> rvalbla = new Dictionary<CircleF, double>();
        List<Image<Bgr, Byte>> forproshow = new List<Image<Bgr, Byte>>();
        List<string> leyeinfo = new List<string>();
        List<string> reyeinfo = new List<string>();
        int timer_counter;

        public Form1()
        {
            InitializeComponent();
        }

        public class MyCV
        {
            public static Bitmap BoundingBoxeyebrow(Image<Gray, Byte> gray, Image<Bgr, byte> draw)
            {
                // 使用 VectorOfVectorOfPoint 類別一次取得多個輪廓。
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    // 在這版本請使用FindContours，早期版本有cvFindContours等等，在這版都無法使用，
                    // 由於這邊是要取得最外層的輪廓，所以第三個參數給 null，第四個參數則用 RetrType.External。
                    CvInvoke.FindContours(gray, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                    Bitmap tempimg = new Bitmap(gray.Bitmap);
                    Bitmap tempcolorimg = new Bitmap(draw.Bitmap);

                    int left = 2147483647, right = 0, top = 2147483647, button = 0;
                    int lefttop = 2147483647, leftdown = 0, righttop = 2147483647, rightdown = 0;
                    int leftpoint = 0, rightpoint = 0, toppoint = 0, buttonpoint = 0, lefttoppoint = 0, leftdownpoint = 0, righttoppoint = 0, rightdownpoint = 0;
                    int count = contours.Size;

                    Console.WriteLine("contours.Size"+ contours.Size);

                    for (int i = 0; i < count; i++)
                    {
                        using (VectorOfPoint contour = contours[i])
                        {
                            
                            // 使用 BoundingRectangle 取得框選矩形
                            Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                            if ((BoundingBox.Width / BoundingBox.Height) > 1.5 && (BoundingBox.Width * BoundingBox.Height) > 1000 && (BoundingBox.Width * BoundingBox.Height) < 15000&& BoundingBox.Y<gray.Height/4)//過濾長寬比太小和面積太小的box
                            //CvInvoke.DrawContours(draw, contours,i, new MCvScalar(255, 0, 255, 255),2);
                            {
                                PointF[] temp = Array.ConvertAll(contour.ToArray(), new Converter<Point, PointF>(Point2PointF));
                                PointF[] pts = CvInvoke.ConvexHull(temp, true);
                                Point[] points = new Point[temp.Length];

                                Console.WriteLine("in");

                                for (int j = 0; j < temp.Length; j++)//找上下左右端點
                                {
                                    points[j] = Point.Round(temp[j]);//PointF2Point


                                    if (j > 1 && points[j].X < left)
                                    {
                                        left = points[j].X;
                                        leftpoint = j;
                                    }

                                    if (j > 1 && points[j].X > right)
                                    {
                                        right = points[j].X;
                                        rightpoint = j;
                                    }
                                    if (j > 1 && points[j].Y < top)
                                    {
                                        top = points[j].Y;
                                        toppoint = j;
                                    }
                                    if (j > 1 && points[j].Y > button)
                                    {
                                        button = points[j].Y;
                                        buttonpoint = j;
                                    }
                                }




                                for (int j = 0; j < temp.Length; j++)
                                {
                                    if (j > 1 && points[j].X == (points[leftpoint].X + points[toppoint].X) / 2 && points[j].Y < lefttop)
                                    {
                                        lefttop = points[j].Y;
                                        lefttoppoint = j;
                                    }

                                    if (j > 1 && points[j].X == (points[leftpoint].X + points[buttonpoint].X) / 2 && points[j].Y > leftdown)
                                    {
                                        leftdown = points[j].Y;
                                        leftdownpoint = j;
                                    }

                                    if (j > 1 && points[j].X == (points[rightpoint].X + points[toppoint].X) / 2 && points[j].Y < righttop)
                                    {
                                        righttop = points[j].Y;
                                        righttoppoint = j;
                                    }

                                    if (j > 1 && points[j].X == (points[rightpoint].X + points[buttonpoint].X) / 2 && points[j].Y > rightdown)
                                    {
                                        rightdown = points[j].Y;
                                        rightdownpoint = j;
                                    }
                                }

                                Point[] pointlefttopright = { points[leftpoint], points[lefttoppoint], points[toppoint], points[righttoppoint], points[rightpoint] };
                                Point[] pointleftbuttonright = { points[leftpoint], points[leftdownpoint], points[buttonpoint], points[rightdownpoint], points[rightpoint] };








                                Point[] subsampling = new Point[points.Length/15+1];
                                for (int j = 0; j < temp.Length; j=j+15)
                                {
                                    subsampling[j / 15] = points[j];
                                }
                                subsampling[points.Length / 15] = subsampling[0];
                                




                                Graphics g = Graphics.FromImage(tempimg);//畫上曲線
                                Pen pen = new Pen(Color.FromArgb(255, 255, 0, 0),3);
                                g.DrawCurve(pen, subsampling, 0.3f);
                                g.Dispose();

                                Graphics g1 = Graphics.FromImage(tempimg);//畫下曲線
                                Pen pen1 = new Pen(Color.Yellow, 4);
                                //g1.DrawCurve(pen1, pointleftbuttonright, 0.3f);
                                g1.Dispose();

                                Graphics g6 = Graphics.FromImage(tempcolorimg);//畫上曲線
                                g6.DrawCurve(pen, subsampling, 0.4f);
                                g6.Dispose();
                                
                                Graphics g7 = Graphics.FromImage(tempcolorimg);//畫下曲線
                                //g7.DrawCurve(pen1, pointleftbuttonright, 0.4f);
                                g7.Dispose();

                                StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
                                sf.Alignment = StringAlignment.Center;
                                sf.LineAlignment = StringAlignment.Center;

                                Graphics g2 = Graphics.FromImage(tempimg);//畫左點
                                SolidBrush drawBrush = new SolidBrush(Color.LightGreen);
                                g2.DrawString("+", new Font("Arial", 25), drawBrush, points[leftpoint].X, points[leftpoint].Y,sf);
                                g2.Dispose();

                                Graphics g3 = Graphics.FromImage(tempimg);//畫右點
                                g3.DrawString("+", new Font("Arial", 25), drawBrush, points[rightpoint].X, points[rightpoint].Y,sf);
                                g3.Dispose();
                                

                                Graphics g4 = Graphics.FromImage(tempcolorimg);//畫左點
                                g4.DrawString("+", new Font("Arial", 25), drawBrush, points[leftpoint].X, points[leftpoint].Y,sf);
                                g4.Dispose();

                                Graphics g5 = Graphics.FromImage(tempcolorimg);//畫右點
                                g5.DrawString("+", new Font("Arial", 25), drawBrush, points[rightpoint].X, points[rightpoint].Y,sf);
                                g5.Dispose();
                            }
                            

                        }
                        left = 2147483647; right = 0; top = 2147483647; button = 0; lefttop = 2147483647; leftdown = 0; righttop = 2147483647; rightdown = 0;
                        leftpoint = 0; rightpoint = 0; toppoint = 0; buttonpoint = 0; lefttoppoint = 0; leftdownpoint = 0; righttoppoint = 0; rightdownpoint = 0;
                    }

                    gray.Bitmap = tempimg;
                    gray.ROI = Rectangle.Empty;
                    draw.Bitmap = tempcolorimg;
                    draw.ROI = Rectangle.Empty;
                    return tempcolorimg;
                }

            }

            

            

            private static PointF Point2PointF(Point P)//Point轉PointF
            {
                PointF PF = new PointF
                {
                    X = P.X,
                    Y = P.Y
                };
                return PF;
            }
        }

        VideoCapture webCam;

        Rectangle[] faces;
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    My_Image1 = new Image<Gray, byte>(Openfile.FileName);
                    My_Image2 = new Image<Bgr, byte>(Openfile.FileName);

                    CascadeClassifier frontalface = new CascadeClassifier("haarcascade_frontalface_default.xml");
                    faces = frontalface.DetectMultiScale(My_Image1, 1.1, 10, new Size(20, 20),Size.Empty);
                    
                    

                    List<Rectangle> face = new List<Rectangle>();
                    face.AddRange(faces);
                    
                    foreach (Rectangle face1 in face)
                    {
                       // My_Image2.Draw(face1, new Bgr(Color.Red), 2);
                    }
                    
                    //眼睛
                    if (faces.Length != 0)
                    {
                        facecutori = new Image<Bgr, Byte>(My_Image2.Bitmap);
                        facecutori.ROI = faces[0];//切出臉的部分
                        facecutorigray = new Image<Gray, Byte>(My_Image1.Bitmap);
                        facecutorigray.ROI = faces[0];







                        int zoomface = 60;
                        for (int i = 0; i < faces.Length; i++)//調整臉範圍大小
                        {
                            faces[i].X = faces[i].X - zoomface;
                            faces[i].Y = faces[i].Y - zoomface * 2;
                            faces[i].Width = faces[i].Width + zoomface * 2;
                            faces[i].Height = faces[i].Height + zoomface * 4;
                        }
                        facecut = new Image<Bgr, Byte>(My_Image2.Bitmap);
                        facecut.ROI = faces[0];
                        imageBox1.Image = My_Image2;
                        //imageBox2.Image = facecut;


                    }
                    //My_Image2.ROI = faces[0];
                    //imageBox1.Image = My_Image2;

                    


                }
                catch (NullReferenceException excpt) { MessageBox.Show(excpt.Message); }
                


            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Form3 form3 = new Form3();
            form3.Tag = this;
            form3.TopMost = true;
            form3.ShowDialog();//鎖定

        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            if (imageBox1.Image != null)
            {

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Image Files(*.BMP)|*.bmp";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    webCam.QueryFrame().ToImage<Bgr, Byte>().Save(saveFileDialog.FileName);
                }
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)//find mouth
        {
            if (facecut != null)
            {
                
                Bitmap image1 = new Bitmap(facecut.Bitmap);
                int w = facecut.Width;
                int h = facecut.Height;

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        Color color = image1.GetPixel(x, y);
                        int R = color.R;
                        int G = color.G;
                        int B = color.B;
                        if (G != 0)
                        {
                            if ((R / G) >= 1.3 && (R / G) <= 2.0)
                            {
                                image1.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                            }
                            else { image1.SetPixel(x, y, Color.FromArgb(0, 0, 0)); }
                        }
                    }
                }
                imageBox2.Image = new Image<Bgr, Byte>(image1);


            }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)//HSV find mouth
        {
            int w = My_Image2.Bitmap.Width;
            int h = My_Image2.Bitmap.Height;
            Bitmap image1 = new Bitmap(My_Image2.Bitmap);
            Bitmap skin = new Bitmap(My_Image2.Bitmap);
            //for (int y = 0; y < h; y++)//find skin RGB
            //{
            //    for (int x = 0; x < w; x++)
            //    {
            //        Color color = image1.GetPixel(x, y);
            //        int R = color.R;
            //        int G = color.G;
            //        int B = color.B;

            //        if ((R > 95 && G > 40 && B > 20 && (R - B) > 15 && (R - G) > 15) || (R > 200 && G > 210 && B > 170 && (R - B) <= 15 && R > B && G > B))
            //        {
            //            skin.SetPixel(x, y, Color.FromArgb(255, 255, 255));
            //        }
            //        else { skin.SetPixel(x, y, Color.FromArgb(0, 0, 0)); }

            //    }
            //}

            for (int y = 0; y < h; y++)//find skin  YCbCr
            {
                for (int x = 0; x < w; x++)
                {
                    Color color = image1.GetPixel(x, y);
                    int R = color.R;
                    int G = color.G;
                    int B = color.B;
                    float hue = color.GetHue();
                    double Y = 0.257 * R + 0.564 * G + 0.098 * B + 16;
                    double Cb = -0.148 * R - 0.291 * G + 0.439 * B + 128;
                    double Cr = 0.439 * R - 0.368 * G - 0.071 * B + 128;
                    if (hue<=15)//另一個參數Cb>76&&Cb<127&&Cr>132&&Cr<173，Cb>101&&Cb<125&&Cr>130&&Cr<155
                    {
                        skin.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    }
                    else { skin.SetPixel(x, y, Color.FromArgb(0, 0, 0)); }

                }
            }
            Image<Bgr, byte> skinimg= new Image<Bgr, byte>(skin);
            //skinimg = skinimg.Erode(1);
            //skinimg = skinimg.Dilate(2);
            //skinimg = skinimg.Erode(2);
            imageBox1.Image = skinimg;
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)//眉毛
        {
            

            Image<Gray, byte> facecutgray = new Image<Gray, byte>(facecutori.Bitmap);
            facecutorigray.Bitmap = facecutorigray.SmoothMedian(9).Bitmap;
            //Image<Gray, byte> Out = new Image<Gray, byte>(facecutorigray.Size);
            CvInvoke.AdaptiveThreshold(facecutorigray, facecutorigray, 255, AdaptiveThresholdType.MeanC, ThresholdType.BinaryInv, 31, 2);

            //facecutorigray.ROI = Rectangle.Empty;
            Bitmap image1 = new Bitmap(facecutori.Bitmap);
            Bitmap skin = new Bitmap(facecutori.Bitmap);

            int w = facecutori.Bitmap.Width;
            int h = facecutori.Bitmap.Height;

            for (int y = 0; y < h; y++)//find skin
            {
                for (int x = 0; x < w; x++)
                {
                    Color color = image1.GetPixel(x, y);
                    int R = color.R;
                    int G = color.G;
                    int B = color.B;
                    float hue = color.GetHue();
                    float saturation = color.GetSaturation();
                    int A = color.A;

                    if ((R > 95 && G > 40 && B > 20 && (R - B) > 15 && (R - G) > 15) || (R > 200 && G > 210 && B > 170 && (R - B) <= 15 && R > B && G > B))
                    {
                        skin.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    }
                    else { skin.SetPixel(x, y, Color.FromArgb(255, 255, 255)); }

                }
            }

            //for (int y = 0; y < h; y++)//find skin  YCbCr
            //{
            //    for (int x = 0; x < w; x++)
            //    {
            //        Color color = image1.GetPixel(x, y);
            //        int R = color.R;
            //        int G = color.G;
            //        int B = color.B;
            //        double Y = 0.257 * R + 0.564 * G + 0.098 * B + 16;
            //        double Cb = -0.148 * R - 0.291 * G + 0.439 * B + 128;
            //        double Cr = 0.439 * R - 0.368 * G - 0.071 * B + 128;
            //        if (Cb > 85 && Cb < 135 && Cr > 135 && Cr < 180 && Y > 80)//另一個參數Cb>76&&Cb<127&&Cr>132&&Cr<173，Cb>101&&Cb<125&&Cr>130&&Cr<155
            //        {
            //            skin.SetPixel(x, y, Color.FromArgb(0, 0, 0));
            //        }
            //        else { skin.SetPixel(x, y, Color.FromArgb(255, 255, 255)); }

            //    }
            //}

            //Image<Gray, float> Outfloat = Out.Convert<Gray, float>();//二值化後的照片轉float
            Image<Gray, byte> faceskin = new Image<Gray, byte>(skin);
            facecutorigray.Bitmap = facecutorigray.And(faceskin).Bitmap;
            
            //Image<Gray, byte> skinsobelbyte = new Image<Gray, byte>(skinsobel.Bitmap);

            Image<Bgr, byte> DrawI = new Image<Bgr, byte>(facecutori.Bitmap);
            //skinsobelbyte = skinsobelbyte.Dilate(2);
            //MyCV.BoundingBox(skinsobelbyte, facecutori);

            //facecutori.ROI = Rectangle.Empty;
            MyCV.BoundingBoxeyebrow(facecutorigray, facecutori);
            //imageBox2.Image = new Image<Bgr, byte>(MyCV.BoundingBox(facecutorigray, facecutori));
            imageBox1.Image = facecutori;
            imageBox2.Image = facecutorigray;


        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)//拍照
        {
            Form3 form3 = new Form3();
            form3.Tag = this;
            form3.TopMost = true;
            form3.ShowDialog();//鎖定置頂
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)//寬高
        {

            Image<Bgr, byte> nose = My_Image2.Clone();
            Image<Gray, byte> nosegray = My_Image1.Clone();

            //nosegray.ROI = faces[0];

            nosegray = nosegray.ThresholdBinaryInv(new Gray(80), new Gray(255));
            Image<Bgr, byte> nosegraytoBgr = nosegray.Convert<Bgr, byte>();
            //faces[0].X+faces[0].Width/2,faces[0].Y+faces[0].Height/2 中心點座標
            Rectangle noserange = new Rectangle(faces[0].X + (faces[0].Width / 5) * 2, faces[0].Y + faces[0].Height / 2, faces[0].Width / 5, faces[0].Height / 6);//取鼻子範圍
            nose.ROI = noserange;
            nosegray.ROI = noserange;
            nosegraytoBgr.ROI = noserange;


            
            VectorOfVectorOfPoint contours1 = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(nosegray, contours1, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            int count1 = contours1.Size;


            Rectangle first = Rectangle.Empty, second = Rectangle.Empty;//最大跟第二大物件的座標
            for (int i = 0; i < count1; i++)//找最大
            {
                using (VectorOfPoint contour = contours1[i])
                {

                    Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                    if (i == 0) { first = BoundingBox; }
                    else if (BoundingBox.Width * BoundingBox.Height > first.Width * first.Height) { first = BoundingBox; }
                }
            }

            for (int i = 0; i < count1; i++)//找第二大
            {
                using (VectorOfPoint contour = contours1[i])
                {

                    Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                    if (i == 0 && BoundingBox.X != first.X && BoundingBox.Y != first.Y) { second = BoundingBox; }
                    else if (BoundingBox.Width * BoundingBox.Height > second.Width * second.Height && BoundingBox.X != first.X && BoundingBox.Y != first.Y) { second = BoundingBox; }
                }
            }

            if (first.X > second.X)//如果最大在右邊 交換
            {
                Rectangle temprect = first;
                first = second;
                second = temprect;
            }

            Point centernose = new Point((first.X + first.Width / 2 + second.X + second.Width / 2) / 2+(faces[0].X + (faces[0].Width / 5) * 2), (first.Y + first.Height / 2 + second.Y + second.Height / 2) / 2+(faces[0].Y + faces[0].Height / 2));//左右鼻孔位置平均




























            Image<Bgr, byte> facewh = new Image<Bgr, byte>(My_Image2.Bitmap);
            Image<Gray, byte> facewhgray = new Image<Gray, byte>(My_Image1.Bitmap);
            
            int w = My_Image2.Bitmap.Width;
            int h = My_Image2.Bitmap.Height;
            Bitmap image1 = new Bitmap(My_Image2.Bitmap);
            Bitmap skin = new Bitmap(My_Image2.Bitmap);
            //for (int y = 0; y < h; y++)//find skin RGB
            //{
            //    for (int x = 0; x < w; x++)
            //    {
            //        Color color = image1.GetPixel(x, y);
            //        int R = color.R;
            //        int G = color.G;
            //        int B = color.B;

            //        if ((R > 95 && G > 40 && B > 20 && (R - B) > 15 && (R - G) > 15) || (R > 200 && G > 210 && B > 170 && (R - B) <= 15 && R > B && G > B))
            //        {
            //            skin.SetPixel(x, y, Color.FromArgb(255, 255, 255));
            //        }
            //        else { skin.SetPixel(x, y, Color.FromArgb(0, 0, 0)); }

            //    }
            //}

            for (int y = 0; y < h; y++)//find skin  YCbCr
            {
                for (int x = 0; x < w; x++)
                {
                    Color color = image1.GetPixel(x, y);
                    int R = color.R;
                    int G = color.G;
                    int B = color.B;
                    double Y = 0.257 * R + 0.564 * G + 0.098 * B + 16;
                    double Cb = -0.148 * R - 0.291 * G + 0.439 * B + 128;
                    double Cr = 0.439 * R - 0.368 * G - 0.071 * B + 128;
                    if (Cb>85&&Cb<135&&Cr>135&&Cr<180&&Y>80)//另一個參數Cb>76&&Cb<127&&Cr>132&&Cr<173，Cb>101&&Cb<125&&Cr>130&&Cr<155
                    {
                        skin.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    }
                    else { skin.SetPixel(x, y, Color.FromArgb(0, 0, 0)); }

                }
            }


            Image<Gray, byte> skinimage = new Image<Gray, byte>(skin);
            skinimage = skinimage.Dilate(2);
            skinimage = skinimage.Erode(2);
            Image<Bgr, byte> skincolorimage = new Image<Bgr, byte>(skinimage.Bitmap);


            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            
                CvInvoke.FindContours(skinimage, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                Bitmap tempimg = new Bitmap(skinimage.Bitmap);
                Bitmap tempcolorimg = new Bitmap(facewh.Bitmap);

                int left = 2147483647, right = 0, top = 2147483647, button = 0;
                int leftpoint = 0, rightpoint = 0, toppoint = 0, buttonpoint = 0;
                int count = contours.Size;



                int prearea = 0, maxarea = 0;
                for (int i = 0; i < count; i++)
                {
                    using (VectorOfPoint contour = contours[i])//找最大面積
                    {
                        Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                        int area = BoundingBox.Width * BoundingBox.Height;
                        if (area > prearea)
                        {
                            maxarea = i;
                            prearea = area;
                        }

                    }
                }
            double facewidth = 0;
                using (VectorOfPoint contour = contours[maxarea])//找端點畫+號
                {

                    // 使用 BoundingRectangle 取得框選矩形
                    Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);

                    PointF[] temp = Array.ConvertAll(contour.ToArray(), new Converter<Point, PointF>(Point2PointF));
                    PointF[] pts = CvInvoke.ConvexHull(temp, true);
                    Point[] points = new Point[temp.Length];


                    for (int j = 0; j < temp.Length; j++)//找上下左右端點
                    {
                        points[j] = Point.Round(temp[j]);//PointF2Point



                        if (j > 1 && points[j].X < left && points[j].Y == centernose.Y)
                        {
                            left = points[j].X;
                            leftpoint = j;
                        }

                        if (j > 1 && points[j].X > right && points[j].Y == centernose.Y)
                        {
                            right = points[j].X;
                            rightpoint = j;
                        }
                        if (j > 1 && points[j].Y < top && points[j].X == centernose.X)
                        {
                            top = points[j].Y;
                            toppoint = j;
                        }
                        if (j > 1 && points[j].Y > button && points[j].X == centernose.X)
                        {
                            button = points[j].Y;
                            buttonpoint = j;
                        }
                    }


                    StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    facewidth = Math.Sqrt(Math.Pow((points[leftpoint].X - points[rightpoint].X), 2) + Math.Pow((points[leftpoint].Y - points[rightpoint].Y), 2));

                    Graphics g2 = Graphics.FromImage(tempimg);
                    SolidBrush drawBrush = new SolidBrush(Color.Red);
                    SolidBrush drawBrushY = new SolidBrush(Color.Green);
                    Pen redPen = new Pen(Color.Red, 3);
                    g2.DrawString("+", new Font("Arial", 25), drawBrush, points[leftpoint].X, points[leftpoint].Y, sf);//畫左點
                    g2.DrawString("+", new Font("Arial", 25), drawBrush, points[rightpoint].X, points[rightpoint].Y, sf);//畫右點
                    g2.DrawString("+", new Font("Arial", 25), drawBrush, points[toppoint].X, points[toppoint].Y, sf);//畫上點
                    g2.DrawString("+", new Font("Arial", 25), drawBrush, points[buttonpoint].X, points[buttonpoint].Y, sf);//畫下點
                    g2.DrawLine(redPen, points[leftpoint].X, points[leftpoint].Y, points[rightpoint].X, points[rightpoint].Y);//畫橫線
                    g2.DrawLine(redPen, points[toppoint].X, points[toppoint].Y, points[buttonpoint].X, points[buttonpoint].Y);//畫縱線
                    g2.DrawString("+", new Font("Arial", 25), drawBrushY, centernose.X, centernose.Y, sf);
                    g2.Dispose();
                

                    Graphics g3 = Graphics.FromImage(tempcolorimg);
                    g3.DrawString("+", new Font("Arial", 25), drawBrush, points[leftpoint].X, points[leftpoint].Y, sf);//畫左點
                    g3.DrawString("+", new Font("Arial", 25), drawBrush, points[rightpoint].X, points[rightpoint].Y, sf);//畫右點
                    g3.DrawString("+", new Font("Arial", 25), drawBrush, points[toppoint].X, points[toppoint].Y, sf);
                    g3.DrawString("+", new Font("Arial", 25), drawBrush, points[buttonpoint].X, points[buttonpoint].Y, sf);
                    g3.DrawLine(redPen, points[leftpoint].X, points[leftpoint].Y, points[rightpoint].X, points[rightpoint].Y);//畫橫線
                    g3.DrawLine(redPen, points[toppoint].X, points[toppoint].Y, points[buttonpoint].X, points[buttonpoint].Y);//畫縱線
                    g3.DrawString("+", new Font("Arial", 25), drawBrushY, centernose.X, centernose.Y, sf);
                    g3.Dispose();
                
                    

            }
            left = 2147483647; right = 0; top = 2147483647; button = 0;
                leftpoint = 0; rightpoint = 0; toppoint = 0; buttonpoint = 0;


                skinimage.Bitmap = tempimg;
                skinimage.ROI = Rectangle.Empty;
                facewh.Bitmap = tempcolorimg;
                facewh.ROI = Rectangle.Empty;
            


            imageBox2.Image = new Image<Bgr,byte>(tempimg);
            imageBox1.Image = facewh;
            label1.Text = "臉的寬度 : "+(facewidth* mmperpixel).ToString("#0.00")+" mm";
            label1.Visible = true;
            label2.Visible = false;
            label3.Visible = false;
        }
        double mmperpixel=0.5;//每個像素幾毫米
        private void toolStripMenuItem8_Click(object sender, EventArgs e)//比例校正
        {
            if (My_Image2 != null)
            {

                Image<Bgr, Byte> img = new Image<Bgr, Byte>(My_Image2.Bitmap);
                img.ROI = new Rectangle(2 * img.Width / 5, img.Height / 2, img.Width / 4, img.Height / 2);
                Image<Bgr, Byte> result = img.Copy();
                CircleF[] colsecircle = scale(img, ref result);
                label1.Text = "硬幣半徑 : "+ colsecircle[0].Radius.ToString("#0.00") + " pixels";
                label2.Text = "硬幣直徑 : " + (colsecircle[0].Radius*2).ToString("#0.00") + " pixels";
                label3.Text = "硬幣真實直徑(26 mm)/硬幣直徑 : " + (26/(colsecircle[0].Radius * 2)).ToString("#0.00") + "mm /pixels";
                mmperpixel = 26 / (colsecircle[0].Radius * 2);
                imageBox1.Image = result;
            }
            else
                MessageBox.Show("No image!");
            label1.Visible = true;
            label2.Visible = true;
            label3.Visible = true;

        }
        SectionDetection sectionDetection = new SectionDetection();
        private CircleF[] scale(Image<Bgr, Byte> src, ref Image<Bgr, Byte> dest)
        {
            CircleF[] closeCircle;
            double[] std;
            //example of GeometryDetection 350,300 should be variable!!
            sectionDetection.GeometryDetection(src, dest, 350, 350, out closeCircle, out std);
            //example of DrewAllCircle
            sectionDetection.DrewAllCircle(ref dest, closeCircle);
            int width = 10;
            //sectionDetection.ALLDefectDetection(src, dest, closeCircle);
            System.Drawing.Rectangle ROIrectangle = new System.Drawing.Rectangle((int)(closeCircle[0].Center.X - closeCircle[0].Radius - width), (int)(closeCircle[0].Center.Y - closeCircle[0].Radius - width), (int)(closeCircle[0].Radius + width) * 2, (int)(closeCircle[0].Radius + width) * 2);
            //src.ROI = ROIrectangle;
            //dest.ROI = ROIrectangle;
            //比例因子
            //label10.Visible = true;
            //Factor_Value = 25 / closeCircle[2].Radius;
            //factor.Left = label10.Right + 5;
            //factor.Text = Factor_Value.ToString("f3") + "mm/pixel";
            return closeCircle;
        }


        public static Bitmap BoundingBox(Image<Gray, Byte> gray, Image<Bgr, byte> draw)
        {
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(gray, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                Bitmap tempimg = new Bitmap(gray.Bitmap);
                Bitmap tempcolorimg = new Bitmap(draw.Bitmap);

                int left = 2147483647, right = 0, top = 2147483647, button = 0;
                int leftpoint = 0, rightpoint = 0, toppoint = 0, buttonpoint = 0;
                int count = contours.Size;



                int prearea = 0, maxarea = 0;
                for (int i = 0; i < count; i++)
                {
                    using (VectorOfPoint contour = contours[i])//找最大面積
                    {
                        Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                        int area = BoundingBox.Width * BoundingBox.Height;
                        if (area > prearea)
                        {
                            maxarea = i;
                            prearea = area;
                        }

                    }
                }

                using (VectorOfPoint contour = contours[maxarea])//找端點畫+號
                {

                    // 使用 BoundingRectangle 取得框選矩形
                    Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);

                    PointF[] temp = Array.ConvertAll(contour.ToArray(), new Converter<Point, PointF>(Point2PointF));
                    PointF[] pts = CvInvoke.ConvexHull(temp, true);
                    Point[] points = new Point[temp.Length];


                    for (int j = 0; j < temp.Length; j++)//找上下左右端點
                    {
                        points[j] = Point.Round(temp[j]);//PointF2Point


                        if (j > 1 && points[j].X < left)
                        {
                            left = points[j].X;
                            leftpoint = j;
                        }

                        if (j > 1 && points[j].X > right)
                        {
                            right = points[j].X;
                            rightpoint = j;
                        }
                        if (j > 1 && points[j].Y < top)
                        {
                            top = points[j].Y;
                            toppoint = j;
                        }
                        if (j > 1 && points[j].Y > button)
                        {
                            button = points[j].Y;
                            buttonpoint = j;
                        }
                    }


                    StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    //facewidth = Math.Sqrt(Math.Pow((points[leftpoint].X - points[rightpoint].X), 2) + Math.Pow((points[leftpoint].Y - points[rightpoint].Y), 2));

                    Graphics g2 = Graphics.FromImage(tempimg);//畫左點
                    SolidBrush drawBrush = new SolidBrush(Color.Red);
                    g2.DrawString("+", new Font("Arial", 25), drawBrush, points[leftpoint].X, points[leftpoint].Y, sf);
                    g2.DrawString("+", new Font("Arial", 25), drawBrush, points[rightpoint].X, points[rightpoint].Y, sf);
                    g2.DrawString("+", new Font("Arial", 25), drawBrush, points[toppoint].X, points[toppoint].Y, sf);
                    g2.DrawString("+", new Font("Arial", 25), drawBrush, points[buttonpoint].X, points[buttonpoint].Y, sf);
                    g2.Dispose();
                    


                    Graphics g3 = Graphics.FromImage(tempcolorimg);//畫左點
                    g3.DrawString("+", new Font("Arial", 25), drawBrush, points[leftpoint].X, points[leftpoint].Y, sf);
                    g3.DrawString("+", new Font("Arial", 25), drawBrush, points[rightpoint].X, points[rightpoint].Y, sf);
                    g3.DrawString("+", new Font("Arial", 25), drawBrush, points[toppoint].X, points[toppoint].Y, sf);
                    g3.DrawString("+", new Font("Arial", 25), drawBrush, points[buttonpoint].X, points[buttonpoint].Y, sf);
                    g3.Dispose();
                    

                }
                left = 2147483647; right = 0; top = 2147483647; button = 0;
                leftpoint = 0; rightpoint = 0; toppoint = 0; buttonpoint = 0;


                gray.Bitmap = tempimg;
                gray.ROI = Rectangle.Empty;
                draw.Bitmap = tempcolorimg;
                draw.ROI = Rectangle.Empty;
                return tempimg;
            }

        }
        private static PointF Point2PointF(Point P)//Point轉PointF
        {
            PointF PF = new PointF
            {
                X = P.X,
                Y = P.Y
            };
            return PF;
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)//ALL
        {
            Image<Gray, byte> facecutgray = new Image<Gray, byte>(facecutori.Bitmap);
            facecutorigray.Bitmap = facecutorigray.SmoothMedian(9).Bitmap;
            //Image<Gray, byte> Out = new Image<Gray, byte>(facecutorigray.Size);
            CvInvoke.AdaptiveThreshold(facecutorigray, facecutorigray, 255, AdaptiveThresholdType.MeanC, ThresholdType.BinaryInv, 31, 2);

            //facecutorigray.ROI = Rectangle.Empty;
            Bitmap image1 = new Bitmap(facecutori.Bitmap);
            Bitmap skin = new Bitmap(facecutori.Bitmap);

            int w = facecutori.Bitmap.Width;
            int h = facecutori.Bitmap.Height;

            for (int y = 0; y < h; y++)//find skin
            {
                for (int x = 0; x < w; x++)
                {
                    Color color = image1.GetPixel(x, y);
                    int R = color.R;
                    int G = color.G;
                    int B = color.B;
                    float hue = color.GetHue();
                    float saturation = color.GetSaturation();
                    int A = color.A;

                    if ((R > 95 && G > 40 && B > 20 && (R - B) > 15 && (R - G) > 15) || (R > 200 && G > 210 && B > 170 && (R - B) <= 15 && R > B && G > B))
                    {
                        skin.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    }
                    else { skin.SetPixel(x, y, Color.FromArgb(255, 255, 255)); }

                }
            }

            //for (int y = 0; y < h; y++)//find skin  YCbCr
            //{
            //    for (int x = 0; x < w; x++)
            //    {
            //        Color color = image1.GetPixel(x, y);
            //        int R = color.R;
            //        int G = color.G;
            //        int B = color.B;
            //        double Y = 0.257 * R + 0.564 * G + 0.098 * B + 16;
            //        double Cb = -0.148 * R - 0.291 * G + 0.439 * B + 128;
            //        double Cr = 0.439 * R - 0.368 * G - 0.071 * B + 128;
            //        if (Cb > 85 && Cb < 135 && Cr > 135 && Cr < 180 && Y > 80)//另一個參數Cb>76&&Cb<127&&Cr>132&&Cr<173，Cb>101&&Cb<125&&Cr>130&&Cr<155
            //        {
            //            skin.SetPixel(x, y, Color.FromArgb(0, 0, 0));
            //        }
            //        else { skin.SetPixel(x, y, Color.FromArgb(255, 255, 255)); }

            //    }
            //}

            //Image<Gray, float> Outfloat = Out.Convert<Gray, float>();//二值化後的照片轉float
            Image<Gray, byte> faceskin = new Image<Gray, byte>(skin);
            facecutorigray.Bitmap = facecutorigray.And(faceskin).Bitmap;

            //Image<Gray, byte> skinsobelbyte = new Image<Gray, byte>(skinsobel.Bitmap);

            Image<Bgr, byte> DrawI = new Image<Bgr, byte>(facecutori.Bitmap);
            //skinsobelbyte = skinsobelbyte.Dilate(2);
            //MyCV.BoundingBox(skinsobelbyte, facecutori);

            //facecutori.ROI = Rectangle.Empty;
            VectorOfVectorOfPoint contours1 = new VectorOfVectorOfPoint();
            
                // 在這版本請使用FindContours，早期版本有cvFindContours等等，在這版都無法使用，
                // 由於這邊是要取得最外層的輪廓，所以第三個參數給 null，第四個參數則用 RetrType.External。
                CvInvoke.FindContours(facecutorigray, contours1, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                Bitmap tempimg1 = new Bitmap(facecutorigray.Bitmap);
                Bitmap tempcolorimg1 = new Bitmap(facecutori.Bitmap);

                int left1 = 2147483647, right1 = 0, top1 = 2147483647, button1 = 0;
                int lefttop = 2147483647, leftdown = 0, righttop = 2147483647, rightdown = 0;
                int leftpoint1 = 0, rightpoint1 = 0, toppoint1 = 0, buttonpoint1 = 0, lefttoppoint = 0, leftdownpoint = 0, righttoppoint = 0, rightdownpoint = 0;
                int count1 = contours1.Size;


                for (int i = 0; i < count1; i++)
                {
                    using (VectorOfPoint contour = contours1[i])
                    {

                        // 使用 BoundingRectangle 取得框選矩形
                        Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                        if ((BoundingBox.Width / BoundingBox.Height) > 1.5 && (BoundingBox.Width * BoundingBox.Height) > 2000 && (BoundingBox.Width * BoundingBox.Height) < 7000 && BoundingBox.Y < facecutorigray.Height / 4)//過濾長寬比太小和面積太小的box
                                                                                                                                                                                                                    //CvInvoke.DrawContours(draw, contours,i, new MCvScalar(255, 0, 255, 255),2);
                        {
                            PointF[] temp = Array.ConvertAll(contour.ToArray(), new Converter<Point, PointF>(Point2PointF));
                            PointF[] pts = CvInvoke.ConvexHull(temp, true);
                            Point[] points = new Point[temp.Length];



                            for (int j = 0; j < temp.Length; j++)//找上下左右端點
                            {
                                points[j] = Point.Round(temp[j]);//PointF2Point


                                if (j > 1 && points[j].X < left1)
                                {
                                    left1 = points[j].X;
                                    leftpoint1 = j;
                                }

                                if (j > 1 && points[j].X > right1)
                                {
                                    right1 = points[j].X;
                                    rightpoint1 = j;
                                }
                                if (j > 1 && points[j].Y < top1)
                                {
                                    top1 = points[j].Y;
                                    toppoint1 = j;
                                }
                                if (j > 1 && points[j].Y > button1)
                                {
                                    button1 = points[j].Y;
                                    buttonpoint1 = j;
                                }
                            }




                            for (int j = 0; j < temp.Length; j++)
                            {
                                if (j > 1 && points[j].X == (points[leftpoint1].X + points[toppoint1].X) / 2 && points[j].Y < lefttop)
                                {
                                    lefttop = points[j].Y;
                                    lefttoppoint = j;
                                }

                                if (j > 1 && points[j].X == (points[leftpoint1].X + points[buttonpoint1].X) / 2 && points[j].Y > leftdown)
                                {
                                    leftdown = points[j].Y;
                                    leftdownpoint = j;
                                }

                                if (j > 1 && points[j].X == (points[rightpoint1].X + points[toppoint1].X) / 2 && points[j].Y < righttop)
                                {
                                    righttop = points[j].Y;
                                    righttoppoint = j;
                                }

                                if (j > 1 && points[j].X == (points[rightpoint1].X + points[buttonpoint1].X) / 2 && points[j].Y > rightdown)
                                {
                                    rightdown = points[j].Y;
                                    rightdownpoint = j;
                                }
                            }

                            Point[] pointlefttopright = { points[leftpoint1], points[lefttoppoint], points[toppoint1], points[righttoppoint], points[rightpoint1] };
                            Point[] pointleftbuttonright = { points[leftpoint1], points[leftdownpoint], points[buttonpoint1], points[rightdownpoint], points[rightpoint1] };








                            Point[] subsampling = new Point[points.Length / 15 + 1];
                            for (int j = 0; j < temp.Length; j = j + 15)
                            {
                                subsampling[j / 15] = points[j];
                            }
                            subsampling[points.Length / 15] = subsampling[0];





                            Graphics g = Graphics.FromImage(tempimg1);//畫上曲線
                            Pen pen = new Pen(Color.FromArgb(255, 255, 0, 0), 3);
                            g.DrawCurve(pen, subsampling, 0.3f);
                            g.Dispose();

                            Graphics g1 = Graphics.FromImage(tempimg1);//畫下曲線
                            Pen pen1 = new Pen(Color.Yellow, 4);
                            //g1.DrawCurve(pen1, pointleftbuttonright, 0.3f);
                            g1.Dispose();

                            Graphics g6 = Graphics.FromImage(tempcolorimg1);//畫上曲線
                            g6.DrawCurve(pen, subsampling, 0.4f);
                            g6.Dispose();

                            Graphics g7 = Graphics.FromImage(tempcolorimg1);//畫下曲線
                                                                           //g7.DrawCurve(pen1, pointleftbuttonright, 0.4f);
                            g7.Dispose();

                            StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
                            sf.Alignment = StringAlignment.Center;
                            sf.LineAlignment = StringAlignment.Center;

                            Graphics g2 = Graphics.FromImage(tempimg1);//畫左點
                            SolidBrush drawBrush = new SolidBrush(Color.LightGreen);
                            g2.DrawString("+", new Font("Arial", 25), drawBrush, points[leftpoint1].X, points[leftpoint1].Y, sf);
                            g2.Dispose();

                            Graphics g3 = Graphics.FromImage(tempimg1);//畫右點
                            g3.DrawString("+", new Font("Arial", 25), drawBrush, points[rightpoint1].X, points[rightpoint1].Y, sf);
                            g3.Dispose();


                            Graphics g4 = Graphics.FromImage(tempcolorimg1);//畫左點
                            g4.DrawString("+", new Font("Arial", 25), drawBrush, points[leftpoint1].X, points[leftpoint1].Y, sf);
                            g4.Dispose();

                            Graphics g5 = Graphics.FromImage(tempcolorimg1);//畫右點
                            g5.DrawString("+", new Font("Arial", 25), drawBrush, points[rightpoint1].X, points[rightpoint1].Y, sf);
                            g5.Dispose();
                        }


                    }
                    left1 = 2147483647; right1 = 0; top1 = 2147483647; button1 = 0; lefttop = 2147483647; leftdown = 0; righttop = 2147483647; rightdown = 0;
                    leftpoint1 = 0; rightpoint1 = 0; toppoint1 = 0; buttonpoint1 = 0; lefttoppoint = 0; leftdownpoint = 0; righttoppoint = 0; rightdownpoint = 0;
                }

                facecutorigray.Bitmap = tempimg1;
                facecutorigray.ROI = Rectangle.Empty;
                facecutori.Bitmap = tempcolorimg1;
                facecutori.ROI = Rectangle.Empty;
            
            //imageBox2.Image = new Image<Bgr, byte>(MyCV.BoundingBox(facecutorigray, facecutori));

































            Image<Bgr, byte> facewh = new Image<Bgr, byte>(facecutori.Bitmap);
            Image<Gray, byte> facewhgray = new Image<Gray, byte>(My_Image1.Bitmap);

            int w1 = My_Image2.Bitmap.Width;
            int h1 = My_Image2.Bitmap.Height;
            Bitmap image11 = new Bitmap(facecutori.Bitmap);
            Bitmap skin1 = new Bitmap(facecutori.Bitmap);
            //for (int y = 0; y < h; y++)//find skin RGB
            //{
            //    for (int x = 0; x < w; x++)
            //    {
            //        Color color = image1.GetPixel(x, y);
            //        int R = color.R;
            //        int G = color.G;
            //        int B = color.B;

            //        if ((R > 95 && G > 40 && B > 20 && (R - B) > 15 && (R - G) > 15) || (R > 200 && G > 210 && B > 170 && (R - B) <= 15 && R > B && G > B))
            //        {
            //            skin.SetPixel(x, y, Color.FromArgb(255, 255, 255));
            //        }
            //        else { skin.SetPixel(x, y, Color.FromArgb(0, 0, 0)); }

            //    }
            //}

            for (int y = 0; y < h1; y++)//find skin  YCbCr
            {
                for (int x = 0; x < w1; x++)
                {
                    Color color = image11.GetPixel(x, y);
                    int R = color.R;
                    int G = color.G;
                    int B = color.B;
                    double Y = 0.257 * R + 0.564 * G + 0.098 * B + 16;
                    double Cb = -0.148 * R - 0.291 * G + 0.439 * B + 128;
                    double Cr = 0.439 * R - 0.368 * G - 0.071 * B + 128;
                    if (Cb > 85 && Cb < 135 && Cr > 135 && Cr < 180 && Y > 80)//另一個參數Cb>76&&Cb<127&&Cr>132&&Cr<173，Cb>101&&Cb<125&&Cr>130&&Cr<155
                    {
                        skin1.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    }
                    else { skin1.SetPixel(x, y, Color.FromArgb(0, 0, 0)); }

                }
            }


            Image<Gray, byte> skinimage = new Image<Gray, byte>(skin1);
            skinimage = skinimage.Dilate(2);
            skinimage = skinimage.Erode(2);
            Image<Bgr, byte> skincolorimage = new Image<Bgr, byte>(skinimage.Bitmap);


            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(skinimage, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            Bitmap tempimg = new Bitmap(skinimage.Bitmap);
            Bitmap tempcolorimg = new Bitmap(facewh.Bitmap);
            Bitmap tempeyebrowimg = new Bitmap(MyCV.BoundingBoxeyebrow(facecutorigray, facecutori));

            int left = 2147483647, right = 0, top = 2147483647, button = 0;
            int leftpoint = 0, rightpoint = 0, toppoint = 0, buttonpoint = 0;
            int count = contours.Size;



            int prearea = 0, maxarea = 0;
            for (int i = 0; i < count; i++)
            {
                using (VectorOfPoint contour = contours[i])//找最大面積
                {
                    Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                    int area = BoundingBox.Width * BoundingBox.Height;
                    if (area > prearea)
                    {
                        maxarea = i;
                        prearea = area;
                    }

                }
            }
            double facewidth = 0;
            using (VectorOfPoint contour = contours[maxarea])//找端點畫+號
            {

                // 使用 BoundingRectangle 取得框選矩形
                Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);

                PointF[] temp = Array.ConvertAll(contour.ToArray(), new Converter<Point, PointF>(Point2PointF));
                PointF[] pts = CvInvoke.ConvexHull(temp, true);
                Point[] points = new Point[temp.Length];


                for (int j = 0; j < temp.Length; j++)//找上下左右端點
                {
                    points[j] = Point.Round(temp[j]);//PointF2Point


                    if (j > 1 && points[j].X < left)
                    {
                        left = points[j].X;
                        leftpoint = j;
                    }

                    if (j > 1 && points[j].X > right)
                    {
                        right = points[j].X;
                        rightpoint = j;
                    }
                    if (j > 1 && points[j].Y < top)
                    {
                        top = points[j].Y;
                        toppoint = j;
                    }
                    if (j > 1 && points[j].Y > button)
                    {
                        button = points[j].Y;
                        buttonpoint = j;
                    }
                }


                StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                facewidth = Math.Sqrt(Math.Pow((points[leftpoint].X - points[rightpoint].X), 2) + Math.Pow((points[leftpoint].Y - points[rightpoint].Y), 2));

                Graphics g2 = Graphics.FromImage(tempimg);
                SolidBrush drawBrush = new SolidBrush(Color.Red);
                Pen redPen = new Pen(Color.Red, 3);
                g2.DrawString("+", new Font("Arial", 25), drawBrush, points[leftpoint].X, points[leftpoint].Y, sf);//畫左點
                g2.DrawString("+", new Font("Arial", 25), drawBrush, points[rightpoint].X, points[rightpoint].Y, sf);//畫右點
                g2.DrawString("+", new Font("Arial", 25), drawBrush, points[toppoint].X, points[toppoint].Y, sf);//畫上點
                g2.DrawString("+", new Font("Arial", 25), drawBrush, points[buttonpoint].X, points[buttonpoint].Y, sf);//畫下點
                g2.DrawLine(redPen, points[leftpoint].X, points[leftpoint].Y, points[rightpoint].X, points[rightpoint].Y);//畫橫線
                g2.DrawLine(redPen, points[toppoint].X, points[toppoint].Y, points[buttonpoint].X, points[buttonpoint].Y);//畫縱線
                g2.Dispose();


                Graphics g3 = Graphics.FromImage(tempcolorimg);
                g3.DrawString("+", new Font("Arial", 25), drawBrush, points[leftpoint].X, points[leftpoint].Y, sf);//畫左點
                g3.DrawString("+", new Font("Arial", 25), drawBrush, points[rightpoint].X, points[rightpoint].Y, sf);//畫右點
                g3.DrawString("+", new Font("Arial", 25), drawBrush, points[toppoint].X, points[toppoint].Y, sf);
                g3.DrawString("+", new Font("Arial", 25), drawBrush, points[buttonpoint].X, points[buttonpoint].Y, sf);
                g3.DrawLine(redPen, points[leftpoint].X, points[leftpoint].Y, points[rightpoint].X, points[rightpoint].Y);//畫橫線
                g3.DrawLine(redPen, points[toppoint].X, points[toppoint].Y, points[buttonpoint].X, points[buttonpoint].Y);//畫縱線
                g3.Dispose();

                Graphics g4 = Graphics.FromImage(tempeyebrowimg);
                g4.DrawString("+", new Font("Arial", 25), drawBrush, points[leftpoint].X, points[leftpoint].Y, sf);//畫左點
                g4.DrawString("+", new Font("Arial", 25), drawBrush, points[rightpoint].X, points[rightpoint].Y, sf);//畫右點
                g4.DrawString("+", new Font("Arial", 25), drawBrush, points[toppoint].X, points[toppoint].Y, sf);
                g4.DrawString("+", new Font("Arial", 25), drawBrush, points[buttonpoint].X, points[buttonpoint].Y, sf);
                g4.DrawLine(redPen, points[leftpoint].X, points[leftpoint].Y, points[rightpoint].X, points[rightpoint].Y);//畫橫線
                g4.DrawLine(redPen, points[toppoint].X, points[toppoint].Y, points[buttonpoint].X, points[buttonpoint].Y);//畫縱線
                g4.Dispose();

            }
            left = 2147483647; right = 0; top = 2147483647; button = 0;
            leftpoint = 0; rightpoint = 0; toppoint = 0; buttonpoint = 0;


            skinimage.Bitmap = tempimg;
            skinimage.ROI = Rectangle.Empty;
            facewh.Bitmap = tempcolorimg;
            facewh.ROI = Rectangle.Empty;



            imageBox2.Image = new Image<Bgr, byte>(tempeyebrowimg);
            imageBox1.Image = facewh;
            label1.Text = "臉的寬度 : " + (facewidth * mmperpixel).ToString("#0.00") + " mm";
            label1.Visible = true;
            label2.Visible = false;
            label3.Visible = false;
        }
        private System.Drawing.Rectangle getPicRoiOld(ref Image<Bgr, byte> img)
        {
            double bh, bs, bv;
            int fullwid = img.Width, fullhei = img.Height;

            ColorToHSV(img[fullhei / 2, fullwid / 2], out bh, out bs, out bv);
            double h, s, v;
            int wid = img.Width, hei = img.Height;
            int top = hei / 5 * 2, bottom = (int)(hei - top);
            //int top = hei / 5, bottom = (int)(top * 3); top = (int)(top *1.5);  //patch for hospital image
            int left = wid / 4, right = wid - left;

            for (int i = fullwid / 2; i > 0; i--)
            {

                int skin = 0;
                for (int j = 0; j < fullhei; j++)
                {
                    ColorToHSV(img[j, i], out h, out s, out v);
                    if (abs(bh, h) <= 8 && abs(bs, s) <= 0.3 && abs(bv, v) <= 0.4)
                    {
                        skin++;
                    }
                }
                if (skin < fullhei / 70)
                {
                    left = i;
                    break;
                }
            }
            for (int i = fullwid / 2; i < fullwid; i++)
            {

                int skin = 0;
                for (int j = 0; j < fullhei; j++)
                {
                    ColorToHSV(img[j, i], out h, out s, out v);
                    if (abs(bh, h) <= 8 && abs(bs, s) <= 0.3 && abs(bv, v) <= 0.4)
                    {
                        skin++;
                    }
                }
                if (skin < fullhei / 70)
                {
                    right = i;
                    break;
                }
            }
            left -= 50;
            right += 50;

            return (new System.Drawing.Rectangle(left, top, right - left, bottom - top));
        }
        public static void ColorToHSV(Bgr color, out double hue, out double saturation, out double value)
        {
            Color drawcolor = Color.FromArgb((int)color.Red, (int)color.Green, (int)color.Blue);

            int max = Math.Max(drawcolor.R, Math.Max(drawcolor.G, drawcolor.B));
            int min = Math.Min(drawcolor.R, Math.Min(drawcolor.G, drawcolor.B));

            hue = drawcolor.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }
        private double abs(double a, double b)
        {
            if (a > b)
                return a - b;
            else
                return b - a;
        }

        private System.Drawing.Rectangle getPicRoi(ref Image<Bgr, byte> img, ref EyeShapeDetection eyeshapedect)
        {
            List<System.Drawing.Rectangle> facesrect = new List<System.Drawing.Rectangle>(), eyesrect = new List<System.Drawing.Rectangle>();
            eyeshapedect.FindEyes(img, facesrect, eyesrect);
            int left = -1, right = -1, top = -1, bottom = -1;
            //if (facesrect.Count == 0)
            //{
            //    MessageBox.Show("face not found");
            //}
            if (eyesrect.Count != 2)
            {
                Console.WriteLine("oldROI");
                return getPicRoiOld(ref img);
            }
            foreach (System.Drawing.Rectangle rect in eyesrect)
            {
                if (left == -1 || left > rect.Left)
                    left = rect.Left;
                if (right == -1 || right < rect.Right)
                    right = rect.Right;
                if (top == -1 || top > rect.Top)
                    top = rect.Top;
                if (bottom == -1 || bottom < rect.Bottom)
                    bottom = rect.Bottom;

                //labeltest.Text += ""+rect.Left + " " + rect.Right + " " + rect.Top + " " + rect.Bottom+"\n";
            }
            if (left < 0) left = 0;
            if (top < 0) top = 0;
            if (right >= img.Width) right = img.Width - 1;
            if (bottom >= img.Height) bottom = img.Height - 1;

            System.Drawing.Rectangle result = new System.Drawing.Rectangle(left, top, right - left, bottom - top);

            return result;
        }
        private Image<Bgr, byte> grayimgtobgr(Image<Gray, byte> img)
        {
            Image<Bgr, byte> bgrimg = new Image<Bgr, byte>(img.Width, img.Height);
            for (int i = 0; i < img.Width; ++i)
            {
                for (int j = 0; j < img.Height; ++j)
                {
                    bgrimg[j, i] = new Bgr(img[j, i].Intensity, img[j, i].Intensity, img[j, i].Intensity);
                }
            }

            return bgrimg;
        }
        private Image<Bgr, byte> colorboost(Image<Bgr, byte> img)
        {
            double maxblue = 1;
            double maxgreen = 1;
            double maxred = 1;

            for (int i = 0; i < img.Width; ++i)
            {
                for (int j = 1; j < img.Height; ++j)
                {
                    if (img[j, i].Blue > maxblue) maxblue = img[j, i].Blue;
                    if (img[j, i].Green > maxgreen) maxgreen = img[j, i].Green;
                    if (img[j, i].Red > maxred) maxred = img[j, i].Red;
                }
            }
            double kb = 255 / maxblue;
            double kg = 255 / maxgreen;
            double kr = 255 / maxred;

            Image<Bgr, byte> boosted = new Image<Bgr, byte>(img.Width, img.Height);

            for (int i = 0; i < img.Width; ++i)
            {
                for (int j = 1; j < img.Height; ++j)
                {
                    double r = img[j, i].Red * kr;
                    double g = img[j, i].Green * kg;
                    double b = img[j, i].Blue * kb;
                    boosted[j, i] = new Bgr(b, g, r);
                }
            }

            return boosted;
        }
        private Image<Gray, byte> getbgedge(Image<Bgr, byte> img)
        {

            Image<Bgr, Byte> bimg = new Image<Bgr, Byte>(img.Width, img.Height);
            CvInvoke.BilateralFilter(img.Mat, bimg.Mat, 9, 300, 300);


            Image<Gray, Byte> green = new Image<Gray, byte>(img.Width, img.Height);
            Image<Gray, Byte> blue = new Image<Gray, byte>(img.Width, img.Height);
            for (int i = 0; i < bimg.Width; ++i)
            {
                for (int j = 0; j < bimg.Height; ++j)
                {
                    blue[j, i] = new Gray(bimg[j, i].Blue);
                    green[j, i] = new Gray(bimg[j, i].Green);
                }
            }
            CvInvoke.MedianBlur(green, green, 3);
            CvInvoke.MedianBlur(blue, blue, 3);
            //Image<Gray, Byte> temp = green.CopyBlank();

            //green._ThresholdBinary(new Gray(40), new Gray(255));
            //blue._ThresholdBinary(new Gray(40), new Gray(255));

            green = green.Canny(10, 25);
            blue = blue.Canny(10, 25);

            green._Not();
            blue._Not();

            green._And(blue);
            return green;
        }
        private List<CircleF> vertifycycle(List<CircleF> circles, Image<Gray, byte> edge, Image<Gray, byte> black)
        {

            List<CircleF> resultl = new List<CircleF>();
            List<CircleF> resultr = new List<CircleF>();
            List<CircleF> result = new List<CircleF>();

            List<CircleF> cyl = new List<CircleF>(), cyr = new List<CircleF>();
            foreach (CircleF cy in circles)
            {
                if (cy.Center.X < 2.5 * cy.Radius || cy.Center.Y < 1.5 * cy.Radius) continue;
                if (cy.Center.X > edge.Width - 2.5 * cy.Radius || cy.Center.Y > edge.Height - 1.5 * cy.Radius) continue;
                if (cy.Radius < 30 || cy.Radius > 38) continue;
                if (cy.Center.X > edge.Width / 2)
                    cyr.Add(cy);
                else
                    cyl.Add(cy);
            }
            //fortestcyl = cyl;
            //fortestcyr = cyr;
            Dictionary<CircleF, double> leyever = new Dictionary<CircleF, double>();
            Dictionary<CircleF, double> leyebla = new Dictionary<CircleF, double>();
            foreach (CircleF cy in cyl)
            {
                Image<Gray, byte> cyedg = edge.CopyBlank();
                cyedg.Draw(cy, new Gray(255), 3);
                Image<Gray, byte> cybla = edge.CopyBlank();
                cybla.Draw(cy, new Gray(255), 0);
                leyever.Add(cy, 0);
                leyebla.Add(cy, 0);
                //double h, s, v;
                for (int i = (int)(cy.Center.X - cy.Radius); i < (int)(cy.Center.X + cy.Radius) && i < edge.Width; ++i)
                {
                    for (int j = (int)(cy.Center.Y - cy.Radius); j < (int)(cy.Center.Y + cy.Radius) && j < edge.Height; ++j)
                    {
                        if (cyedg[j, i].Intensity == 255 && edge[j, i].Intensity == 0)
                        {
                            leyever[cy] += 1;
                        }
                        if (cybla[j, i].Intensity == 255 && black[j, i].Intensity == 0)
                        {
                            leyebla[cy] += 1;
                        }
                    }
                }
            }
            double max = 0;
            CircleF maxcy = cyl[0];
            foreach (KeyValuePair<CircleF, double> kvp in leyever)
            {
                if (kvp.Value > max)
                {
                    max = kvp.Value;
                }
            }
            foreach (CircleF cy in cyl)
            {
                leyever[cy] /= max;
                lvalver.Add(cy, leyever[cy]);
            }
            max = 0;
            foreach (KeyValuePair<CircleF, double> kvp in leyebla)
            {
                if (kvp.Value > max)
                {
                    max = kvp.Value;
                }
            }
            foreach (CircleF cy in cyl)
            {
                //leyebla[cy] /= max;
                //leyebla[cy] = 1 - leyebla[cy];
                //lvalbla.Add(cy, leyebla[cy]);
                //leyever[cy] += 1.5 * leyebla[cy];
                leyebla[cy] /= max;
                lvalbla.Add(cy, leyebla[cy]);
                leyever[cy] += 1.5 * leyebla[cy];
            }
            var dicSort = from objDic in leyever orderby objDic.Value descending select objDic;
            foreach (KeyValuePair<CircleF, double> kvp in dicSort)
            {
                resultl.Add(kvp.Key);
                fortestcyl.Add(kvp.Key);
            }


            Dictionary<CircleF, double> reyever = new Dictionary<CircleF, double>();
            Dictionary<CircleF, double> reyebla = new Dictionary<CircleF, double>();
            foreach (CircleF cy in cyr)
            {
                Image<Gray, byte> cyedg = edge.CopyBlank();
                cyedg.Draw(cy, new Gray(255), 3);
                Image<Gray, byte> cybla = edge.CopyBlank();
                cybla.Draw(cy, new Gray(255), 0);
                reyever.Add(cy, 0);
                reyebla.Add(cy, 0);
                for (int i = (int)(cy.Center.X - cy.Radius); i < (int)(cy.Center.X + cy.Radius) && i < edge.Width; ++i)
                {
                    for (int j = (int)(cy.Center.Y - cy.Radius); j < (int)(cy.Center.Y + cy.Radius) && j < edge.Height; ++j)
                    {
                        if (cyedg[j, i].Intensity == 255 && edge[j, i].Intensity == 0)
                        {
                            reyever[cy] += 1;
                        }
                        if (cybla[j, i].Intensity == 255 && black[j, i].Intensity == 0)
                        {
                            reyebla[cy] += 1;
                        }
                    }
                }
            }
            max = 0;
            maxcy = cyr[0];
            foreach (KeyValuePair<CircleF, double> kvp in reyever)
            {
                if (kvp.Value > max)
                {
                    max = kvp.Value;
                }
            }
            foreach (CircleF cy in cyr)
            {
                reyever[cy] /= max;
                rvalver.Add(cy, reyever[cy]);
            }
            max = 0;
            foreach (KeyValuePair<CircleF, double> kvp in reyebla)
            {
                if (kvp.Value > max)
                {
                    max = kvp.Value;
                }
            }
            foreach (CircleF cy in cyr)
            {
                //reyebla[cy] /= max;
                //reyebla[cy] = 1 - reyebla[cy];
                //rvalbla.Add(cy, reyebla[cy]);
                //reyever[cy] += 1.5 * reyebla[cy];
                reyebla[cy] /= max;
                rvalbla.Add(cy, reyebla[cy]);
                reyever[cy] += 1.5 * reyebla[cy];
            }
            var dicSort2 = from objDic in reyever orderby objDic.Value descending select objDic;
            foreach (KeyValuePair<CircleF, double> kvp in dicSort2)
            {
                resultr.Add(kvp.Key);
                fortestcyr.Add(kvp.Key);
            }


            for (int i = 0; i < 20 && resultl.Count > 0 && resultr.Count > 0; i++)
            {

                if (resultr[0].Center.Y - resultl[0].Center.Y > 40)
                {
                    resultl.RemoveAt(0);
                }
                else if (resultl[0].Center.Y - resultr[0].Center.Y > 40)
                {
                    resultr.RemoveAt(0);
                }
                else if (resultl[0].Radius - resultr[0].Radius > 6)
                {
                    resultl.RemoveAt(0);
                }
                else if (resultr[0].Radius - resultl[0].Radius > 6)
                {
                    resultr.RemoveAt(0);
                }
                else
                    break;

            }

            result.Add(resultl[0]);
            result.Add(resultr[0]);
            return result;


        }
        private void setmrd(ref Image<Bgr, byte> img, List<CircleF> pupils, List<PointF> leftCornerPoints, List<PointF> rightCornerPoints, int pix_cm)
        {
            CircleF leye, reye;
            if (pupils[0].Center.X < pupils[1].Center.X)
            {
                leye = pupils[0];
                reye = pupils[1];
            }
            else
            {
                leye = pupils[1];
                reye = pupils[0];
            }
            PointF lup, ldn;
            if (leftCornerPoints[0].Y < leftCornerPoints[1].Y)
            {
                lup = leftCornerPoints[0];
                ldn = leftCornerPoints[1];
            }
            else
            {
                lup = leftCornerPoints[1];
                ldn = leftCornerPoints[0];
            }
            PointF ll, lr;
            if (leftCornerPoints[2].X < leftCornerPoints[3].X)
            {
                ll = leftCornerPoints[2];
                lr = leftCornerPoints[3];
            }
            else
            {
                ll = leftCornerPoints[3];
                lr = leftCornerPoints[2];
            }
            PointF rup, rdn;
            if (rightCornerPoints[0].Y < rightCornerPoints[1].Y)
            {
                rup = rightCornerPoints[0];
                rdn = rightCornerPoints[1];
            }
            else
            {
                rup = rightCornerPoints[1];
                rdn = rightCornerPoints[0];
            }
            PointF rl, rr;
            if (rightCornerPoints[2].X < rightCornerPoints[3].X)
            {
                rl = rightCornerPoints[2];
                rr = rightCornerPoints[3];
            }
            else
            {
                rl = rightCornerPoints[3];
                rr = rightCornerPoints[2];
            }
            double lmrd1, lmrd2, rmrd1, rmrd2;

            img.Draw(new LineSegment2DF(leye.Center, lup), new Bgr(Color.BlueViolet), 4);
            lmrd1 = distent2d(leye.Center, lup);
            img.Draw(new LineSegment2DF(leye.Center, ldn), new Bgr(Color.DarkCyan), 4);
            lmrd2 = distent2d(leye.Center, ldn);
            img.Draw(new LineSegment2DF(reye.Center, rup), new Bgr(Color.BlueViolet), 4);
            rmrd1 = distent2d(reye.Center, rup);
            img.Draw(new LineSegment2DF(reye.Center, rdn), new Bgr(Color.DarkCyan), 4);
            rmrd2 = distent2d(reye.Center, rdn);



            label1.Text += "半  徑   R : " + pixtomm(pix_cm, leye.Radius).ToString("##0.##") + "\nMRD1   : " + pixtomm(pix_cm, lmrd1).ToString("##0.##") + "\nMRD2   : " + pixtomm(pix_cm, lmrd2).ToString("##0.##") + "\n";
            label2.Text += "半  徑   R : " + pixtomm(pix_cm, reye.Radius).ToString("##0.##") + "\nMRD1   : " + pixtomm(pix_cm, rmrd1).ToString("##0.##") + "\nMRD2   : " + pixtomm(pix_cm, rmrd2).ToString("##0.##") + "\n";


            int rimgna = 9, limgna = 9;
            findimgna(out limgna, lmrd1 / leye.Radius);
            findimgna(out rimgna, rmrd1 / reye.Radius);
            double lang = Math.Atan2(lr.Y - ll.Y, lr.X - ll.X) * (180 / Math.PI);
            double rang = Math.Atan2(rr.Y - rl.Y, rr.X - rl.X) * (180 / Math.PI);
            //lang = -30;
            /*.Image = new Image<Bgr, byte>("eyes\\n_" + limgna + ".png").Rotate(lang, new Bgr(Color.White));
            imageBox3.Image = new Image<Bgr, byte>("eyes\\n_" + rimgna + ".png").Rotate(rang, new Bgr(Color.White));*/

            double lslid, rslid;
            lslid = distent2d(ldn, lup);
            rslid = distent2d(rdn, rup);
            leyeinfo.Add(pixtomm(pix_cm, lslid).ToString("##0.##"));
            leyeinfo.Add(pixtomm(pix_cm, lmrd1).ToString("##0.##"));
            leyeinfo.Add(pixtomm(pix_cm, (leye.Radius - lmrd1)).ToString("##0.##"));
            reyeinfo.Add(pixtomm(pix_cm, rslid).ToString("##0.##"));
            reyeinfo.Add(pixtomm(pix_cm, rmrd1).ToString("##0.##"));
            reyeinfo.Add(pixtomm(pix_cm, (reye.Radius - rmrd1)).ToString("##0.##"));

        }
        private void findimgna(out int limgna, double mrd1)
        {
            if (mrd1 < 0)
                limgna = 0;
            else if (mrd1 < 0.1)
                limgna = 1;
            else if (mrd1 < 0.2)
                limgna = 2;
            else if (mrd1 < 0.3)
                limgna = 3;
            else if (mrd1 < 0.4)
                limgna = 4;
            else if (mrd1 < 0.5)
                limgna = 5;
            else if (mrd1 < 0.6)
                limgna = 6;
            else if (mrd1 < 0.7)
                limgna = 7;
            else if (mrd1 < 0.8)
                limgna = 8;
            else
                limgna = 9;
        }
        private double distent2d(PointF p, PointF q)
        {
            return Math.Sqrt((q.X - p.X) * (q.X - p.X) + (q.Y - p.Y) * (q.Y - p.Y));
        }
        private double pixtomm(int pixof_cm, double length)
        {
            double temp = length / pixof_cm;
            return temp * 10;
        }
        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            fortestcyl = new List<CircleF>();
            fortestcyr = new List<CircleF>();
            lvalver = new Dictionary<CircleF, double>();
            rvalver = new Dictionary<CircleF, double>();
            lvalbla = new Dictionary<CircleF, double>();
            rvalbla = new Dictionary<CircleF, double>();

            EyeShapeDetection eyeshapedect = new EyeShapeDetection();

            int pix_cm = 208;
            //pix_cm = findptm(My_Image);



            Image<Gray, Byte> processimg;
            Ori_Image = My_Image2.Clone();
            My_Image2 = My_Image2.Resize(1500, 1000, Inter.Cubic);


            #region eyeshapedectROI

            #endregion
            Ori_Image.ROI = getPicRoi(ref Ori_Image, ref eyeshapedect);

            #region origional input
            //int wid = My_Image.Width, hei = My_Image.Height;
            //int top = hei / 3, bottom = top * 2;
            //int left = wid / 6, right = wid - left;
            #endregion
            #region cannoninputgetROI

            #endregion
            System.Drawing.Rectangle cannonRoi = getPicRoiOld(ref My_Image2);
            Image<Bgr, Byte> forshow = My_Image2.Copy();
            forshow.Draw(cannonRoi, new Bgr(Color.Red), 3);
            forproshow.Add(forshow.Copy());
            My_Image2.ROI = cannonRoi;
            forproshow.Add(My_Image2.Copy());


            //去雜訊
            Image<Bgr, Byte> cc = new Image<Bgr, byte>(My_Image2.Width, My_Image2.Height);
            CvInvoke.BilateralFilter(My_Image2.Mat, cc.Mat, 29, 58, 14);
            Image<Gray, Byte> ccgray = cc.Convert<Gray, Byte>();



            CvInvoke.MedianBlur(ccgray, ccgray, 3);

            processimg = ccgray.Clone();

            forproshow.Add(grayimgtobgr(processimg));

            //顏色增強
            Image<Bgr, Byte> boost = colorboost(cc);

            //找黑色
            double blacklimit = 50;

            //****找二值化參數
            blacklimit = CvInvoke.Threshold(ccgray, ccgray, 0, 255, ThresholdType.Otsu);
            //labeltest.Text = "" + blacklimit;
            ccgray = processimg.Clone();
            //if (blacklimit < 80) blacklimit = 30;
            //else if (blacklimit > 100) blacklimit *= 0.75;
            //else
            //    blacklimit *= 0.5;
            if (blacklimit < 100) blacklimit = 70;
            else if (blacklimit < 110) blacklimit *= 0.75;
            else
                blacklimit *= 0.5;

            ccgray._ThresholdBinary(new Gray(blacklimit), new Gray(255));
            //labeltest.Text += "  " + blacklimit;
            forproshow.Add(grayimgtobgr(ccgray));

            //ccgray = ccgray.Erode(3);
            //ccgray = ccgray.Dilate(3);


            //processimg = processimg.Erode(3).Dilate(3);  //侵蝕3後膨脹3

            //CvInvoke.Rectangle(My_Image, new Rectangle( left, top, right - left, hei / 3), new Bgr(Color.White).MCvScalar, 2);  //在检测到的区域绘制红框



            //CvInvoke.ConnectedComponents(src,dst);
            //CvInvoke.MedianBlur(src.Mat, dst.Mat, 5);
            //CvInvoke.BilateralFilter(src.Mat, dst.Mat, 5,30,30);
            //cc.Save("eye.jpg");


            //Image<Gray, Byte> concom = new Image<Gray, Byte>(processimg.Width, processimg.Height);
            //CvInvoke.ConnectedComponents(processimg, concom);

            //邊緣偵測
            //Image<Gray, Byte> grayimg = My_Image.Convert<Gray, Byte>();
            Image<Gray, Byte> cannyImage = ccgray.Canny(10, 35);
            cannyImage._ThresholdBinary(new Gray(128), new Gray(255));
            cannyImage._Not();
            Image<Gray, Byte> bgcanny = getbgedge(cc);



            Image<Gray, Byte> cccombine = ccgray.Clone();
            cccombine = cccombine.Erode(3);
            cccombine._Or(cannyImage);

            forproshow.Add(grayimgtobgr(bgcanny));


            double cannyThreshold = 30;
            double circleAccumulatorThreshold = 10;


            CircleF[] circles =
                CvInvoke.HoughCircles(ccgray,
                HoughType.Gradient,
                2.0,//Resolution of the accumulator used to detect centers of the circles
                1.0,//min distance 
                cannyThreshold,
                circleAccumulatorThreshold,
                20,//min radius
                40//max radius
                );
            List<CircleF> preeyes = new List<CircleF>();
            foreach (CircleF cy in circles)
            {
                //if (cy.Center.X < 1.1 * cy.Radius || cy.Center.Y < 1.1 * cy.Radius) continue;
                //if (cy.Center.X > ccgray.Width - 1.1 * cy.Radius || cy.Center.Y > ccgray.Height - 1.1 * cy.Radius) continue;
                //if (cy.Radius < 25 || cy.Radius > 35) continue;
                preeyes.Add(cy);

                //My_Image.Draw(cy, new Bgr(Color.Red), 2);
                ////My_Image.Draw(new CircleF(cy.Center, 6), new Bgr(Color.Green), 0);
                //My_Image.Draw(new Cross2DF(cy.Center, 15, 15), new Bgr(Color.White), 2);
                //label1.Text += cy.Radius + "   ";
            }

            List<CircleF> eyes = vertifycycle(preeyes, bgcanny, ccgray);
            label1.Text = "";
            //bool a = true;



            forshow = grayimgtobgr(bgcanny);
            foreach (CircleF cy in eyes)
            {
                forshow.Draw(cy, new Bgr(Color.Red), 2);
                forshow.Draw(new Cross2DF(cy.Center, 15, 15), new Bgr(Color.Red), 2);
            }
            forproshow.Add(forshow.Clone());
            forshow = grayimgtobgr(bgcanny);



            //Image<Bgr, byte> renewbgcanny = dealwithbgcanny(bgcanny, ccgray, eyes[0], eyes[1]);
            //foreach (CircleF cy in eyes){
            //    renewbgcanny.Draw(cy, new Bgr(Color.Red), 2);
            //    renewbgcanny.Draw(new Cross2DF(cy.Center, 15, 15), new Bgr(Color.Red), 2);
            //}
            //forproshow.Add(renewbgcanny);



            #region lid
            /*
            label1.Text = "";
            label2.Text = "";
            //int up = 0, dn = 0;
            double lmrd1 = 0, lmrd2 = 0;
            double rmrd1 = 0, rmrd2 = 0;
            double lang = 0, rang = 0;
            double lslid = 0, rslid = 0;
            PointF leyel, leyer, reyel, reyer;
            leyer = leyel = reyer = reyel = new PointF(0, 0);
            //findwhite6_4(boost, ccgray, My_Image, bgcanny, eyes[0].Center, eyes[0].Radius, out lmrd1, out lmrd2, forshow, out lang, out leyel, out leyer);

            //findwhite6_4(boost, ccgray, My_Image, bgcanny, eyes[1].Center, eyes[1].Radius, out rmrd1, out rmrd2, forshow, out rang, out reyel, out reyer);
            findwhite7(boost, ccgray, My_Image, bgcanny, eyes[0], out lmrd1, out lmrd2, forshow, out lang, out leyel, out leyer);
            labeltest.Text += "r\n";
            findwhite7(boost, ccgray, My_Image, bgcanny, eyes[1], out rmrd1, out rmrd2, forshow, out rang, out reyel, out reyer);

            //forproshow.Add(forshow.Clone());

            lslid = lmrd1 + lmrd2;
            rslid = rmrd1 + rmrd2;

            #region TooLittleSlidPad
            if (lslid < eyes[0].Radius)
            {
                //float pad = (float)(lmrd2 - lslid * 0.5);
                float pad = findsmalleyepad(eyes[0], ref ccgray, (int)lmrd2);
                eyes[0] = new CircleF(new PointF(eyes[0].Center.X, eyes[0].Center.Y + pad), eyes[0].Radius);
                lmrd1 += pad;
                lmrd2 -= pad;
                labeltest.Text += "\nlpad" + pad;
            }
            if (rslid < eyes[1].Radius)
            {
                //float pad = (float)(rmrd2 - rslid * 0.5);
                float pad = findsmalleyepad(eyes[1], ref ccgray, (int)rmrd2);
                eyes[1] = new CircleF(new PointF(eyes[1].Center.X, eyes[1].Center.Y + pad), eyes[1].Radius);
                rmrd1 += pad;
                rmrd2 -= pad;
                labeltest.Text += "\nrpad" + pad;
            }
            #endregion
            #region NearEdgePad
            if (distent2d(leyel, eyes[0].Center) < 1.1 * eyes[0].Radius)
            {
                //float pad = (float)(lmrd2 - lslid * 0.5);
                float pad = findhoreyepad(eyes[0], ref ccgray, (int)eyes[0].Radius, -1);
                eyes[0] = new CircleF(new PointF(eyes[0].Center.X - pad, eyes[0].Center.Y), eyes[0].Radius);
                
                labeltest.Text += "\nlmvl" + pad;
            }
            if (distent2d(leyer, eyes[0].Center) < 1.1 * eyes[0].Radius)
            {
                //float pad = (float)(lmrd2 - lslid * 0.5);
                float pad = findhoreyepad(eyes[0], ref ccgray, (int)eyes[0].Radius, 1);
                eyes[0] = new CircleF(new PointF(eyes[0].Center.X + pad, eyes[0].Center.Y), eyes[0].Radius);
                
                labeltest.Text += "\nlmvr" + pad;
            }
            if (distent2d(reyel, eyes[1].Center) < 1.1 * eyes[1].Radius)
            {
                //float pad = (float)(rmrd2 - rslid * 0.5);
                float pad = findhoreyepad(eyes[1], ref ccgray, (int)eyes[1].Radius, -1);
                eyes[1] = new CircleF(new PointF(eyes[1].Center.X - pad, eyes[1].Center.Y), eyes[1].Radius);
                
                labeltest.Text += "\nrmvl" + pad;
            }
            if (distent2d(reyer, eyes[1].Center) < 1.1 * eyes[1].Radius)
            {
                //float pad = (float)(rmrd2 - rslid * 0.5);
                float pad = findhoreyepad(eyes[1], ref ccgray, (int)eyes[1].Radius, 1);
                eyes[1] = new CircleF(new PointF(eyes[1].Center.X + pad, eyes[1].Center.Y), eyes[1].Radius);
                
                labeltest.Text += "\nrmvr" + pad;
            }
            #endregion

            #region draw Mrd
            //My_Image.Draw(new LineSegment2DF(eyes[0].Center, new PointF(eyes[0].Center.X, eyes[0].Center.Y - (float)lmrd1)), new Bgr(Color.BlueViolet), 2);
            //My_Image.Draw(new LineSegment2DF(eyes[0].Center, new PointF(eyes[0].Center.X, eyes[0].Center.Y + (float)lmrd2)), new Bgr(Color.DarkCyan), 2);
            //My_Image.Draw(new LineSegment2DF(eyes[1].Center, new PointF(eyes[1].Center.X, eyes[1].Center.Y - (float)rmrd1)), new Bgr(Color.BlueViolet), 2);
            //My_Image.Draw(new LineSegment2DF(eyes[1].Center, new PointF(eyes[1].Center.X, eyes[1].Center.Y + (float)rmrd2)), new Bgr(Color.DarkCyan), 2);

            //forshow.Draw(new LineSegment2DF(eyes[0].Center, new PointF(eyes[0].Center.X, eyes[0].Center.Y - (float)lmrd1)), new Bgr(Color.BlueViolet), 2);
            //forshow.Draw(new LineSegment2DF(eyes[0].Center, new PointF(eyes[0].Center.X, eyes[0].Center.Y + (float)lmrd2)), new Bgr(Color.DarkCyan), 2);
            //forshow.Draw(new LineSegment2DF(eyes[1].Center, new PointF(eyes[1].Center.X, eyes[1].Center.Y - (float)rmrd1)), new Bgr(Color.BlueViolet), 2);
            //forshow.Draw(new LineSegment2DF(eyes[1].Center, new PointF(eyes[1].Center.X, eyes[1].Center.Y + (float)rmrd2)), new Bgr(Color.DarkCyan), 2);
            #endregion

            leyeinfo.Add(pixtomm(pix_cm, lslid).ToString("##0.##"));
            leyeinfo.Add(pixtomm(pix_cm, lmrd1).ToString("##0.##"));
            leyeinfo.Add(pixtomm(pix_cm, (eyes[0].Radius - lmrd1)).ToString("##0.##"));
            reyeinfo.Add(pixtomm(pix_cm, rslid).ToString("##0.##"));
            reyeinfo.Add(pixtomm(pix_cm, rmrd1).ToString("##0.##"));
            reyeinfo.Add(pixtomm(pix_cm, (eyes[1].Radius - rmrd1)).ToString("##0.##"));

            //lmrd1 /= eyes[0].Radius;
            //lmrd2 /= eyes[0].Radius;
            //rmrd1 /= eyes[1].Radius;
            //rmrd2 /= eyes[1].Radius;

            #region cheat mrd
            //if (lmrd2 > 1 && lmrd2 < 1.2) lmrd2 = 1;
            //if (rmrd2 > 1 && rmrd2 < 1.2) rmrd2 = 1;
            //if (lmrd1 > 1) lmrd1 = 1;
            //if (rmrd1 > 1) rmrd1 = 1;
            #endregion

            label1.Text += "半  徑   R : " + pixtomm(pix_cm, eyes[0].Radius).ToString("##0.##") + "\nMRD1   : " + pixtomm(pix_cm, lmrd1).ToString("##0.##") + "\nMRD2   : " + pixtomm(pix_cm, lmrd2).ToString("##0.##") + "\n";
            label2.Text += "半  徑   R : " + pixtomm(pix_cm, eyes[1].Radius).ToString("##0.##") + "\nMRD1   : " + pixtomm(pix_cm, rmrd1).ToString("##0.##") + "\nMRD2   : " + pixtomm(pix_cm, rmrd2).ToString("##0.##") + "\n";

            //label3.Text += findmosthue(boost) +"\n";

            //forproshow.Add(forshow.Clone());
            int rimgna = 9, limgna = 9;
            findimgna(out limgna, lmrd1);
            findimgna(out rimgna, rmrd1);
            //lang = -30;
            imageBox_out.Image = new Image<Bgr, byte>("eyes\\n_" + limgna + ".png").Rotate(lang, new Bgr(Color.White));
            imageBox3.Image = new Image<Bgr, byte>("eyes\\n_" + rimgna + ".png").Rotate(rang, new Bgr(Color.White));
            */
            #endregion
            List<CircleF> pupils = new List<CircleF>();
            List<PointF> leftCornerPoints = new List<PointF>();
            List<PointF> rightCornerPoints = new List<PointF>();
            Image<Bgr, byte> image = Ori_Image.Clone();
            Image<Bgr, byte> result = Ori_Image.Clone();
            eyeshapedect.FindEyeShape(ref result, ref image, pupils, leftCornerPoints, rightCornerPoints);

            //**************************************************畫眼球
            #region draw eye
            //foreach (CircleF cy in eyes)
            //{
            //    if (cy.Center.X == 0)
            //    {
            //        label1.Text += cy.Radius + "   ";
            //    }
            //    else
            //    {
            //        My_Image.Draw(cy, new Bgr(Color.Green), 2);
            //        My_Image.Draw(new Cross2DF(cy.Center, 15, 15), new Bgr(Color.White), 2);

            //        forshow.Draw(cy, new Bgr(Color.Red), 2);
            //        forshow.Draw(new Cross2DF(cy.Center, 15, 15), new Bgr(Color.Red), 2);
            //    }
            //    //label1.Text += cy.Radius + "   ";
            //}
            //forproshow.Add(forshow.Clone());


            #endregion

            setmrd(ref result, pupils, leftCornerPoints, rightCornerPoints, pix_cm);


            forproshow.Add(result.Clone());

            if (flowon)
            {
                imageBox1.Image = Ori_Image;
                timer1.Interval = 900;
                timer1.Start();
            }
            else
            {
                imageBox1.Image = bgcanny;
                //imageBox1.Image = forshow;
            }
            Ori_Image.ROI = System.Drawing.Rectangle.Empty;
            imageBox1.Image = Ori_Image;
            imageBox2.Image = result;
            result.ROI = System.Drawing.Rectangle.Empty;
            forproshow.Add(result.Clone());

            //imageBox1.Image = ccgray;


            //My_Image.Save(Openfile.FileName + "_結果.jpg");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (timer_counter < forproshow.Count)
            {
                imageBox1.Image = forproshow[timer_counter];
                //forproshow[timer_counter].Save("fs"+ timer_counter + ".jpg");
            }
            else
            {
                timer1.Stop();
            }
            timer_counter++;
        }

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            Image<Bgr, byte> nose = My_Image2.Clone();
            Image<Gray, byte> nosegray = My_Image1.Clone();
            
            //nosegray.ROI = faces[0];

            nosegray = nosegray.ThresholdBinaryInv(new Gray(80),new Gray(255));
            Image<Bgr, byte> nosegraytoBgr = nosegray.Convert<Bgr, byte>();
            //faces[0].X+faces[0].Width/2,faces[0].Y+faces[0].Height/2 中心點座標
            Rectangle noserange = new Rectangle(faces[0].X+ (faces[0].Width/5)*2, faces[0].Y+ faces[0].Height/2, faces[0].Width/5, faces[0].Height/6);//取鼻子範圍
            nose.ROI = noserange;
            nosegray.ROI = noserange;
            nosegraytoBgr.ROI = noserange;


            Bitmap tempimg = new Bitmap(nosegray.Bitmap);
            Bitmap tempcolorimg = new Bitmap(nose.Bitmap);
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(nosegray, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            int count = contours.Size;


            Rectangle first = Rectangle.Empty, second = Rectangle.Empty;//最大跟第二大物件的座標
            for (int i = 0; i < count; i++)//找最大
            {
                using (VectorOfPoint contour = contours[i])
                {
                    
                    Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                    if (i == 0) { first = BoundingBox; }
                    else if(BoundingBox.Width* BoundingBox.Height>first.Width*first.Height) { first = BoundingBox; }
                }
            }

            for (int i = 0; i < count; i++)//找第二大
            {
                using (VectorOfPoint contour = contours[i])
                {

                    Rectangle BoundingBox = CvInvoke.BoundingRectangle(contour);
                    if (i == 0 && BoundingBox.X != first.X && BoundingBox.Y != first.Y) { second = BoundingBox; }
                    else if (BoundingBox.Width * BoundingBox.Height > second.Width * second.Height && BoundingBox.X != first.X && BoundingBox.Y != first.Y) { second = BoundingBox; }
                }
            }

            if (first.X > second.X)//如果最大在右邊 交換
            {
                Rectangle temprect = first;
                first = second;
                second = temprect;
            }

            Point centernose = new Point((first.X + first.Width / 2 + second.X + second.Width / 2)/2, (first.Y + first.Height / 2 + second.Y + second.Height / 2) / 2);//左右鼻孔位置平均

            StringFormat sf = new StringFormat();//設定string置中，drawString才不會錯位
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            Graphics g = Graphics.FromImage(tempimg);//畫左鼻孔中心點
            SolidBrush drawBrush = new SolidBrush(Color.Red);
            g.DrawString("+", new Font("Arial", 25), drawBrush, first.X + first.Width / 2, first.Y + first.Height / 2, sf);
            g.DrawString("+", new Font("Arial", 25), drawBrush, second.X + second.Width / 2, second.Y + second.Height / 2, sf);
            g.DrawString("+", new Font("Arial", 25), drawBrush, centernose.X, centernose.Y, sf);

            g.Dispose();

            Graphics g1 = Graphics.FromImage(tempcolorimg);//畫右鼻孔中心點
            g1.DrawString("+", new Font("Arial", 25), drawBrush, first.X + first.Width / 2, first.Y + first.Height / 2, sf);
            g1.DrawString("+", new Font("Arial", 25), drawBrush, second.X + second.Width / 2, second.Y + second.Height / 2, sf);
            g1.DrawString("+", new Font("Arial", 25), drawBrush, centernose.X, centernose.Y, sf);
            g1.Dispose();

            
            


            nosegray.Bitmap = tempimg;
            nosegray.ROI = Rectangle.Empty;
            nose.Bitmap = tempcolorimg;
            nose.ROI = Rectangle.Empty;
            nosegraytoBgr.Bitmap = tempimg;
            nosegraytoBgr.ROI = Rectangle.Empty;

            imageBox1.Image = nose;
            imageBox2.Image = nosegraytoBgr;
        }


        List<KeyValuePair<double, basicparcitlt>> weightlist;
        List<KeyValuePair<double, basicparcitlt>> nextlist;
        bool doppff;
        int timercounter = 0;
        int loopcounter = 0;
        int numofpar = 300;
        int looplimit = 5;
        int loopmode = 3;//0 old ; 1 test ;2 new
        int slowshowstate = 0;
        Parcitle ma;
        Parcitle ma_one;
        double maxw = -100;
        double maxd1 = -100;
        double maxd2 = -100;
        double maxd3 = -100;
        double maxd4 = -100;
        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            imageBox3.Visible = true;
            imageBox4.Visible = true;
            timer2.Stop();
            while (My_Image2 == null)
                toolStripMenuItem2_Click(sender, e);
            if (weightlist != null) { weightlist.Clear(); weightlist = null; }
            if (nextlist != null) { nextlist.Clear(); nextlist = null; }
            loopcounter = 0;
            EyeShapeDetection eyeshapedect = new EyeShapeDetection();
            Ori_Image = My_Image2.Clone();
            Image<Gray, byte> otsuimg = new Image<Gray, byte>(Ori_Image.Width, Ori_Image.Height);
            CvInvoke.Threshold(Ori_Image.Convert<Gray, byte>(), otsuimg, 0, 255, ThresholdType.Otsu);
            
            // call getPicRoi() to set face region ROI
            Ori_Image.ROI = getPicRoi(ref Ori_Image, ref eyeshapedect);
            System.Drawing.Rectangle idk = Ori_Image.ROI;
            // Resize ROI
            Ori_Image.ROI = new System.Drawing.Rectangle(idk.X, 0, idk.Width, My_Image2.Height);
            // Declare a Gray Image with Ori_Image.ROI size (CopyBlank)
            Image<Gray, byte> idkwid = new Image<Gray, byte>(Ori_Image.Width, Ori_Image.Height);
            
            // Thresholding based on HSV , set the dark part as white , like pupils
            for (int i = 0; i < Ori_Image.Height; i++)
            {
                for (int j = 0; j < Ori_Image.Width; j++)
                {
                    double h, s, v;
                    ColorToHSV(Ori_Image[i, j], out h, out s, out v);
                    if (v < 0.2)
                    {
                        idkwid[i, j] = new Gray(255);
                    }

                }
            }
            // fu : a copy of Ori_Image.ROI
            Image<Bgr, byte> fu = Ori_Image.Copy();
            

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

            CvInvoke.FindContours(idkwid.Mat, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            

            // parameter for contours
            MCvMoments moments;
            double area;
            MCvPoint2D64f center;
            Dictionary<int, MCvPoint2D64f> c = new Dictionary<int, MCvPoint2D64f>();
            //

            // Filter contours by area, add it to 'c' Dictionary
            int areaThreshold = 500;
            for (int i = 0; i < contours.Size; i++)
            {
                area = CvInvoke.ContourArea(contours[i], false);
                moments = CvInvoke.Moments(contours[i]);
                center = moments.GravityCenter;
                if (area > areaThreshold)
                {
                    c.Add(i, center);
                }
            }
            

            //依據objDic.Value.Y , 按遞增順序排序序列中的項目
            var dicSort = from objDic in c orderby objDic.Value.Y ascending select objDic;
            int cou = 0;
            PointF upmost = new PointF();
            PointF dnmost = new PointF();
            int[] ebowlng = new int[2];
            KeyValuePair<int, MCvPoint2D64f>[] ebow = new KeyValuePair<int, MCvPoint2D64f>[2];
            PointF[] leftmost = new PointF[2];
            PointF[] rightmost = new PointF[2];
            foreach (var item in dicSort)
            {
                if (cou == 1 || cou == 2)
                {
                    ebow[cou - 1] = item;
                    for (int i = 0; i < contours[item.Key].Size; ++i)
                    {
                        if (upmost.IsEmpty)
                        {
                            upmost = contours[item.Key][i];
                        }
                        else if (upmost.Y > contours[item.Key][i].Y)
                        {
                            upmost = contours[item.Key][i];
                        }
                        if (dnmost.IsEmpty)
                        {
                            dnmost = contours[item.Key][i];
                        }
                        else if (dnmost.Y < contours[item.Key][i].Y)
                        {
                            dnmost = contours[item.Key][i];
                        }
                        if (leftmost[cou - 1].IsEmpty)
                        {
                            leftmost[cou - 1] = contours[item.Key][i];
                        }
                        else if (leftmost[cou - 1].X > contours[item.Key][i].X)
                        {
                            leftmost[cou - 1] = contours[item.Key][i];
                        }
                        if (rightmost[cou - 1].IsEmpty)
                        {
                            rightmost[cou - 1] = contours[item.Key][i];
                        }
                        else if (rightmost[cou - 1].X < contours[item.Key][i].X)
                        {
                            rightmost[cou - 1] = contours[item.Key][i];
                        }
                    }
                    ebowlng[cou - 1] = (int)(rightmost[cou - 1].X - leftmost[cou - 1].X);
                }
                cou++;
            }

            int fixeb = 0;////need check
            if (ebowlng[0] > ebowlng[1] * 1.5)
            {
                fixeb = 1;
                fu.Draw(contours, ebow[0].Key, new Bgr(Color.Red), 2);
                fu.Draw(new CircleF(leftmost[0], 5), new Bgr(Color.Green), 0);
                fu.Draw(new CircleF(rightmost[0], 5), new Bgr(Color.Green), 0);
            }
            else if (ebowlng[1] > ebowlng[0] * 1.5)
            {
                fixeb = 0;
                fu.Draw(contours, ebow[1].Key, new Bgr(Color.Red), 2);
                fu.Draw(new CircleF(leftmost[1], 5), new Bgr(Color.Green), 0);
                fu.Draw(new CircleF(rightmost[1], 5), new Bgr(Color.Green), 0);
            }
            else
            {
                fixeb = 2;
                fu.Draw(contours, ebow[0].Key, new Bgr(Color.Red), 2);
                fu.Draw(new CircleF(leftmost[0], 5), new Bgr(Color.Green), 0);
                fu.Draw(new CircleF(rightmost[0], 5), new Bgr(Color.Green), 0);
                fu.Draw(contours, ebow[1].Key, new Bgr(Color.Red), 2);
                fu.Draw(new CircleF(leftmost[1], 5), new Bgr(Color.Green), 0);
                fu.Draw(new CircleF(rightmost[1], 5), new Bgr(Color.Green), 0);
            }
            if (fixeb == 1 || fixeb == 0)
            {
                Image<Gray, byte> fixebowotsu = otsuimg.Clone();
                int stX = 0;

                if (ebow[fixeb].Value.X > idkwid.Width / 2)
                    stX = idkwid.Width / 2;
                fixebowotsu.ROI = new System.Drawing.Rectangle(stX + idk.X, (int)upmost.Y, idkwid.Width / 2, (int)(dnmost.Y - upmost.Y));
                fixebowotsu._Not();
                //-------------------
                VectorOfVectorOfPoint recontours = new VectorOfVectorOfPoint();
                MCvMoments remoments;
                double rearea;
                MCvPoint2D64f recenter;
                int renn;

                CvInvoke.FindContours(fixebowotsu.Mat, recontours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                renn = recontours.Size;

                Dictionary<int, MCvPoint2D64f> rec = new Dictionary<int, MCvPoint2D64f>();
                for (int i = 0; i < renn; i++)
                {
                    rearea = CvInvoke.ContourArea(recontours[i], false);
                    remoments = CvInvoke.Moments(recontours[i]);
                    recenter = remoments.GravityCenter;
                    if (rearea > 500)
                    {
                        rec.Add(i, recenter);
                    }
                }
                var redicSort = from objDic in rec orderby objDic.Value.Y ascending select objDic;
                PointF l = new PointF(), r = new PointF();
                foreach (var item in redicSort)
                {
                    fu.ROI = new System.Drawing.Rectangle(stX, fixebowotsu.ROI.Y, fixebowotsu.Width, fixebowotsu.Height);
                    fu.Draw(recontours, item.Key, new Bgr(Color.Red), 2);
                    for (int i = 0; i < recontours[item.Key].Size; i++)
                    {
                        if (l.IsEmpty)
                        {
                            l = recontours[item.Key][i];
                        }
                        else if (l.X > recontours[item.Key][i].X)
                        {
                            l = recontours[item.Key][i];
                        }
                        if (r.IsEmpty)
                        {
                            r = recontours[item.Key][i];
                        }
                        else if (r.X < recontours[item.Key][i].X)
                        {
                            r = recontours[item.Key][i];
                        }
                    }
                    fu.Draw(new CircleF(l, 10), new Bgr(Color.Green), 0);
                    fu.Draw(new CircleF(r, 10), new Bgr(Color.Green), 0);
                    fu.ROI = new System.Drawing.Rectangle();
                    break;
                }
            }
            //-------------------

            //idkwid.ROI = new System.Drawing.Rectangle(0, (int)upmost.Y, idkwid.Width, (int)(dnmost.Y - upmost.Y));


            //foreach (var item in dicSort)
            //{
            //    if (cou == 1 || cou == 2)
            //    {
            //        fu.Draw(contours, item.Key, new Bgr(Color.Red), 2);
            //        PointF lm = new PointF(), rm = new PointF();
            //        if (lm.IsEmpty) { Console.Write("empty true"); }
            //        for (int i = 0; i < contours[item.Key].Size; ++i)
            //        {
            //            if (lm.IsEmpty)
            //            {
            //                lm = contours[item.Key][i];
            //            }
            //            else if (lm.X > contours[item.Key][i].X)
            //            {
            //                lm = contours[item.Key][i];
            //            }
            //            if (rm.IsEmpty)
            //            {
            //                rm = contours[item.Key][i];
            //            }
            //            else if (rm.X < contours[item.Key][i].X)
            //            {
            //                rm = contours[item.Key][i];
            //            }
            //        }
            //        fu.Draw(new CircleF(lm, 10), new Bgr(Color.Green), 0);
            //        fu.Draw(new CircleF(rm, 10), new Bgr(Color.Green), 0);
            //    }
            //    cou++;
            //}


            //****Lipps
            Image<Gray, byte> red = new Image<Gray, byte>(Ori_Image.Width, Ori_Image.Height);

            for (int i = 0; i < Ori_Image.Height; i++)
            {
                for (int j = 0; j < Ori_Image.Width; j++)
                {
                    double h, s, v;
                    ColorToHSV(Ori_Image[i, j], out h, out s, out v);
                    if ((h < 20 || h > 340) && s > 0.2)
                    {
                        red[i, j] = new Gray(255);
                    }
                }
            }
            VectorOfVectorOfPoint contours2 = new VectorOfVectorOfPoint();
            MCvMoments moments2;
            double area2;
            MCvPoint2D64f center2;
            int nn2;

            CvInvoke.FindContours(red.Mat, contours2, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            nn2 = contours2.Size;

            Dictionary<int, double> d = new Dictionary<int, double>();
            for (int i = 0; i < nn2; i++)
            {
                area2 = CvInvoke.ContourArea(contours2[i], false);
                moments2 = CvInvoke.Moments(contours2[i]);
                center2 = moments2.GravityCenter;
                if (center2.Y > red.Height / 2 && area2 > 1000)
                {
                    d.Add(i, area2);
                }

            }
            var dicSort2 = from objDic in d orderby objDic.Value descending select objDic;
            int cou2 = 0;
            foreach (var item in dicSort2)
            {
                if (cou2 != 0)
                {
                    break;
                }
                fu.Draw(contours2, item.Key, new Bgr(Color.Red), 2);
                cou2++;
            }
            //fu.ROI = new System.Drawing.Rectangle(0, idk.Y, f.Width, idk.Height);
            //****end Lipps


            imageBox1.Image = fu;

            imageBox1.Image =//Ori_Image.Convert<Gray, byte>();
                             fu;
            imageBox5.Image = new Image<Bgr, byte>("eyes\\n_0.png");
            imageBox6.Image = new Image<Bgr, byte>("eyes\\n_0.png");

            imageBox4.Image = fu;

            Ori_Image.ROI = idk;

            // Particle Filter method
            doppff = true;
            weightlist = new List<KeyValuePair<double, basicparcitlt>>();

            maxw = double.MaxValue;
            timercounter = 0;
            loopcounter = 0;
            ma = null;
            ma_one = null;
            sum[0] = 0;
            sum[1] = 0;
            sum[2] = 0;
            sum[3] = 0;
            //timer2.Enabled = true;
            //timer2.Start();
        }
        double[] sum = new double[4];
        Particle_parameter_for_fullimg ppff;
        Image<Bgr, Byte> cs;
        Parcitle par;
        int parcount;
        private void timer2_Tick(object sender, EventArgs e)
        {
            label4.Text = timercounter.ToString();


            if (doppff)
            {
                ppff = new Particle_parameter_for_fullimg(Ori_Image);
                cs = grayimgtobgr(Ori_Image.Convert<Gray, byte>());
                //imageBox2.Image = ppff.Gradpic();
                imageBox4.Image = Ori_Image;
                doppff = false;

            }
            Image<Bgr, byte> draw = cs.Clone();
            #region normalloop
            if (loopmode == 0)
            {
                if (timercounter >= numofpar)
                {
                    //weightlist.Sort((pair1, pair2) => pair1.Key.CompareTo(pair2.Key) * -1);
                    double max = 0;
                    basicparcitlt temp = weightlist[0].Value;
                    double totle = 0;
                    foreach (KeyValuePair<double, basicparcitlt> kp in weightlist)
                    {
                        totle += kp.Key;
                        if (kp.Key > max)
                        {
                            max = kp.Key;
                            temp = kp.Value;
                        }
                    }

                    label6.Text = "\n" + max;
                    par = new Parcitle(temp);
                    draw = cs.Clone();
                    par.Graddraw(ref draw);
                    imageBox3.Image = draw;


                    double alpha = 1 / totle;
                    List<KeyValuePair<double, basicparcitlt>> templist = new List<KeyValuePair<double, basicparcitlt>>();
                    totle = 0;
                    label5.Text = "";
                    foreach (KeyValuePair<double, basicparcitlt> kp in weightlist)
                    {
                        double scaledval = kp.Key * alpha;
                        //label1.Text += scaledval + "\n";
                        totle += scaledval;
                        templist.Add(new KeyValuePair<double, basicparcitlt>(totle, kp.Value));
                    }
                    Random rand = new Random();
                    //if (nextlist != null)
                    //    nextlist.Clear();
                    nextlist = new List<KeyValuePair<double, basicparcitlt>>();
                    for (int i = 0; i < weightlist.Count; i++)
                    {
                        double pick = rand.NextDouble();
                        foreach (var kp in templist)
                        {
                            if (kp.Key >= pick)
                            {
                                nextlist.Add(kp);
                                break;
                            }
                        }

                    }

                    if (nextlist.Count != weightlist.Count) label4.Text += "count not fit";
                    label6.Text = "";
                    //foreach (var kp in nextlist)
                    //{
                    //    label2.Text += kp.Key + "\n";
                    //}


                    timercounter = 0;
                    timer2.Stop();
                    loopcounter++;
                    if (loopcounter < looplimit)
                    {
                        weightlist = new List<KeyValuePair<double, basicparcitlt>>();
                        timer2.Start();
                    }
                    else
                    {
                        weightlist.Sort((pair1, pair2) => pair1.Key.CompareTo(pair2.Key) * -1);
                        //label2.Text = "\n" + weightlist[0].Key.ToString("0.###E000");
                        par = new Parcitle(weightlist[0].Value);

                        double d1 = par.Gradient(ref ppff);
                        double d2 = par.Saturation(ref ppff);
                        double d3 = par.symmetric(ref ppff);
                        double d4 = par.corner(ref ppff);
                        this.label6.Text = "\n" + d1.ToString("0.###E000") + "\n" + d2.ToString("0.###E000") + "\n" + d3.ToString("0.###E000") + "\n" + d4.ToString("0.###E000") + "\n" + weightlist[0].Key.ToString("0.###E000");

                        draw = cs.Clone();
                        par.Graddraw(ref draw);
                        par = new Parcitle(new PointF(507, 303), 717, 233, 0, 0.85, 0.85);

                        d1 = par.Gradient(ref ppff);
                        d2 = par.Saturation(ref ppff);
                        d3 = par.symmetric(ref ppff);
                        d4 = par.corner(ref ppff);
                        this.label5.Text = "\n" + d1.ToString("0.###E000") + "\n" + d2.ToString("0.###E000") + "\n" + d3.ToString("0.###E000") + "\n" + d4.ToString("0.###E000") + "\n" + par.getweight(d1, d2, d3, d4).ToString("0.###E000");
                        par.Graddraw(ref draw);
                        imageBox3.Image = draw;
                    }

                }
                else
                {
                    if (nextlist == null)
                    {
                        if (timercounter <= 30)
                            par = new Parcitle(new PointF(507, 303), 717, 233, 0, 0.85, 0.85);
                        else
                            par = new Parcitle(Ori_Image);
                    }
                    else
                        par = new Parcitle(nextlist[timercounter].Value);
                    double weightnow = par.getweight(ref ppff);
                    weightlist.Add(new KeyValuePair<double, basicparcitlt>(weightnow, par.getbasic()));
                    par.Graddraw(ref draw);
                    this.label6.Text = "\n" + weightnow.ToString("0.###E000");
                    this.imageBox3.Image = draw;
                    //Console.WriteLine(weightnow + "\n");
                }

                timercounter++;
            }
            #endregion
            #region testloop
            else if (loopmode == 1)
            {
                if (timer2.Interval == 50)
                {
                    timer2.Interval = 60;
                    numofpar = 1000;
                    looplimit = 1;
                }
                if (timercounter >= numofpar)
                {
                    timercounter = 0;
                    //timer2.Stop();
                }
                else
                {
                    if (slowshowstate % 5 == 0)
                    {
                        if (nextlist == null)
                        {
                            //if (timercounter <= 0)
                            //    par = new Parcitle(new PointF(540, 290), 620, 258, 0, 0.8);
                            //else
                            //    par = new Parcitle(Ori_Image);
                            par = new Parcitle(Ori_Image);
                            //if (maxd3 > 0.8) {
                            //    Console.WriteLine(ma.center+" "+ ppff.width+" "+ ppff.height);
                            //    par = new Parcitle(ma.center, ppff.width, ppff.height);
                            //}
                            //else
                            //    par = new Parcitle(Ori_Image);
                        }
                        else
                            par = new Parcitle(nextlist[timercounter].Value);

                        double d1 = par.Gradient(ref ppff);
                        par.Graddraw(ref draw);
                        if (ma != null)
                            ma.Graddraw(ref draw);
                        this.label6.Text = "\n" + d1.ToString("##0.###");
                        if (d1 > maxd1)
                            this.label6.Text += " ⁂";
                        this.imageBox3.Image = draw;
                        sum[0] += d1;
                        slowshowstate++;
                        //}
                        //else if (slowshowstate % 5 == 1)
                        //{
                        double d2 = par.Saturation(ref ppff);
                        //par.Graddraw(ref draw);
                        ////par.Satudraw(ref draw);
                        //if (ma != null)
                        //    ma.Graddraw(ref draw);
                        //this.imageBox1.Image = draw;
                        this.label6.Text += "\n" + d2.ToString("##0.###");
                        if (d2 > maxd2)
                            this.label6.Text += " ⁂";
                        sum[1] += d2;
                        slowshowstate++;

                        //}
                        //else if (slowshowstate % 5 == 2)
                        //{
                        double d3 = par.symmetric(ref ppff);
                        //par.Graddraw(ref draw);
                        ////par.symmdraw(ref ppff, ref draw);
                        //if (ma != null)
                        //    ma.Graddraw(ref draw);
                        //this.imageBox1.Image = draw;
                        this.label6.Text += "\n" + d3.ToString("##0.###");
                        if (d3 > maxd3)
                            this.label6.Text += " ⁂";
                        sum[2] += d3;
                        slowshowstate++;
                        //}
                        //else if (slowshowstate % 5 == 3)
                        //{
                        double d4 = par.corner(ref ppff);
                        //par.Graddraw(ref draw);
                        ////par.corndraw(ref ppff, ref draw);
                        //if (ma != null)
                        //    ma.Graddraw(ref draw);
                        //this.imageBox1.Image = draw;
                        this.label6.Text += "\n" + d4.ToString("##0.###");
                        if (d4 > maxd4)
                            this.label6.Text += " ⁂";
                        sum[3] += d4;
                        slowshowstate++;
                        //}
                        //else if (slowshowstate % 5 == 4)
                        //{
                        parcount++;
                        double weightnow = par.getweight(d1, sum[0] / parcount, d2, sum[1] / parcount, d3, sum[2] / parcount, d4, sum[3] / parcount);

                        weightlist.Add(new KeyValuePair<double, basicparcitlt>(weightnow, par.getbasic()));
                        //par.Graddraw(ref draw);
                        //if (ma != null)
                        //    ma.Graddraw(ref draw);
                        //this.imageBox1.Image = draw;

                        this.label6.Text += "\n" + weightnow.ToString("0.###E000");
                        //if (weightnow > maxw)
                        //    this.label2.Text += " ⁂";
                        if (ma != null)
                            maxw = ma.getweight(maxd1, sum[0] / parcount, maxd2, sum[1] / parcount, maxd3, sum[2] / parcount, maxd4, sum[3] / parcount);

                        label5.Text = "\n" + maxd1.ToString("##0.###") + "\n" + maxd2.ToString("##0.###") + "\n" + maxd3.ToString("##0.###") + "\n" + maxd4.ToString("##0.###") + "\n" + maxw.ToString("0.###E000");

                        label7.Text = "\n" + (sum[0] / parcount).ToString("0.###E000") + "\n" + (sum[1] / parcount).ToString("0.###E000") + "\n" + (sum[2] / parcount).ToString("0.###E000") + "\n" + (sum[3] / parcount).ToString("0.###E000");

                        //label1.Text = "\n" + maxw.ToString("0.###E000");

                        if (weightnow < maxw || ma == null)
                        {
                            maxw = weightnow;
                            ma = new Parcitle(par.getbasic());
                            maxd1 = d1;
                            maxd2 = d2;
                            maxd3 = d3;
                            maxd4 = d4;

                        }
                        //if(weightnow < maxw)
                        //{
                        //    timer2.Stop();
                        //}

                        slowshowstate++;
                        timercounter++;
                        //timer2.Stop();
                    }



                }


            }
            #endregion
            #region 3steploop
            else if (loopmode == 2)
            {
                if (timer2.Interval == 50)
                {
                    timer2.Interval = 60;
                    numofpar = 1000;
                    looplimit = 4;
                }
                if (timercounter >= numofpar)
                {
                    timercounter = 0;
                    loopcounter++;
                    Console.WriteLine("next loop");
                    sum[0] = 0;
                    sum[1] = 0;
                    sum[2] = 0;
                    sum[3] = 0;
                    if (loopcounter == 3)
                    {
                        ma_one = new Parcitle(ma.getbasic());
                        ma = null;
                    }
                }
                if (loopcounter >= looplimit)
                {
                    timer2.Stop();
                    ma.Graddraw(ref draw);
                    if (ma_one != null)
                        ma_one.Graddraw(ref draw);
                    this.imageBox3.Image = draw;
                }
                else
                {
                    if (slowshowstate % 5 == 0)
                    {

                        if (loopcounter == 1)
                        {
                            par = new Parcitle(ma.center, ppff.width, ppff.height, 3);
                        }
                        else if (loopcounter == 2)
                        {
                            par = new Parcitle(ma, ppff.width, ppff.height);
                        }
                        else if (loopcounter == 3)
                        {
                            PointF na = new PointF(ppff.width - ma_one.center.X, ma_one.center.Y);
                            par = new Parcitle(na, ppff.width, ppff.height, 3);
                        }
                        else
                        {
                            par = new Parcitle(Ori_Image);
                        }


                        double d1 = par.Gradient(ref ppff);
                        par.Graddraw(ref draw);
                        if (ma != null)
                            ma.Graddraw(ref draw);
                        if (ma_one != null)
                            ma_one.Graddraw(ref draw);
                        this.label6.Text = "\n" + d1.ToString("##0.##");
                        if (d1 > maxd1)
                            this.label6.Text += " ⁂";
                        this.imageBox3.Image = draw;
                        sum[0] += d1;
                        slowshowstate++;

                        double d2 = par.Saturation(ref ppff);
                        this.label6.Text += "\n" + d2.ToString("##0.##");
                        if (d2 > maxd2)
                            this.label6.Text += " ⁂";
                        sum[1] += d2;
                        slowshowstate++;

                        double d3 = par.symmetric(ref ppff);
                        this.label6.Text += "\n" + d3.ToString("##0.##");
                        if (d3 > maxd3)
                            this.label6.Text += " ⁂";
                        sum[2] += d3;
                        slowshowstate++;

                        double d4 = par.corner(ref ppff);
                        this.label6.Text += "\n" + d4.ToString("##0.##");
                        if (d4 > maxd4)
                            this.label6.Text += " ⁂";
                        sum[3] += d4;
                        slowshowstate++;

                        parcount++;
                        double weightnow = par.getweight(d1, sum[0] / parcount, d2, sum[1] / parcount, d3, sum[2] / parcount, d4, sum[3] / parcount, loopcounter);

                        //weightlist.Add(new KeyValuePair<double, basicparcitlt>(weightnow, par.getbasic()));

                        this.label6.Text += "\n" + weightnow.ToString("0.##E##0");
                        if (ma != null)
                            maxw = ma.getweight(maxd1, sum[0] / parcount, maxd2, sum[1] / parcount, maxd3, sum[2] / parcount, maxd4, sum[3] / parcount, loopcounter);

                        label5.Text = "\n" + maxd1.ToString("##0.##") + "\n" + maxd2.ToString("##0.##") + "\n" + maxd3.ToString("##0.##") + "\n" + maxd4.ToString("##0.##") + "\n" + maxw.ToString("0.##E##0");

                        //label3.Text = "\n" + (sum[0] / parcount).ToString("0.##E##0") + "\n" + (sum[1] / parcount).ToString("0.##E##0") + "\n" + (sum[2] / parcount).ToString("0.##E##0") + "\n" + (sum[3] / parcount).ToString("0.##E##0");

                        //label1.Text = "\n" + maxw.ToString("0.###E000");

                        if (weightnow < maxw || ma == null)
                        {
                            maxw = weightnow;
                            ma = new Parcitle(par.getbasic());
                            maxd1 = d1;
                            maxd2 = d2;
                            maxd3 = d3;
                            maxd4 = d4;
                        }

                        slowshowstate++;
                        timercounter++;
                    }
                }
            }
            #endregion
            else if (loopmode == 3)
            {
                if (timer2.Interval == 50)
                {
                    timer2.Interval = 30;
                    numofpar = 1000;
                    looplimit = 6;
                }
                if (timercounter >= numofpar)
                {
                    timercounter = 0;
                    loopcounter++;
                    Console.WriteLine("next loop" + loopcounter.ToString());
                    sum[0] = 0;
                    sum[1] = 0;
                    sum[2] = 0;
                    sum[3] = 0;
                    if (loopcounter == 3)
                    {
                        ma_one = new Parcitle(ma.getbasic());
                        ma = null;
                    }
                }
                if (loopcounter >= looplimit)
                {
                    //draw=ppff.getcornordraw();
                    List<PointF> cons = new List<PointF>();
                    cons.Add(ma.right);
                    cons.Add(ma.left);
                    ma.fixtip(ref ppff);
                    ma.Graddraw(ref draw);
                    Console.WriteLine("end");
                    ma.symmetric(ref ppff);
                    ma.irsdraw(ref draw);
                    if (ma_one != null)
                    {
                        cons.Add(ma_one.right);
                        cons.Add(ma_one.left);
                        ma_one.fixtip(ref ppff);
                        ma_one.Graddraw(ref draw);
                        ma_one.symmetric(ref ppff);
                        ma_one.irsdraw(ref draw);
                    }
                    Image<Bgr, byte> leye, reye;
                    leye = Ori_Image.Copy();
                    reye = leye.Clone();
                    int lew = (int)ma.width;
                    int leh = (int)ma.height;
                    int lex = (int)(ma.left.X - 0.1 * lew);
                    int ley = (int)(ma.up.Y - 0.2 * leh);
                    lew = (int)(lew * 1.2);
                    leh = (int)(leh * 1.4);
                    leye.ROI = new System.Drawing.Rectangle(lex, ley, lew, leh);
                    int rew = (int)ma_one.width;
                    int reh = (int)ma_one.height;
                    int rex = (int)(ma_one.left.X - 0.1 * rew);
                    int rey = (int)(ma_one.up.Y - 0.2 * reh);
                    rew = (int)(rew * 1.2);
                    reh = (int)(reh * 1.4);
                    reye.ROI = new System.Drawing.Rectangle(rex, rey, rew, reh);
                    //imageBox_out.Image = leye;
                    //imageBox3.Image = reye;

                    #region oldcyclemark

                    Image<Gray, byte> tempimg = leye.Convert<Gray, byte>();
                    Image<Gray, byte> tempthred = tempimg.CopyBlank();
                    double blacklimit = CvInvoke.Threshold(tempimg, tempthred, 0, 255, ThresholdType.Otsu);

                    int cythred = 60;
                    CircleF[] circles;
                    List<CircleF> eyes;
                    do
                    {
                        circles =
                        CvInvoke.HoughCircles(tempthred,
                        HoughType.Gradient,
                        2.0,//Resolution of the accumulator used to detect centers of the circles
                        1.0,//min distance 
                        40,
                        cythred,
                        30,//min radius
                        60//max radius
                        );

                        eyes = new List<CircleF>();
                        PointF rad = ma.getirs();
                        foreach (CircleF cy in circles)
                        {
                            if ((cy.Center.X - rad.X) * (cy.Center.X - rad.X) + (cy.Center.Y - rad.Y) * (cy.Center.Y - rad.Y) < cy.Radius * cy.Radius)
                            {

                                eyes.Add(cy);
                            }
                        }
                        cythred -= 5;
                        if (cythred <= 0)
                        {
                            Console.WriteLine("cythred=0");
                            if (eyes.Count == 0)
                                foreach (CircleF cy in circles)
                                {
                                    eyes.Add(cy);
                                }
                            break;
                        }

                    } while (eyes.Count <= 5);

                    Image<Gray, Byte> bgcanny = getbgedge(leye);
                    eyes = vertifycyclefromroi(eyes, bgcanny, tempthred, ma.getirs());
                    foreach (CircleF cy in eyes)
                    {

                        leye.Draw(cy, new Bgr(Color.Red), 3);
                        draw.Draw(new CircleF(new PointF(cy.Center.X + leye.ROI.X, cy.Center.Y + leye.ROI.Y), cy.Radius), new Bgr(Color.Red), 3);
                    }

                    tempimg = reye.Convert<Gray, byte>();
                    tempthred = tempimg.CopyBlank();
                    blacklimit = CvInvoke.Threshold(tempimg, tempthred, 0, 255, ThresholdType.Otsu);
                    cythred = 60;
                    do
                    {
                        circles =
                        CvInvoke.HoughCircles(tempthred,
                        HoughType.Gradient,
                        2.0,//Resolution of the accumulator used to detect centers of the circles
                        1.0,//min distance 
                        40,
                        cythred,
                        30,//min radius
                        60//max radius
                        );
                        eyes = new List<CircleF>();
                        PointF rad = ma_one.getirs();
                        foreach (CircleF cy in circles)
                        {
                            if ((cy.Center.X - rad.X) * (cy.Center.X - rad.X) + (cy.Center.Y - rad.Y) * (cy.Center.Y - rad.Y) < cy.Radius * cy.Radius)
                            {
                                //leye.Draw(cy, new Gray(0), 5);
                                //fir = false;
                                eyes.Add(cy);
                            }
                        }
                        cythred -= 5;
                        if (cythred <= 0)
                        {
                            Console.WriteLine("cythred=0");
                            if (eyes.Count == 0)
                                foreach (CircleF cy in circles)
                                {
                                    eyes.Add(cy);
                                }
                            break;
                        }
                    } while (eyes.Count <= 5);
                    bgcanny = getbgedge(reye);
                    eyes = vertifycyclefromroi(eyes, bgcanny, tempthred, ma_one.getirs());
                    foreach (CircleF cy in eyes)
                    {
                        reye.Draw(cy, new Bgr(Color.Red), 3);
                        draw.Draw(new CircleF(new PointF(cy.Center.X + reye.ROI.X, cy.Center.Y + reye.ROI.Y), cy.Radius), new Bgr(Color.Red), 3);
                    }




                    imageBox5.Image = leye;
                    imageBox6.Image = reye;
                    #endregion
                    foreach (var p in cons)
                    {

                        draw.Draw(new CircleF(p, 3), new Bgr(Color.Red), 5);
                    }

                    this.imageBox3.Image = draw;
                    imageBox1.Image = My_Image2;

                    timer2.Stop();
                }
                else
                {


                    if (loopcounter == 1)
                    {
                        par = new Parcitle(ma.center, ppff.width, ppff.height, 3);
                    }
                    else if (loopcounter == 2)
                    {
                        par = new Parcitle(ma, ppff.width, ppff.height);
                    }
                    else if (loopcounter == 3)
                    {
                        PointF na = new PointF(ppff.width - ma_one.center.X, ma_one.center.Y);
                        par = new Parcitle(na, ppff.width, ppff.height, 30);
                    }
                    else if (loopcounter == 4)
                    {
                        par = new Parcitle(ma.center, ppff.width, ppff.height, 3);
                    }
                    else if (loopcounter == 5)
                    {
                        par = new Parcitle(ma, ppff.width, ppff.height);
                    }
                    else //0
                    {
                        par = new Parcitle(Ori_Image);
                    }


                    double d1 = par.Gradient(ref ppff);
                    par.Graddraw(ref draw);
                    if (ma != null)
                        ma.Graddraw(ref draw);
                    if (ma_one != null)
                        ma_one.Graddraw(ref draw);
                    this.label6.Text = "\n" + d1.ToString("##0.##");
                    if (d1 > maxd1)
                        this.label6.Text += " ⁂";
                    this.imageBox3.Image = draw;
                    sum[0] += d1;

                    if (loopcounter == 1) { }
                    double d2 = par.Saturation(ref ppff);
                    this.label6.Text += "\n" + d2.ToString("##0.##");
                    if (d2 > maxd2)
                        this.label6.Text += " ⁂";
                    sum[1] += d2;

                    double d3 = par.symmetric(ref ppff);
                    this.label6.Text += "\n" + d3.ToString("##0.##");
                    if (d3 > maxd3)
                        this.label6.Text += " ⁂";
                    sum[2] += d3;

                    double d4 = par.corner(ref ppff);
                    this.label6.Text += "\n" + d4.ToString("##0.##");
                    if (d4 > maxd4)
                        this.label6.Text += " ⁂";
                    sum[3] += d4;

                    parcount++;
                    double weightnow = par.getweight(d1, sum[0] / parcount, d2, sum[1] / parcount, d3, sum[2] / parcount, d4, sum[3] / parcount, loopcounter);

                    //weightlist.Add(new KeyValuePair<double, basicparcitlt>(weightnow, par.getbasic()));

                    this.label6.Text += "\n" + weightnow.ToString("##0.##");
                    if (ma != null)
                        maxw = ma.getweight(maxd1, sum[0] / parcount, maxd2, sum[1] / parcount, maxd3, sum[2] / parcount, maxd4, sum[3] / parcount, loopcounter);

                    label5.Text = "\ngradian     : " + maxd1.ToString("##0.##") + "\nsaturation: " + maxd2.ToString("##0.##") + "\nsymmtric  : " + maxd3.ToString("##0.##") + "\ncornor       : " + maxd4.ToString("##0.##") + "\n" + maxw.ToString("##0.##");

                    //label3.Text = "\n" + (sum[0] / parcount).ToString("0.##E##0") + "\n" + (sum[1] / parcount).ToString("0.##E##0") + "\n" + (sum[2] / parcount).ToString("0.##E##0") + "\n" + (sum[3] / parcount).ToString("0.##E##0");

                    //label1.Text = "\n" + maxw.ToString("0.###E000");
                    if (loopcounter == 0)
                    {
                        if (d3 > maxd3 || (d3 == maxd3 && weightnow < maxw) || ma == null)
                        {
                            maxw = weightnow;
                            ma = new Parcitle(par.getbasic());
                            maxd1 = d1;
                            maxd2 = d2;
                            maxd3 = d3;
                            maxd4 = d4;
                        }
                    }
                    else
                    {
                        if (weightnow < maxw || ma == null)
                        {
                            maxw = weightnow;
                            ma = new Parcitle(par.getbasic());
                            maxd1 = d1;
                            maxd2 = d2;
                            maxd3 = d3;
                            maxd4 = d4;
                        }
                    }


                    timercounter++;

                }
            }




        }
        private List<CircleF> vertifycyclefromroi(List<CircleF> circles, Image<Gray, byte> edge, Image<Gray, byte> black, PointF rad)
        {
            if (circles.Count < 1)
            {
                Console.WriteLine("no cy to vertify");
                return circles;
            }
            else
            {
                Console.WriteLine("cy num:" + circles.Count);
            }
            List<CircleF> result = new List<CircleF>();


            //fortestcyl = cyl;
            //fortestcyr = cyr;
            Dictionary<CircleF, double> leyever = new Dictionary<CircleF, double>();
            Dictionary<CircleF, double> leyebla = new Dictionary<CircleF, double>();
            foreach (CircleF cy in circles)
            {
                Image<Gray, byte> cyedg = edge.CopyBlank();
                cyedg.Draw(cy, new Gray(255), 3);
                Image<Gray, byte> cybla = edge.CopyBlank();
                cybla.Draw(cy, new Gray(255), 0);
                leyever.Add(cy, 0);
                leyebla.Add(cy, 0);
                //double h, s, v;
                for (int i = (int)(cy.Center.X - cy.Radius); i < (int)(cy.Center.X + cy.Radius) && i < edge.Width; ++i)
                {
                    for (int j = (int)(cy.Center.Y - cy.Radius); j < (int)(cy.Center.Y + cy.Radius) && j < edge.Height; ++j)
                    {
                        //if (i < 0 || i > cyedg.Width || j < 0 || j > cyedg.Height)
                        //    Console.WriteLine("*"+cy.Center+ cyedg.Size+i+" "+j);
                        if (i < 0) i = 0;
                        if (j < 0) j = 0;
                        if (cyedg[j, i].Intensity == 255 && edge[j, i].Intensity == 0)
                        {
                            leyever[cy] += 1;
                        }
                        if (cybla[j, i].Intensity == 255 && black[j, i].Intensity == 0)
                        {
                            leyebla[cy] += 1;
                        }
                    }
                }
            }
            double max = 0;
            CircleF maxcy = circles[0];
            foreach (KeyValuePair<CircleF, double> kvp in leyever)
            {
                if (kvp.Value > max)
                {
                    max = kvp.Value;
                }
            }
            max = 0;
            foreach (KeyValuePair<CircleF, double> kvp in leyebla)
            {
                if (kvp.Value > max)
                {
                    max = kvp.Value;
                }
            }
            foreach (CircleF cy in circles)
            {
                leyebla[cy] /= max;
                leyever[cy] += 1.5 * leyebla[cy];
                //leyever[cy] -= Math.Sqrt((rad.X - cy.Center.X) * (rad.X - cy.Center.X) + (rad.Y - cy.Center.Y) * (rad.Y - cy.Center.Y));
            }
            var dicSort = from objDic in leyever orderby objDic.Value descending select objDic;

            result.Add(dicSort.FirstOrDefault().Key);
            //foreach (var item in dicSort)
            //{
            //    Console.Write(item.Value + " ");
            //}


            return result;


        }

        private void 眼部黑白ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScleraPupil_ratio SPr = new ScleraPupil_ratio(My_Image2);
            Bitmap[] imgs = SPr.GetEyesThreshold();
            Image<Bgr, Byte>[] imgBgr = SPr.GetEyeImg(My_Image2);
            imgs[0] = MyCV.BoundingBoxeyebrow(new Image<Gray, byte>(imgs[0]), imgBgr[0]);
            imgs[1] = MyCV.BoundingBoxeyebrow(new Image<Gray, byte>(imgs[1]), imgBgr[1]);
            imageBox5.Image = new Image<Bgr, Byte>(imgs[0]);
            imageBox6.Image = new Image<Bgr,Byte>(imgs[1]);
        }
    }
}
