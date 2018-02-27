using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;

namespace SomeCalibrations
{
    class Particle_parameter_for_fullimg
    {
        public Image<Gray, float> sobelX;
        public Image<Gray, float> sobelY;
        public Mat frst;
        public Matrix<double> S_frst;
        public Image<Gray, Byte> m_SourceImage;
        public Image<Gray, float> m_CornerImage;
        //public Image<Gray, byte> c;
        //public Image<Gray, byte> m_ThresholdImage;
        public Image<Gray, byte> saturation;
        public Image<Gray, byte> grayimg;
        public List<PointF> mc;
        public int width;
        public int height;
        public Image<Bgr, byte> gradimg;
        public Image<Bgr, byte> symmimg;
        public Image<Bgr, byte> cornimg;
        //adiust var
        //int rad_symm = 135;
        int alpha_symm = 2;
        int HarrisBlockSize = 24; //adjust by img size 100D : 13
        int symm_rad_range = 5; //100D : 66  //indu 20  //hospi 35
        public Particle_parameter_for_fullimg(Image<Bgr,byte>img) {

            



            width = img.Width;
            height = img.Height;
            grayimg = img.Convert<Gray, byte>();
            //**Gradient
            //horizontal filter
            sobelX = grayimg.Sobel(1, 0, 3);
            //vertical filter
            sobelY = grayimg.Sobel(0, 1, 3);

            //**Saturation
            saturation = grayimg.CopyBlank();
            for (int j = 0; j < height; ++j)
            {
                for (int i = 0; i < width; ++i)
                {
                    saturation[j, i] = new Gray(Color.FromArgb((int)img[j, i].Red, (int)img[j, i].Green, (int)img[j, i].Blue).GetSaturation() * 256);
                }
            }
            Image<Gray, Byte> imageL = saturation.Not();
            CvInvoke.MedianBlur(imageL.Mat, saturation.Mat, 3);
            saturation._EqualizeHist();
            double otsu;
            otsu = CvInvoke.Threshold(saturation, imageL, 0, 255, ThresholdType.Otsu);
            CvInvoke.Threshold(saturation, saturation, (otsu+255)*0.6, 255, ThresholdType.Binary);


            //**frst
            Mat input = grayimg.Mat;
            Mat output;
            fullfrst(ref input, alpha_symm, 0.1);
            //frst2d(ref input, out output, rad_symm, alpha_symm, 0.1);
            //frst = output.Clone();


            //for (int i = 0; i < contours.Size; i++)
            //{
            //    img.Draw(new CircleF(mc[i], 2), new Bgr(Color.Green), 0);
            //}


            //**harris
            m_SourceImage = grayimg.Clone();
            // create corner strength image and do Harris
            m_CornerImage = new Image<Gray, float>(m_SourceImage.Width, m_SourceImage.Height);
            //CvInvoke.CornerHarris(m_SourceImage, m_CornerImage, 3, 3);
            // create and show inverted threshold image
            //m_ThresholdImage = new Image<Gray, Byte>(m_SourceImage.Size);
            //CvInvoke.Threshold(m_CornerImage, m_ThresholdImage, 0.0001,
            //    255.0, Emgu.CV.CvEnum.ThresholdType.BinaryInv);

            //Image<Gray, byte>  c = new Image<Gray, byte>(m_SourceImage.Width, m_SourceImage.Height);
            CvInvoke.CornerHarris(m_SourceImage, m_CornerImage, HarrisBlockSize);     //注意：角点检测传出的为Float类型的数据

            CvInvoke.Normalize(m_CornerImage, m_CornerImage, 0, 255, NormType.MinMax, DepthType.Cv32F);  //标准化处理
            double min = 0, max = 0;
            Point minp = new Point(0, 0);
            Point maxp = new Point(0, 0);
            CvInvoke.MinMaxLoc(m_CornerImage, ref min, ref max, ref minp, ref maxp);
            double scale = 255 / (max - min);
            double shift = min * scale;
            CvInvoke.ConvertScaleAbs(m_CornerImage, m_SourceImage, scale, shift);//进行缩放，转化为byte类型
            //c.Save("harris.bmp");
            //byte[] data = c.Bytes;
            //for (int i = 0; i < m_CornerImage.Height; i++)
            //{
            //    for (int j = 0; j < m_CornerImage.Width; j++)
            //    {
            //        int k = i * m_SourceImage.Width + j;
            //        if (data[k] > 100)    //通过阈值判断
            //        {
            //            CvInvoke.Circle(m_SourceImage, new Point(j, i), 1, new MCvScalar(0, 0, 255, 255), 2);
            //        }
            //    }
            //}


        }

