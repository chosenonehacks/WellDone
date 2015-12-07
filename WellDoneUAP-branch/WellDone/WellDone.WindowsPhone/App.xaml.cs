using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace WellDone
{
    public sealed partial class App : Microsoft.Practices.Prism.Mvvm.MvvmAppBase
    {
        protected override void OnHardwareButtonsBackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            DebugThis();            

            // don't do this if we call the base - goes back twice!
            //if (NavigationService.CanGoBack())
            //{
            //    NavigationService.GoBack();
            //    e.Handled = true;
            //}
            base.OnHardwareButtonsBackPressed(sender, e);
        }

        
    }
}
