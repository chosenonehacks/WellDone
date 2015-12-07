using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace WellDone.Services
{
    public class DialogService : IDialogService
    {
        bool _open = false;

        public DialogService()
        {
            // not thread safe
        }
        public async void Show(string content)
        {
            while (_open) { await Task.Delay(1000); }
            _open = true;
            var dialog = new MessageDialog(content);
            await dialog.ShowAsync();
            _open = false;
        }

        public async void Show(string content, string title = default(string))
        {
            while (_open) { await Task.Delay(1000); }
            _open = true;
            var dialog = new MessageDialog(content, title);
            await dialog.ShowAsync();
            _open = false;
        }

        public async void Show(string content, string title, params UICommand[] commands)
        {
            while (_open) { await Task.Delay(1000); }
            _open = true;
            var dialog = new MessageDialog(content, title);
            foreach (var item in commands)
                dialog.Commands.Add(item);
            await dialog.ShowAsync();
            _open = false;
        }
    }
}