        public Image<Bgr, Byte> Gradpic()
        {
            if (gradimg == null) { 

                gradimg = new Image<Bgr, byte>(sobelX.Width, sobelX.Height);
                //double ax = Math.PI / 2;
                int counter = 0;
                for (int i = 0; i < sobelX.Height; i++)
                {
                    for (int j = 0; j < sobelX.Width; j++)
                    {
                        double theta = Math.Atan2(sobelY[i, j].Intensity, sobelX[i, j].Intensity);
                        if (theta > 0 && theta < 0.5 * Math.PI)
                        {
                            //quadrant1
                            gradimg.Draw(new CircleF(new PointF(j, i), 0), new Bgr(Color.Red), 0);
                        }
                        if (theta > 0.5 * Math.PI && theta < Math.PI)
                        {
                            //quadrant2
                            gradimg.Draw(new CircleF(new PointF(j, i), 0), new Bgr(Color.Green), 0);
                        }
                        if (theta > -0.5 * Math.PI && theta < 0)
                        {
                            //quadrant3
                            gradimg.Draw(new CircleF(new PointF(j, i), 0), new Bgr(Color.Blue), 0);

                        }
                        if (theta > -1 * Math.PI && theta < -0.5 * Math.PI)
                        {
                            //quadrant4
                            gradimg.Draw(new CircleF(new PointF(j, i), 0), new Bgr(Color.Yellow), 0);
                        }
                    }
                }
                Console.WriteLine("" + counter);
                
            }
            return gradimg;
        }


        
        public Image<Bgr, byte> getfrstdraw3()
        {
            if (symmimg == null) {
                symmimg = grayimg.Convert<Bgr,Byte>();
                for (int i = 0; i < S_frst.Height; i++)
                {
                    for (int j = 0; j < S_frst.Width; j++)
                    {
                        if (S_frst.Data[i, j] > 1)    //通过阈值判断
                        {
                            symmimg.Draw(new CircleF(new PointF(j, i), 0), new Bgr(Color.Red), 0);

                        }
                        else if (S_frst.Data[i, j] > 0.8)    //通过阈值判断
                        {
                            symmimg.Draw(new CircleF(new PointF(j, i), 0), new Bgr(Color.Orange), 0);

                        }
                        else if (S_frst.Data[i, j] > 0.6)    //通过阈值判断
                        {
                            symmimg.Draw(new CircleF(new PointF(j, i), 0), new Bgr(Color.Yellow), 0);

                        }
                        else if (S_frst.Data[i, j] > 0.4)    //通过阈值判断
                        {
                            symmimg.Draw(new CircleF(new PointF(j, i), 0), new Bgr(Color.Green), 0);

                        }
                        else if (S_frst.Data[i, j] > 0.2)    //通过阈值判断
                        {
                            symmimg.Draw(new CircleF(new PointF(j, i), 0), new Bgr(Color.Blue), 0);

                        }
                        //else if (S_frst.Data[i, j] > 0)    //通过阈值判断
                        //{
                        //    symmimg.Draw(new CircleF(new PointF(j, i), 0), new Bgr(Color.Indigo), 0);

                        //}
                        //else
                        //{
                        //    symmimg.Draw(new CircleF(new PointF(j, i), 0), new Bgr(Color.Purple), 0);

                        //}

                    }
                }
            }
            return symmimg;
        }

