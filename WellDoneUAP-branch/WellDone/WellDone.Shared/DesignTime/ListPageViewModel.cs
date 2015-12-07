using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Text;
using WellDone.DataModel;

namespace WellDone.DesignTime
{
    public class ListPageViewModel
    {
        public string SelectedList { get; set; }

        public MainPageViewModel MainViewModel { get; set; }
        public ListPageViewModel()
        {
            SelectedList = "inbox";         

            MainViewModel = new MainPageViewModel
            {
                Title = "WellDone",
                Login = "Design Login",
                Password = "Design Password",
                Total = 0,


                LoggedUser = new MobileServiceUser("DesignUserId"),

                SelectedList = "inbox",

                IsTaskSelected = true,
                IsTaskListVisibile = true,

                Tasks = new List<WellDoneTask>() 
                { 
                    new WellDoneTask { Id = Guid.NewGuid().ToString(), Topic = "First taak", IsComplete = false, IsProject = false, DueDate = new DateTime(2015,03,30),ProjectId = null, ContextId = "someContext1", UserId = "DesignUserId"},
                    new WellDoneTask { Id = Guid.NewGuid().ToString(), Topic = "Second task", IsComplete = false, IsProject = false, ProjectId = null, ContextId = "someContext2", UserId = "DesignUserId" }
                }

            };

            
        }
    }
}
