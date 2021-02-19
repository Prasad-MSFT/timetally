// <copyright file="FakeHttpContext.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.Fakes
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Security.Principal;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Teams.Apps.Timesheet.Authentication;
    using Moq;

    /// <summary>
    /// Class to fake HTTP Context.
    /// </summary>
    public class FakeHttpContext
    {
        /// <summary>
        /// Make fake HTTP context for unit testing.
        /// </summary>
        /// <returns>Fake HTTP context.</returns>
        public static HttpContext MakeFakeContext()
        {
            var userAadObjectId = Guid.NewGuid().ToString();
            var context = new Mock<HttpContext>();
            var request = new Mock<HttpContext>();
            var response = new Mock<HttpContext>();
            var user = new Mock<ClaimsPrincipal>();
            var identity = new Mock<IIdentity>();
            var claim = new Claim[]
            {
                new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", userAadObjectId),
            };

            context.Setup(ctx => ctx.User).Returns(user.Object);
            user.Setup(ctx => ctx.Identity).Returns(identity.Object);
            user.Setup(ctx => ctx.Claims).Returns(claim);
            identity.Setup(id => id.IsAuthenticated).Returns(true);
            identity.Setup(id => id.Name).Returns("test");
            return context.Object;
        }

        /// <summary>
        /// Get fake authorization handler context for MustBeValidReporteePolicy.
        /// </summary>
        /// <returns>Authorization handler context for MustBeValidReporteePolicy.</returns>
        public static AuthorizationHandlerContext GetFakeAuthorizationHandlerContextForMustBeValidReporteePolicy()
        {
            var mustBeProjectMemberPolicyRequirement = new[] { new MustBeValidReporteePolicyRequirement() };

            var context = GetFakeHttpContext();
            var filters = new List<IFilterMetadata>();
            var resource = new AuthorizationFilterContext(new ActionContext(context, new RouteData(), new ActionDescriptor()), filters);

            context.Request.RouteValues.Add("reporteeId", "99051013-15d3-4831-a301-ded45bf3d12a");

            return new AuthorizationHandlerContext(mustBeProjectMemberPolicyRequirement, context.User, resource);
        }

        /// <summary>
        /// Get fake authorization handler context.
        /// </summary>
        /// <returns>Authorization handler context.</returns>
        public static AuthorizationHandlerContext GetFakeAuthorizationHandlerContext()
        {
            var mustBeProjectMemberPolicyRequirement = new[] { new MustBeProjectMemberPolicyRequirement() };

            var context = GetFakeHttpContext();

            var filters = new List<IFilterMetadata>();
            var resource = new AuthorizationFilterContext(new ActionContext(context, new RouteData(), new ActionDescriptor()), filters);

            return new AuthorizationHandlerContext(mustBeProjectMemberPolicyRequirement, context.User, resource);
        }

        /// <summary>
        /// Get fake authorization handler context for MustBeProjectCreatorPolicy.
        /// </summary>
        /// <returns>Authorization handler context for MustBeProjectCreatorPolicy.</returns>
        public static AuthorizationHandlerContext GetFakeAuthorizationHandlerContextForMustBeProjectCreatorPolicy()
        {
            var mustBeProjectMemberPolicyRequirement = new[] { new MustBeProjectCreatorRequirement() };

            var context = GetFakeHttpContext();
            var filters = new List<IFilterMetadata>();
            var resource = new AuthorizationFilterContext(new ActionContext(context, new RouteData(), new ActionDescriptor()), filters);

            context.Request.RouteValues.Add("projectId", "1a1cce71-2833-4345-86e2-e9047f73e6af");

            return new AuthorizationHandlerContext(mustBeProjectMemberPolicyRequirement, context.User, resource);
        }

        /// <summary>
        /// Gets fake HTTP accessors for must be valid reportee policy.
        /// </summary>
        /// <returns>Fake http context accessors.</returns>
        public static HttpContextAccessor GetFakeHttpAccessorsForMustBeValidReporteePolicy()
        {
            var httpAccessors = new HttpContextAccessor();
            var httpContext = GetFakeHttpContext();
            httpContext.Request.RouteValues.Add("reporteeId", "99051013-15d3-4831-a301-ded45bf3d12a");
            httpAccessors.HttpContext = httpContext;
            return httpAccessors;
        }

        /// <summary>
        /// Gets fake HTTP accessors for must be valid reportee policy.
        /// </summary>
        /// <returns>Fake http context accessors.</returns>
        public static HttpContextAccessor GetFakeHttpAccessorsForMustBeProjectCreatorPolicy()
        {
            var httpAccessors = new HttpContextAccessor();
            var httpContext = GetFakeHttpContext();
            httpContext.Request.RouteValues.Add("projectId", "1a1cce71-2833-4345-86e2-e9047f73e6af");
            httpAccessors.HttpContext = httpContext;
            return httpAccessors;
        }


        /// <summary>
        /// Get fake authorization handler context for MustHaveReporteesPolicyHandler.
        /// </summary>
        /// <returns>Authorization handler context for MustHaveReporteesPolicyHandler.</returns>
        public static AuthorizationHandlerContext GetFakeAuthorizationHandlerContextForMustHaveReporteesPolicy()
        {
            var mustHaveReporteesPolicyRequirement = new[] { new MustBeManagerPolicyRequirement() };

            var context = GetFakeHttpContext();
            var filters = new List<IFilterMetadata>();
            var resource = new AuthorizationFilterContext(new ActionContext(context, new RouteData(), new ActionDescriptor()), filters);

            context.Request.RouteValues.Add("projectId", "1a1cce71-2833-4345-86e2-e9047f73e6af");

            return new AuthorizationHandlerContext(mustHaveReporteesPolicyRequirement, context.User, resource);
        }
    }
}
