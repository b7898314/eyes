using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Forms;
using Emgu.CV.Util;

namespace bearing
{
    class SectionDetection
    {
        public class myReverserClass : IComparer
        {

            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            int IComparer.Compare(Object x, Object y)
            {
                VectorOfPoint a = x as VectorOfPoint;
                VectorOfPoint b = y as VectorOfPoint;
                double aSize = CvInvoke.BoundingRectangle(a).Width * CvInvoke.BoundingRectangle(a).Height;
                double bSize = CvInvoke.BoundingRectangle(b).Width * CvInvoke.BoundingRectangle(b).Height;
                if (aSize > bSize)
                {
                    return -1;
                }
                else if (aSize < bSize)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

        }
        public Image<Gray, Byte> CoordinateLinear(Image<Bgr, Byte> src, CircleF[] closeCircle)
        {
            Image<Gray, Byte> grayImage = src.Convert<Gray, Byte>();
            int offset = 100;
            double Cx = closeCircle[0].Center.X, Cy = closeCircle[0].Center.Y, RadiusUpper = closeCircle[0].Radius, RadiusLower = closeCircle[1].Radius;
            double xp = 0, yp = 0, thet = 0;
            double xf = 0, yf = 0;
            Image<Gray, Byte> linearImage = new Image<Gray, byte>((int)(RadiusUpper - RadiusLower + offset), (int)(2 * Math.PI * RadiusUpper));
            for (int rq = 0; rq < linearImage.Width; rq++)
            {
                for (int hq = 0; hq < linearImage.Height; hq++)
                {
                    thet = (double)hq / RadiusUpper;
                    xp = Cx + (rq + RadiusLower - offset) * Math.Cos(thet);
                    yp = Cy - (rq + RadiusLower - offset) * Math.Sin(thet);
                    xf = xp - (int)xp;
                    yf = yp - (int)yp;
                    //Bilinear
                    linearImage[hq, rq] = new Gray((1 - yf) * ((1 - xf) * grayImage[(int)yp, (int)xp].Intensity + xf * grayImage[(int)yp, (int)xp + 1].Intensity)
                                                                               + yf * ((1 - xf) * grayImage[(int)yp + 1, (int)xp].Intensity + xf * grayImage[(int)yp + 1, (int)xp + 1].Intensity));
                }
            }
            return linearImage;
        }
        public List<PointF> DefectDetection(Image<Gray, Byte> linearImage, Image<Bgr, Byte> dest, double threshold = -0.9)
        {
            List<PointF> defectList = new List<PointF>();
            linearImage._SmoothGaussian(3);
            //Normal image step1
            List<double> colAvg = new List<double>();
            List<double> colNormal = new List<double>();
            List<double> colSTD = new List<double>();
            double allAvg = 0, STD = 0, sum = 0;
            //calculate average of each col.
            for (int x = 0; x < linearImage.Width; x++)
            {
                sum = 0;
                for (int y = 0; y < linearImage.Height; y++)
                {
                    sum += linearImage[y, x].Intensity;
                }
                colAvg.Add(sum / linearImage.Height);
            }
            allAvg = colAvg.Average();//total avg.
            //calculate STD of each col.
            foreach (double item in colAvg)
            {
                STD += Math.Pow(allAvg - item, 2);
            }
            STD = Math.Sqrt(STD / colAvg.Count);
            //normalize
            foreach (double item in colAvg)
            {
                colNormal.Add((item - allAvg) / STD);
            }
            //calculate avg. STD of pixel at each col.
            for (int x = 0; x < linearImage.Width; x++)
            {
                sum = 0;
                for (int y = 0; y < linearImage.Height; y++)
                {
                    sum += Math.Pow(linearImage[y, x].Intensity - colAvg[x], 2);
                }
                colSTD.Add(Math.Sqrt(sum / linearImage.Height));
            }
            //Normal image step2
            Image<Bgr, Byte> sNorImage = linearImage.Convert<Bgr, Byte>();
            List<double> rowAvg = new List<double>();
            List<double> rowNormal = new List<double>();
            List<double> rowSTD = new List<double>();
            //calculate average of each row
            for (int y = 0; y < linearImage.Height; y++)
            {
                sum = 0;
                for (int x = 0; x < linearImage.Width; x++)
                {
                    sum += linearImage[y, x].Intensity;
                }
                rowAvg.Add(sum / linearImage.Width);
            }
            //calculate STD of each row
            foreach (var item in rowAvg)
            {
                STD += Math.Pow(allAvg - item, 2);
            }
            STD = Math.Sqrt(STD / rowAvg.Count);
            //normalize
            foreach (var item in rowAvg)
            {
                rowNormal.Add((item - allAvg) / STD);
            }
            //calculate avg. STD of pixel at each row
            for (int y = 0; y < linearImage.Height; y++)
            {
                sum = 0;
                for (int x = 0; x < linearImage.Width; x++)
                {
                    sum += Math.Pow(linearImage[y, x].Intensity - rowAvg[y], 2);
                }
                rowSTD.Add(Math.Sqrt(sum / linearImage.Width));
            }
            //output
            for (int x = 0; x < linearImage.Width; x++)
            {
                for (int y = 0; y < linearImage.Height; y++)
                {
                    if ((linearImage[y, x].Intensity - rowAvg[y]) / rowSTD[y] - colNormal[x] < threshold)
                    {
                        dest[y, x] = new Bgr(Color.Red);
                        defectList.Add(new PointF(x, y));
                    }
                }
            }
            return defectList;
        }
        public Image<Bgr, Byte> ALLDefectDetection(Image<Bgr, Byte> src, Image<Bgr, Byte> dest, CircleF[] closeCircle, double threshold = -0.87, int option = 0)
        {
            /*
             * Purpose : detection Defect used 
             * closeCircle[0]=upper close circle
             * closeCircle[1]=lower close circle
             */
            //coordinate
            if (closeCircle[0].Radius - closeCircle[1].Radius < 5)
            {
                return null;
            }
            Image<Gray, Byte> grayImage = src.Convert<Gray, Byte>();
            int offset = 50;
            double Cx = closeCircle[0].Center.X, Cy = closeCircle[0].Center.Y, RadiusUpper = closeCircle[0].Radius, RadiusLower = closeCircle[1].Radius;
            double xp = 0, yp = 0, thet = 0;
            double xf = 0, yf = 0;
            try
            {
                Image<Gray, Byte> linearImage = new Image<Gray, byte>((int)(RadiusUpper - RadiusLower + offset), (int)(2 * Math.PI * RadiusUpper));
                for (int rq = 0; rq < linearImage.Width; rq++)
                {
                    for (int hq = 0; hq < linearImage.Height; hq++)
                    {
                        thet = (double)hq / RadiusUpper;
                        xp = Cx + (rq + RadiusLower - offset) * Math.Cos(thet);
                        yp = Cy - (rq + RadiusLower - offset) * Math.Sin(thet);
                        xf = xp - (int)xp;
                        yf = yp - (int)yp;
                        //Bilinear
                        linearImage[hq, rq] = new Gray((1 - yf) * ((1 - xf) * grayImage[(int)yp, (int)xp].Intensity + xf * grayImage[(int)yp, (int)xp + 1].Intensity)
                                                                                   + yf * ((1 - xf) * grayImage[(int)yp + 1, (int)xp].Intensity + xf * grayImage[(int)yp + 1, (int)xp + 1].Intensity));
                    }
                }
                linearImage._SmoothGaussian(3);
                //Normal image step1
                List<double> colAvg = new List<double>();
                List<double> colNormal = new List<double>();
                List<double> colSTD = new List<double>();
                double allAvg = 0, STD = 0, sum = 0;
                //calculate average of each col.
                for (int x = 0; x < linearImage.Width; x++)
                {
                    sum = 0;
                    for (int y = 0; y < linearImage.Height; y++)
                    {
                        sum += linearImage[y, x].Intensity;
                    }
                    colAvg.Add(sum / linearImage.Height);
                }
                allAvg = colAvg.Average();//total avg.
                                          //calculate STD of each col.
                foreach (double item in colAvg)
                {
                    STD += Math.Pow(allAvg - item, 2);
                }
                STD = Math.Sqrt(STD / colAvg.Count);
                //normalize
                foreach (double item in colAvg)
                {
                    colNormal.Add((item - allAvg) / STD);
                }
                //calculate avg. STD of pixel at each col.
                for (int x = 0; x < linearImage.Width; x++)
                {
                    sum = 0;
                    for (int y = 0; y < linearImage.Height; y++)
                    {
                        sum += Math.Pow(linearImage[y, x].Intensity - colAvg[x], 2);
                    }
                    colSTD.Add(Math.Sqrt(sum / linearImage.Height));
                }
                //Normal image step2
                Image<Bgr, Byte> sNorImage = linearImage.Convert<Bgr, Byte>();
                List<double> rowAvg = new List<double>();
                List<double> rowNormal = new List<double>();
                List<double> rowSTD = new List<double>();
                //calculate average of each row
                for (int y = 0; y < linearImage.Height; y++)
                {
                    sum = 0;
                    for (int x = 0; x < linearImage.Width; x++)
                    {
                        sum += linearImage[y, x].Intensity;
                    }
                    rowAvg.Add(sum / linearImage.Width);
                }
                //calculate STD of each row
                foreach (var item in rowAvg)
                {
                    STD += Math.Pow(allAvg - item, 2);
                }
                STD = Math.Sqrt(STD / rowAvg.Count);
                //normalize
                foreach (var item in rowAvg)
                {
                    rowNormal.Add((item - allAvg) / STD);
                }
                //calculate avg. STD of pixel at each row
                for (int y = 0; y < linearImage.Height; y++)
                {
                    sum = 0;
                    for (int x = 0; x < linearImage.Width; x++)
                    {
                        sum += Math.Pow(linearImage[y, x].Intensity - rowAvg[y], 2);
                    }
                    rowSTD.Add(Math.Sqrt(sum / linearImage.Width));
                }
                //rainbowImage
                Image<Hsv, Byte> rainbowImage = dest.Convert<Hsv, Byte>().CopyBlank();
                double factor = 2 * RadiusUpper / 180;
                int keyPoint = (int)(threshold * (-100) * factor);
                for (int i = (int)(Cx - RadiusUpper - 10); i < (int)(Cx - RadiusUpper); i++)
                {
                    for (int j = (int)(Cy - RadiusUpper); j < Cy + RadiusUpper; j++)
                    {
                        rainbowImage[j, i - 10] = new Hsv((j - (Cy - RadiusUpper)) / factor, 255, 255);
                        if (j > (Cy - RadiusUpper + keyPoint - 3) && j < (Cy - RadiusUpper + keyPoint + 3))
                        {
                            rainbowImage[j, i - 20] = new Hsv(0, 255, 255);
                        }
                    }
                }

                //drew output
                for (int x = offset; x < linearImage.Width; x++)
                {
                    for (int y = 0; y < linearImage.Height; y++)
                    {
                        thet = (double)y / RadiusUpper;
                        xp = Cx + (x + RadiusLower - offset) * Math.Cos(thet);
                        yp = Cy - (x + RadiusLower - offset) * Math.Sin(thet);
                        double temp = -100 * (((linearImage[y, x].Intensity - rowAvg[y]) / rowSTD[y] - colNormal[x]));
                        rainbowImage[(int)yp, (int)xp] = new Hsv((temp > 180) ? 180 : temp, 255, 255);
                        if (((linearImage[y, x].Intensity - rowAvg[y]) / rowSTD[y] - colNormal[x]) < threshold)
                        {
                            sNorImage[y, x] = new Bgr(Color.Red);
                            thet = (double)y / RadiusUpper;
                            xp = Cx + (x + RadiusLower - offset) * Math.Cos(thet);
                            yp = Cy - (x + RadiusLower - offset) * Math.Sin(thet);
                            dest[(int)yp, (int)xp] = new Bgr(Color.Red);
                        }
                    }
                }
                if (option == 0)
                {
                    return rainbowImage.Convert<Bgr, Byte>();
                }
                else
                {
                    return sNorImage;
                }
            }catch(Exception e)
            {
                MessageBox.Show(e.ToString());
                return dest;
            }
        }
        public void GeometryDetection(Image<Bgr, Byte> src, Image<Bgr, Byte> dest, double upperCircleRadius, double lowerCircleRadius, out CircleF[] closeCircle, out double[] STD)
        {
            /*
             * Purpose : detection geometry radius
             * closeCircle[0]=upper close circle
             * closeCircle[1]=lower close circle
             * closeCircle[2]=upper max circle
             * closeCircle[3]=upper min circle
             * closeCircle[4]=lower max circle
             * closeCircle[5]=lower min circle
             * 
             * STD[0]=upper close circle STD
             * STD[1]=lower close circle STD
             */
            //Gray level
            Image<Gray, Byte> grayImage = src.Convert<Gray, Byte>();
            //Otsu
            Image<Gray, Byte> otsuImage = grayImage.CopyBlank();
            CvInvoke.Threshold(grayImage, otsuImage, 128, 255, ThresholdType.Otsu);
            //contour
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(otsuImage, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            //otsuImage.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_NONE, RETR_TYPE.CV_RETR_LIST);
            ArrayList contoursArrayList = new ArrayList();
            for (int i = 0; i < contours.Size; i++)
            {
                //dest.Draw(contours.ApproxPoly(1), new Bgr(Color.Blue), 1);
                contoursArrayList.Add(contours[i]);
            }
            IComparer mycomparer = new myReverserClass();
            contoursArrayList.Sort(mycomparer);
            closeCircle = new CircleF[6];
            STD = new double[2];
            //最小平方圓
            closeCircle[0] = GetCloseCircle(((VectorOfPoint)contoursArrayList[0]).ToArray());
            List<Point> upperContour = new List<Point>(), lowerContour = new List<Point>();
            if (closeCircle[0].Radius < upperCircleRadius * 0.99 && closeCircle[0].Radius > lowerCircleRadius * 1.01)
            {
                Point[] points = ((VectorOfPoint)contoursArrayList[0]).ToArray();
                double r = Math.Pow(closeCircle[0].Radius, 2);
                double x = closeCircle[0].Center.X;
                double y = closeCircle[0].Center.Y;
                double distance = 0.0;
                foreach (var item in points)
                {
                    distance = Math.Pow(item.X - x, 2) + Math.Pow(item.Y - y, 2);
                    if (distance > r)
                    {
                        upperContour.Add(item);
                    }
                    else if (distance < r)
                    {
                        lowerContour.Add(item);
                    }
                }
                closeCircle[0] = GetCloseCircle(upperContour.ToArray());
                closeCircle[1] = GetCloseCircle(lowerContour.ToArray());
                closeCircle[2] = GetMaxMinCircle(upperContour.ToArray(), closeCircle[0], 0);
                closeCircle[3] = GetMaxMinCircle(upperContour.ToArray(), closeCircle[0], 1);
                closeCircle[4] = GetMaxMinCircle(lowerContour.ToArray(), closeCircle[1], 0);
                closeCircle[5] = GetMaxMinCircle(lowerContour.ToArray(), closeCircle[1], 1);
                //DrewCloseCircle(ref dest, closeCircle[0], Color.Yellow);
                //DrewCloseCircle(ref dest, closeCircle[1], Color.Yellow, 1);
                //DrewCloseCircle(ref dest, closeCircle[2], Color.Red, 2);
                //DrewCloseCircle(ref dest, closeCircle[3], Color.Green, 3);
                //DrewCloseCircle(ref dest, closeCircle[4], Color.Red, 4);
                //DrewCloseCircle(ref dest, closeCircle[5], Color.Green, 5);
                STD[0] = CalculateSTD(upperContour.ToArray(), closeCircle[0]);
                STD[1] = CalculateSTD(lowerContour.ToArray(), closeCircle[1]);
            }
            else
            {
                closeCircle[1] = GetCloseCircle(((VectorOfPoint)contoursArrayList[1]).ToArray());
                closeCircle[2] = GetMaxMinCircle(((VectorOfPoint)contoursArrayList[0]).ToArray(), closeCircle[0], 0);
                closeCircle[3] = GetMaxMinCircle(((VectorOfPoint)contoursArrayList[0]).ToArray(), closeCircle[0], 1);
                closeCircle[4] = GetMaxMinCircle(((VectorOfPoint)contoursArrayList[1]).ToArray(), closeCircle[1], 0);
                closeCircle[5] = GetMaxMinCircle(((VectorOfPoint)contoursArrayList[1]).ToArray(), closeCircle[1], 1);
                //DrewCloseCircle(ref dest, closeCircle[0], Color.Yellow);
                //DrewCloseCircle(ref dest, closeCircle[1], Color.Yellow, 1);
                //DrewCloseCircle(ref dest, closeCircle[2], Color.Red, 2);
                //DrewCloseCircle(ref dest, closeCircle[3], Color.Green, 3);
                //DrewCloseCircle(ref dest, closeCircle[4], Color.Red, 4);
                //DrewCloseCircle(ref dest, closeCircle[5], Color.Green, 5);
                STD[0] = CalculateSTD(((VectorOfPoint)contoursArrayList[0]).ToArray(), closeCircle[0]);
                STD[1] = CalculateSTD(((VectorOfPoint)contoursArrayList[1]).ToArray(), closeCircle[1]);
            }
        }
        //最小平方圓
        public CircleF GetCloseCircle(Point[] pointsArray)
        {
            //Draw the enclosing circle
            PointF center;
            float radius;
            double[] constArray = new double[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] rowArray = new double[3] { 0, 0, 0 };
            double x = 0, y = 0, h = 0, k = 0, p = 0, delta = 0;
            foreach (var item in pointsArray)
            {
                x = item.X;
                y = item.Y;
                constArray[0] += 2 * Math.Pow(x, 2);
                constArray[1] += 2 * x * y;
                constArray[2] += -x;
                constArray[4] += 2 * Math.Pow(y, 2);
                constArray[5] += -y;
                rowArray[0] += Math.Pow(x, 3) + x * Math.Pow(y, 2);
                rowArray[1] += Math.Pow(x, 2) * y + Math.Pow(y, 3);
                rowArray[2] += Math.Pow(x, 2) + Math.Pow(y, 2);
            }
            constArray[3] = constArray[1];
            constArray[6] = -2 * constArray[2];
            constArray[7] = -2 * constArray[5];
            constArray[8] = -pointsArray.Length;
            delta = constArray[0] * constArray[4] * constArray[8] + constArray[2] * constArray[3] * constArray[7] + constArray[1] * constArray[5] * constArray[6] - constArray[2] * constArray[4] * constArray[6] - constArray[0] * constArray[5] * constArray[7] - constArray[1] * constArray[3] * constArray[8];
            h = ((constArray[4] * constArray[8] - constArray[5] * constArray[7]) * rowArray[0] - (constArray[1] * constArray[8] - constArray[2] * constArray[7]) * rowArray[1] + (constArray[1] * constArray[5] - constArray[2] * constArray[4]) * rowArray[2]) / delta;
            k = -((constArray[3] * constArray[8] - constArray[5] * constArray[6]) * rowArray[0] - (constArray[0] * constArray[8] - constArray[2] * constArray[6]) * rowArray[1] + (constArray[0] * constArray[5] - constArray[2] * constArray[3]) * rowArray[2]) / delta;
            p = ((constArray[3] * constArray[7] - constArray[4] * constArray[6]) * rowArray[0] - (constArray[0] * constArray[7] - constArray[1] * constArray[6]) * rowArray[1] + (constArray[0] * constArray[4] - constArray[1] * constArray[3]) * rowArray[2]) / delta;

            radius = (float)Math.Sqrt(Math.Pow(h, 2) + Math.Pow(k, 2) - p);
            center = new PointF((float)h, (float)k);
            return new CircleF(center, radius);
        }
        private void DrewCloseCircle(ref Image<Bgr, Byte> src, CircleF closeCircle, Color color, int yshift = 0)
        {
            if (color == null)
            {
                color = Color.Yellow; 
            }
            //MCvFont mCvFont = new MCvFont(mCvFont.CV_FONT_HERSHEY_COMPLEX, 1, 1);
            src.Draw(closeCircle, new Bgr(color), 1);
            src.Draw(new Cross2DF(closeCircle.Center, 20, 20), new Bgr(Color.Tomato), 1);
            //src.Draw(closeCircle.Radius.ToString(), ref mCvFont, new Point((int)closeCircle.Center.X + 20, (int)closeCircle.Center.Y + 50 * yshift), new Bgr(Color.Yellow));
        }
        public void DrewAllCircle(ref Image<Bgr, Byte> src, CircleF[] closeCircle)
        {
            if (closeCircle.Count() == 6)
            {
                src.Draw(closeCircle[0], new Bgr(Color.Yellow), 1);
                src.Draw(new Cross2DF(closeCircle[0].Center, 20, 20), new Bgr(Color.Tomato), 1);
                //src.Draw(closeCircle[1], new Bgr(Color.Yellow), 1);
                //src.Draw(new Cross2DF(closeCircle[0].Center, 20, 20), new Bgr(Color.Tomato), 1);
                //src.Draw(closeCircle[2], new Bgr(Color.Red), 1);
                //src.Draw(closeCircle[3], new Bgr(Color.Green), 1);
                //src.Draw(closeCircle[4], new Bgr(Color.Red), 1);
                //src.Draw(closeCircle[5], new Bgr(Color.Green), 1);
            }
        }
        private CircleF GetMaxMinCircle(Point[] pointsArray, CircleF refCircle, int option = 0)
        {
            /*
             * option =0  :  get max circle
             * option =1 :  get min cirlce
             */
            float radius = 0;
            double a = 0.0, b = 0.0, distance = 0.0;
            foreach (var item in pointsArray)
            {
                a = item.X - refCircle.Center.X;
                b = item.Y - refCircle.Center.Y;
                distance = Math.Sqrt(a * a + b * b);
                if (option == 0)
                {
                    if (distance > radius)
                    {
                        radius = (float)distance;
                    }
                }
                else if (option == 1)
                {
                    if (radius == 0)
                    {
                        radius = (float)distance;
                    }
                    else if (distance < radius)
                    {
                        radius = (float)distance;
                    }
                }
            }
            return new CircleF(refCircle.Center, radius);
        }
        private double CalculateSTD(Point[] pointsArray, CircleF refCircle)
        {
            List<double> distance = new List<double>();
            double mean = 0.0;
            double STD = 0.0;
            foreach (var item in pointsArray)
            {
                distance.Add(Math.Sqrt(Math.Pow(item.X - refCircle.Center.X, 2) + Math.Pow(item.Y - refCircle.Center.Y, 2)));
            }
            mean = distance.Average();
            foreach (var item in distance)
            {
                STD += Math.Pow(item - mean, 2);
            }
            STD = Math.Sqrt(STD / distance.Count);
            return STD;
        }
    }
}