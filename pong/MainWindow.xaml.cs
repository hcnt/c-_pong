using System;
using System.Collections.Generic;
using System.Linq;
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


namespace pong
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    /// 
    class PongGame
    {

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
            Canvas.SetTop(player1, player1Pos.Y);
            Canvas.SetLeft(player1, player1Pos.X);

            Canvas.SetTop(player2, player2Pos.Y);
            Canvas.SetLeft(player2, player2Pos.X);

            Canvas.SetTop(ball, ballPos.Y);
            Canvas.SetLeft(ball, ballPos.X);
        }

        Point boardSize = new Point(800, 450);
        int player1Score = 0;
        int player2Score = 0;
        Point player1Pos = new Point(10, 200);
        Point player2Pos = new Point(770, 200);
        Point ballPos = new Point(200, 200);
        Vector ballSpeed = new Vector(5, 1);
        Ellipse ball = new Ellipse
        {
            Fill = Brushes.Red,
            Width = 50,
            Height = 50
        };

        Rectangle player1;
        Rectangle player2;

        int whoWon = 0;
        DispatcherTimer timer = new DispatcherTimer();
        MainWindow window;

        private void updateBall()
        {
            if(ballPos.Y + ball.Width >= boardSize.Y || ballPos.Y <= 0)
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
            Point mousePos = GetMousePos();

            player1Pos.Y = map(mousePos.Y,0,window.Height,0,boardSize.Y - player2.Height/2);
            player2Pos.Y = map(mousePos.Y,0,window.Height,0,boardSize.Y - player2.Height/2);

            Canvas.SetTop(player1, player1Pos.Y);
            Canvas.SetTop(player2, player2Pos.Y);
        }
        int checkForWin()
        {
            if (ballPos.X > boardSize.X)
            {
                return 1;
            }
            else if (ballPos.X + ball.Width < 0)
            {
                return 2;
            }
            return 0;
        }
        public void startGame()
        {
            window.paintCanvas.Children.Add(ball);
            window.paintCanvas.Children.Add(player1);
            window.paintCanvas.Children.Add(player2);
            timer.Tick += new EventHandler(playFrame);
            timer.Interval = new TimeSpan(0, 0, 0,0, 1);
            timer.Start();
        }
       
        private void playFrame(object sender, EventArgs e)
        {
            updateBall();
            updatePlayers();
            whoWon = checkForWin();
            if (whoWon != 0)
            {
                timer.Stop();
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
        public MainWindow()
        {
            InitializeComponent();
            PongGame game = new PongGame(this);
            game.startGame();

        }
    }
}
