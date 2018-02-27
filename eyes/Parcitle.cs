using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;


namespace SomeCalibrations
{
    class Parcitle
    {
        public Parcitle() { }
        int siz_basic_para = 17; //100D 50
        public Parcitle(PointF c,double w,double h,double ori,double relL, double relR)
        {
            center = c;
            width = w;
            height = h;
            orientation = ori;
            relativeL = relL;
            relativeR = relR;
            initpoint();
        }
        public Parcitle(basicparcitlt basic)
        {
            center = basic.getcenter();
            width = basic.getwidth();
            height = basic.getheight();
            orientation = basic.getorientation();
            relativeL = basic.getrelativeL();
            relativeR = basic.getrelativeR();
            initpoint();
        }
        public Parcitle(Image<Bgr,Byte> img) {
            Random rand = new Random();
            while (true)
            {
                center = new PointF(rand.Next(0, img.Width), rand.Next(0, img.Height));
                width = rand.Next(img.Width / 8, img.Width / 4);
                height = rand.Next(0, (int)width);
                orientation = //rand.NextDouble() * Math.PI * 0.5 - Math.PI * 0.25;
                    0;
                relativeL = rand.NextDouble();
                relativeR = rand.NextDouble();
                initpoint();
                if (up.Y > 0 && up.Y < img.Height && up.X > 0 && up.X < img.Width &&
                    down.Y > 0 && down.Y < img.Height && down.X > 0 && down.X < img.Width &&
                    left.Y > 0 && left.Y < img.Height && left.X > 0 && left.X < img.Width &&
                    right.Y > 0 && right.Y < img.Height && right.X > 0 && right.X < img.Width
                    )
                {
                    break;
                }
            }
        }
        public Parcitle(Parcitle p, int Width, int Height)
        {
            Random rand = new Random();
            while (true)
            {
                center = p.center;
                width = p.width + rand.NextDouble() * siz_basic_para - siz_basic_para / 2;
                height = p.height + rand.NextDouble() * siz_basic_para - siz_basic_para / 2;
                orientation = //rand.NextDouble() * Math.PI * 0.5 - Math.PI * 0.25;
                    0;
                relativeL = p.relativeL + rand.NextDouble() * 0.1 - 0.05;
                relativeR = p.relativeR + rand.NextDouble() * 0.1 - 0.05;
                initpoint();
                if (up.Y > 0 && up.Y < Height && up.X > 0 && up.X < Width &&
                    down.Y > 0 && down.Y < Height && down.X > 0 && down.X < Width &&
                    left.Y > 0 && left.Y < Height && left.X > 0 && left.X < Width &&
                    right.Y > 0 && right.Y < Height && right.X > 0 && right.X < Width
                    )
                {
                    break;
                }
            }
        }
        public Parcitle(PointF cen, int Width, int Height, double para)
        {
            int difuzz = (int)(siz_basic_para * para);
            //Console.WriteLine(cen+" "+Width + " " +Height);
            Random rand = new Random();
            while (true)
            {
                center = new PointF(cen.X+rand.Next(0, difuzz) - difuzz/2, cen.Y+rand.Next(0, difuzz) - difuzz / 2);
                width = rand.Next(Width / 8, Width / 4);
                height = rand.Next(0, (int)width);
                orientation = //rand.NextDouble() * Math.PI * 0.5 - Math.PI * 0.25;
                    0;
                relativeL = rand.NextDouble();
                relativeR = rand.NextDouble();
                initpoint();
                if (up.Y > 0 && up.Y < Height && up.X > 0 && up.X < Width &&
                    down.Y > 0 && down.Y < Height && down.X > 0 && down.X < Width &&
                    left.Y > 0 && left.Y < Height && left.X > 0 && left.X < Width &&
                    right.Y > 0 && right.Y < Height && right.X > 0 && right.X < Width
                    )
                {
                    break;
                }
            }
            //Console.WriteLine(center + " " + width + " " + height);
        }
        private void initpoint() {
            up = new PointF((float)(center.X - (height / 2) * Math.Sin(orientation)), (float)(center.Y - (height / 2) * Math.Cos(orientation)));
            down = new PointF((float)(center.X + (height / 2) * Math.Sin(orientation)), (float)(center.Y + (height / 2) * Math.Cos(orientation)));
            temp = new PointF((float)(up.X * (1 - relativeL) + down.X * relativeL), (float)(up.Y * (1 - relativeL) + down.Y * relativeL));
            left = new PointF((float)(temp.X - width / 2 * Math.Cos(orientation)), (float)(temp.Y + width / 2 * Math.Sin(orientation)));
            temp = new PointF((float)(up.X * (1 - relativeR) + down.X * relativeR), (float)(up.Y * (1 - relativeR) + down.Y * relativeR));
            right = new PointF((float)(temp.X + width / 2 * Math.Cos(orientation)), (float)(temp.Y - width / 2 * Math.Sin(orientation)));
            above = new Parabola(up, left, right);
            below = new Parabola(down, left, right);
        }
        public void drawtest(ref Image<Bgr, Byte> img)
        {
            Bgr bgr = new Bgr(Color.Green);
            img.Draw(new CircleF(center,10), new Bgr(Color.Red), 0);
            img.Draw(new CircleF(up, 10), bgr, 0);
            img.Draw(new CircleF(down, 10), bgr, 0);
            img.Draw(new CircleF(left, 10), bgr, 0);
            img.Draw(new CircleF(right, 10), bgr, 0);
        }
        public basicparcitlt getbasic()
        {
            return new basicparcitlt(center,width,height,orientation,relativeL, relativeR);
        }
        public PointF center { get; set; }
        public PointF irs { get; set; }
        public double width;
        public double height;
        public double orientation;
        public double relativeL;
        public double relativeR;
        public PointF left;
        public PointF right;
        public PointF up;
        public PointF down;
        PointF temp;
        public Parabola above;
        public Parabola below;


