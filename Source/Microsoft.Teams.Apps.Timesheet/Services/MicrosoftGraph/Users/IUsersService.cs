﻿// <copyright file="IUsersService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Services.MicrosoftGraph
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Graph;

    /// <summary>
    /// Get the reportees data.
    /// </summary>
    public interface IUsersService
    {
        /// <summary>
        /// Get direct reportees for logged in user.
        /// If search text is provided then reportees will be filtered on the basis of display name and email.
        /// </summary>
        /// <param name="search">Text by which reportees will be filtered.</param>
        /// <returns>List of reportees.</returns>
        Task<IEnumerable<User>> GetReporteesAsync(string search);

        /// <summary>
        /// Get manager of logged in user.
        /// </summary>
        /// <returns>Directory object associated with manager.</returns>
        Task<DirectoryObject> GetManagerAsync();

        /// <summary>
        /// Get users information from graph API.
        /// </summary>
        /// <param name="userObjectIds">Collection of AAD Object ids of users.</param>
        /// <returns>A task that returns collection of user information.</returns>
        Task<IEnumerable<User>> GetUsersAsync(IEnumerable<string> userObjectIds);
    }
}