        public Image<Bgr,Byte> getcornordraw()
        {
            if (cornimg == null) {
                cornimg = grayimg.Convert<Bgr, Byte>();
                for (int i = 0; i < m_SourceImage.Height; i++)
                {
                    for (int j = 0; j < m_SourceImage.Width; j++)
                    {
                        if (m_SourceImage[i, j].Intensity > 200)    //通过阈值判断
                        {
                            cornimg.Draw(new CircleF(new PointF(j, i), 3), new Bgr(Color.Red), 0);

                        }
                        else if (m_SourceImage[i, j].Intensity > 100)    //通过阈值判断
                            cornimg.Draw(new CircleF(new PointF(j, i), 3), new Bgr(Color.Blue), 0);
                        else if (m_SourceImage[i, j].Intensity > 50)    //通过阈值判断
                            cornimg.Draw(new CircleF(new PointF(j, i), 3), new Bgr(Color.Green), 0);
                        //else if (m_SourceImage[i, j].Intensity > 10)    //通过阈值判断
                        //    cornimg.Draw(new CircleF(new PointF(j, i), 1), new Bgr(Color.Yellow), 0);
                    }
                }
            }
            return cornimg;
        }

        public void fullfrst(ref Mat inputImage, double alpha, double stdFactor)
        {
            int rad = symm_rad_range;
            int radlit = symm_rad_range + 10;
            Matrix<double> S = new Matrix<double>(1,1);
            Matrix<double> temp;
            double lastSmax=double.MinValue, thisSmax;
            for (; rad < radlit; rad++)
            {
                frstsimple(ref inputImage, out temp, 2 * rad - 1, alpha, stdFactor, out thisSmax);
                //Console.WriteLine("S"+ S.Rows+" "+ S.Cols+"t"+temp.Rows+" "+ temp.Cols);
                if (S.Cols < temp.Cols && S.Rows < temp.Rows)
                {
                    //Console.WriteLine("clone");
                    S = temp.Clone();
                }
                else
                {
                    //Console.WriteLine("sum");
                    S = S.Add(temp);
                }
                    

                Console.WriteLine("last" + lastSmax + "\nthis" + thisSmax + "\nrad" + rad);
                //double min = 0, max = 0;
                //Point minloc = new Point(), maxloc = new Point();
                //CvInvoke.MinMaxLoc(S, ref min, ref max, ref minloc, ref maxloc);
                //Console.WriteLine("Ssum max" + max + "  min" + min);
                //if (thisSmax < lastSmax)
                //{
                //    break;
                //}
                //else
                //{
                //    lastSmax = thisSmax;
                //}
            }
            double min = 0, max = 0;
            Point minloc = new Point(), maxloc = new Point();
            CvInvoke.MinMaxLoc(S, ref min, ref max, ref minloc, ref maxloc);
            Console.WriteLine("Ssum max" + max + "  min" + min);
            S._Mul((1d / max));

            S_frst = S.Clone();
            

        }
        public void frstsimple(ref Mat inputImage,out Matrix<double> S, int radii, double alpha, double stdFactor, out double Smax)
        {

            int width = inputImage.Cols;
            int height = inputImage.Rows;
            Matrix<double> S_temp = new Matrix<double>(inputImage.Rows + 2 * radii, inputImage.Cols + 2 * radii, inputImage.NumberOfChannels);
            Matrix<double> O_n = new Matrix<double>(S_temp.Rows, S_temp.Cols, S_temp.NumberOfChannels);
            Matrix<double> M_n = new Matrix<double>(S_temp.Rows, S_temp.Cols, S_temp.NumberOfChannels);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    PointF p = new PointF(j, i);

                    double[] g = new double[2];
                    g[0] = sobelX[i, j].Intensity;
                    g[1] = sobelY[i, j].Intensity;

                    double gnorm = Math.Sqrt(g[0] * g[0] + g[1] * g[1]);

                    if (gnorm > 0)
                    {
                        int[] gp = new int[2];
                        gp[0] = (int)Math.Round((g[0] / gnorm) * radii);
                        gp[1] = (int)Math.Round((g[1] / gnorm) * radii);

                        PointF pnve = new PointF(j - gp[0] + radii, i - gp[1] + radii);
                        O_n[(int)pnve.Y, (int)pnve.X] -= 1;
                        M_n[(int)pnve.Y, (int)pnve.X] -= gnorm;

                    }
                }
            }

