// <copyright file="Conversation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Models
{
    using System;

    /// <summary>
    /// Holds user and conversation details.
    /// </summary>
    public partial class Conversation
    {
        /// <summary>
        /// Gets or sets of sets AAD user object identifier.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets of sets user-bot conversation Id.
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets service URL.
        /// </summary>
        public string ServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets bot installation date.
        /// </summary>
        public DateTime BotInstalledOn { get; set; }
    }
}
