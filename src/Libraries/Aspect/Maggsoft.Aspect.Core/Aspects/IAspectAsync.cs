using System.Threading.Tasks;

namespace Maggsoft.Aspect.Core.Aspects;
public interface IAspectAsync
{
    /// <summary>
    /// Triggered at the beginning of the method.
    /// If the result property of the args parameter is set, method is interrupted without executing
    /// </summary>
    /// <param name="args">Method executing arguments</param>
    public Task OnBeforeAsync(MethodExecutionArgs args);

    /// <summary>
    /// Triggers if an error occurs within the method.
    /// The error is set to the exception property of the args parameter.
    /// </summary>
    /// <param name="args">Method executing arguments</param>
    public Task OnExceptionAsync(MethodExecutionArgs args);

    /// <summary>
    /// Triggers when method completed without error.
    /// </summary>
    /// <param name="args">Method executing arguments</param>
    public Task OnSuccessAsync(MethodExecutionArgs args);

    /// <summary>
    /// After running the method (including the error condition) it is triggered.
    /// </summary>
    /// <param name="args">Method executing arguments</param>
    public Task OnAfterAsync(MethodExecutionArgs args);
}