// <copyright file="user-timesheet.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import IProjectDetails from "./project-details";

export default interface IUserTimesheet {
    timesheetDate: Date,
    projectDetails: IProjectDetails[]
}