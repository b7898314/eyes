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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using bearing;

namespace eyes
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        VideoCapture webCam;
        Image<Gray, Byte> My_Image1;
        Image<Bgr, Byte> My_Image2;

        private void Form3_Load(object sender, EventArgs e)
        {
            webCam = new VideoCapture(0);
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 900);//900*675是4:3最大，解析度再上去就會強制16:9
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 675);
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, 160);
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus, 20);
            webCam.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.XiAutoWb, 0);

            Application.Idle += Application_Idle;
        }
        void Application_Idle(object sender, EventArgs e)
        {
            try
            {
                Image<Bgr, Byte> camImage = webCam.QueryFrame().ToImage<Bgr, Byte>();
                Image<Bgr, Byte> VideoC = new Image<Bgr, Byte>(camImage.Bitmap);
                Image<Bgr, Byte> img = new Image<Bgr, Byte>(VideoC.Bitmap);
                Image<Bgr, Byte> result = img.Copy();
                //scale(img, ref result);
                imageBox1.Image = result;
                //imageBox1.Image = VideoC;
            }
            catch { }
        }

        SectionDetection sectionDetection = new SectionDetection();
        private void scale(Image<Bgr, Byte> src, ref Image<Bgr, Byte> dest)
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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string strPicFile = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".bmp";
            imageBox1.Image.Save(Application.StartupPath + strPicFile);//照日期存檔
            Form1 Form1 = (Form1)this.Tag;
            Form1.camera = imageBox1.Image.Bitmap;
            //Form1.imageBox1.Image = new Image<Bgr,byte>(imageBox1.Image.Bitmap);

            My_Image1 = new Image<Gray, byte>(Application.StartupPath + strPicFile);
            My_Image2 = new Image<Bgr, byte>(Application.StartupPath + strPicFile);
            Form1.My_Image1 = My_Image1;
            Form1.My_Image2 = My_Image2;
            CascadeClassifier frontalface = new CascadeClassifier("haarcascade_frontalface_default.xml");
            Rectangle[] faces = frontalface.DetectMultiScale(My_Image1, 1.1, 5, new Size(200, 200), Size.Empty);
            

            List<Rectangle> face = new List<Rectangle>();
            face.AddRange(faces);

            foreach (Rectangle face1 in face)
            {
                // My_Image2.Draw(face1, new Bgr(Color.Red), 2);
            }

            //眼睛
            if (faces.Length != 0)
            {
                Form1.facecutori = new Image<Bgr, Byte>(My_Image2.Bitmap);
                Form1.facecutori.ROI = faces[0];
                Form1.facecutorigray = new Image<Gray, Byte>(My_Image1.Bitmap);
                Form1.facecutorigray.ROI = faces[0];

                int zoomface = 60;
                for (int i = 0; i < faces.Length; i++)//調整臉範圍大小
                {
                    faces[i].X = faces[i].X - zoomface;
                    faces[i].Y = faces[i].Y - zoomface * 2;
                    faces[i].Width = faces[i].Width + zoomface * 2;
                    faces[i].Height = faces[i].Height + zoomface * 4;
                }
                Form1.facecut = new Image<Bgr, Byte>(My_Image2.Bitmap);
                Form1.facecut.ROI = faces[0];
                Form1.imageBox1.Image = My_Image2;
            }


            //Application.Idle -= Application_Idle;
            this.Close();
        }
        
    }
}
