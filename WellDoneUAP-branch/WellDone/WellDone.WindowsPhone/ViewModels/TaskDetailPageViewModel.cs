using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WellDone.DataModel;
using WellDone.Services;

namespace WellDone.ViewModels
{
    public class TaskDetailPageViewModel : ViewModel, ITaskDetailViewModel
    {
        private INavigationService _navigationService;
        private IMainPageViewModel _mainPageViewModel;
        public TaskDetailPageViewModel(INavigationService navigationService, IMainPageViewModel mainPageViewModel)
        {
            _navigationService = navigationService;
            _mainPageViewModel = mainPageViewModel;
            
        }

        public override void OnNavigatedTo(object navigationParameter, Windows.UI.Xaml.Navigation.NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
           SelectedTask = navigationParameter as WellDoneTask;
           
           
        }

        //public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        //{
        //    //_navigationService.Navigate(Experiences.List);
        //    //base.OnNavigatedFrom(viewModelState, suspending);            
        //}


        private WellDoneTask _SelectedTask;
        public WellDoneTask SelectedTask
        {
            get { return _SelectedTask; }
            set 
            {                
                SetProperty(ref _SelectedTask, value);
                _mainPageViewModel.SelectedTask = _SelectedTask;
            }
        }

        private string _ControlHeaderText;
        public string ControlHeaderText
        {
            get { return _mainPageViewModel.ControlHeaderText; }
            set { SetProperty(ref _ControlHeaderText, value); }
        }

        private bool _IsTaskDueDate;
        public bool IsTaskDueDate
        {
            get { return _mainPageViewModel.IsTaskDueDate; }
            set 
            { 
                SetProperty(ref _IsTaskDueDate, value);
                _mainPageViewModel.IsTaskDueDate = _IsTaskDueDate;
            }
        }

        private MobileServiceCollection<ContextDescription,ContextDescription> _contextsCollection;
        public MobileServiceCollection<ContextDescription,ContextDescription> ContextsCollection
        {
            get { return _mainPageViewModel.ContextsCollection; }
            set 
            { 
                SetProperty(ref _contextsCollection, value);                
            }
        }

        private MobileServiceCollection<WellDoneTask,WellDoneTask> _ProjectTasksCollection;
        public MobileServiceCollection<WellDoneTask,WellDoneTask> ProjectTasksCollection
        {
            get { return _mainPageViewModel.ProjectTasksCollection; }
            set { SetProperty(ref _ProjectTasksCollection, value); }
        }

        private bool _IsAddState;
        public bool IsAddState
        {
            get { return _mainPageViewModel.IsAddState; }
            set { SetProperty(ref _IsAddState, value); }
        }

        DelegateCommand<string> _SaveNewTaskCommand;
        public DelegateCommand<string> SaveNewTaskCommand
        {
            get
            {
                if (_SaveNewTaskCommand != null)
                    return _SaveNewTaskCommand;
                _SaveNewTaskCommand = _mainPageViewModel.SaveNewTaskCommand;  
                  
                this.PropertyChanged += (s, e) => _SaveNewTaskCommand.RaiseCanExecuteChanged();
                return _SaveNewTaskCommand;
            }
        }

        DelegateCommand _UpdateCommand;
        public DelegateCommand UpdateCommand
        {
            get
            {
                if (_UpdateCommand != null)
                    return _UpdateCommand;
                _UpdateCommand = _mainPageViewModel.UpdateCommand;
                    
                this.PropertyChanged += (s, e) => _UpdateCommand.RaiseCanExecuteChanged();
                return _UpdateCommand;
            }
        }


        

        
    }
}
