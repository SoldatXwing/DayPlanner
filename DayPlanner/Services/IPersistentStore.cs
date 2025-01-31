using DayPlanner.Abstractions.Models.Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Services;

/// <summary>
/// A store interface that provides methods to store and access data to persist.
/// </summary>
internal interface IPersistentStore
{
    /// <summary>
    /// Persists a user and an access token in the devices storage.
    /// </summary>
    /// <param name="user">The user to persist.</param>
    /// <param name="accessToken">The access token of this user.</param>
    /// <returns></returns>
    public Task SetUserAsync(User user, string accessToken);

    /// <summary>
    /// Gets the currently persisted user and the access token also stored.
    /// </summary>
    /// <returns>The persisted user and his token. If <c>null</c> no user is currently persisted.</returns>
    public Task<(User user, string accessToken)?> GetUserAsync();

    /// <summary>
    /// Removes the currently persisted user and access token.
    /// </summary>
    /// <returns>A task to await this operation.</returns>
    public Task RemoveUserAsync();
}