            double min = 0, max = 0;
            Point minloc = new Point(), maxloc = new Point();

            Matrix<double> temp_0 = O_n.CopyBlank();
            Matrix<double> abs_n = O_n.CopyBlank();
            temp_0.SetZero();
            CvInvoke.AbsDiff(O_n, temp_0, abs_n);
            CvInvoke.MinMaxLoc(abs_n, ref min, ref max, ref minloc, ref maxloc);
            Console.WriteLine("On " + min + " " + max);
            O_n = abs_n.Mul(1d / max);
            CvInvoke.MinMaxLoc(O_n, ref min, ref max, ref minloc, ref maxloc);
            Console.WriteLine("On " + min + " " + max);

            temp_0.SetZero();
            abs_n.SetZero();
            min = max = 0;

            CvInvoke.AbsDiff(M_n, temp_0, abs_n);
            CvInvoke.MinMaxLoc(abs_n, ref min, ref max, ref minloc, ref maxloc);
            Console.WriteLine("Mn " + min + " " + max);
            M_n = abs_n.Mul(1d / max);
            CvInvoke.MinMaxLoc(M_n, ref min, ref max, ref minloc, ref maxloc);
            Console.WriteLine("Mn " + min + " " + max);

            CvInvoke.Pow(O_n, alpha, S_temp);
            S_temp._Mul(M_n);

            int kSize = (int)Math.Ceiling(radii / 2d);
            if (kSize % 2 == 0)
                kSize++;

            CvInvoke.MinMaxLoc(S_temp, ref min, ref max, ref minloc, ref maxloc);
            Console.WriteLine("S " + min + " " + max);

            CvInvoke.GaussianBlur(S_temp, S_temp, new Size(kSize, kSize), radii * stdFactor);
            CvInvoke.MinMaxLoc(S_temp, ref min, ref max, ref minloc, ref maxloc);
            Console.WriteLine("Sg " + min + " " + max);

