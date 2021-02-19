// <copyright file="RequestType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    /// <summary>
    /// API request status.
    /// </summary>
    public enum RequestType
    {
        /// <summary>
        /// This represents the request is initiated.
        /// </summary>
        Initiated,

        /// <summary>
        /// TThis represents the request is completed.
        /// </summary>
        Succeeded,

        /// <summary>
        /// This represents the request is failed
        /// </summary>
        Failed,
    }
}