        public double Gradient(ref Particle_parameter_for_fullimg ppff) {
            double D_grad = 0;
            int counter = 0;
            for (int x = (int)left.X; x < right.X; x++)
            {
                float y = (float)above.FY(x);
                float y2 = (float)below.FY(x);
                float dy = (float)above.DifferentialFY(x);
                float dy2 = (float)below.DifferentialFY(x);
                
                
                if (y > 0 && y < ppff.height)
                {
                    double gx, gy;
                    double theta1;
                    double theta2;
                    double deltheta;
                    double cs2;
                    gx = ppff.sobelX[(int)y, x].Intensity;
                    gy = ppff.sobelY[(int)y, x].Intensity;
                    theta1 = Math.Atan2(gy , gx);
                    theta2 = Math.Atan(dy);
                    deltheta = (theta1 - theta2) ;
                    cs2 = Math.Cos(deltheta);
                    cs2 *= cs2;
                    D_grad += cs2;
                    counter++;
                }

                if (y2 > 0 && y2 < ppff.height)
                {
                    double gx, gy;
                    double theta1;
                    double theta2;
                    double deltheta;
                    double cs2;
                    gx = ppff.sobelX[(int)y2, x].Intensity;
                    gy = ppff.sobelY[(int)y2, x].Intensity;
                    theta1 = Math.Atan2(gy , gx);
                    theta2 = Math.Atan(dy2);
                    deltheta = (theta1 - theta2);
                    cs2 = Math.Cos(deltheta);
                    cs2 *= cs2;
                    D_grad += cs2;
                    counter++;
                }


            }
            return (D_grad / counter);
        }
        public void Graddraw(ref Image<Bgr, Byte> pic)
        {
            for (int x = ((int)left.X > 0) ? (int)left.X : 0; x < right.X && x < pic.Width; x++)
            {
                float y = (float)above.FY(x);
                float y2 = (float)below.FY(x);
                pic.Draw(
                    new CircleF(new PointF(x, y), 3), new Bgr(Color.Red), 0
                 );
                pic.Draw(
                    new CircleF(new PointF(x, y2), 3), new Bgr(Color.Blue), 0
                 );
            }
        }

