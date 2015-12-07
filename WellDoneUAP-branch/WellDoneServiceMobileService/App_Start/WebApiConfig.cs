using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Web.Http;
using System.Linq;
using WellDoneMobileService.DataObjects;
using WellDoneMobileService.Models;
using Microsoft.WindowsAzure.Mobile.Service;
using System.Data.Entity.Migrations;
using WellDoneMobileService.Migrations;

namespace WellDoneMobileService
{
    public static class WebApiConfig
    {
        public static void Register()
        {
            // Use this class to set configuration options for your mobile service
            ConfigOptions options = new ConfigOptions();

            // Use this class to set WebAPI configuration options
            HttpConfiguration config = ServiceConfig.Initialize(new ConfigBuilder(options));

            //config.SetIsHosted(true);//This tells the local mobile service project to run as if it is being hosted in Azure, including honoring the AuthorizeLevel settings. Without this setting, all HTTP requests to localhost are permitted without authentication despite the AuthorizeLevel setting.

            // To display errors in the browser during development, uncomment the following
            // line. Comment it out again when you deploy your service for production use.
            // config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            //Database.SetInitializer(new MobileServiceInitializer());
            Database.SetInitializer<MobileServiceContext>(null);

            var migrator = new DbMigrator(new Configuration());
            migrator.Update();
        }
    }

    public class MobileServiceInitializer : DropCreateDatabaseIfModelChanges<MobileServiceContext>
    {
        protected override void Seed(MobileServiceContext mobileServiceContext)
        {
            
            
            var context1 = new ContextDescription { Id = Guid.NewGuid().ToString(), Name = "@Home", UserId = null};               
            var context2 = new ContextDescription { Id = Guid.NewGuid().ToString(), Name = "@Work", UserId = null};
            
            List<ContextDescription> contextes = new List<ContextDescription>();

            contextes.Add(context1);
            contextes.Add(context2);

            foreach (ContextDescription contextItem in contextes)
            {
                mobileServiceContext.Set<ContextDescription>().Add(contextItem);
            }

            List<WellDoneTask> tasks = new List<WellDoneTask>
            {
                new WellDoneTask { Id = Guid.NewGuid().ToString(), Topic = "First Project", IsComplete = false, IsProject = true, DueDate = new DateTime(2015,03,30),ProjectId = null, ContextId = context1.Id, UserId = null},                
                new WellDoneTask { Id = Guid.NewGuid().ToString(), Topic = "Second task", IsComplete = false, IsProject = false, ProjectId = null, ContextId = context2.Id, UserId = null  },
            };

            var taskWithProject = new WellDoneTask { Id = Guid.NewGuid().ToString(), Topic = "First task", IsComplete = false, IsProject = false, ProjectId = tasks.FirstOrDefault().Id, ContextId = context1.Id, UserId = null };

            tasks.Add(taskWithProject);

            foreach (WellDoneTask task in tasks)
            {
                mobileServiceContext.Set<WellDoneTask>().Add(task);
            }

            base.Seed(mobileServiceContext);
        }
    }
}

