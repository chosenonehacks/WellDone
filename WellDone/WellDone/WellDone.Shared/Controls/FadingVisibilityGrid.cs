using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace WellDone.Controls
{
    public class FadingVisibilityGrid : Grid
    {
        public static readonly DependencyProperty DeferredVisibilityProperty = DependencyProperty.Register(
            "DeferredVisibility", typeof(Visibility), typeof(FadingVisibilityGrid), new PropertyMetadata(default(Visibility), DeferredVisibilityChanged));

        private static void DeferredVisibilityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var newVisibility = (Visibility)e.NewValue;
            var grid = (FadingVisibilityGrid)sender;


            var animation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(200))
            };
            Storyboard.SetTarget(animation, grid);
            Storyboard.SetTargetProperty(animation, "Grid.Opacity");
            grid.FadeStoryBoard.Stop();
            grid.FadeStoryBoard = new Storyboard();
            grid.FadeStoryBoard.Children.Add(animation);

            if (newVisibility == Visibility.Visible)
            {
                animation.From = 0;
                animation.To = 1;
                grid.Visibility = Visibility.Visible;
                grid.FadeStoryBoard.Begin();
            }
            else
            {
                animation.From = 1;
                animation.To = 0;
                grid.FadeStoryBoard.Completed += (o, o1) =>
                {
                    grid.Visibility = newVisibility;
                };
                grid.FadeStoryBoard.Begin();
            }

        }

        public Visibility DeferredVisibility
        {
            get { return (Visibility)GetValue(DeferredVisibilityProperty); }
            set { SetValue(DeferredVisibilityProperty, value); }
        }

        private Storyboard _fadeStoryBoard = new Storyboard();

        public Storyboard FadeStoryBoard
        {
            get { return _fadeStoryBoard; }
            set { _fadeStoryBoard = value; }
        }
    }
}