        public void irsdraw(ref Image<Bgr, Byte> pic)
        {
            if(irs!=null)
                pic.Draw(
                    new CircleF(irs, 5), new Bgr(Color.Blue), 0
                 );
            Console.WriteLine(irs);
        }

        public PointF getirs()
        {
            if (irs != null)
                return irs;
            else
                return new PointF();
            //Console.WriteLine(irs);
        }

        public void fixtip(ref Particle_parameter_for_fullimg ppff)
        {
            Console.WriteLine("L" + left + "R" + right);
            //Image<Gray, byte> mask = new Image<Gray, byte>(ppff.m_CornerImage.Width, ppff.m_CornerImage.Height);
            //mask.SetZero();
            //mask.Draw(new CircleF(left, (float)(width / 3)), new Gray(255), 0);
            //mask.Draw(new CircleF(right, (float)(width / 3)), new Gray(255), 0);
            double localmax = double.MinValue;
            PointF maxloc = new PointF();
            for (int i= (int)(left.Y- width / 5);i< left.Y + width / 5; ++i)
            {
                for (int j = (int)(left.X - width / 8); j < left.X + width / 8; ++j)
                {
                    if (i < 0 || j < 0 || i > ppff.m_CornerImage.Height || j > ppff.m_CornerImage.Width) continue;
                    if (/*mask[i, j].Intensity > 0 &&*/ ppff.m_CornerImage[i, j].Intensity > localmax)
                    {
                        maxloc.X = j; maxloc.Y = i;
                        localmax = ppff.m_CornerImage[i, j].Intensity;
                    }
                }
            }
            left.X = maxloc.X; left.Y = maxloc.Y;

            localmax = double.MinValue;
            maxloc = new PointF();
            for (int i = (int)(right.Y - width / 5); i < right.Y + width / 5; ++i)
            {
                for (int j = (int)(right.X - width / 8); j < right.X + width / 8; ++j)
                {
                    if (i < 0 || j < 0 || i >= ppff.m_CornerImage.Height || j >= ppff.m_CornerImage.Width) continue;
                    if (/*mask[i, j].Intensity > 0 &&*/ ppff.m_CornerImage[i, j].Intensity > localmax)
                    {
                        maxloc.X = j; maxloc.Y = i;
                        localmax = ppff.m_CornerImage[i, j].Intensity;
                    }
                }
            }
            right.X = maxloc.X; right.Y = maxloc.Y;
            Console.WriteLine("L" + left + "R" + right);
            above = new Parabola(up, left, right);
            below = new Parabola(down, left, right);

        }

        public double Saturation(ref Particle_parameter_for_fullimg ppff) {
            double D_satu = 0;

            double sigpscl = 0, sigpsk = 0;
            int countera = 0, counterb = 0;
            for (int x = ((int)left.X > 0) ? (int)left.X + 30 : 30; x < right.X && x < ppff.width; x += 50)
            {
                float y = (float)above.FY(x);
                float y2 = (float)below.FY(x);
                

                if (y - 20 > 0 && y - 20 < ppff.height) { 
                    sigpsk += (ppff.saturation[(int)y - 20, x].Intensity == 0) ? 0 : 1;
                    counterb++;
                }
                if (y2 + 20 > 0 && y2 + 20 < ppff.height) { 
                    sigpsk += (ppff.saturation[(int)y2 + 20, x].Intensity == 0) ? 0 : 1;
                    counterb++;
                }
                if (y + 20 > 0 && y + 20 < ppff.height && y + 20 < y2){ 
                    sigpscl += (ppff.saturation[(int)y + 20, x].Intensity == 0) ? 0 : 1;
                    countera++;
                }
                if (y2 - 20 > 0 && y2 - 20 < ppff.height && y2 - 20 > y) {
                    sigpscl += (ppff.saturation[(int)y2 - 20, x].Intensity == 0) ? 0 : 1;
                    countera++;
                }

            }
            double alpha, beta;
            alpha = (counterb * 1d) / (countera + counterb);
            beta = 1 - alpha;
            D_satu = (alpha * sigpscl - beta * sigpsk) / (alpha + beta);

            

            return D_satu;
        }

