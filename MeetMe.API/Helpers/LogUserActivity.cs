using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MeetMe.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace MeetMe.API.Helpers
{
    public class LogUserActivity: IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            var userId = int.Parse(resultContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var unitOfWork = resultContext.HttpContext.RequestServices.GetService<IUnitOfWork>();
            var user = await unitOfWork.Users.GetUser(userId);
            user.LastActive = DateTime.Now;
            await unitOfWork.SaveAll();
        }
    }
}
