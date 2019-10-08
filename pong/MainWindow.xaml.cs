using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Kinect;


namespace pong
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    /// 

    enum GameState
    {
        PLAYER1WON = 1,
        PLAYER2WON = 2,
        WAITINGFORPLAYERS = 3,
        GAMEISRUNNING = 4 

    }
    class PongGame
    {
        Size boardSize = new Size(800, 450);
        int player1Score = 0;
        int player2Score = 0;
        Point player1Pos = new Point(10, 200);
        Point player2Pos = new Point(770, 200);
        List<Point> oldPlayer1Positions = new List<Point>();
        List<Point> oldPlayer2Positions = new List<Point>();
        Point player1PosTarget;
        Point player2PosTarget;
        double player1Speed;
        double player2Speed;
        Point ballPos;
        Vector ballSpeed;
        KinectHandler kinectHandler = new KinectHandler();
        Rectangle ball;
        Rectangle player1;
        Rectangle player2;
        Rectangle test;
        int whoWon = 0;
        int whichPlayerIsPlaying = 2;
        bool kinectMode = true;
        DispatcherTimer timer = new DispatcherTimer();
        MainWindow window;
        double xD = 0;
        int activePlayers = 0;
        public PongGame(MainWindow window)
        {
            this.window = window;
            player1 = new Rectangle
            {
                Fill = Brushes.Black,
                Width = 20,
                Height = 100,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            player2 = new Rectangle
            {
                Fill = Brushes.Black,
                Width = 20,
                Height = 100,
                HorizontalAlignment = HorizontalAlignment.Center

            };
            ball = new Rectangle
            {
                Fill = Brushes.Red,
                Width = 20,
                Height = 20
            };

            test = new Rectangle
            {
                Fill = Brushes.Blue,
                Width = 20,
                Height = 20
            };
            Canvas.SetTop(player1, player1Pos.Y);
            Canvas.SetLeft(player1, player1Pos.X);

            Canvas.SetTop(player2, player2Pos.Y);
            Canvas.SetLeft(player2, player2Pos.X);

            Canvas.SetTop(ball, ballPos.Y);
            Canvas.SetLeft(ball, ballPos.X);

            window.playAgainButton.Click += playAgainHandler;
            window.playAgainButton.Visibility = Visibility.Collapsed;
        }
        private void updateBall()
        {
            if(ballPos.Y + ball.Width >= boardSize.Height || ballPos.Y <= 0)
            {
                ballSpeed.Y *= -1;
            }
            if(ballPos.X + ball.Width >= player2Pos.X && ballPos.Y + ball.Height/2 <= player2Pos.Y + player2.Height && ballPos.Y + ball.Height/2 >= player2Pos.Y && whichPlayerIsPlaying == 2)
            {
                ballSpeed.X *= -1;
                calculateNewBallSpeed(-0.15 * player2Speed);
                whichPlayerIsPlaying = 1;
            }
            if (ballPos.X <= player1Pos.X + player1.Width && ballPos.Y + ball.Height / 2 <= player1Pos.Y + player1.Height && ballPos.Y + ball.Height / 2 >= player1Pos.Y && whichPlayerIsPlaying == 1)
            {
                ballSpeed.X *= -1;
                calculateNewBallSpeed(-0.15 * player1Speed);
                whichPlayerIsPlaying = 2;

            }
            ballPos = Point.Add(ballPos, ballSpeed);
            Canvas.SetTop(ball, ballPos.Y);
            Canvas.SetLeft(ball, ballPos.X);
        }

        void setRandomStartingBallDirection()
        {
            bool areValuesInGoodRange = false;
            Random rnd = new Random();
            double x = 0;
            double y = 0;
            while (!areValuesInGoodRange)
            {
                x = map(rnd.NextDouble(), 0, 1, -1, 1);
                y = map(rnd.NextDouble(), 0, 1, -1, 1);
                if(y/x < 1 && y/x > -1)
                {
                    areValuesInGoodRange = true;
                }
            }
            ballSpeed = new Vector(x, y);
            ballSpeed.Normalize();
            ballSpeed *= 8;
        }
        void calculateNewBallSpeed(double yd)
        {
            xD = yd;
            yd /= 2;
            if (yd < 0)
            {
                ballSpeed.Y -= Math.Log10(1 - yd)*4;
            }
            else
            {
                ballSpeed.Y += Math.Log10(1 + yd)*4;
            }
            //ballSpeed.Normalize();
            //ballSpeed *= 8;
        }
        private void updatePlayers()
        {

            if (kinectMode) {
               //player1PosTarget.X = map(kinectHandler.point1.X,15,450,-player1.Height/2,boardSize.Height - player2.Height*2/3);
               player1PosTarget.Y = map(kinectHandler.point1.Y,15,450,-player1.Height/2-20,boardSize.Height - player2.Height*2/3);
               //player2PosTarget.X = map(kinectHandler.point2.X,25,450,-player2.Height/2,boardSize.Height - player2.Height*2/3);
               player2PosTarget.Y = map(kinectHandler.point2.Y,25,450,-player2.Height/2-20,boardSize.Height - player2.Height*2/3);

               Canvas.SetTop(test, player1PosTarget.Y);
               Canvas.SetLeft(test, player1PosTarget.X);

               player1Pos.Y = player1Pos.Y + (player1PosTarget.Y - player1Pos.Y) * 0.4;
               player2Pos.Y = player2Pos.Y + (player2PosTarget.Y - player2Pos.Y) * 0.4;
            } else
            {
                player1Pos.Y = GetMousePos().Y;
                player2Pos.Y = GetMousePos().Y;
            }

            oldPlayer1Positions.Add(new Point(player1Pos.X, player1Pos.Y));
            oldPlayer2Positions.Add(new Point(player2Pos.X, player2Pos.Y));

            if(oldPlayer1Positions.Count() > 5)
            {
                oldPlayer1Positions.RemoveAt(0);
            }

            if (oldPlayer2Positions.Count() > 5)
            {
                oldPlayer2Positions.RemoveAt(0);
            }

            player1Speed = oldPlayer1Positions[0].Y - oldPlayer1Positions[oldPlayer1Positions.Count()-1].Y;
            player2Speed = oldPlayer2Positions[0].Y - oldPlayer2Positions[oldPlayer2Positions.Count() - 1].Y;

            //player2.Width = 20 - Math.Log10(Math.Abs(player2Speed)+0.1)*2;
            //player1.Width = 20 - Math.Log10(Math.Abs(player1Speed)+0.1)*2;

            //player1Pos.Y = map(newPlayer1Pos.Y,15,450,-player1.Height/2,boardSize.Height - player2.Height*2/3);
            //player2Pos.Y = map(newPlayer2Pos.Y,15,450,-player1.Height/2,boardSize.Height - player2.Height*2/3);
            //player1Pos = newPlayer1Pos;
            //player2Pos = newPlayer2Pos;

            Canvas.SetTop(player1, player1Pos.Y);
            Canvas.SetTop(player2, player2Pos.Y);
        }
        int checkForWin()
        {
            if (ballPos.X > boardSize.Width)
            {
                return 1;
            }
            else if (ballPos.X + ball.Width < 0)
            {
                return 2;
            }
            return 0;
        }

        public void playAgainHandler(object sender, RoutedEventArgs e)
        {
            setStartingPosition();
            window.playAgainButton.Visibility = Visibility.Collapsed;
            whoWon = 0;
        }
        public void setStartingPosition()
        {
            whoWon = 0;
            player1Pos = new Point(10, 200);
            player2Pos = new Point(770, 200);
            ballPos = new Point(window.Width/2,window.Height/2);
            setRandomStartingBallDirection();
            if (ballSpeed.X < 0)
            {
                whichPlayerIsPlaying = 1;
            } else
            {
                whichPlayerIsPlaying = 2;
            }
        }
        public void startGame()
        {
            window.paintCanvas.Children.Add(ball);
            //window.paintCanvas.Children.Add(test);
            window.paintCanvas.Children.Add(player1);
            window.paintCanvas.Children.Add(player2);
            timer.Tick += new EventHandler(playFrame);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            timer.Start();
            setStartingPosition();
            kinectHandler.SetupKinectSensor();
        }
        private void playFrame(object sender, EventArgs e)
        {
            Point mousePos = GetMousePos();
            // int cursorY = (int)mousePos.Y; // (int) map((int)kinectHandler.point.Y, 100, 300, 0, window.Height);
            //int cursorX = (int)mousePos.X; //  map((int)kinectHandler.point.X, 0, 300, 0, window.Width);
            //MainWindow.SetCursor(cursorX,cursorY);
            window.centerLabel.Content = "Oczekiwanie na graczy " + kinectHandler.numberOfSkeletonsActive.ToString() + "/2";
            if (whoWon == 0 && (kinectHandler.kinectTracking && kinectHandler.numberOfSkeletonsActive > 0  || !kinectMode))
            {
                //if (window.centerLabel.Visibility == Visibility.Visible){
                //    window.centerLabel.Visibility = Visibility.Hidden;
                //}
                updateBall();
                updatePlayers();
                whoWon = checkForWin();
                //window.text.Content = xD.ToString();
            }
            else if (whoWon != 0)
            {
                if (window.centerLabel.Visibility == Visibility.Hidden)
                {
                    window.centerLabel.Visibility = Visibility.Visible;
                }
                //window.playAgainButton.Visibility = Visibility.Visible;
                if(whoWon == 1)
                {
                    player1Score += 1;
                    window.points1.Content = player1Score.ToString();
                } else if(whoWon == 2)
                {
                    player2Score += 1;
                    window.points2.Content = player2Score.ToString();
                }
                whoWon = 0;
                setStartingPosition();
            }
        }
        Point GetMousePos()
        {
            return Mouse.GetPosition(Application.Current.MainWindow);
        }
        double map(double s, double a1, double a2, double b1, double b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }
    }
    public partial class MainWindow : Window
    {
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public static void SetCursor(int x, int y)
        {
            // Left boundary
            var xL = (int)App.Current.MainWindow.Left;
            // Top boundary
            var yT = (int)App.Current.MainWindow.Top;

            SetCursorPos(x + xL, y + yT);
        }
        public MainWindow()
        {
            InitializeComponent();
            //Mouse.OverrideCursor = Cursors.None;
            PongGame game = new PongGame(this);
            game.startGame();
        }
    }
}
