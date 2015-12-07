using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using WellDoneMobileService.DataObjects;
using WellDoneMobileService.Models;
using Microsoft.WindowsAzure.Mobile.Service.Security;

namespace WellDoneMobileService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.User)]
    public class WellDoneTaskController : TableController<WellDoneTask>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<WellDoneTask>(context, Request, Services);
        }

        // GET tables/WellDoneTask
        public IQueryable<WellDoneTask> GetAllWellDoneTask()
        {
            return Query(); 
        }

        // GET tables/WellDoneTask/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<WellDoneTask> GetWellDoneTask(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/WellDoneTask/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<WellDoneTask> PatchWellDoneTask(string id, Delta<WellDoneTask> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/WellDoneTask
        public async Task<IHttpActionResult> PostWellDoneTask(WellDoneTask item)
        {
            WellDoneTask current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/WellDoneTask/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteWellDoneTask(string id)
        {
             return DeleteAsync(id);
        }

    }
}