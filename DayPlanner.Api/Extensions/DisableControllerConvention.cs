using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace DayPlanner.Api.Extensions
{
    /// <summary>
    /// Helper class to disable a controller
    /// </summary>
    /// <param name="controllerType">Type of controller</param>
    public class DisableControllerConvention(Type controllerType) : IControllerModelConvention
    {
        /// <summary>
        /// Apply the convention
        /// </summary>
        /// <param name="controller"></param>
        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType == controllerType)
            {
                controller.ApiExplorer.IsVisible = false; // Hide it from Swagger
                controller.Actions.Clear(); // Remove all actions
            }
        }
    }
}
