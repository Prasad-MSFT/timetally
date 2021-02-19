// <copyright file="constants.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

export default class Constants {
    public static readonly ProjectTitleMaxLength: number = 50;
    public static readonly ClientNameMaxLength: number = 50;
    public static readonly TaskMaxLength: number = 300;
    public static readonly taskModuleHeight: number = 746;
    public static readonly taskModuleWidth: number = 600;
    public static readonly billableStatusColor: string = "#61AEE5";
    public static readonly underUtilizedBillableStatusColor: string = "#f58442";
    public static readonly nonBillableStatusColor: string = "#D54130";
    public static readonly underutilizedNonUtilizedStatusColor: string = "#858C98";

    // The calendar day of month on which past month timesheet will get freeze.
    public static readonly timesheetFreezeDayOfMonth: number = 10;

    // The max screen width up to which mobile view is enabled.
    public static readonly maxWidthForMobileView: number = 750;

    // The maximum efforts limit that can be filled per day.
    public static readonly dailyEffortsLimit: number = 8;

    // The maximum efforts limit that can be filled per week.
    public static readonly weeklyEffortsLimit: number = 40;

    // The maximum length manager can enter for reason's description.
    public static readonly reasonDescriptionMaxLength: number = 100;

    // Table's check-box column width.
    public static readonly tableCheckboxColumnWidth: string = "17vw";
}

// Indicates Teams theme names
export enum Themes {
	dark = "dark",
	contrast = "contrast",
	light = "light",
	default = "default"
}

// Project card navigation command.
export enum NavigationCommand {
	Forward,
	Backward,
	Default
}

// Indicates the response status codes.
export enum ResponseStatus {
	OK = 200,
	Created = 201,
	NoContent = 204,
}

// Formating model type.
export enum ModelType {
	Member,
	Task
}

// Indicates UI steps rendered while creating new project.
export enum AddProjectUISteps {
    step1 = 1,
    step2 = 2
}