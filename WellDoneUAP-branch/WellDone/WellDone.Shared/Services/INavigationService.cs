using System;
using System.Collections.Generic;
using System.Text;

namespace WellDone.Services
{
    public enum Experiences { Main, Login, List, TaskDetail, ContextDetail, About }
    public interface INavigationService
    {
        bool Navigate(Experiences experience, object param = null);
        void GoBack();
        bool CanGoBack { get; }
        void ClearHistory();
    }
}
