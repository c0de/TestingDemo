// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace TestingDemo.Api;

/// <summary>
/// Interface for managing assets (files).
/// </summary>
public interface ITemplateService
{
    /// <summary>
    /// Get Template string for the given identifier.
    /// </summary>
    /// <param name="template"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> GetTemplate(Template template, CancellationToken cancellationToken);
    /// <summary>
    /// Apply the template for the given object using the mustache spec.
    /// </summary>
    /// <remarks>
    /// <para>
    ///		See mustache: https://mustache.github.io/
    ///		Docs: https://mustache.github.io/mustache.5.html
    /// </para>
    /// <para>
    ///		Handlebars.Net: https://github.com/Handlebars-Net/Handlebars.Net
    ///		Helpers: https://github.com/Handlebars-Net/Handlebars.Net.Helpers
    /// </para>
    /// <para>
    ///		Helpers and options for templates:
    ///		Value
    ///			"<div>{{{User.Team.DisplayName}}}</div>"
    ///		App Root url
    ///			"<a href={{RootUrl}}/users>App Users</a>"
    ///		Allow Html (3 brackets wont encode an html string)
    ///			"<div>{{{CoachNotes}}}</div>"
    ///		Short Date Format
    ///			"<div>{{shortDate CreatedAt}}</div>"
    ///		Check if Null
    ///			"<div>{{#if EndOn}}End Date: {{EndOn}}{{/if}}</div>"
    ///		Check if Array Empty
    ///			"<div>{{ifEmpty Items}}No Items{{/isEmpty}}</div>"
    ///		Check if Equal
    ///			"<div>{{ifEq Id 1}}Id: 1{{/ifEq}}</div>"
    ///		Loop
    ///			"<ul>{{#each Items}}<li>{{Name}}</li>{{/each}}</ul>"
    /// </para>
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">source string containing place holders. ie: {{body}}</param>
    /// <param name="obj">object with replacement values ie: { body = "Test" }</param>
    /// <returns>formatted display string</returns>
    string ApplyTemplate<T>(string source, T obj) where T : class;
    /// <summary>
    /// Apply the template for the given object using the mustache spec.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="notificationTemplate">Notification Template to apply</param>
    /// <param name="obj">Object for template</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>formatted display string</returns>
    Task<string> ApplyTemplateAsync<T>(Template template, T obj, CancellationToken cancellationToken) where T : class;
    /// <summary>
    /// Apply the template for the given object using the mustache spec and wraps it in the <see cref="NotificationTemplate.BaseEmailTemplate"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="notificationTemplate"></param>
    /// <param name="obj"></param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>formatted display string</returns>
    Task<string> ApplyEmailTemplateAsync<T>(Template template, T obj, CancellationToken cancellationToken) where T : class;
}
