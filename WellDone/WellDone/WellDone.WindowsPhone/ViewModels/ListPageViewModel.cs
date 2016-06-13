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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WellDone.ViewModels
{
    public class ListPageViewModel : ViewModel, IListPageViewModel
    {
        //private IMobileServiceTable<WellDoneTask> wellDoneTable;
        private INavigationService _navigationService;
        public ListPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }        

        public override void OnNavigatedTo(object navigationParameter, Windows.UI.Xaml.Navigation.NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {

           
           MainViewModel = navigationParameter as MainPageViewModel;

           if (MainViewModel.SelectedList == "contexts")
           {
               MainViewModel.IsInsideContextList = true;               
           }
           
           
           IsInsideProjectList = false;
           
        }

        public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            MainViewModel.IsInsideContextList = false;
            IsInsideProjectList = false;

            base.OnNavigatedFrom(viewModelState, suspending);
            
            
        }

        public MainPageViewModel MainViewModel { get; set; }


        DelegateCommand _EditProjectCommand;
        public DelegateCommand EditProjectCommand
        {
            get
            {
                if (_EditProjectCommand != null)
                    return _EditProjectCommand;
                _EditProjectCommand = new DelegateCommand
                    (
                        () => { EditProject(); },
                        () => { return CanEditProject(); }
                    );
                this.PropertyChanged += (s, e) => _EditProjectCommand.RaiseCanExecuteChanged();
                return _EditProjectCommand;
            }
        }
 
        private bool CanEditProject()
        {
            if (SelectedTask != null && SelectedTask.IsProject == true)
                return true;
            else
                return false;

        }

        private void EditProject()
        {
            _navigationService.Navigate(Experiences.TaskDetail, SelectedTask);
        }

        DelegateCommand<ItemClickEventArgs> _ShowContextTasksCommand;
        public DelegateCommand<ItemClickEventArgs> ShowContextTasksCommand
        {
            get
            {
                if (_ShowContextTasksCommand != null)
                    return _ShowContextTasksCommand;
                _ShowContextTasksCommand = new DelegateCommand<ItemClickEventArgs>
                    (
                      async  (args) => { await ShowContextTasks(args); },
                        (args) => { return CanShowContextTasks(args); }
                    );
                this.PropertyChanged += (s, e) => _ShowContextTasksCommand.RaiseCanExecuteChanged();
                return _ShowContextTasksCommand;
            }
        }

        private bool CanShowContextTasks(ItemClickEventArgs args)
        {

            if (MainViewModel.SelectedList == "contexts")
                return true;
            else
                return false;
        }


        private async Task ShowContextTasks(ItemClickEventArgs args)
        {   
            SelectedContext = args.ClickedItem as ContextDescription;
            MainViewModel.Tasks = await MainViewModel.wellDoneTable.Where(list => list.IsComplete == false).Where(task => task.ContextId == SelectedContext.Id).OrderByDescending(order => order.CreatedAt).ToCollectionAsync();
            MainViewModel.IsTaskListVisibile = true; //switch to task listview
            MainViewModel.SelectedList = SelectedContext.Name;
            MainViewModel.IsInsideContextList = true;
        }

        


        private ContextDescription _SelectedContext;
        public ContextDescription SelectedContext
        {
            get { return _SelectedContext; }
            set 
            { 
                SetProperty(ref _SelectedContext, value);
                //IsAppBarVisibile = true;
                MainViewModel.SelectedContext = _SelectedContext;
            }
        }

        DelegateCommand<RoutedEventArgs> _CheckTaskCommand;
        public DelegateCommand<RoutedEventArgs> CheckTaskCommand
        {
            get
            {
                if (_CheckTaskCommand != null)
                    return _CheckTaskCommand;
                _CheckTaskCommand = new DelegateCommand<RoutedEventArgs>
                    (
                      async  (args) => { await CheckTask(args); },
                        (args) => { return true; }
                    );
                this.PropertyChanged += (s, e) => _CheckTaskCommand.RaiseCanExecuteChanged();
                return _CheckTaskCommand;
            }
        }

        private async Task CheckTask(RoutedEventArgs args)
        {
            CheckBox cb = (CheckBox)args.OriginalSource;
            
            MainViewModel.SelectedTask = cb.DataContext as WellDoneTask;            
            await MainViewModel.CompleteTaskCommand.Execute();
        }

        DelegateCommand<ItemClickEventArgs> _ShowProjectTasksCommand;
        public DelegateCommand<ItemClickEventArgs> ShowProjectTasksCommand
        {
            get
            {
                if (_ShowProjectTasksCommand != null)
                    return _ShowProjectTasksCommand;
                _ShowProjectTasksCommand = new DelegateCommand<ItemClickEventArgs>
                    (
                      async  (args) => { await ShowProjectTasks(args); },
                        (args) => { return CanShowProjectTasks(args); }
                    );
                this.PropertyChanged += (s, e) => _ShowProjectTasksCommand.RaiseCanExecuteChanged();
                return _ShowProjectTasksCommand;
            }
        }

        private bool CanShowProjectTasks(ItemClickEventArgs args)
        {            
            SelectedTask = args.ClickedItem as WellDoneTask;
            if (SelectedTask != null && SelectedTask.IsProject == true)
                return true;
            else
            {
                _navigationService.Navigate(Experiences.TaskDetail, SelectedTask);
                return false;
                
            }
        }

        private async Task ShowProjectTasks(ItemClickEventArgs args)
        {
            SelectedTask = args.ClickedItem as WellDoneTask;
            
            try
            {
                MainViewModel.Tasks = await MainViewModel.wellDoneTable.Where(list => list.IsComplete == false).Where(task => task.ProjectId == SelectedTask.Id).OrderByDescending(order => order.CreatedAt).ToCollectionAsync();
                
                MainViewModel.SelectedList = SelectedTask.Topic;
                IsInsideProjectList = true;
            }
#if WINDOWS_PHONE_APP
            catch (MobileServiceInvalidOperationException ex)
            {
#elif WINDOWS_APP
            catch (HttpRequestException ex)
            {
#endif
                       System.Diagnostics.Debug.WriteLine("Error: There has been some problems with completing project tasks: " + ex);
                       MainViewModel.MobileServiceExceptionNotification();
            }
            
        }


        //to samo jest w mainpageview
        private bool _IsInsideProjectList; //prop do pokazania przycisku edycji projektu na appbarze
        public bool IsInsideProjectList
        {
            get { return _IsInsideProjectList; }
            set { SetProperty(ref _IsInsideProjectList, value); }
        }


        private WellDoneTask _SelectedTask;
        public WellDoneTask SelectedTask
        {
            get { return _SelectedTask; }
            set 
            { 
                SetProperty(ref _SelectedTask, value);
                if (_SelectedTask != null)
                {   
                    //Show datepicker if DueDate is not null
                    if (_SelectedTask.DueDate.HasValue)
                        MainViewModel.IsTaskDueDate = true;
                    else
                        MainViewModel.IsTaskDueDate = false;

                    //Show AppBar when updating task
                    MainViewModel.IsAppBarVisibile = true;

                    
                    //Controlling update or save state on TaskControl
                    if(_SelectedTask.Id == null)
                    {
                        //Show Save Button
                        MainViewModel.ControlHeaderText = "Add new task";                        
                        MainViewModel.IsAddState = true;
                    }
                    else
                    {
                        //Show Update Button
                        if (_SelectedTask.IsProject == true)
                        {
                            MainViewModel.ControlHeaderText = "Update project";
                        }
                        else
                        {
                            MainViewModel.ControlHeaderText = "Update task";
                        }
                        MainViewModel.IsAddState = false;
                    }
                   
                    //Visibility for Combobox of projects at task control
                    if(_SelectedTask.IsProject == true)
                    {
                        MainViewModel.IsProjectComboBoxVisibile = false;
                    }
                    else
                    {
                        MainViewModel.IsProjectComboBoxVisibile = true;
                    }
                    //MainViewModel.IsTaskControlVisibile = true;
                }
                    
                //else
                //{                    
                //    MainViewModel.IsTaskControlVisibile = false;
                    
                //}

            }
        }
        
    }
}
