// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


namespace TestingDemo.Api.Services;

/// <summary>
/// Service for managing assets (files).
/// </summary>
public class TemplateService : ITemplateService
{
    /// <inheritdoc/>
    public Task<string> ApplyEmailTemplateAsync<T>(Template template, T obj, CancellationToken cancellationToken) where T : class => throw new NotImplementedException();
    /// <inheritdoc/>
    public string ApplyTemplate<T>(string source, T obj) where T : class => throw new NotImplementedException();
    /// <inheritdoc/>
    public Task<string> ApplyTemplateAsync<T>(Template template, T obj, CancellationToken cancellationToken) where T : class => throw new NotImplementedException();
    /// <inheritdoc/>
    public Task<string> GetTemplate(Template template, CancellationToken cancellationToken) => throw new NotImplementedException();
}
