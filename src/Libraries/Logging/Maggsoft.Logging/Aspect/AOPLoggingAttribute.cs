using Maggsoft.Aspect.Core;
using Maggsoft.Aspect.Core.Aspects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Maggsoft.Logging.Aspect
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AOPLoggingAttribute : AspectAttribute
    {
        private ILogger<AOPLoggingAttribute> _logger;
        public override int Order => 1;

        public override void OnBefore(MethodExecutionArgs args)
        {
            _logger.LogInformation($"OnBefore method excecuting. Method Name : {args.Method.Name}, AttributeName = {nameof(AOPLoggingAttribute)}, Order : {Order}");
        }

        public override Task OnBeforeAsync(MethodExecutionArgs args)
        {
            _logger.LogInformation($"OnBeforeAsync method excecuting. Method Name : {args.Method.Name}, AttributeName = {nameof(AOPLoggingAttribute)}, Order : {Order}");
            return Task.CompletedTask;
        }

        public override void OnSuccess(MethodExecutionArgs args)
        {
            _logger.LogInformation($"OnSuccess method excecuting. Method Name : {args.Method.Name}, AttributeName = {nameof(AOPLoggingAttribute)}, Order : {Order}");
        }

        public override void OnAfter(MethodExecutionArgs args)
        {
            _logger.LogInformation($"OnAfter method excecuting. Method Name : {args.Method.Name}, AttributeName = {nameof(AOPLoggingAttribute)}, Order : {Order}");
        }

        public override AspectAttribute LoadDependencies(IServiceProvider serviceProvider)
        {
            _logger ??= serviceProvider.GetService<ILogger<AOPLoggingAttribute>>();
            return this;
        }
    }
}