using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WellDone.DataModel;
using WellDone.Services;

namespace WellDone.ViewModels
{
    public class ContextDetailPageViewModel : ViewModel, IContextDetailViewModel
    {
        private INavigationService _navigationService;
        private IMainPageViewModel _mainPageViewModel;
        public ContextDetailPageViewModel(INavigationService navigationService, IMainPageViewModel mainPageViewModel)
        {
            _navigationService = navigationService;
            _mainPageViewModel = mainPageViewModel;
        }

        public override void OnNavigatedTo(object navigationParameter, Windows.UI.Xaml.Navigation.NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
           SelectedContext = navigationParameter as ContextDescription;
            
        }

        //public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        //{
            
        //    base.OnNavigatedFrom(viewModelState, suspending);            
        //}

        private ContextDescription _SelectedContext;
        public ContextDescription SelectedContext
        {
            get { return _SelectedContext; }
            set 
            { 
                SetProperty(ref _SelectedContext, value);                
            }
        }

        DelegateCommand<string> _SaveNewContextCommand;
        public DelegateCommand<string> SaveNewContextCommand
        {
            get
            {
                if (_SaveNewContextCommand != null)
                    return _SaveNewContextCommand;
                _SaveNewContextCommand = _mainPageViewModel.SaveNewContextCommand;
                    
                this.PropertyChanged += (s, e) => _SaveNewContextCommand.RaiseCanExecuteChanged();
                return _SaveNewContextCommand;
            }
        }
        
    }
}
