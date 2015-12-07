using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using WellDone.DataModel;
using WellDone.Messages;
using WellDone.Services;
using Windows.UI.Popups;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using System.Net.Http;

namespace WellDone.ViewModels
{
    public sealed partial class MainPageViewModel : ViewModel, IMainPageViewModel
    {
        
        private IDialogService _dialogService;
        private IEventAggregator _eventAggregator;
        private IMobileServiceClient _mobileServiceClient;        
        public IMobileServiceTable<WellDoneTask> wellDoneTable;
        private IMobileServiceTable<ContextDescription> contextTable;
        private INavigationService _navigationService;
        private IResourceLoader _resourceLoader;

        public MainPageViewModel(IDialogService dialogService, IEventAggregator eventAggregator, IMobileServiceClient mobileServiceClient, INavigationService navigationService, IResourceLoader resourceLoader)
        {
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;
            _mobileServiceClient = mobileServiceClient;
            _navigationService = navigationService;
            _resourceLoader = resourceLoader;

            if (_mobileServiceClient != null)
            {
                wellDoneTable = _mobileServiceClient.GetTable<WellDoneTask>();
                contextTable = _mobileServiceClient.GetTable<ContextDescription>();
            }
        }

        

        public async override void OnNavigatedTo(object navigationParameter, Windows.UI.Xaml.Navigation.NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            LoggedUser = navigationParameter as MobileServiceUser;
            
            //UserIdNavigation = user.UserId;
            //_eventAggregator.GetEvent<Logout>().Subscribe(HandleLouout);            
            //_navigationService.ClearHistory();
            if(_mobileServiceClient.CurrentUser == null) _mobileServiceClient.CurrentUser = LoggedUser;               

            
            IsTaskListVisibile = true;

            if(SelectedList == null)
            SelectedList = "inbox"; 

            IsBoxCountersRefreshing = true;
            await RefreshLists(); 
 
        }

        public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatedFrom(viewModelState, suspending);
        }

        private MobileServiceCollection<ContextDescription,ContextDescription> _contextsCollection;
        public MobileServiceCollection<ContextDescription,ContextDescription> ContextsCollection
        {
            get { return _contextsCollection; }
            set 
            { 
                SetProperty(ref _contextsCollection, value);
                if (_contextsCollection != null)
                    ContextsBoxCount = _contextsCollection.Count();
            }
        }

        private MobileServiceCollection<WellDoneTask,WellDoneTask> _tasks;
        public MobileServiceCollection<WellDoneTask,WellDoneTask> Tasks
        {
            get { return _tasks; }
            set { SetProperty(ref _tasks, value); }
        }

        private MobileServiceCollection<WellDoneTask,WellDoneTask> _ProjectTasksCollection;
        public MobileServiceCollection<WellDoneTask,WellDoneTask> ProjectTasksCollection
        {
            get { return _ProjectTasksCollection; }
            set { SetProperty(ref _ProjectTasksCollection, value); }
        }


        private async Task RefreshLists()
        {            
            try
            {
                await SwitchList(SelectedList);
                ContextsCollection = await contextTable.OrderByDescending(c => c.CreatedAt).ToCollectionAsync();
                ProjectTasksCollection = await wellDoneTable.Where(t => t.IsProject == true && t.IsComplete == false).OrderByDescending(c => c.CreatedAt).ToCollectionAsync();                
                await GetTaskListCounters();
            }
#if WINDOWS_PHONE_APP
            catch (MobileServiceInvalidOperationException ex)
            {
#elif WINDOWS_APP
            catch (HttpRequestException ex)
            {
#endif
            
                MobileServiceExceptionNotification();
            }
                   
        }

        public void MobileServiceExceptionNotification()
        {
            string content = _resourceLoader.GetString("MobileServiceException-Content");
            string title = _resourceLoader.GetString("MobileServiceException-Title");

            //string exitCommand = _resourceLoader.GetString("MobileServiceException-ExitCommand");
            //var exit = new UICommand(exitCommand, e => { App.Current.Exit(); });

            _dialogService.Show(content, title);

            IsBoxCountersRefreshing = false;
        }

        //private void HandleLouout(string value)
        //{
        //    _dialogService.Show(value);
        //}
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
            
            SelectedTask = cb.DataContext as WellDoneTask;
            await CompleteTask();
        }

