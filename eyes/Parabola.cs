using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SomeCalibrations
{
    class Parabola
    {
        //y=a*x^2+b*x+c
        double a, b, c;
        public Parabola() { }
        public Parabola(PointF c,PointF l,PointF r) {
            double[,] ma = { 
                { Math.Pow(c.X,2), c.X, 1, c.Y }, 
                { Math.Pow(l.X,2), l.X, 1, l.Y }, 
                { Math.Pow(r.X,2), r.X, 1, r.Y } };
            double[] x = new double[3];
            GaussEliminate.Gauss(3,ma,x);
            this.a = x[0];
            this.b = x[1];
            this.c = x[2];
        }
        public Parabola(double a,double b,double c) {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public double FY(double x)
        {
            double y = a * Math.Pow(x, 2) + b * x + c;
            return y;
        }

        public double DifferentialFY(double x)
        {
            double y = 2 * a * x + b ;
            return y;
        }
    }
}
