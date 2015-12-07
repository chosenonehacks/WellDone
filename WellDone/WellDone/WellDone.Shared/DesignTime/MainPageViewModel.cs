using Microsoft.Practices.Prism.Commands;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Text;
using WellDone.DataModel;
using WellDone.ViewModels;

namespace WellDone.DesignTime
{
    public class MainPageViewModel : IMainPageViewModel
    {
        public string Title { get; set; }

        public string Login { get; set; }
        public string Password { get; set; }

        public int Total { get; set; }

        public MobileServiceUser LoggedUser { get; set; }

        public List<WellDoneTask> Tasks { get; set; }

        public string SelectedList { get; set; }

        public bool IsTaskSelected { get; set; }

        public int InBoxCount { get; set; }
        public int TodayBoxCount { get; set; }
        public int ProjectBoxCount { get; set; }
        public int ContextsBoxCount { get; set; }

        public bool IsTaskListVisibile { get; set; }

        public string ControlHeaderText { get; set; }

        public bool IsTaskDueDate { get; set; }

        public MobileServiceCollection<ContextDescription, ContextDescription> ContextsCollection { get; set; }
        public MobileServiceCollection<WellDoneTask, WellDoneTask> ProjectTasksCollection { get; set; }
        public DelegateCommand<string> SaveNewTaskCommand { get; private set;}
        public DelegateCommand<string> SaveNewContextCommand { get; private set; }
        public DelegateCommand UpdateCommand { get; private set;}
        public WellDoneTask SelectedTask { get; set; }

        public bool IsAddState { get; set; }

        public MainPageViewModel()
        {
            Title = "WellDone";
            Login = "Design Login";
            Password = "Design Password";
            Total = 0;

            Tasks = new List<WellDoneTask>();
            
            Tasks.Add( new WellDoneTask { Id = Guid.NewGuid().ToString(), Topic = "First taak", IsComplete = false, IsProject = false, DueDate = new DateTime(2015,03,30),ProjectId = null, ContextId = "someContext1", UserId = "DesignUserId"});
            Tasks.Add( new WellDoneTask { Id = Guid.NewGuid().ToString(), Topic = "Second task", IsComplete = false, IsProject = false, ProjectId = null, ContextId = "someContext2", UserId = "DesignUserId" });

            var task = new WellDoneTask();
            var testbind = task.Topic;

            LoggedUser = new MobileServiceUser("DesignUserId");

            SelectedList = "inbox";

            IsTaskSelected = true;

            InBoxCount = 15;
            TodayBoxCount = 3;
            ProjectBoxCount = 4;
            ContextsBoxCount = 2;

            IsTaskListVisibile = true;

            ControlHeaderText = "task control header";
        }
    }
}
