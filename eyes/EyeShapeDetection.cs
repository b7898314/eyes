using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using System.Drawing;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;

namespace EyeDection
{
    class EyeShapeDetection
    {
        public EyeShapeDetection() { }

        //use CascadeClassifier to find face and eyes
        public void FindEyes(Image<Bgr, Byte> img, List<Rectangle> faces, List<Rectangle> eyes)
        {
            using (CascadeClassifier faceClassifier = new CascadeClassifier("haarcascade_frontalface_default.xml"))
            using (CascadeClassifier eyeClassifier = new CascadeClassifier("haarcascade_eye.xml"))
            {
                using (Image<Gray, Byte> gray = img.Convert<Gray, Byte>()) //Convert it to Grayscale
                {
                    //normalizes brightness and increases contrast of the image
                    gray._EqualizeHist();

                    //Detect the faces  from the gray scale image and store the locations as rectangle
                    //The first dimensional is the channel
                    //The second dimension is the index of the rectangle in the specific channel

                    //The Following algr only chose one
                    #region Face detection and eye
                    Rectangle[] facesDetected = faceClassifier.DetectMultiScale(
                       gray,
                       1.1,
                       10,
                       new Size(200, 200),
                       Size.Empty);
                    faces.AddRange(facesDetected);
                    Image<Bgr, Byte> img2 = img.Copy();
                    foreach (Rectangle f in facesDetected)
                    {
                        //Set the region of interest on the faces
                        gray.ROI = f;
                        //outputpic
                        img2.ROI = f;
                        img2.Save("100.jpg");
                        img2.ROI = Rectangle.Empty;
                        Rectangle[] eyesDetected = eyeClassifier.DetectMultiScale(
                           gray,
                           1.1,
                           10,
                           new Size(200, 200),
                           Size.Empty);
                        gray.ROI = Rectangle.Empty;
                        foreach (Rectangle eyeRect in eyesDetected)
                        {
                            eyeRect.Offset(f.X, f.Y);
                            eyes.Add(eyeRect);
                            //outputpic
                            img2.Draw(eyeRect,new Bgr(Color.Red),3);
                        }
                        //outputpic
                        img2.ROI = f;
                        img2.Save("101.jpg");
                    }
                    #endregion

                    #region Only eyes
                    //Rectangle[] eyesDetected = eyeClassifier.DetectMultiScale(
                    //       gray,
                    //       1.1,
                    //       10,
                    //       new Size(200, 200),
                    //       Size.Empty);
                    //foreach (Rectangle eyeRect in eyesDetected)
                    //{
                    //    eyes.Add(eyeRect);
                    //    img.Draw(eyeRect, new Bgr(0, 0, 255), 5);
                    //    pictureBox2.Image = img.ToBitmap();
                    //}
                    #endregion

                }
            }
        }
        public void FindEyeShape(ref Image<Bgr, Byte> result, ref Image<Bgr, Byte> image, List<CircleF> pupils, List<PointF> leftCornerPoints, List<PointF> rightCornerPoints)
        {
            Image<Bgr, Byte> skinImage = image.Copy();
            FindEyeShape(ref result, ref image, skinImage, pupils, leftCornerPoints, rightCornerPoints);
        }
        public void FindEyeShape(ref Image<Bgr, Byte> result, ref Image<Bgr, Byte> image, Image<Bgr, Byte> skinBgrImage, List<CircleF> pupils, List<PointF> leftCornerPoints, List<PointF> rightCornerPoints)
        {
            /*
             * leftCornerPoints: top down left right
             * rightCornerPoints:top down left right
             */
            //outputpic
            result.Save("000.jpg");
            Image<Hsv, Byte> HsvImage = image.Convert<Hsv, Byte>();
            Image<Gray, Byte> grayImage = image.Convert<Gray, Byte>();//gray level image
            Image<Gray, Byte> pupilsImage = grayImage.CopyBlank();
            Image<Gray, Byte> skinMap = grayImage.CopyBlank();
            Image<Gray, Byte> adaptiveImage = grayImage.CopyBlank();
            double otsu = CvInvoke.Threshold(grayImage, adaptiveImage, 128, 255, ThresholdType.Otsu);
            CvInvoke.AdaptiveThreshold(grayImage, adaptiveImage, 255, AdaptiveThresholdType.MeanC, ThresholdType.BinaryInv, 49, otsu * 0.1);
            //3X3 structuring element
            Mat SElement = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new Size(3, 3), new Point(1, 1));
            //StructuringElementEx SElement = new StructuringElementEx(3, 3, 1, 1, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_CROSS);
            //adaptiveImage._MorphologyEx(SElement, Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_CLOSE, 1);
            adaptiveImage._MorphologyEx(Emgu.CV.CvEnum.MorphOp.Close, SElement,new Point(1,1), 1, BorderType.Default, new MCvScalar(255, 0, 0, 255));
            int x = 0, y = 0;
            int count = 0, count2 = 0;
            Gray keyPointCheck = new Gray(255);
            Image<Gray, Byte> redImage = image[2];
            Image<Gray, Byte> otsuImage = grayImage.CopyBlank();
            otsu = CvInvoke.Threshold(redImage, otsuImage, 128, 255, ThresholdType.Otsu) * 0.4;
            redImage._ThresholdBinaryInv(new Gray(otsu), new Gray(255));
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            IOutputArray hierarchy = null;
            CvInvoke.FindContours(redImage, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxNone);
            //Contour<Point> contours = redImage.FindContours(
            //        //all pixels of each contours
            //        Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_NONE,
            //        //retrieve the external contours
            //        Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL);


