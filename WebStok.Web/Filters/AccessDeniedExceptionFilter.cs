using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebStok.Business.Exceptions;

namespace WebStok.Web.Filters;

public class AccessDeniedExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not AccessDeniedException)
        {
            return;
        }

        context.Result = new ForbidResult();
        context.ExceptionHandled = true;
    }
}