        public void Satudraw( ref Image<Bgr, Byte> pic)
        {
            for (int x = ((int)left.X > 0) ? (int)left.X + 30 : 30; x < right.X && x < pic.Width; x += 50)
            {
                float y = (float)above.FY(x);
                float y2 = (float)below.FY(x);

                pic.Draw(new CircleF(new PointF(x, y - 20), 5), new Bgr(Color.Blue), 0);
                pic.Draw(new CircleF(new PointF(x, y2 + 20), 5), new Bgr(Color.Blue), 0);
                pic.Draw(new CircleF(new PointF(x, y + 20), 5), new Bgr(Color.Green), 0);
                pic.Draw(new CircleF(new PointF(x, y2 - 20), 5), new Bgr(Color.Green), 0);
            }
        }

        public double symmetric(ref Particle_parameter_for_fullimg ppff) {
            //double D_frst = 300;
            //for (int x = (left.X > 0) ? (int)left.X : 0; x < right.X && x < ppff.width; x++)
            //{
            //    float y = (float)above.FY(x);
            //    float y2 = (float)below.FY(x);
            //    for (int j = (y > 0) ? (int)y : 0; j < y2 && j < ppff.height; j++)
            //    {
            //        double temp = ppff.frst.GetValue(j, x);
            //        //if (temp != 0)
            //        //{
            //        //    counter++;
            //        //}
            //        if (temp < D_frst)
            //            D_frst = temp;
            //    }
            //}
            //return D_frst;
            double D_frst =0;
            int st = (int)left.X;
            int ed = (int)right.X;
            st += (int)(width / 10);
            ed -= (int)(width / 10);
            if (st < 0) st = 0;
            if (ed > ppff.width) ed = ppff.width;
            for (int x = st; x < ed; x++)
            {
                float y = (float)above.FY(x);
                float y2 = (float)below.FY(x);
                for (int j = (y > 0) ? (int)y : 0; j < y2 && j < ppff.height; j++)
                {
                    double temp = ppff.S_frst.Data[j,x];
                    if (temp > D_frst)
                    {
                        D_frst = temp;
                        irs = new PointF(x,j);
                    }
                        
                }
            }
            return D_frst;
        }

        public void symmdraw(ref Particle_parameter_for_fullimg ppff, ref Image<Bgr, Byte> pic)
        {
            for (int i = 0; i < ppff.S_frst.Height; i++)
            {
                for (int j = 0; j < ppff.S_frst.Width; j++)
                {
                    if (ppff.S_frst.Data[i, j] > 1)    //通过阈值判断
                    {
                        pic.Draw(new CircleF(new PointF(j, i), 0), new Bgr(Color.Red), 0);

                    }
                    else if (ppff.S_frst.Data[i, j] > 0.8)    //通过阈值判断
                    {
                        pic.Draw(new CircleF(new PointF(j, i), 0), new Bgr(Color.Orange), 0);

                    }
                    else if (ppff.S_frst.Data[i, j] > 0.6)    //通过阈值判断
                    {
                        pic.Draw(new CircleF(new PointF(j, i), 0), new Bgr(Color.Yellow), 0);

                    }
                    else if (ppff.S_frst.Data[i, j] > 0.4)    //通过阈值判断
                    {
                        pic.Draw(new CircleF(new PointF(j, i), 0), new Bgr(Color.Green), 0);

                    }
                    else if (ppff.S_frst.Data[i, j] > 0.2)    //通过阈值判断
                    {
                        pic.Draw(new CircleF(new PointF(j, i), 0), new Bgr(Color.Blue), 0);

                    }
                    //else if (ppff.S_frst.Data[i, j] > 0)    //通过阈值判断
                    //{
                    //    pic.Draw(new CircleF(new PointF(j, i), 0), new Bgr(Color.Indigo), 0);

                    //}
                    //else
                    //{
                    //    pic.Draw(new CircleF(new PointF(j, i), 0), new Bgr(Color.Purple), 0);

                    //}

                }
            }
        }

