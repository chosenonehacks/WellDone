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
    public class ContextDescriptionsController : TableController<ContextDescription>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<ContextDescription>(context, Request, Services);
        }

        // GET tables/Context
        public IQueryable<ContextDescription> GetAllContext()
        {
            return Query(); 
        }

        // GET tables/Context/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<ContextDescription> GetContext(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Context/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<ContextDescription> PatchContext(string id, Delta<ContextDescription> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/Context
        public async Task<IHttpActionResult> PostContext(ContextDescription item)
        {
            ContextDescription current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Context/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteContext(string id)
        {
             return DeleteAsync(id);
        }

    }
}