        DelegateCommand<string> _NavigateCommand;
        public DelegateCommand<string> NavigateCommand
        {
            get
            {
                if (_NavigateCommand != null)
                    return _NavigateCommand;
                _NavigateCommand = new DelegateCommand<string>
                    (
                        (args) => { Navigate(args); },
                        (args) => { return true; }
                    );
                this.PropertyChanged += (s, e) => _NavigateCommand.RaiseCanExecuteChanged();
                return _NavigateCommand;
            }
        }

        private void Navigate(string parameter)
        {
            switch (parameter)
            {
                case "about":
#if WINDOWS_PHONE_APP
                    _navigationService.Navigate(Experiences.About);
#elif WINDOWS_APP
                    string content = _resourceLoader.GetString("AboutTextBlockDialogContent");
                    string title = _resourceLoader.GetString("AboutTitle");     
                    _dialogService.Show(content, title);
#endif
                    break;
                case "main":
                    _navigationService.Navigate(Experiences.Main, LoggedUser);
                    
                    break;
                default:
                    _navigationService.Navigate(Experiences.Main, LoggedUser);
                    
                    break;
            }
            
        }

        DelegateCommand<string> _ShowListDetailCommand;
        public DelegateCommand<string> ShowListDetailCommand
        {
            get
            {
                if (_ShowListDetailCommand != null)
                    return _ShowListDetailCommand;
                _ShowListDetailCommand = new DelegateCommand<string>
                    (
                        async (args) => { await SwitchList(args); },
                        (args) => { return true; }
                    );
                this.PropertyChanged += (s, e) => _ShowListDetailCommand.RaiseCanExecuteChanged();
                return _ShowListDetailCommand;
            }
        }

        DelegateCommand<string> _AddNewTaskCommand;
        public DelegateCommand<string> AddNewTaskCommand
        {
            get
            {
                if (_AddNewTaskCommand != null)
                    return _AddNewTaskCommand;
                _AddNewTaskCommand = new DelegateCommand<string>
                    (
                        (args) => { AddNewTask(args); },
                        (args) => { return true; }
                    );
                this.PropertyChanged += (s, e) => _AddNewTaskCommand.RaiseCanExecuteChanged();
                return _AddNewTaskCommand;
            }
        }

        DelegateCommand<string> _AddNewContextCommand;
        public DelegateCommand<string> AddNewContextCommand
        {
            get
            {
                if (_AddNewContextCommand != null)
                    return _AddNewContextCommand;
                _AddNewContextCommand = new DelegateCommand<string>
                    (
                        (args) => { AddNewContext(args); },
                        (args) => { return true; }
                    );
                this.PropertyChanged += (s, e) => _AddNewContextCommand.RaiseCanExecuteChanged();
                return _AddNewContextCommand;
            }
        }        

        DelegateCommand<string> _SaveNewTaskCommand;
        public DelegateCommand<string> SaveNewTaskCommand
        {
            get
            {
                if (_SaveNewTaskCommand != null)
                    return _SaveNewTaskCommand;
                _SaveNewTaskCommand = new DelegateCommand<string>
                    (
                        async (args) => { await SaveNewTask(args); },
                        (args) => { return true; }
                    );
                this.PropertyChanged += (s, e) => _SaveNewTaskCommand.RaiseCanExecuteChanged();
                return _SaveNewTaskCommand;
            }
        }

