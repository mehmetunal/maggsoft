
using System;
using System.Threading.Tasks;

namespace Maggsoft.Aspect.Core.Aspects
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class AspectAttribute : Attribute, IAspect, IAspectAsync
    {
        public virtual int Order => 1;

        public virtual void OnBefore(MethodExecutionArgs args)
        {
        }

        public virtual Task OnBeforeAsync(MethodExecutionArgs args)
            => Task.Run(() => OnBefore(args));

        public virtual void OnSuccess(MethodExecutionArgs args)
        {
        }

        public virtual Task OnSuccessAsync(MethodExecutionArgs args)
            => Task.Run(() => OnSuccess(args));

        public virtual void OnException(MethodExecutionArgs args)
            => throw args.Exception;

        public virtual Task OnExceptionAsync(MethodExecutionArgs args)
            => throw args.Exception;

        public virtual void OnAfter(MethodExecutionArgs args)
        {
        }

        public virtual Task OnAfterAsync(MethodExecutionArgs args)
            => Task.Run(() => OnAfter(args));

        public virtual AspectAttribute LoadDependencies(IServiceProvider serviceProvider)
            => this;
    }
}