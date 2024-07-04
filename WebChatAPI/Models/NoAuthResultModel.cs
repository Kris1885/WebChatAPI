using Microsoft.AspNetCore.Mvc;

namespace WebChatAPI.Models
{
    public class NoAuthResultModel : IActionResult
    {
        public Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = 403;
            return Task.CompletedTask;
        }
    }
}
