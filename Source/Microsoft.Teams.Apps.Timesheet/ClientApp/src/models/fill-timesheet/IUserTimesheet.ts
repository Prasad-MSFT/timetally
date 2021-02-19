// <copyright file="IUserTimesheet.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import IProjectDetails from "./IProjectDetails";

export default interface IUserTimesheet {
    timesheetDate: Date;
    projectDetails: Array<IProjectDetails>;
}