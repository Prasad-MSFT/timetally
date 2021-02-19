// <copyright file="TimesheetActivityHandlerOptions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Bot
{
    /// <summary>
    /// The TimesheetActivityHandlerOptions are the options for the <see cref="TimesheetActivityHandler" /> bot.
    /// </summary>
    public sealed class TimesheetActivityHandlerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the response to a message should be all uppercase.
        /// </summary>
        public bool IsUpperCaseResponse { get; set; }
    }
}