// <copyright file="ResultResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    using System.Net;

    /// <summary>
    /// Generic model to pass data operation result to controller action method.
    /// </summary>
    public class ResultResponse
    {
        /// <summary>
        /// Gets or sets HTTP status code to be returned from controller.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets error messages.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets response data.
        /// </summary>
        public object Response { get; set; }
    }
}