            S = new Matrix<double>(S_temp.Rows - 2 * radii, S_temp.Cols - 2 * radii);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    S[i, j] = S_temp[i + radii, j + radii];
                }
            }
            CvInvoke.MinMaxLoc(S, ref min, ref max, ref minloc, ref maxloc);
            Console.WriteLine("S_frst " + min + " " + max);
            Smax = max;
        }

        public void frst2d(ref Mat inputImage, out Mat outputImage, int radii, double alpha, double stdFactor)
        {

            int width = inputImage.Cols;
            int height = inputImage.Rows;

            //Mat gx, gy;

            //gradx(ref inputImage, out gx);

            //grady(ref inputImage, out gy);

            // set dark/bright mode
            //bool dark = false;
            //bool bright = false;

            //if (mode == 0)//FRST_MODE_BRIGHT
            //    bright = true;
            //else if (mode == 1)//FRST_MODE_DARK
            //    dark = true;
            //else if (mode == 2)//FRST_MODE_BOTH
            //{
            //    bright = true;
            //    dark = true;
            //}
            //else
            //{
            //    throw new Exception("invalid mode!");
            //}

            outputImage = new Mat(inputImage.Size, Emgu.CV.CvEnum.DepthType.Cv64F, 1);

            Matrix<double> S = new Matrix<double>(inputImage.Rows + 2 * radii, inputImage.Cols + 2 * radii, outputImage.NumberOfChannels);
            //s size = imput.wid + 2*r  , input.hei + 2*r

            //Mat O_n = new Mat(S.Size, Emgu.CV.CvEnum.DepthType.Cv64F, 1);
            //Mat M_n = new Mat(S.Size, Emgu.CV.CvEnum.DepthType.Cv64F, 1);
            Matrix<double> O_n = new Matrix<double>(S.Rows, S.Cols, S.NumberOfChannels);
            Matrix<double> M_n = new Matrix<double>(S.Rows, S.Cols, S.NumberOfChannels);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    PointF p = new PointF(j, i);

                    double[] g = new double[2];
                    //g[0] = (double)gx.GetValue(i, j);
                    //g[1] = (double)gy.GetValue(i, j);
                    g[0] = sobelX[i, j].Intensity;
                    g[1] = sobelY[i, j].Intensity;
                    //Vec2d g = cv::Vec2d(gx.GetValue(i, j), gy.GetValue(i, j));

                    double gnorm = Math.Sqrt(g[0] * g[0] + g[1] * g[1]);

                    if (gnorm > 0)
                    {


                        int[] gp = new int[2];
                        gp[0] = (int)Math.Round((g[0] / gnorm) * radii);
                        gp[1] = (int)Math.Round((g[1] / gnorm) * radii);

                        //bright is not need this time
                        //PointF ppve = new PointF(p.X + gp[0] + radii, p.Y + gp[1] + radii);
                        //O_n[(int)ppve.X, (int)ppve.Y] += 1;
                        //M_n[(int)ppve.X, (int)ppve.Y] += gnorm;
                        
                        PointF pnve = new PointF(p.X - gp[0] + radii, p.Y - gp[1] + radii);
                        O_n[(int)pnve.X, (int)pnve.Y] -= 1;
                        M_n[(int)pnve.X, (int)pnve.Y] -= gnorm;
                        
                    }
                }
            }

            double min = 0, max = 0;
            Point minloc = new Point(), maxloc = new Point();

            Matrix<double> temp_0 = O_n.CopyBlank();
            Matrix<double> abs_n = O_n.CopyBlank();

            CvInvoke.AbsDiff(O_n, temp_0, abs_n);
            CvInvoke.MinMaxLoc(abs_n, ref min, ref max, ref minloc, ref maxloc);
            O_n = abs_n / max;
            Console.WriteLine("On "+min+" "+max);
            
            temp_0.SetZero();
            abs_n.SetZero();
            min = max = 0;

            CvInvoke.AbsDiff(M_n, temp_0, abs_n);
            CvInvoke.MinMaxLoc(abs_n, ref min, ref max, ref minloc, ref maxloc);
            M_n = abs_n / max;
            Console.WriteLine("Mn " + min + " " + max);

            CvInvoke.Pow(O_n, alpha, S);
            S._Mul(M_n);

            int kSize = (int)Math.Ceiling(radii / 2d);
            if (kSize % 2 == 0)
                kSize++;

            CvInvoke.MinMaxLoc(S, ref min, ref max, ref minloc, ref maxloc);
            Console.WriteLine("S " + min + " " + max + " " + minloc + " " + maxloc);

            CvInvoke.GaussianBlur(S, S, new Size(kSize, kSize), radii * stdFactor);
            CvInvoke.MinMaxLoc(S, ref min, ref max, ref minloc, ref maxloc);
            Console.WriteLine("Sg " + min + " " + max + " " + minloc + " " + maxloc);

            S_frst = new Matrix<double>(S.Rows- 2 * radii,S.Cols- 2 * radii);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    S_frst[i, j] = S[i + radii, j + radii];
                }
            }
            CvInvoke.MinMaxLoc(S_frst, ref min, ref max, ref minloc, ref maxloc);
            Console.WriteLine("S_frst " + min + " " + max);
            //outputImage = S(cv::Rect(radii, radii, width, height));
            //outputImage.Create(height, width, Emgu.CV.CvEnum.DepthType.Cv64F, 1);
            //for (int i = 0; i < height; i++)
            //{
            //    for (int j = 0; j < width; j++)
            //    {
            //        outputImage.SetValue(i, j, S[radii + i, radii + j]);
            //    }
            //}

        }


        //void grady(ref Mat input, out Mat output)
        //{
        //    output = new Mat(input.Size, input.Depth, input.NumberOfChannels);
        //    for (int y = 0; y < input.Rows; y++)
        //    {
        //        for (int x = 1; x < input.Cols - 1; x++)
        //        {
        //            //*((double*)output.Data + y* output.Cols + x) = (double)(*(input.Data + y* input.Cols + x + 1) - *(input.Data + y* input.Cols + x - 1)) / 2;
        //            byte temp = (byte)(((double)input.GetValue(y, x + 1) - (double)input.GetValue(y, x - 1)) / 2);
        //            output.SetValue(y, x, temp);

        //        }
        //    }
        //}

        //void gradx(ref Mat input, out Mat output)
        //{
        //    output = new Mat(input.Size, input.Depth, input.NumberOfChannels);
        //    for (int y = 1; y < input.Rows - 1; y++)
        //    {
        //        for (int x = 0; x < input.Cols; x++)
        //        {
        //            //*((double*)output.data + y * output.Cols + x) = (double)(*(input.data + (y + 1) * input.Cols + x) - *(input.data + (y - 1) * input.Cols + x)) / 2;
        //            byte temp = (byte)(((double)input.GetValue(y + 1, x) - (double)input.GetValue(y - 1, x)) / 2);
        //            output.SetValue(y, x, temp);
        //        }
        //    }
        //}
        public void bwMorph(ref Mat inputImage, MorphOp operation, ElementShape mShape = ElementShape.Rectangle, int mSize = 3, int iterations = 1)
        {

            int _mSize = (mSize % 2 == 1) ? mSize : mSize + 1;

            Mat element = CvInvoke.GetStructuringElement(mShape, new Size(_mSize, _mSize), new Point(-1, -1));
            CvInvoke.MorphologyEx(inputImage, inputImage, operation, element, new Point(-1, -1), iterations, BorderType.Default, new MCvScalar(255, 0, 0, 255));
        }
        public void bwMorph(ref Mat inputImage, ref Mat outputImage, MorphOp operation, ElementShape mShape = ElementShape.Rectangle, int mSize = 3, int iterations = 1)
        {
            inputImage.CopyTo(outputImage);

            bwMorph(ref outputImage, operation, mShape, mSize, iterations);
        }

        

        //public Color HSLToRGB(double Hue, double Saturation, double Luminosity)
        //{
        //    byte r, g, b;
        //    if (Saturation == 0)
        //    {
        //        r = (byte)Math.Round(Luminosity * 255d);
        //        g = (byte)Math.Round(Luminosity * 255d);
        //        b = (byte)Math.Round(Luminosity * 255d);
        //    }
        //    else
        //    {
        //        double t1, t2;
        //        double th = Hue / 6.0d;

        //        if (Luminosity < 0.5d)
        //        {
        //            t2 = Luminosity * (1d + Saturation);
        //        }
        //        else
        //        {
        //            t2 = (Luminosity + Saturation) - (Luminosity * Saturation);
        //        }
        //        t1 = 2d * Luminosity - t2;

        //        double tr, tg, tb;
        //        tr = th + (1.0d / 3.0d);
        //        tg = th;
        //        tb = th - (1.0d / 3.0d);

        //        tr = ColorCalc(tr, t1, t2);
        //        tg = ColorCalc(tg, t1, t2);
        //        tb = ColorCalc(tb, t1, t2);
        //        r = (byte)Math.Round(tr * 255d);
        //        g = (byte)Math.Round(tg * 255d);
        //        b = (byte)Math.Round(tb * 255d);
        //    }
        //    return Color.FromArgb(r, g, b);
        //}
        //private static double ColorCalc(double c, double t1, double t2)
        //{

        //    if (c < 0) c += 1d;
        //    if (c > 1) c -= 1d;
        //    if (6.0d * c < 1.0d) return t1 + (t2 - t1) * 6.0d * c;
        //    if (2.0d * c < 1.0d) return t2;
        //    if (3.0d * c < 2.0d) return t1 + (t2 - t1) * (2.0d / 3.0d - c) * 6.0d;
        //    return t1;
        //}

    }
}
