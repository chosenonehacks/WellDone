using System;
using System.Collections.Generic;
using System.Text;
using WellDone.DataModel;

namespace WellDone.DesignTime
{
    public class TaskDetailViewModel
    {
        public TaskDetailViewModel()
        {
            SelectedTask = new WellDoneTask();
            SelectedTask.Topic = "task detail";
        }

        public WellDoneTask SelectedTask { get; set; }
    }
}
