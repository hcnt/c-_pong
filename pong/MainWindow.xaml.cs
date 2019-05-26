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
    class PongGame
    {
        Size boardSize = new Size(800, 450);
        int player1Score = 0;
        int player2Score = 0;
        Point player1Pos = new Point(10, 200);
        Point player2Pos = new Point(770, 200);
        List<Point> oldPlayer1Positions = new List<Point>();
        List<Point> oldPlayer2Positions = new List<Point>();
        Point ballPos = new Point(200, 200);
        Vector ballSpeed = new Vector(6, 2);
        KinectHandler kinectHandler = new KinectHandler();
        Rectangle ball;
        Rectangle player1;
        Rectangle player2;
        int whoWon = 0;
        DispatcherTimer timer = new DispatcherTimer();
        MainWindow window;
        public PongGame(MainWindow window)
        {
            this.window = window;
            player1 = new Rectangle
            {
                Fill = Brushes.Black,
                Width = 20,
                Height = 100
            };
            player2 = new Rectangle
            {
                Fill = Brushes.Black,
                Width = 20,
                Height = 100
            };

            ball = new Rectangle
            {
                Fill = Brushes.Red,
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
            if(ballPos.X + ball.Width >= player2Pos.X && ballPos.Y + ball.Height/2 <= player2Pos.Y + player2.Height && ballPos.Y + ball.Height/2 >= player2Pos.Y)
            {
                ballSpeed.X *= -1;
            }
            if (ballPos.X <= player1Pos.X + player1.Width && ballPos.Y + ball.Height / 2 <= player1Pos.Y + player1.Height && ballPos.Y + ball.Height / 2 >= player1Pos.Y)
            {
                ballSpeed.X *= -1;
            }
            ballPos = Point.Add(ballPos, ballSpeed);
            Canvas.SetTop(ball, ballPos.Y);
            Canvas.SetLeft(ball, ballPos.X);
        }
        private void updatePlayers()
        {
          
            Point newPlayer1Pos = GetMousePos();//kinectHandler.point;
            Point newPlayer2Pos = GetMousePos(); //kinectHandler.point2;

            if (newPlayer1Pos.X > newPlayer2Pos.X)
            {
                Point tmp = newPlayer1Pos;
                newPlayer1Pos = newPlayer2Pos;
                newPlayer2Pos = tmp;
            }

            player1Pos.Y = map(newPlayer1Pos.Y,15,450,-player1.Height/2,boardSize.Height - player2.Height*2/3);
            player2Pos.Y = map(newPlayer2Pos.Y,15,450,-player1.Height/2,boardSize.Height - player2.Height*2/3);


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
            ballPos = new Point(200, 200);
            ballSpeed = new Vector(5, 1);
        }
        public void startGame()
        {
            window.paintCanvas.Children.Add(ball);
            window.paintCanvas.Children.Add(player1);
            window.paintCanvas.Children.Add(player2);
            timer.Tick += new EventHandler(playFrame);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timer.Start();
            kinectHandler.SetupKinectSensor();
        }
       
        private void playFrame(object sender, EventArgs e)
        {
            Point mousePos = GetMousePos();

            int cursorY = (int) mousePos.Y;  //(int) map((int)kinectHandler.point.Y, 100, 300, 0, window.Height);
            int cursorX = (int) mousePos.X; //(int) map((int)kinectHandler.point.X, 0, 300, 0, window.Width);
            
            //MainWindow.SetCursor(cursorX,cursorY);
            if (whoWon == 0) //&& kinectHandler.kinectTracking)
            {
                updateBall();
                updatePlayers();
                whoWon = checkForWin();
                window.text.Content = kinectHandler.skeletonsLength.ToString();
            }
            else if (whoWon != 0)
            {
                //window.playAgainButton.Visibility = Visibility.Visible;
                if(whoWon == 1)
                {
                    player1Score += 1;
                    window.points1.Content = player1Score.ToString();
                } else if(whoWon == 2)
                {
                    player2Score += 1;
                    window.points2.Content = player1Score.ToString();
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
