// <copyright file="IConversationRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Repositories
{
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Exposes methods which will be used to perform database operations on user conversation entity.
    /// </summary>
    public interface IConversationRepository : IBaseRepository<Conversation>
    {
    }
}