        public double corner(ref Particle_parameter_for_fullimg ppff) {
            //Image<Gray, byte> c = ppff.m_CornerImage.Convert<Gray, byte>();
            //byte[] data = c.Bytes;
            //double D_cono = (data[(int)(left.Y * c.Width + left.X)] + data[(int)(right.Y * c.Width + right.X)]) / 2;
            //return D_cono;
            double D_cono = (ppff.m_CornerImage[(int)left.Y, (int)left.X].Intensity + ppff.m_CornerImage[(int)right.Y, (int)right.X].Intensity) / 512d;
            //Console.WriteLine(ppff.m_CornerImage[(int)left.Y, (int)left.X].Intensity+"  " + c[(int)left.Y, (int)left.X].Intensity);
            return D_cono;
        }

        public void corndraw(ref Particle_parameter_for_fullimg ppff, ref Image<Bgr, Byte> pic)
        {
            for (int i = 0; i < ppff.m_SourceImage.Height; i++)
            {
                for (int j = 0; j < ppff.m_SourceImage.Width; j++)
                {
                    if (ppff.m_SourceImage[i,j].Intensity > 200)    //通过阈值判断
                    {
                        pic.Draw(new CircleF(new PointF(j, i), 10), new Bgr(Color.Red), 0);
                        
                    }
                    else if (ppff.m_SourceImage[i, j].Intensity < 10)    //通过阈值判断
                    {
                        pic.Draw(new CircleF(new PointF(j, i), 10), new Bgr(Color.Green), 0);

                    }
                    else if(ppff.m_SourceImage[i, j].Intensity > 100)    //通过阈值判断
                        pic.Draw(new CircleF(new PointF(j, i), 10), new Bgr(Color.Blue), 0);
                }
            }
            double min=0, max=0;
            Point minl = new Point(), maxl = new Point();
            CvInvoke.MinMaxLoc(ppff.m_SourceImage, ref min, ref max, ref minl, ref maxl);
            Console.WriteLine("using old method of corndraw");
        }
        public double getweight(double d1, double m1, double d2, double m2, double d3, double m3, double d4, double m4, int mode)
        {
            double weight;
            double[] zig = new double[4];
            switch (mode)
            {
                case 0:
                case 3:
                    zig[0] = 3;
                    zig[1] = 5;
                    zig[2] = 15;
                    zig[3] = 5;
                    break;
                case 1:
                case 4:
                    zig[0] = 3;
                    zig[1] = 5;
                    zig[2] = 5;
                    zig[3] = 15;
                    break;
                case 2:
                case 5:
                    zig[0] = 7;
                    zig[1] = 15;
                    zig[2] = 5;
                    zig[3] = 5;
                    break;
                default:
                    zig[0] = 5;
                    zig[1] = 5;
                    zig[2] = 5;
                    zig[3] = 5;
                    break;
            }
            double g1 = guass(d1, m1, zig[0]);
            double g2 = guass(d2, m2, zig[1]);
            double g3 = guass(d3, m3, zig[2]);
            double g4 = guass(d4, m4, zig[3]);
            //Console.WriteLine(g1.ToString("#.##E#") + " " + g2.ToString("#.##E#") + " " + g3.ToString("#.##E#") + " " + g4.ToString("#.##E#") + " " + mode);
            weight = g1 * g2 * g3 * g4;
            return weight;
        }
        public double getweight(double d1, double m1, double d2, double m2, double d3, double m3, double d4, double m4)
        {
            double weight;
            double zig = 2;
            double g1 = guass(d1, m1, 3);
            double g2 = guass(d2, m2, 5);
            double g3 = guass(d3, m3, 15);
            double g4 = guass(d4, m4, 5);
            //Console.WriteLine(g1.ToString("#.##E#") + " " + g2.ToString("#.##E#") + " " + g3.ToString("#.##E#") + " " + g4.ToString("#.##E#"));
            weight = g1 * g2 * g3 * g4;
            return weight;
        }
        public double getweight(double d1, double d2, double d3, double d4)
        {
            double weight;
            double zig = 2;
            double mu = (d1 + d2 + d3 + d4) / 4;
            //Console.WriteLine("using the old method of getweight!");
            return getweight(d1, mu, d2, mu, d3, mu, d4, mu);
        }
        public double getweight(ref Particle_parameter_for_fullimg ppff)
        {
            double d1 = Gradient(ref ppff);
            double d2 = Saturation(ref ppff);
            double d3 = symmetric(ref ppff);
            double d4 = corner(ref ppff);
            return getweight(d1, d2, d3, d4);
        }

