using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using WellDoneMobileService.DataObjects;
using WelldoneMobileService.Models;
using Microsoft.WindowsAzure.Mobile.Service.Security;

namespace WelldoneMobileService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.User)]
    public class ContextDescriptionController : TableController<ContextDescription>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<ContextDescription>(context, Request, Services);
        }

        // GET tables/ContextDescription
        public IQueryable<ContextDescription> GetAllContextDescription()
        {
            // Get the logged-in user.
            var currentUser = User as ServiceUser;

            return Query().Where(context => context.UserId == currentUser.Id);
            
        }

        // GET tables/ContextDescription/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<ContextDescription> GetContextDescription(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/ContextDescription/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<ContextDescription> PatchContextDescription(string id, Delta<ContextDescription> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/ContextDescription
        public async Task<IHttpActionResult> PostContextDescription(ContextDescription item)
        {
            // Get the logged-in user.
            var currentUser = User as ServiceUser;

            // Set the user ID on the item.
            item.UserId = currentUser.Id;

            ContextDescription current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/ContextDescription/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteContextDescription(string id)
        {
             return DeleteAsync(id);
        }

    }
}