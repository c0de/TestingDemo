// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace TestingDemo.Api.Services;

/// <summary>
/// Service for managing assets (files).
/// </summary>
public class AssetService : IAssetService
{
    /// <inheritdoc/>
    public Task CreateAssetAsync(Guid id, byte[] content) => throw new NotImplementedException();
    /// <inheritdoc/>
    public Task DeleteAssetAsync(Guid id) => throw new NotImplementedException();
    /// <inheritdoc/>
    public Task GetAssetAsync(Guid id) => throw new NotImplementedException();
}
