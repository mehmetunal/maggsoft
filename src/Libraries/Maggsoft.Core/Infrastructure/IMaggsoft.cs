using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;

namespace Maggsoft.Core.Infrastructure;

public interface IMaggsoft
{
    /// <summary>
    /// Configure HTTP request pipeline
    /// </summary>
    /// <param name="application">Builder for configuring an application's request pipeline</param>
    void ConfigureRequestPipeline(IApplicationBuilder application);

    /// <summary>
    /// Resolve dependency
    /// </summary>
    /// <typeparam name="T">Type of resolved service</typeparam>
    /// <returns>Resolved service</returns>
    T Resolve<T>() where T : class;

    /// <summary>
    /// Resolve dependency
    /// </summary>
    /// <param name="type">Type of resolved service</param>
    /// <returns>Resolved service</returns>
    object Resolve(Type type);
    /// <summary>
    /// Resolve dependencies
    /// </summary>
    /// <typeparam name="T">Type of resolved services</typeparam>
    /// <returns>Collection of resolved services</returns>
    IEnumerable<T> ResolveAll<T>();
}
