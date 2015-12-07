using Microsoft.Practices.Prism.Commands;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Text;
using WellDone.DataModel;

namespace WellDone.ViewModels
{
    public interface IMainPageViewModel
    {
        string Title { get; set; }
        string ControlHeaderText { get; set; }

        bool IsTaskDueDate { get; set; }

        MobileServiceCollection<ContextDescription, ContextDescription> ContextsCollection { get; set; }

        MobileServiceCollection<WellDoneTask, WellDoneTask> ProjectTasksCollection { get; set; }

        bool IsAddState { get; set; }

        DelegateCommand<string> SaveNewTaskCommand { get; }

        DelegateCommand<string> SaveNewContextCommand { get; }

        DelegateCommand UpdateCommand { get;}

        WellDoneTask SelectedTask { get; set; }
        
    }
}
