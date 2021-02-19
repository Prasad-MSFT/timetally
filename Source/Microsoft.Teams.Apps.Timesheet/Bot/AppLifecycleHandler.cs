// <copyright file="AppLifecycleHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Bot
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Timesheet.Services;

    /// <summary>
    /// Helper for handling bot related activities.
    /// </summary>
    public class AppLifecycleHandler : IAppLifecycleHandler
    {
        /// <summary>
        /// Instance to send logs to the Application Insights service.
        /// </summary>
        private readonly ILogger<AppLifecycleHandler> logger;

        /// <summary>
        /// Instance of adaptive card service to create and get adaptive cards.
        /// </summary>
        private readonly IAdaptiveCardService adaptiveCardService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppLifecycleHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="adaptiveCardService">Instance of adaptive card service to create and get adaptive cards.</param>
        public AppLifecycleHandler(
            ILogger<AppLifecycleHandler> logger,
            IAdaptiveCardService adaptiveCardService)
        {
            this.logger = logger;
            this.adaptiveCardService = adaptiveCardService;
        }

        /// <summary>
        /// Sends welcome card to user when bot is installed in personal scope.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <returns>A task that represents a response.</returns>
        public async Task OnBotInstalledInPersonalAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext), "Turncontext cannot be null");

            this.logger.LogInformation($"Bot added in personal scope for user {turnContext.Activity.From.AadObjectId}");
            var userWelcomeCardAttachment = this.adaptiveCardService.GetWelcomeCardForPersonalScope();
            await turnContext.SendActivityAsync(MessageFactory.Attachment(userWelcomeCardAttachment));

            var activity = turnContext.Activity;

            // TODO: Save user conversation id, AAD object id, service URL in DB.
            this.logger.LogInformation($"Successfully installed app for user {activity.From.AadObjectId}.");
        }
    }
}
