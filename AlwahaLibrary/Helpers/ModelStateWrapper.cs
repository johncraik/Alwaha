using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AlwahaLibrary.Helpers;

public class ModelStateWrapper
{
    private readonly ModelStateDictionary _modelState;
    public bool IsValid => _modelState.IsValid;
    public string ModelStatePrefix { get; } = "Input";

    public ModelStateWrapper(ModelStateDictionary modelState)
    {
        _modelState = modelState;
    }
    
    public ModelStateWrapper(ModelStateDictionary modelState, string modelStatePrefix) 
    {
        _modelState = modelState;
        ModelStatePrefix = modelStatePrefix;
    }

    /// <summary>
    /// Adds an error to the model state with the specified key and error message.
    /// The key is prefixed with the ModelStatePrefix before being added to the model state.
    /// </summary>
    /// <param name="key">The key to which the error message will be associated in the model state.</param>
    /// <param name="errorMessage">The error message to add for the specified key.</param>
    public void AddModelError(string key, string errorMessage)
    {
        _modelState.AddModelError($"{ModelStatePrefix}.{key}", errorMessage);
    }
}