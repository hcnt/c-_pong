namespace pong
{
    using Microsoft.Kinect;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Input;
    using System.Windows.Threading;

    public partial class KinectHandler
    {
        public Point point1;
        public Point point2;
        public bool kinectTracking = false;
        public int skeletonsLength = 0;
        public List<Point> points1 = new List<Point>(); 
        public List<Point> points2 = new List<Point>();
        public int numberOfSkeletonsActive = 0;

        private KinectSensor sensor;

        private DrawingImage imageSource;

        public System.Windows.Shapes.Ellipse mouseEllipse;
        public System.Windows.Shapes.Ellipse mouseEllipseSmall;

        DispatcherTimer dispatcherTimer;

        private List<UserControl> userControls;
        private int slideIndex = 0;
        public void SetupKinectSensor()
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                this.sensor.SkeletonStream.Enable();
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                Console.WriteLine("Kinect nie działa");
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            SolidColorBrush GreenYellowBrush = new SolidColorBrush();
            GreenYellowBrush.Color = Colors.GreenYellow;
            SolidColorBrush WhiteBrush = new SolidColorBrush();
            WhiteBrush.Color = Colors.White;
            SetupKinectSensor();
        }
        
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }
            bool isScelTracked = false;
            int skeletonNumber = 0;
            if (skeletons.Length != 0)
            {
                foreach (Skeleton skeleton in skeletons)
                {
                    if (isScelTracked) {
                       // break;
                    }
                   
                    if (skeleton.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked)
                    {
                        Updates(skeleton, skeletonNumber);
                        skeletonNumber += 1;
                        isScelTracked = true;
                        kinectTracking = true;
                    }
                    //else if (skeleton.TrackingState == SkeletonTrackingState.NotTracked)
                    //{
                        
                    //}
                }
            }
            numberOfSkeletonsActive = skeletonNumber;
        }

        private void Updates(Skeleton skeleton,int skeletonNumber)
        {
            Joint handJoint;
            float downLimit;

            handJoint = skeleton.Joints[JointType.HandRight];

            if (skeletonNumber == 0)
            {
               points1.Add(SkeletonPointToScreen(handJoint.Position));
                //point1 = SkeletonPointToScreen(handJoint.Position);
            } else if (skeletonNumber == 1)
            {
                //point2 = SkeletonPointToScreen(handJoint.Position);
                points2.Add(SkeletonPointToScreen(handJoint.Position));
            }
            point1 = ArithmeticAverage(points1,1);
            point2 = ArithmeticAverage(points2,1);

            if(point1.X > point2.X)
            {
                Point tmp = point1;
                point1 = point2;
                point2 = tmp;
            }
        }
        public Point ArithmeticAverage(List<Point> points,int val = 3)
        {
            int amount = 0;
            double sumX = 0;
            double sumY = 0;
            for (int i = points.Count - 1; i >= 0 && i >= points.Count - 1 - val; i--)
            {
                sumX += points[i].X;
                sumY += points[i].Y;
                amount++;
            }
            return new Point(sumX / amount, sumY / amount);
        }

        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }
    }
}