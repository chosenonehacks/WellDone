using System;
using System.Collections.Generic;
using System.Text;

namespace WellDone.Services
{
    public interface IDialogService
    {
        void Show(string content);
        void Show(string content, string title = default(string));
        void Show(string content, string title, params global::Windows.UI.Popups.UICommand[] commands);
    }
}