        private async Task SaveNewTask(string args)
        {
            try
                {
                    if (IsTaskDueDate == false)
                        SelectedTask.DueDate = null;

                    await wellDoneTable.InsertAsync(SelectedTask);
                    IsTaskControlVisibile = false;
                    
                    if (SelectedTask.IsProject == true)
                    {
                        var content = _resourceLoader.GetString("MobileServiceAddProjectOperation-Content");
                        var title = _resourceLoader.GetString("MobileServiceAddProjectOperation-Title");
                       _dialogService.Show(content, title);

                        SelectedList = "projects";

                    }
                    else if(SelectedTask.ProjectId == null && SelectedTask.DueDate != null && SelectedTask.DueDate == DateTime.Today.Date)
                    {
                        var content = _resourceLoader.GetString("MobileServiceAddTaskOperation-Content");
                        var title = _resourceLoader.GetString("MobileServiceAddTaskOperation-Title");
                       _dialogService.Show(content, title);                       

                        SelectedList = "today";
                    }
                    else if(SelectedTask.ProjectId != null) // for project name list
                    {
                        var content = _resourceLoader.GetString("MobileServiceAddTaskOperation-Content");
                        var title = _resourceLoader.GetString("MobileServiceAddTaskOperation-Title");
                       _dialogService.Show(content, title);

                        SelectedList = SelectedTask.ProjectId;
                    }
                    else
                    {
                        var content = _resourceLoader.GetString("MobileServiceAddTaskOperation-Content");
                        var title = _resourceLoader.GetString("MobileServiceAddTaskOperation-Title");
                        _dialogService.Show(content, title);

                        SelectedList = "inbox";
                    }
                    RefreshLists();
                    
                }
#if WINDOWS_PHONE_APP
            catch (MobileServiceInvalidOperationException ex)
            {
#elif WINDOWS_APP
            catch (HttpRequestException ex)
            {
#endif
                    System.Diagnostics.Debug.WriteLine("Error: There has been some problems with updating data: " + ex);
                    MobileServiceExceptionNotification();
            }

        }

        DelegateCommand<string> _SaveNewContextCommand;
        public DelegateCommand<string> SaveNewContextCommand
        {
            get
            {
                if (_SaveNewContextCommand != null)
                    return _SaveNewContextCommand;
                _SaveNewContextCommand = new DelegateCommand<string>
                    (
                        async (args) => { await SaveNewContext(args); },
                        (args) => { return true; }
                    );
                this.PropertyChanged += (s, e) => _SaveNewContextCommand.RaiseCanExecuteChanged();
                return _SaveNewContextCommand;
            }
        }

        private async Task SaveNewContext(string args)
        {
            try
                {
                    await contextTable.InsertAsync(SelectedContext);
                    IsContextControlVisibile = false;
                    
                    SelectedContext = null;
                    SelectedList = "contexts";
                    RefreshLists();

                        var content = _resourceLoader.GetString("MobileServiceAddContextOperation-Content");
                        var title = _resourceLoader.GetString("MobileServiceAddContextOperation-Title");
                       _dialogService.Show(content, title);  
                    
                }
#if WINDOWS_PHONE_APP
            catch (MobileServiceInvalidOperationException ex)
            {
#elif WINDOWS_APP
            catch (HttpRequestException ex)
            {
#endif
                    System.Diagnostics.Debug.WriteLine("Error: There has been some problems with adding data: " + ex);
                    MobileServiceExceptionNotification();
            }
            
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
            if (args != null)
            {
                SelectedTask = args.ClickedItem as WellDoneTask;
                if (SelectedTask == null || SelectedTask.Id == null || SelectedTask.IsProject == false)
                    return false;
                else
                    return true;
            }
            return false;
        }

        private async Task ShowProjectTasks(ItemClickEventArgs args)
        {
            SelectedTask = args.ClickedItem as WellDoneTask;
            SelectedList = SelectedTask.Topic;
            Tasks = await wellDoneTable.Where(list => list.IsComplete == false).Where(task => task.ProjectId == SelectedTask.Id).OrderByDescending(order => order.CreatedAt).ToCollectionAsync();
            IsInsideProjectList = true;
        }

        private bool _IsInsideProjectList; //prop do pokazania przycisku edycji projektu na appbarze
        public bool IsInsideProjectList
        {
            get { return _IsInsideProjectList; }
            set { SetProperty(ref _IsInsideProjectList, value); }
        }

        DelegateCommand _EditProjectCommand;
        public DelegateCommand EditProjectCommand
        {
            get
            {
                if (_EditProjectCommand != null)
                    return _EditProjectCommand;
                _EditProjectCommand = new DelegateCommand
                    (
                       async () => { await EditProject(); },
                        () => { return CanEditProject(); }
                    );
                this.PropertyChanged += (s, e) => _EditProjectCommand.RaiseCanExecuteChanged();
                return _EditProjectCommand;
            }
        }
 
        private bool CanEditProject()
        {
            if (IsInsideProjectList == true)
                return true;
            else
                return false;

        }

        private async Task EditProject()
        {
            try
            {
                var selectedProject = await wellDoneTable.Where(project => project.Topic == SelectedList && project.IsProject == true).ToCollectionAsync();
                IsInProjectEditMode = true;
                SelectedTask = selectedProject.FirstOrDefault();     
            }
#if WINDOWS_PHONE_APP
            catch (MobileServiceInvalidOperationException ex)
            {
#elif WINDOWS_APP
            catch (HttpRequestException ex)
            {
#endif
                       System.Diagnostics.Debug.WriteLine("Error: There has been some problems selecting project to edit: " + ex);
                       MobileServiceExceptionNotification();
            }              
            
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
            if (args != null)
            {
                SelectedContext = args.ClickedItem as ContextDescription;     
                if (SelectedList == "contexts" && SelectedContext != null)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }


        private async Task ShowContextTasks(ItemClickEventArgs args)
        {
            SelectedContext = args.ClickedItem as ContextDescription;

            SelectedList = SelectedContext.Name;
            IsInsideContextList = true;
            Tasks = await wellDoneTable.Where(list => list.IsComplete == false).Where(task => task.ContextId == SelectedContext.Id).OrderByDescending(order => order.CreatedAt).ToCollectionAsync();
            IsTaskListVisibile = true;
            //SelectedContext = null;
        }

        private ContextDescription _SelectedContext;
        public ContextDescription SelectedContext
        {
            get { return _SelectedContext; }
            set 
            { 
                SetProperty(ref _SelectedContext, value);
                IsAppBarVisibile = true;
                if (SelectedContext != null)
                {
                    IsContextButtonVisibile = true;
                }
                else
                {
                    IsContextButtonVisibile = false;
                }
            }
        }

        private bool _IsContextButtonVisibile;
        public bool IsContextButtonVisibile
        {
            get { return _IsContextButtonVisibile; }
            set { SetProperty(ref _IsContextButtonVisibile, value); }
        }

        private bool _IsListLoading;
        public bool IsListLoading
        {
            get { return _IsListLoading; }
            set { SetProperty(ref _IsListLoading, value); }
        }



        DelegateCommand _UpdateCommand;
        public DelegateCommand UpdateCommand
        {
            get
            {
                if (_UpdateCommand != null)
                    return _UpdateCommand;
                _UpdateCommand = new DelegateCommand
                    (
                      async  () => { await Update(); },
                        () => { return true; }
                    );
                this.PropertyChanged += (s, e) => _UpdateCommand.RaiseCanExecuteChanged();
                return _UpdateCommand;
            }
        }

        DelegateCommand _CompleteTaskCommand;
        public DelegateCommand CompleteTaskCommand
        {
            get
            {
                if (_CompleteTaskCommand != null)
                    return _CompleteTaskCommand;
                _CompleteTaskCommand = new DelegateCommand
                    (
                      async  () => { await CompleteTask(); },
                        () => { return CanCompleteTask(); }
                    );
                this.PropertyChanged += (s, e) => _CompleteTaskCommand.RaiseCanExecuteChanged();
                return _CompleteTaskCommand;
            }
        }

        private bool CanCompleteTask()
        {
            if (SelectedTask == null || SelectedTask.Id == null)
                return false;
            else
                return true;
        }

        private async Task CompleteTask()
        {
            //SelectedTask.IsComplete = true;
            
            //Complete sub tasks if this task was project
            if(SelectedTask.IsProject == true)
            {
               var taskOfProjectList = await wellDoneTable.Where(task => task.ProjectId == SelectedTask.Id).ToCollectionAsync();
               foreach (var task in taskOfProjectList)
               {
                   task.IsComplete = true;
                        try
                        {
                            await wellDoneTable.UpdateAsync(task);

                        }
            #if WINDOWS_PHONE_APP
                        catch (MobileServiceInvalidOperationException ex)
                        {
            #elif WINDOWS_APP
                        catch (HttpRequestException ex)
                        {
            #endif
                                    System.Diagnostics.Debug.WriteLine("Error: There has been some problems with completing project tasks: " + ex);
                                    MobileServiceExceptionNotification();
                        }
                   
               }
            }

            try
            {
                await wellDoneTable.UpdateAsync(SelectedTask);
                if(SelectedTask.IsProject == true)
                {
                        var content = _resourceLoader.GetString("MobileServiceCompleteProjectOperation-Content");
                        var title = _resourceLoader.GetString("MobileServiceCompleteProjectOperation-Title");
                       _dialogService.Show(content, title);

                }
                else
                {
                        var content = _resourceLoader.GetString("MobileServiceCompleteTaskOperation-Content");
                        var title = _resourceLoader.GetString("MobileServiceCompleteTaskOperation-Title");
                       _dialogService.Show(content, title);

                }
                
                RefreshLists();
            }
#if WINDOWS_PHONE_APP
            catch (MobileServiceInvalidOperationException ex)
            {
#elif WINDOWS_APP
            catch (HttpRequestException ex)
            {
#endif
                System.Diagnostics.Debug.WriteLine("Error: There has been some problems with completing.: " + ex);
                MobileServiceExceptionNotification();
            }
            
            
        }
        

        DelegateCommand _DeleteCommand;
        public DelegateCommand DeleteCommand
        {
            get
            {
                if (_DeleteCommand != null)
                    return _DeleteCommand;
                _DeleteCommand = new DelegateCommand
                    (
                      async  () => { await Delete(); },
                        () => { return CanDelete(); }
                    );
                this.PropertyChanged += (s, e) => _DeleteCommand.RaiseCanExecuteChanged();
                return _DeleteCommand;
            }
        }

        private bool CanDelete()
        {
            if (SelectedTask == null || SelectedTask.Id == null)
                return false;
            else
                return true;
        }

        private async Task Delete()
        {
            //Delete sub tasks if this task was project
            if(SelectedTask.IsProject == true)
            {
               var taskOfProjectList = await wellDoneTable.Where(task => task.ProjectId == SelectedTask.Id).ToCollectionAsync();
               foreach (var task in taskOfProjectList)
               {
                   task.IsComplete = true;
                   try
                   {
                       await wellDoneTable.DeleteAsync(task);

                   }
#if WINDOWS_PHONE_APP
            catch (MobileServiceInvalidOperationException ex)
            {
#elif WINDOWS_APP
            catch (HttpRequestException ex)
            {
#endif
                       System.Diagnostics.Debug.WriteLine("Error: There has been some problems with deleting project tasks: " + ex);
                       MobileServiceExceptionNotification();
                   }
                   
               }
            }

            try
                {      
                    await wellDoneTable.DeleteAsync(SelectedTask);
                    
                    IsTaskControlVisibile = false;

                    if(SelectedTask.IsProject == true)
                    {
                        var content = _resourceLoader.GetString("MobileServiceDeleteProjectOperation-Content");
                        var title = _resourceLoader.GetString("MobileServiceDeleteProjectOperation-Title");
                       _dialogService.Show(content, title);
                    }
                    else
                    {
                        var content = _resourceLoader.GetString("MobileServiceDeleteTaskOperation-Content");
                        var title = _resourceLoader.GetString("MobileServiceDeleteTaskOperation-Title");
                       _dialogService.Show(content, title);
                    }
                    RefreshLists();
                }
#if WINDOWS_PHONE_APP
            catch (MobileServiceInvalidOperationException ex)
            {
#elif WINDOWS_APP
            catch (HttpRequestException ex)
            {
#endif
                    System.Diagnostics.Debug.WriteLine("Error: There has been some problems with updating data: " + ex);
                    MobileServiceExceptionNotification();
            }
            
        }

        DelegateCommand _DeleteContextCommand;
        public DelegateCommand DeleteContextCommand
        {
            get
            {
                if (_DeleteContextCommand != null)
                    return _DeleteContextCommand;
                _DeleteContextCommand = new DelegateCommand
                    (
                      async  () => { await DeleteContext(); },
                        () => { return CanDeleteContext(); }
                    );
                this.PropertyChanged += (s, e) => _DeleteContextCommand.RaiseCanExecuteChanged();
                return _DeleteContextCommand;
            }
        }

        private async Task DeleteContext()
        {
             try
                {      
                    await contextTable.DeleteAsync(SelectedContext);
                    
                    IsContextControlVisibile = false;                    

                    SelectedList = "contexts";
                    RefreshLists();

                    var content = _resourceLoader.GetString("MobileServiceDeleteContextOperation-Content");
                    var title = _resourceLoader.GetString("MobileServiceDeleteContextOperation-Title");
                    _dialogService.Show(content, title);
                }
#if WINDOWS_PHONE_APP
            catch (MobileServiceInvalidOperationException ex)
            {
#elif WINDOWS_APP
            catch (HttpRequestException ex)
            {
#endif
                    System.Diagnostics.Debug.WriteLine("Error: There has been some problems with deleting data: " + ex);
                    MobileServiceExceptionNotification();
            }
            
        }

        private bool CanDeleteContext()
        {
            if (SelectedContext == null || SelectedContext.Id == null)
                return false;
            else
                return true;
        }

        private async Task Update()
        {
            
            //Update
            if (SelectedTask!= null)
            {
                try
                {
                    if (IsTaskDueDate == false && SelectedTask.DueDate.HasValue)
                    {
                        SelectedTask.DueDate = null;
                    }
                    await wellDoneTable.UpdateAsync(SelectedTask);
#if WINDOWS_APP
                    IsTaskControlVisibile = false;
#endif
                    if (SelectedTask.IsProject == true)
                    {
                        var content = _resourceLoader.GetString("MobileServiceUpdateProjectOperation-Content");
                        var title = _resourceLoader.GetString("MobileServiceUpdateProjectOperation-Title");
                        _dialogService.Show(content, title);

                        SelectedList = "projects";

                    }
                    else if(SelectedTask.ProjectId == null && SelectedTask.DueDate != null && SelectedTask.DueDate == DateTime.Today.Date)
                    {
                        var content = _resourceLoader.GetString("MobileServiceUpdateTaskOperation-Content");
                        var title = _resourceLoader.GetString("MobileServiceUpdateTaskOperation-Title");
                        _dialogService.Show(content, title);

                        SelectedList = "today";
                    }
                    else if(SelectedTask.ProjectId != null) // for project name list
                    {
                        var content = _resourceLoader.GetString("MobileServiceUpdateTaskOperation-Content");
                        var title = _resourceLoader.GetString("MobileServiceUpdateTaskOperation-Title");
                        _dialogService.Show(content, title);

                        SelectedList = SelectedTask.ProjectId;
                    }
                    else
                    {
                        var content = _resourceLoader.GetString("MobileServiceUpdateTaskOperation-Content");
                        var title = _resourceLoader.GetString("MobileServiceUpdateTaskOperation-Title");
                        _dialogService.Show(content, title);

                        SelectedList = "inbox";
                    }
                    RefreshLists();
                }
#if WINDOWS_PHONE_APP
                catch (MobileServiceInvalidOperationException ex)
                {
#elif WINDOWS_APP
                catch (HttpRequestException ex)
                {
#endif
                    System.Diagnostics.Debug.WriteLine("Error: There has been some problems with updating data: " + ex);
                    MobileServiceExceptionNotification();
                }
            }
            
        }

        private bool _IsProjectComboBoxVisibile;
        public bool IsProjectComboBoxVisibile
        {
            get { return _IsProjectComboBoxVisibile; }
            set 
            { 
                SetProperty(ref _IsProjectComboBoxVisibile, value);
                this.OnPropertyChanged(() => SelectedTask);
            }
        }


        private void AddNewTask(string parameter)
        {
            IsContextControlVisibile = false;

                IsTaskDueDate = false;
                WellDoneTask newTask = new WellDoneTask();                
                newTask.Topic = _resourceLoader.GetString("NewTaskTopic");
                newTask.IsProject = false;
                newTask.DueDate = DateTime.Now.Date;                    
                newTask.IsComplete = false;

                SelectedTask = newTask; 
#if WINDOWS_PHONE_APP
                _navigationService.Navigate(Experiences.TaskDetail, SelectedTask);
#endif

        }

        private void AddNewContext(string args)
        {
            IsTaskControlVisibile = false;

            ContextDescription newContext = new ContextDescription();
            newContext.Name = _resourceLoader.GetString("NewContextName"); 

            SelectedContext = newContext;
            IsContextControlVisibile = true;
            ControlHeaderText = _resourceLoader.GetString("NewContextControlHeaderText");
#if WINDOWS_PHONE_APP
                _navigationService.Navigate(Experiences.ContextDetail, SelectedContext);
#endif
        }

        private bool _IsContextControlVisibile;
        public bool IsContextControlVisibile
        {
            get { return _IsContextControlVisibile; }
            set { SetProperty(ref _IsContextControlVisibile, value); }
        }


        private bool _IsTaskDueDate;
        public bool IsTaskDueDate
        {
            get { return _IsTaskDueDate; }
            set 
            {                 
                SetProperty(ref _IsTaskDueDate, value);
                if(_IsTaskDueDate == true)
                {
                    if (SelectedTask != null && SelectedTask.DueDate == null)
                        SelectedTask.DueDate = DateTime.Now.Date;                    
                }
            }
        }



        private bool _IsAddState;
        public bool IsAddState
        {
            get { return _IsAddState; }
            set { SetProperty(ref _IsAddState, value); }
        }

        private bool _IsAppBarVisibile;
        public bool IsAppBarVisibile
        {
            get { return _IsAppBarVisibile; }
            set { SetProperty(ref _IsAppBarVisibile, value); }
        }



        private string _ControlHeaderText;
        public string ControlHeaderText
        {
            get { return _ControlHeaderText; }
            set { SetProperty(ref _ControlHeaderText, value); }
        }

 
        private async Task SwitchList(string parameter)
        {
            IsListLoading = true;
            IsInsideProjectList = false;
            try
            {

                Tasks = null;
                SelectedList = null;
                
                IsContextControlVisibile = false;
                IsTaskControlVisibile = false;

                if (parameter != "contexts")
                {
                    IsTaskListVisibile = true;
                    SelectedContext = null;
                    switch (parameter)
                    {
                        case "inbox":
#if WINDOWS_PHONE_APP
                            _navigationService.Navigate(Experiences.List, this);
#endif
                            Tasks = await wellDoneTable.Where(list => list.IsProject == false && list.IsComplete == false)
                                                        .Where(d => d.DueDate != DateTime.Now.Date).OrderByDescending(t => t.CreatedAt).ToCollectionAsync();
                            IsTaskControlVisibile = false;
                            SelectedList = parameter;
                            break;
                        case "today":
#if WINDOWS_PHONE_APP
                            _navigationService.Navigate(Experiences.List, this);
#endif
                            Tasks = await wellDoneTable.Where(t => t.DueDate == DateTime.Now.Date && t.IsComplete == false).OrderByDescending(t => t.CreatedAt).ToCollectionAsync();
                            IsTaskControlVisibile = false;
                            SelectedList = parameter;
                            break;
                        case "projects":
#if WINDOWS_PHONE_APP
                            _navigationService.Navigate(Experiences.List, this);
#endif
                            Tasks = await wellDoneTable.Where(list => list.IsProject == true && list.IsComplete == false).OrderByDescending(t => t.CreatedAt).ToCollectionAsync();
                            IsTaskControlVisibile = false;
                            SelectedList = parameter;
                            break;
                        default:
                            
#if WINDOWS_PHONE_APP
                            _navigationService.Navigate(Experiences.List, this);
#endif

                            await GetProjectNameForSelectedList(parameter);
                            
                            break;
                    }
                }
                else
                {
#if WINDOWS_PHONE_APP
                    _navigationService.Navigate(Experiences.List, this);
#endif
                    SelectedList = "contexts";
                    SelectedTask = null;
                    IsTaskListVisibile = false;
                    IsTaskControlVisibile = false;
                    IsInsideContextList = true;
                }
                IsListLoading = false;
                
            }
#if WINDOWS_PHONE_APP
            catch (MobileServiceInvalidOperationException ex)
            {
#elif WINDOWS_APP
            catch (HttpRequestException ex)
            {
#endif
                System.Diagnostics.Debug.WriteLine("Error: There has been some problems with switch lists: " + ex);
                _navigationService.Navigate(Experiences.Main, this);
                //MobileServiceExceptionNotification();
            }
        }

        private async Task GetProjectNameForSelectedList(string parameter)
        {
            if (parameter != null)
            {
                var projectName = await wellDoneTable.Where(project => project.Id == parameter).ToCollectionAsync();
                if (projectName.Any())
                {
                    SelectedList = projectName.FirstOrDefault().Topic;

                    Tasks =
                        await
                            wellDoneTable.Where(
                                list => list.ProjectId == projectName.FirstOrDefault().Id && list.IsComplete == false)
                                .OrderByDescending(t => t.CreatedAt)
                                .ToCollectionAsync();
                }
                IsTaskControlVisibile = false;
            }
        }

        private bool _IsInsideContextList;
        public bool IsInsideContextList
        {
            get { return _IsInsideContextList; }
            set { SetProperty(ref _IsInsideContextList, value); }
        }

        private bool _IsTaskListVisibile;
        public bool IsTaskListVisibile
        {
            get { return _IsTaskListVisibile; }
            set { SetProperty(ref _IsTaskListVisibile, value); }
        }

        private int _InBoxCount;
        public int InBoxCount
        {
            get { return _InBoxCount; }
            set { SetProperty(ref _InBoxCount, value); }
        }

        private int _TodayBoxCount;
        public int TodayBoxCount
        {
            get { return _TodayBoxCount; }
            set { SetProperty(ref _TodayBoxCount, value); }
        }

        private int _ProjectBoxCount;
        public int ProjectBoxCount
        {
            get { return _ProjectBoxCount; }
            set { SetProperty(ref _ProjectBoxCount, value); }
        }

        private int _ContextsBoxCount;
        public int ContextsBoxCount
        {
            get { return _ContextsBoxCount; }
            set { SetProperty(ref _ContextsBoxCount, value); }
        }



        private async Task GetTaskListCounters()
        {
            var InboxCollection = await wellDoneTable.Where(list => list.IsProject == false && list.IsComplete == false).Where(d => d.DueDate != DateTime.Now.Date).ToCollectionAsync();
            InBoxCount = InboxCollection.Count();
            var TodayCollection = await wellDoneTable.Where(t => t.DueDate == DateTime.Now.Date && t.IsComplete == false).ToCollectionAsync();
            TodayBoxCount = TodayCollection.Count();
            var ProjectsCollection = await wellDoneTable.Where(list => list.IsProject == true && list.IsComplete == false).ToCollectionAsync();
            ProjectBoxCount = ProjectsCollection.Count();
            IsBoxCountersRefreshing = false;
        }

        private bool _IsBoxCountersRefreshing;
        public bool IsBoxCountersRefreshing
        {
            get { return _IsBoxCountersRefreshing; }
            set { SetProperty(ref _IsBoxCountersRefreshing, value); }
        }

        

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set { SetProperty(ref _Title, value); }
        }

        private string _SelectedList;
        public string SelectedList
        {
            get { return _SelectedList; }
            set 
            { 
                SetProperty(ref _SelectedList, value);
                //OnPropertyChanged(() => PhoneShowContextTasks);
            }
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
                        IsTaskDueDate = true;
                    else
                        IsTaskDueDate = false;

                    //Show AppBar when updating task
                    IsAppBarVisibile = true;

                    
                    //Controling update or save state on TaskControl
                    if(_SelectedTask.Id == null)
                    {
                        //Show Save Button
                        ControlHeaderText = _resourceLoader.GetString("NewTaskControlHeaderText"); 
                        IsAddState = true;
                    }
                    else
                    {
                        //Show Update Button
                        if (_SelectedTask.IsProject == true)
                        {
                            ControlHeaderText = _resourceLoader.GetString("UpdateProjectControlHeaderText");
                        }
                        else
                        {
                            ControlHeaderText = _resourceLoader.GetString("UpdateTaskControlHeaderText");
                        }
                        //IsAddState = false;
                    }
                   
                    //Visibility for Combobox of projects at task control
                    if(_SelectedTask.IsProject == true)
                    {
                        IsProjectComboBoxVisibile = false;                        
                    }
                    else
                    {
                        IsProjectComboBoxVisibile = true;                        
                    }
#if WINDOWS_APP
                    if (IsInProjectEditMode)
                    {
                        IsTaskControlVisibile = true;
                        IsInProjectEditMode = false;
                    }

                    if (_SelectedTask.IsProject != true)
                    {
                        IsTaskControlVisibile = true;
                    }
#endif
                }
                    
                else
                {  
#if WINDOWS_APP
                    IsTaskControlVisibile = false;
#endif
                }

            }
        }

        private bool _IsInProjectEditMode;
        public bool IsInProjectEditMode
        {
            get { return _IsInProjectEditMode; }
            set 
            { 
                SetProperty(ref _IsInProjectEditMode, value);                
            }
        }

        private bool _IsTaskControlVisibile;
        public bool IsTaskControlVisibile
        {
            get { return _IsTaskControlVisibile; }
            set 
            { 
                SetProperty(ref _IsTaskControlVisibile, value);                
            }
        }


        private int _Total;
        public int Total
        {
            get { return _Total; }
            set { SetProperty(ref _Total, value); }
        }

        private MobileServiceUser _LoggedUser;
        public MobileServiceUser LoggedUser
        {
            get { return _LoggedUser; }
            set { SetProperty(ref _LoggedUser, value); }
        }
        


    }
}
