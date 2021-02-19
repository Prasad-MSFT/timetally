// <copyright file="GraphPermissionType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Authentication
{
    /// <summary>
    /// Microsoft Graph permission types.
    /// </summary>
    public enum GraphPermissionType
    {
        /// <summary>
        /// This represents application permission of Microsoft Graph.
        /// </summary>
        Application,

        /// <summary>
        /// This represents delegate permission of Microsoft Graph.
        /// </summary>
        UserDelegated,
    }
}
