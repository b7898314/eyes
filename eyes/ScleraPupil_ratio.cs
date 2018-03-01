using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;

namespace eyes
{
    class ScleraPupil_ratio
    {
        private Image<Bgr, byte> faceBgr;
        private Image<Gray, byte> OriGray;
        private Image<Bgr, byte>[] Eyes = new Image<Bgr, byte>[2];
        int num = 0;

        public ScleraPupil_ratio(Image<Bgr, Byte> OriganImg) {
            this.faceBgr = OriganImg.Clone();
            this.OriGray = OriganImg.Convert<Gray, Byte>();
        }

        public Bitmap[] GetEyesThreshold() {
            Image<Bgr, Byte>[] Eyes = GetEyeImg(faceBgr);
            Bitmap[] EyesGray = new Bitmap[2];

            foreach (var e in Eyes)
            {
                int w = e.Bitmap.Width;
                int h = e.Bitmap.Height;
                EyesGray[num] = new Bitmap(w, h);

                for (int y = 0; y < h; y++)//find skin
                {
                    for (int x = 0; x < w; x++)
                    {
                        Bgr color = e[y, x];
                        Double R = color.Red;
                        Double G = color.Green;
                        Double B = color.Blue;
                        


                        if ((R > 95 && G > 40 && B > 20 && (R - B) > 15 && (R - G) > 15)//皮膚 
                            || (R > 200 && G > 210 && B > 170 && (R - B) <= 15 && R > B && G > B)//皮膚 
                            || (R < 50 && G < 50 && B < 50))//瞳孔
                        {
                            EyesGray[num].SetPixel(x, y, Color.FromArgb(0, 0, 0));
                        }
                        else { EyesGray[num].SetPixel(x, y, Color.FromArgb(255, 255, 255)); }
                    }
                }
                num++;
            }
            //Image<Bgr, byte> faceskin = new Image<Bgr, byte>(Eyes);
            //Image<Gray, byte> faceskinGray = faceskin.Convert<Gray, byte>();

            //HarrisDetector harris = new HarrisDetector();
            //harris.Detect(My_Image1);
            //List<Point> featurePoints = new List<Point>();
            //harris.GetCorners(featurePoints, 0.01);
            //harris.DrawFeaturePoints(My_Image1, featurePoints);

            //CvInvoke.MedianBlur(faceskin, faceskin, 3);
            //CvInvoke.MedianBlur(faceskin, faceskin, 3);
            //faceskinGray = MyCV.BoundingBoxeyebrow(faceskin.Convert<Gray,byte>(), faceskin);
            //顯示眼睛ROI

            return EyesGray;

        }


        public Image<Bgr, byte>[] GetEyeImg(Image<Bgr, Byte> OriBgr) {

            Eyes[0] = OriBgr.Copy();
            Eyes[1] = OriBgr.Clone();
            Console.WriteLine(Eyes[0][12, 24]);
            //Eye Classifier
            CascadeClassifier frontaleyes = new CascadeClassifier("haarcascade_eye.xml");
            Rectangle[] ClassifierOutcome = frontaleyes.DetectMultiScale(OriBgr, 1.3, 10, new Size(20, 20), Size.Empty);

            int zoomface = 90;
            for (int i = 0; i < ClassifierOutcome.Length; i++)//調整眼部範圍大小
            {
                //ClassifierOutcome[i].X = ClassifierOutcome[i].X - zoomface;
                ClassifierOutcome[i].Y = ClassifierOutcome[i].Y + zoomface/2;
                ClassifierOutcome[i].Width = ClassifierOutcome[i].Width + zoomface/2;
                ClassifierOutcome[i].Height = ClassifierOutcome[i].Height - zoomface ;
                Console.WriteLine(ClassifierOutcome[i].Width +" , "+ ClassifierOutcome[i].Height);
            }

            // That may include other objects, like nose
            if (ClassifierOutcome.Length >= 2)
            {
                List<Rectangle> eyeList = new List<Rectangle>();
                eyeList.AddRange(ClassifierOutcome);

                //切出眼睛的部分
                Eyes[0].ROI = ClassifierOutcome[0];
                Eyes[1].ROI = ClassifierOutcome[1];
            }
            if (ClassifierOutcome.Length == 1)
            {
                Eyes[0].ROI = ClassifierOutcome[0];
            }

            //分類器偵測結果之數量
            Console.WriteLine("eyes nums : " + ClassifierOutcome.Length);

            return Eyes;

        }
    }
}


            

                