            //Filter right eye and left eye
            int contoursRightIdx = -1, contoursLeftIdx = -1;
            for (int i = 0; i < contours.Size; i++)
            {
                if (CvInvoke.BoundingRectangle(contours[i]).Left >= image.Width / 2)
                {
                    if (contoursRightIdx == -1)
                    {
                        contoursRightIdx = i;
                    }
                    else
                    {
                        if (CvInvoke.ContourArea(contours[i], false) > CvInvoke.ContourArea(contours[contoursRightIdx], false))
                        {
                            contoursRightIdx = i;
                        }
                    }
                }
                else
                {
                    if (contoursLeftIdx == -1)
                    {
                        contoursLeftIdx = i;
                    }
                    else
                    {
                        if (CvInvoke.ContourArea(contours[i], false) > CvInvoke.ContourArea(contours[contoursLeftIdx], false))
                        {
                            contoursLeftIdx = i;
                        }
                    }
                }
            }
            //Drew on result image and add in list
            //PointF contoursCenter;
            //float contoursRadius = 0;
            CircleF contoursCircle;
            //Right pupil
            contoursCircle = CvInvoke.MinEnclosingCircle(contours[contoursRightIdx]);
            result.Draw(contoursCircle, new Bgr(Color.Coral), 4);
            //result.Draw(contoursRight, new Bgr(Color.LightPink), 2);
            //CvInvoke.DrawContours(result, contours, contoursRightIdx, new MCvScalar(255, 0, 0), 2);
            result.Draw(new Cross2DF(contoursCircle.Center, 50, 50), new Bgr(Color.Coral), 3);
            pupils.Add(contoursCircle);
            //Left pupil
            contoursCircle = CvInvoke.MinEnclosingCircle(contours[contoursLeftIdx]);
            result.Draw(contoursCircle, new Bgr(Color.Coral), 4);
            //result.Draw(contoursLeft, new Bgr(Color.LightPink), 2);
            //CvInvoke.DrawContours(result, contours, contoursLeftIdx, new MCvScalar(255, 0, 0), 2);
            result.Draw(new Cross2DF(contoursCircle.Center, 50, 50), new Bgr(Color.Coral), 3);
            pupils.Add(contoursCircle);

