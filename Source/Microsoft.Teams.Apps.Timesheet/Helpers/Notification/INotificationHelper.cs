// <copyright file="INotificationHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Helpers
{
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// Helper for sending notifications to users.
    /// </summary>
    public interface INotificationHelper
    {
        /// <summary>
        /// Sends notification to the users.
        /// </summary>
        /// <param name="user">The users to which notification need to send</param>
        /// <param name="card">The notification card that to be send</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SendNotificationToUserAsync(Conversation user, Attachment card);
    }
}