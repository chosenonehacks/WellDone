using System;
using System.Linq;
using WellDone.Controls;


namespace WellDone.Views
{
    public sealed partial class TaskDetailPage : PageBase
    {
        public TaskDetailPage()
        {
            this.InitializeComponent();            
        }

        //protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        //{
        //        if(this.Frame.BackStack.Count > 1)
        //        this.Frame.BackStack.RemoveAt(this.Frame.BackStackDepth - 1);
             
        //    base.OnNavigatedFrom(e);
        //}
    }
}