            //outputpic
            result.Save("001.jpg");


            #region Detect non Skin color
            List<int> skinHueList = new List<int>();
            double skinHue = 0, skinSatuation = 0, skinValue = 0;
            Hsv skinHsv = new Hsv();
            //Take mid pixel of image to be skin color
            int captureRange = 50;
            x = image.Width / 2;
            y = image.Width * 2 / 3;
            count = 0;
            x = 0;
            y = 0;
            for (int i = 0; i < image.Width; i += captureRange)
            {
                for (int j = 0; j < image.Height; j += captureRange)
                {
                    if ((x + i) > 0 && (x + i) < HsvImage.Width && (y + j) > 0 && (y + j) < HsvImage.Height)
                    {
                        skinHueList.Add((int)HsvImage[y + j, x + i].Hue);
                    }
                }
            }

            //Average skin color
            skinHsv.Hue = GetElevationMode(skinHueList);
            skinHsv.Satuation = skinSatuation / (Double)count;
            skinHsv.Value = skinValue / (Double)count;
            //Detect non skin color
            double skinDiffRate = 4;
            for (int i = 0; i < HsvImage.Width; i++)
            {
                for (int j = 0; j < HsvImage.Height; j++)
                {
                    if (HsvImage[j, i].Hue >= skinHsv.Hue)
                    {
                        if (Math.Abs(HsvImage[j, i].Hue - skinHsv.Hue) > skinDiffRate && (Math.Abs(HsvImage[j, i].Hue - 180) + Math.Abs(skinHsv.Hue)) > skinDiffRate)
                        {
                            //skinBgrImage[j, i] = new Bgr(Color.CadetBlue);
                            skinMap[j, i] = new Gray(255);
                        }
                    }
                    else
                    {
                        if (Math.Abs(HsvImage[j, i].Hue - skinHsv.Hue) > skinDiffRate && (Math.Abs(HsvImage[j, i].Hue) + Math.Abs(skinHsv.Hue - 180)) > skinDiffRate)
                        {
                            //skinBgrImage[j, i] = new Bgr(Color.CadetBlue);
                            skinMap[j, i] = new Gray(255);
                        }
                    }
                }
            }
            #endregion

            #region Top &Down
            //Right top
            count = 0;
            x = (int)pupils[0].Center.X;
            y = (int)pupils[0].Center.Y;
            for (int i = 0; y - i > 0; i++)
            {
                if (!keyPointCheck.Equals(otsuImage[y - i, x]))  // or if (keyPointCheck.Equals(skinMap[y - i, x])) 
                {
                    count = 0;
                }
                else
                {
                    count++;
                }
                if (count == 50)
                {
                    y = y - i + count;
                    break;
                }
            }
            result.Draw(new Cross2DF(new PointF(x, y), 50, 50), new Bgr(Color.LightSkyBlue), 3);
            rightCornerPoints.Add(new PointF(x, y));
            //Right down
            count = 0;
            x = (int)pupils[0].Center.X;
            y = (int)pupils[0].Center.Y;
            for (int i = 0; y + i < image.Height; i++)
            {
                if (!keyPointCheck.Equals(otsuImage[y + i, x]))
                {
                    count = 0;
                }
                else
                {
                    count++;
                }
                if (count == 100)
                {
                    y = y + i - count;
                    break;
                }
            }
            result.Draw(new Cross2DF(new PointF(x, y), 50, 50), new Bgr(Color.LightSkyBlue), 3);
            rightCornerPoints.Add(new PointF(x, y));
            //Left top
            count = 0;
            x = (int)pupils[1].Center.X;
            y = (int)pupils[1].Center.Y;
            for (int i = 0; y - i > 0; i++)
            {
                if (!keyPointCheck.Equals(otsuImage[y - i, x]))
                {
                    count = 0;
                }
                else
                {
                    count++;
                }
                if (count == 50)
                {
                    y = y - i + count;
                    break;
                }
            }
            result.Draw(new Cross2DF(new PointF(x, y), 50, 50), new Bgr(Color.LightGreen), 3);
            leftCornerPoints.Add(new PointF(x, y));
            //Left down
            count = 0;
            x = (int)pupils[1].Center.X;
            y = (int)pupils[1].Center.Y;
            for (int i = 0; y + i < image.Height; i++)
            {
                if (!keyPointCheck.Equals(otsuImage[y + i, x]))
                {
                    count = 0;
                }
                else
                {
                    count++;
                }
                if (count == 100)
                {
                    y = y + i - count;
                    break;
                }
            }
            result.Draw(new Cross2DF(new PointF(x, y), 50, 50), new Bgr(Color.LightGreen), 3);
            leftCornerPoints.Add(new PointF(x, y));
            #endregion

