using System;

namespace Maggsoft.Aspect.Core.Aspects;
public interface IAspect
{
    /// <summary>
    /// Triggered at the beginning of the method.
    /// If the result property of the args parameter is set, method is interrupted without executing
    /// </summary>
    /// <param name="args">Method executing arguments</param>
    public void OnBefore(MethodExecutionArgs args);

    /// <summary>
    /// Triggers if an error occurs within the method.
    /// The error is set to the exception property of the args parameter.
    /// </summary>
    /// <param name="args">Method executing arguments</param>
    public void OnException(MethodExecutionArgs args);

    /// <summary>
    /// Triggers when method completed without error.
    /// </summary>
    /// <param name="args">Method executing arguments</param>
    public void OnSuccess(MethodExecutionArgs args);

    /// <summary>
    /// After running the method (including the error condition) it is triggered.
    /// </summary>
    /// <param name="args">Method executing arguments</param>
    public void OnAfter(MethodExecutionArgs args);

    /// <summary>
    /// Load required dependencies from DI
    /// This method is called before all aspect methods.
    /// </summary>
    /// <param name="serviceProvider">DI ServiceProvider</param>
    /// <returns>AspectAttribute instance</returns>
    public AspectAttribute LoadDependencies(IServiceProvider serviceProvider);
}