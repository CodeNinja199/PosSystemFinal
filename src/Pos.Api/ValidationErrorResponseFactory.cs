using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Pos.Api;

// Called by ASP.NET Core instead of its default 400 response when the DataAnnotations on a request class fail.
// Registered in Program.cs through ConfigureApiBehaviorOptions.
public static class ValidationErrorResponseFactory
{
    public static IActionResult CreateResponse(ActionContext actionContext)
    {
        List<string> errorMessages = new List<string>();
        foreach (KeyValuePair<string, ModelStateEntry?> modelStateEntry in actionContext.ModelState)
        {
            if (modelStateEntry.Value == null)
            {
                continue;
            }

            foreach (ModelError modelError in modelStateEntry.Value.Errors)
            {
                errorMessages.Add(modelError.ErrorMessage);
            }
        }

        string message = string.Join(" ", errorMessages);

        return new BadRequestObjectResult(new { message = message });
    }
}
