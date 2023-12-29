using Microsoft.AspNetCore.Builder;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Maggsoft.Core.Infrastructure;

public class MaggsoftEngine : IMaggsoft
{

    #region Utilities

    /// <summary>
    /// Get IServiceProvider
    /// </summary>
    /// <returns>IServiceProvider</returns>
    protected IServiceProvider GetServiceProvider()
    {
        if (ServiceProvider == null)
            return null;
        var accessor = ServiceProvider?.GetService<IHttpContextAccessor>();
        var context = accessor?.HttpContext;
        return context?.RequestServices ?? ServiceProvider;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Service provider
    /// </summary>
    public virtual IServiceProvider ServiceProvider { get; protected set; }

    #endregion

    #region Method

    /// <summary>
    /// Resolve dependency
    /// </summary>
    /// <typeparam name="T">Type of resolved service</typeparam>
    /// <returns>Resolved service</returns>
    public T Resolve<T>() where T : class
    {
        return (T)Resolve(typeof(T));
    }

    /// <summary>
    /// Resolve dependency
    /// </summary>
    /// <param name="type">Type of resolved service</param>
    /// <returns>Resolved service</returns>
    public object Resolve(Type type)
    {
        var sp = GetServiceProvider();
        if (sp == null)
            return null;
        return sp.GetService(type);
    }

    /// <summary>
    /// Resolve dependencies
    /// </summary>
    /// <typeparam name="T">Type of resolved services</typeparam>
    /// <returns>Collection of resolved services</returns>
    public virtual IEnumerable<T> ResolveAll<T>()
    {
        return (IEnumerable<T>)GetServiceProvider().GetServices(typeof(T));
    }

    /// <summary>
    /// Configure the application HTTP request pipeline
    /// </summary>
    /// <param name="application">Builder for configuring an application's request pipeline</param>
    public void ConfigureRequestPipeline(IApplicationBuilder application)
    {
        ServiceProvider = application.ApplicationServices;
    }

    #endregion
}