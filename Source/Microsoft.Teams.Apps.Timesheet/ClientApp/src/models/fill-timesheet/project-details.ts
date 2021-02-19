// <copyright file="project-details.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import ITimesheetDetails from "./timesheet-details";

export default interface IProjectDetails {
    id: string,
    title: string,
    timesheetDetails: ITimesheetDetails[],
    isProjectViewExpanded?: boolean,
    isAddNewTaskActivated: boolean,
    isAddNewTaskInProgress: boolean,
    startDate: Date,
    endDate: Date
}