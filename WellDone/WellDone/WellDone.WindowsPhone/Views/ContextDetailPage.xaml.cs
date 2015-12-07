using System;
using System.Linq;
using WellDone.Controls;

namespace WellDone.Views
{
    
    public sealed partial class ContextDetailPage : PageBase
    {
        public ContextDetailPage()
        {
            this.InitializeComponent();
        }

        //protected override void OnNavigatingFrom(Windows.UI.Xaml.Navigation.NavigatingCancelEventArgs e)
        //{
            
        //    base.OnNavigatingFrom(e);
        //}

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            var previousPage = this.Frame.BackStack.FirstOrDefault();

             if (previousPage != null && previousPage.SourcePageType == typeof(MainPage))
             {
                 this.Frame.BackStack.RemoveAt(this.Frame.BackStackDepth - 1);
             }
            base.OnNavigatedFrom(e);
        }
    }
}
