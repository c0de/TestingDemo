// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace TestingDemo.Api;

/// <summary>
/// Interface for managing assets (files).
/// </summary>
public interface IAssetService
{
    /// <summary>
    /// Get Asset by Id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task GetAssetAsync(Guid id);
    /// <summary>
    /// Delete Asset by Id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task DeleteAssetAsync(Guid id);
    /// <summary>
    /// Create Asset with Id and content.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    Task CreateAssetAsync(Guid id, byte[] content);
}
