using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SomeCalibrations
{
    class GaussEliminate
    {
        // 利用高斯消元法求线性方程组的解  
        public static void Gauss(int n, double[,] a, double[] x)
        {
            double d;

            //Console.WriteLine("高斯消去法解方程组的中间过程");
            //Console.WriteLine("============================");
            //Console.WriteLine("中间过程");
            //Console.WriteLine("增广矩阵：");
            //printArray(n, a); Console.WriteLine();

            // 消元  
            for (int k = 0; k < n; k++)
            {
                //Console.WriteLine("第{0}步", k + 1);
                //Console.WriteLine("初始矩阵：");
                //printArray(n, a); Console.WriteLine();

                selectMainElement(n, k, a); // 选择主元素  
                //Console.WriteLine("选择主元素后的矩阵：");
                //printArray(n, a); Console.WriteLine();

                // for (int j = k; j <= n; j++ ) a[k, j] = a[k, j] / a[k, k];  
                // 若将下面两个语句改为本语句，则程序会出错，因为经过第1次循环  
                // 后a[k,k]=1，a[k,k]的值发生了变化，所以在下面的语句中先用d  
                // 将a[k,k]的值保存下来  
                d = a[k, k];
                for (int j = k; j <= n; j++) a[k, j] = a[k, j] / d;
                //Console.WriteLine("将第{0}行中a[{0},{0}]化为1后的矩阵：", k + 1);
                //printArray(n, a); Console.WriteLine();

                // Guass消去法与Jordan消去法的主要区别就是在这一步，Gauss消去法是从k+1  
                // 到n循环，而Jordan消去法是从1到n循环，中间跳过第k行  
                for (int i = k + 1; i < n; i++)
                {
                    d = a[i, k];  // 这里使用变量d将a[i,k]的值保存下来的原理与上面注释中说明的一样  
                    for (int j = k; j <= n; j++) a[i, j] = a[i, j] - d * a[k, j];
                }

                //Console.WriteLine("消元后的矩阵：");
                //printArray(n, a); Console.WriteLine();
            }

            // 回代  
            x[n - 1] = a[n - 1, n];
            for (int i = n - 1; i >= 0; i--)
            {
                x[i] = a[i, n];
                for (int j = i + 1; j < n; j++) x[i] = x[i] - a[i, j] * x[j];
            }
        }

        // 选择主元素  
        public static void selectMainElement(int n, int k, double[,] a)
        {
            // 寻找第k列的主元素以及它所在的行号  
            double t, mainElement;            // mainElement用于保存主元素的值  
            int l;                            // 用于保存主元素所在的行号  

            // 从第k行到第n行寻找第k列的主元素，记下主元素mainElement和所在的行号l  
            mainElement = Math.Abs(a[k, k]);  // 注意别忘了取绝对值  
            l = k;
            for (int i = k + 1; i < n; i++)
            {
                if (mainElement < Math.Abs(a[i, k]))
                {
                    mainElement = Math.Abs(a[i, k]);
                    l = i;                        // 记下主元素所在的行号  
                }
            }

            // l是主元素所在的行。将l行与k行交换，每行前面的k个元素都是0，不必交换  
            if (l != k)
            {
                for (int j = k; j <= n; j++)
                {
                    t = a[k, j]; a[k, j] = a[l, j]; a[l, j] = t;
                }
            }
        }

    }
}
