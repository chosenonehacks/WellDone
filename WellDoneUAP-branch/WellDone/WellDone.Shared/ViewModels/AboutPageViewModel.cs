using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace WellDone.ViewModels
{
    public class AboutPageViewModel : ViewModel, IAboutPageViewModel
    {
        public AboutPageViewModel()
        {
            Title = "Well Done";
        }

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set { SetProperty(ref _Title, value); }
        }

    }
}