        private double guass(double d, double mu, double zig)
        {
            return Math.Pow(Math.E, ((d - mu) * (d - mu) / (-2 * zig * zig))) / (Math.Sqrt(2 * Math.PI * zig * zig));
        }

        public void frst2d(ref Mat inputImage, out  Mat outputImage, int radii, double alpha, double stdFactor, int mode) {

            int width = inputImage.Cols;
            int height = inputImage.Rows;

            Mat gx, gy;

            gradx(ref inputImage, out gx);

            grady(ref inputImage, out gy);

            // set dark/bright mode
            bool dark = false;
            bool bright = false;

	        if (mode == 0)//FRST_MODE_BRIGHT
		        bright = true;
	        else if (mode == 1)//FRST_MODE_DARK
		        dark = true;
	        else if (mode == 2)//FRST_MODE_BOTH
            {
		        bright = true;
		        dark = true;
	        }
	        else {
		        throw new Exception("invalid mode!");
	        }

            outputImage = new Mat(inputImage.Size, Emgu.CV.CvEnum.DepthType.Cv64F, 1);

            Matrix<double> S = new Matrix<double>(inputImage.Rows + 2 * radii, inputImage.Cols + 2 * radii, outputImage.NumberOfChannels);

            //Mat O_n = new Mat(S.Size, Emgu.CV.CvEnum.DepthType.Cv64F, 1);
            //Mat M_n = new Mat(S.Size, Emgu.CV.CvEnum.DepthType.Cv64F, 1);
            Matrix<double> O_n = new Matrix<double>(S.Rows,S.Cols,S.NumberOfChannels);
            Matrix<double> M_n = new Matrix<double>(S.Rows, S.Cols, S.NumberOfChannels);

            for (int i = 0; i<height; i++) {
		        for (int j = 0; j<width; j++) {		
			        PointF p = new PointF(i, j);

                    double[] g = new double[2];
                    g[0] = (double)gx.GetValue(i, j);
                    g[1] = (double)gy.GetValue(i, j);
                    //Vec2d g = cv::Vec2d(gx.GetValue(i, j), gy.GetValue(i, j));

                    double gnorm = Math.Sqrt(g[0] * g[0] + g[1] * g[1]);
			
			        if (gnorm > 0) {


                        int[] gp = new int[2];
                        gp[0] = (int)Math.Round((g[0] / gnorm) * radii);
                        gp[1] = (int)Math.Round((g[1] / gnorm) * radii);
				
				        if (bright) {
					        PointF ppve=new PointF(p.X + gp[0] + radii, p.Y + gp[1] + radii);
                            O_n[(int)ppve.X, (int)ppve.Y] = O_n[(int)ppve.X, (int)ppve.Y] + 1;
                            M_n[(int)ppve.X, (int)ppve.Y] = M_n[(int)ppve.X, (int)ppve.Y] + gnorm;
                        }

				        if (dark) {
                            PointF pnve=new PointF(p.X - gp[0] + radii, p.Y - gp[1] + radii);
                            O_n[(int)pnve.X, (int)pnve.Y] = O_n[(int)pnve.X, (int)pnve.Y] - 1;
                            M_n[(int)pnve.X, (int)pnve.Y] = M_n[(int)pnve.X, (int)pnve.Y] - gnorm;
                        }
			        }
		        }
	        }

            double min = 0, max = 0;
            Point minloc = new Point(), maxloc = new Point();

            Matrix<double> temp_0 = O_n.CopyBlank();
            CvInvoke.AbsDiff(O_n,temp_0,O_n);//***src dst同樣?
            CvInvoke.MinMaxLoc(O_n, ref min, ref max, ref minloc, ref maxloc);
	        O_n = O_n / max;


            temp_0.SetZero();
            CvInvoke.AbsDiff(M_n, temp_0, M_n);
            CvInvoke.MinMaxLoc(O_n, ref min, ref max, ref minloc, ref maxloc);
	        M_n = M_n / max;

            CvInvoke.Pow(O_n, alpha, S);
	        S._Mul(M_n);
		
	        int kSize = (int)Math.Ceiling(radii / 2d);
	        if (kSize % 2 == 0)
		        kSize++;
	
	        CvInvoke.GaussianBlur(S, S, new Size(kSize, kSize), radii* stdFactor);

            //outputImage = S(cv::Rect(radii, radii, width, height));
            outputImage.Create(height, width, Emgu.CV.CvEnum.DepthType.Cv64F, 1);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    outputImage.SetValue(i, j, S[radii + i, radii + j]);
                }
            }

        }


        void grady(ref Mat input, out Mat output)
        {
            output = new Mat(input.Size, input.Depth, input.NumberOfChannels);
	        for (int y = 0; y<input.Rows; y++)
	        {
		        for (int x = 1; x<input.Cols - 1; x++)
		        {
                    //*((double*)output.Data + y* output.Cols + x) = (double)(*(input.Data + y* input.Cols + x + 1) - *(input.Data + y* input.Cols + x - 1)) / 2;
                    byte temp = (byte)(((double)input.GetValue(y, x + 1) - (double)input.GetValue(y, x - 1)) / 2);
                    output.SetValue(y, x, temp);

                }
            }
        }

        void gradx(ref Mat input, out Mat output)
        {
            output = new Mat(input.Size, input.Depth, input.NumberOfChannels);
            for (int y = 1; y < input.Rows - 1; y++)
            {
                for (int x = 0; x < input.Cols; x++)
                {
                    //*((double*)output.data + y * output.Cols + x) = (double)(*(input.data + (y + 1) * input.Cols + x) - *(input.data + (y - 1) * input.Cols + x)) / 2;
                    byte temp = (byte)(((double)input.GetValue(y + 1, x) - (double)input.GetValue(y - 1, x)) / 2);
                    output.SetValue(y, x, temp);
                }
            }
        }
        public void bwMorph(ref Mat inputImage, MorphOp operation, ElementShape mShape = ElementShape.Rectangle, int mSize = 3, int iterations = 1)
        {

            int _mSize = (mSize % 2 == 1) ? mSize : mSize + 1;

            Mat element = CvInvoke.GetStructuringElement(mShape, new Size(_mSize, _mSize), new Point(-1,-1));
            CvInvoke.MorphologyEx(inputImage, inputImage, operation, element, new Point(-1, -1), iterations, BorderType.Default, new MCvScalar(255, 0, 0, 255));
        }
        public void bwMorph(ref Mat inputImage,ref Mat outputImage, MorphOp operation, ElementShape mShape = ElementShape.Rectangle, int mSize = 3, int iterations = 1)
        {
            inputImage.CopyTo(outputImage);

            bwMorph(ref outputImage, operation, mShape, mSize, iterations);
        }










}

    struct basicparcitlt
    {
        private PointF center;
        private double width;
        private double height;
        private double orientation;
        private double relativeL;
        private double relativeR;

        public basicparcitlt(PointF center, double width, double height, double orientation, double relativeL, double relativeR) : this()
        {
            this.center = center;
            this.width = width;
            this.height = height;
            this.orientation = orientation;
            this.relativeL = relativeL;
            this.relativeR = relativeR;
        }
        public PointF getcenter() { return center; }
        public double getwidth() { return width; }
        public double getheight() { return height; }
        public double getorientation() { return orientation; }
        public double getrelativeL() { return relativeL; }
        public double getrelativeR() { return relativeR; }
    }
}
