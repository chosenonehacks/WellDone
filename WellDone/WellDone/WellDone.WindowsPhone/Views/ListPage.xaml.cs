using System;
using System.Linq;
using WellDone.Controls;

namespace WellDone.Views
{
    public sealed partial class ListPage : PageBase
    {
        public ListPage()
        {
            this.InitializeComponent();
        }

        //http://stackoverflow.com/questions/8241529/clearing-backstack-in-navigationservice
        //protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        //{
        //    var previousPage = this.Frame.BackStack.FirstOrDefault();

        //     if (previousPage != null && previousPage.SourcePageType == typeof(MainPage))
        //     {
        //         this.Frame.BackStack.RemoveAt(this.Frame.BackStackDepth - 1);
        //     }
        //    base.OnNavigatedFrom(e);
        //}
    }
}
