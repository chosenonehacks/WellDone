using System;
using System.Collections.Generic;
using System.Text;
using WellDone.ViewModels;

namespace WellDone.DesignTime
{
    public class AboutPageViewModel : IAboutPageViewModel
    {
        public AboutPageViewModel()
        {
            Title = "Well Done design";
        }



        public string Title { get; set; }
    }
}
