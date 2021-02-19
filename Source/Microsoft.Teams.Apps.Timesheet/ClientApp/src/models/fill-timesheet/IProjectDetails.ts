// <copyright file="IProjectDetails.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import ITimesheetDetails from "./ITimesheetDetails";
import { Guid } from "guid-typescript";

export default interface IProjectDetails {
    id: Guid;
    title: string;
    timesheetDetails: Array<ITimesheetDetails>;
}