using API.Extentions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace API.Heplers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ActionExecutedContext resultContext = await next();

            if(!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

            string userId = resultContext.HttpContext.User.GetUserId();

            IUserRepository repo = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            Entities.AppUser user = await repo.GetUserByIdAsync(int.Parse(userId));
            
            user.LastActive = DateTime.UtcNow;
            await repo.SaveAllAsinc();
        }
    }
}
