using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestAnimation {
    public class LambdaCollection<T> : Collection<T>
        where T : DependencyObject, new() {
        public LambdaCollection(int count) {
            while (count-- > 0) {
                Add(new T());
            } 
        }

        public LambdaCollection<T> WithProperty<U>(DependencyProperty property, Func<int, U> generator) {
            for (int i = 1; i < Count; i++) {
                this[i].SetValue(property, generator(i));
            }

            return this;
        }

        public LambdaCollection<T> WithPropertyRect<U>(DependencyProperty property, Func<int, U> generator) {
            this[0].SetValue(property, generator(0));
            return this;
        }

        public LambdaCollection<T> WithXY<U>(Func<int, U> xGen, Func<int, U> yGen) {
            for (int i = 0; i < Count; i++) {
                this[i].SetValue(Canvas.LeftProperty,  xGen(i - 2));
                this[i].SetValue(Canvas.TopProperty, yGen(i));
            }

            return this;
        }
        
    }

    public class LambdaDoubleAnimation : DoubleAnimation {
        public Func<double, double> ValueGenerator { get; set; }

        // protected override double GetCurrentValueCore(double defaultOriginValue, double defaultDestinationValue,
        //     AnimationClock animationClock) {
        //     return ValueGenerator(base.GetCurrentValueCore(defaultOriginValue, defaultDestinationValue, animationClock));
        // }
    }

    public class LambdaDoubleAnimationCollection : Collection<LambdaDoubleAnimation> {
        public LambdaDoubleAnimationCollection(int count, Func<int, double> from, Func<int, double> to, Func<int, 
            Duration> duration, Func<int, Func<double, double>> valueGenerator) {
            for (int i = 0; i < count; i++) {
                var lda = new LambdaDoubleAnimation {
                    From = from(i),
                    To = to(i),
                    Duration = duration(i),
                    //ValueGenerator = valueGenerator(i)
                };
                Add(lda);
            }
        }

        public void BeginApplyAnimation(UIElement[] targets, DependencyProperty property) {
            for (int i = 0; i < Count; i++) {
                targets[i].BeginAnimation(property, Items[i]);
            }
        }
    }



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LambdaCollection<Ellipse> circles;
        private LambdaCollection<Rectangle> channel;
        private const double _height = 300;
        private const double _width = 600;

        public class Ball
        {
            public Ellipse Shape { get; set; }
            public Point Position { get; set; }
            public double Radius { get; set; }
            public bool IsDeleted { get; set; }
            public SolidColorBrush BallColor = Brushes.DodgerBlue;
        }

        public class BallMovement
        {
            public Ball Ball { get; set; }
            public double Speed { get; set; }
        }

        private BallMovement[] balls;

        public MainWindow()
        {
            InitializeComponent();
            //InitializeBalls();
            //StartTimer();

            const int count = 19;
            MyCanvas.Height = _height;
            MyCanvas.Width = _width;
            MyCanvas.Background = Brushes.Transparent;
            CanvasBorder.BorderBrush = Brushes.Black;
            CanvasBorder.Height = _height;
            CanvasBorder.Width = _width;
            // CanvasBorder.BorderThickness = new Thickness(1);

            System.Windows.Shapes.Rectangle rect;
            rect = new System.Windows.Shapes.Rectangle
            {
                Stroke = new SolidColorBrush(Colors.Black),
                Fill = Brushes.Transparent,
                Width = 30,
                Height = _height
            };
            Canvas.SetLeft(rect, MyCanvas.Width);
            // Canvas.SetTop(rect,MyCanvas);

            var countOfRows = (int)_height / 35;

            balls = new BallMovement[count * countOfRows];
            int k = 0;
            for (int j = 0; j < countOfRows; j++)
            {
                circles = new LambdaCollection<Ellipse>(count)
                    .WithProperty(WidthProperty, i => 20.0)
                    .WithProperty(HeightProperty, i => 20.0)
                    .WithProperty(Shape.FillProperty, i => new SolidColorBrush(Color.FromArgb(255, 135, 206, 250)))
                    .WithXY(x => x * 25.0,
                        y => y * 0.0 + j * 35.0 - 23.0);
                var widthBetween = -50;
                foreach (var ellipse in circles)
                {
                    //MyCanvas.Children.Add(ellipse);

                    balls[k] = new BallMovement()
                    {
                        Ball = new Ball
                        {
                            Shape = ellipse,
                            Position = new Point(widthBetween, j * 35.0 - 16.0),
                            Radius = 10
                        },
                        Speed = 1 + j,
                        //Angle = 45 // задаем направление движения шарика в градусах
                    };
                    MyCanvas.Children.Add(balls[k].Ball.Shape);
                    widthBetween += 25;
                    k++;
                }


            }

        }

        private void ButtonDo_Click(object sender, RoutedEventArgs e)
        {
            foreach (var ball in balls)
            {
                MakeAnimation(ball, true);
            }
        }

        private void MakeAnimation(BallMovement ball, bool isStarted = false)
        {
            var startPositionX = isStarted ? ball.Ball.Position.X : -25;
            var myDoubleAnimation = new DoubleAnimation
            {
                
                From = startPositionX,
                To = MyCanvas.Width - 100,
                FillBehavior = FillBehavior.Stop,
                Duration = new Duration(
                    new TimeSpan((long)((MyCanvas.Width - 100 - startPositionX) * 30000))) // какое-то число для нормальных тиков
            }; 
            myDoubleAnimation.Completed += (o, args) =>  MakeAnimation(ball);
            Storyboard.SetTarget(myDoubleAnimation, ball.Ball.Shape);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(Canvas.LeftProperty));
            var myStoryboard = new Storyboard
            {
                SpeedRatio = ball.Speed / 20
            };
            myStoryboard.Children.Add(myDoubleAnimation);
            myStoryboard.Begin();
        }

    }
}