            //outputpic
            result.Save("003.jpg");

            #region SiftDetector Features
            Image<Gray, Byte> keyPointsMap = grayImage.CopyBlank();
            SURF surf = new SURF(
                                                8000   //threshold
                                                ); //extended descriptors
                                                   //feature point detection
            //VectorOfKeyPoint keypointsVector = surf.DetectKeyPointsRaw(adaptiveImage, null);
            VectorOfKeyPoint keypointsVector = new VectorOfKeyPoint();
            surf.DetectRaw(adaptiveImage, keypointsVector);

            //filtering keypoints by point size 
            keypointsVector.FilterByKeypointSize(20, 35);
            //drew keypoints
            MKeyPoint[] mKeyPoints = keypointsVector.ToArray();
            foreach (var item in mKeyPoints)
            {
                keyPointsMap.Draw(new CircleF(item.Point, 10), new Gray(255), -1);
                image.Draw(new CircleF(item.Point, 10), new Bgr(Color.Red), -1);
            }
            //outputpic
            image.Save("005.jpg");
            surf = new SURF(
                                   600   //threshold
                                   ); //extended descriptors
                                           //feature point detection
            //keypointsVector = surf.DetectKeyPointsRaw(grayImage, null);
            surf.DetectRaw(grayImage, keypointsVector);
            //filtering keypoints by point size 
            keypointsVector.FilterByKeypointSize(10, 100);
            //drew keypoints
            Image<Bgr, byte> oriimage = image.Copy();
            Features2DToolbox.DrawKeypoints(
                                oriimage,     //original image
                                keypointsVector, //vector of keypoints 
                                image,      //output image
                                new Bgr(Color.Yellow), // keypoint color
                                Features2DToolbox.KeypointDrawType.DrawRichKeypoints); //drawing type
            //outputpic
            image.Save("007.jpg");
            mKeyPoints = keypointsVector.ToArray();
            List<PointF> keyPointsArray = new List<PointF>();
            int leftEyeRX = 0, leftEyeRY = 0, leftEyeLX = (int)result.Width, leftEyeLY = 0;
            int rightEyeRX = 0, rightEyeRY = 0, rightEyeLX = (int)result.Width, rightEyeLY = 0;
            captureRange = 8;
            foreach (var item in mKeyPoints)
            {
                x = (int)item.Point.X;
                y = (int)item.Point.Y;
                count = 0;
                count2 = 0;
                captureRange = (int)item.Size;
                for (int i = -captureRange; i < captureRange + 1; i += captureRange / 2)
                {
                    for (int j = -captureRange; j < captureRange + 1; j += captureRange / 2)
                    {
                        if ((x + i) > 0 && (x + i) < image.Width && (y + j) > Math.Max(Math.Max(pupils[0].Center.Y - pupils[0].Radius, pupils[1].Center.Y - pupils[1].Radius), 0) && (y + j) < Math.Min(Math.Max(pupils[0].Center.Y + pupils[0].Radius * 1.5, pupils[1].Center.Y + pupils[1].Radius * 1.5), image.Height))
                        {
                            if (keyPointCheck.Equals(keyPointsMap[y + j, x + i]))
                            {
                                count++;
                            }
                            if (keyPointCheck.Equals(skinMap[y + j, x + i]))
                            {
                                count2++;
                            }
                            if (count > 5 && count2 > 5)
                            {
                                i = captureRange + 1;
                                keyPointsArray.Add(new PointF(x, y));
                                //result.Draw(new Cross2DF(new Point(x, y), 20, 20), new Bgr(Color.Green), 2);
                                if (x < image.Width * 3 / 7)
                                {
                                    //left eye detect
                                    if (x > leftEyeRX)
                                    {
                                        leftEyeRX = x;
                                        leftEyeRY = y;
                                    }
                                    if (x < leftEyeLX)
                                    {
                                        leftEyeLX = x;
                                        leftEyeLY = y;
                                    }
                                }
                                else if (x > image.Width * 4 / 7)
                                {
                                    //right eye detect
                                    if (x > rightEyeRX)
                                    {
                                        rightEyeRX = x;
                                        rightEyeRY = y;
                                    }
                                    if (x < rightEyeLX)
                                    {
                                        rightEyeLX = x;
                                        rightEyeLY = y;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
            //drew keypoints
            int size = 50, thickness = 3;
            Bgr drewColorL = new Bgr(Color.LightGreen);
            Bgr drewColorR = new Bgr(Color.LightSkyBlue);
            result.Draw(new Cross2DF(new Point(leftEyeRX, leftEyeRY), size, size), drewColorL, thickness);
            result.Draw(new Cross2DF(new Point(leftEyeLX, leftEyeLY), size, size), drewColorL, thickness);
            result.Draw(new Cross2DF(new Point(rightEyeRX, rightEyeRY), size, size), drewColorR, thickness);
            result.Draw(new Cross2DF(new Point(rightEyeLX, rightEyeLY), size, size), drewColorR, thickness);
            leftCornerPoints.Add(new Point(leftEyeLX, leftEyeLY));
            leftCornerPoints.Add(new Point(leftEyeRX, leftEyeRY));
            rightCornerPoints.Add(new Point(rightEyeLX, rightEyeLY));
            rightCornerPoints.Add(new Point(rightEyeRX, rightEyeRY));
            #endregion
            //outputpic
            result.Save("009.jpg");
        }

        private int GetElevationMode(List<int> elevationList)
        {
            try
            {
                int count;
                bool flag = false;
                Dictionary<int, int> dictionary = new Dictionary<int, int>();
                for (int i = 0; i < elevationList.Count; i++)
                {
                    if (dictionary.TryGetValue(elevationList[i], out count))
                    {
                        flag = true;
                        dictionary[elevationList[i]]++;
                    }
                    else
                        dictionary.Add(elevationList[i], 1);
                }
                //reture 0,if there is no mode
                if (!flag)
                    return 0;
                int max = 0;
                int position = 0;
                int[] modeArray = new int[elevationList.Count];
                //find max count of Mode
                foreach (KeyValuePair<int, int> myKey in dictionary)
                {
                    if (myKey.Value > max)
                    {
                        max = myKey.Value;
                        position = 0;
                        modeArray[0] = myKey.Key;
                    }
                    else if (myKey.Value == max)
                        modeArray[++position] = myKey.Key;
                }
                Array.Resize(ref modeArray, position + 1);
                int mode = 0;
                //if there are a lot of mode , calculate average
                if (modeArray.Length > 1)
                {
                    for (int i = 0; i < modeArray.Length; i++)
                    {
                        mode += modeArray[i];
                    }
                    double elevationMode = mode / modeArray.Length;
                }
                else
                {
                    mode = modeArray[0];
                }
                return mode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }
    }
}
