using System;
using System.Collections.Generic;
using System.Text;
using WellDone.ViewModels;

namespace WellDone.DesignTime
{
    public class LoginPageViewModel : ILoginPageViewModel
    {
        public bool IsLogingStarted { get; set; }
        public LoginPageViewModel()
        {
            IsLogingStarted = false;
        }

    }
}
