// <copyright file="project.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { ResponseStatus } from "../../constants/constants";
import IProjectMember from "../../models/project-member";
import IProjectMemberOverview from "../../models/project-member-overview";
import IProjectTaskOverview from "../../models/project-task-overview";
import IProjectUtilization from "../../models/project-utilization";
import { Guid } from "guid-typescript";

const projectUtilization: IProjectUtilization = {
    billableUtilizedHours: 30,
    id: "1212",
    nonBillableUtilizedHours: 22,
    underutilizedBillableHours: 2,
    underutilizedNonBillableHours: 2,
    title: "Test",
    totalHours: 100,
    projectEndDate: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate() + 1),
    projectStartDate: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate() + 2)
};

const memberOverview: Array<IProjectMemberOverview> = [
    { id: "34344", isBillable: true, isRemoved: false, isSelected: false, projectId: "1212", totalHours: 50, userId: "1212", userName: "demo 1" },
    { id: "34345", isBillable: true, isRemoved: false, isSelected: false, projectId: "1212", totalHours: 50, userId: "1213", userName: "demo 1" }
];

const projectTaskOverview: Array<IProjectTaskOverview> = [
    {
        id: Guid.createEmpty().toString(), isRemoved: false, isSelected: false, projectId: "1212", title: "test", totalHours: 44, startDate: new Date(), endDate: new Date()
    }
];

/**
 * Get project utilization details between date range.
 * @param projectId The project Id of which project details to get.
 * @param startDate The start date of the date range.
 * @param endDate The end date of the date range.
 */
export const getProjectUtilizationAsync = async (
    projectId: string,
    startDate: Date,
    endDate: Date) => {
    return Promise.resolve({
        data: projectUtilization,
        status: ResponseStatus.OK
    });
};

/**
 * Upload image photo
 * @param formData Form data containing selected image
 * @param teamId The LnD team ID
 */
export const addMembersAsync = async (members: Array<IProjectMember>) => {
    return Promise.resolve({
        data: true,
        status: ResponseStatus.OK
    });
};

/**
 * Save event as draft
 * @param event Event details to be saved as draft
 * @param teamId The LnD team ID
 */
export const updateMembersAsync = async (members: Array<IProjectMemberOverview>) => {
    return Promise.resolve({
        data: true,
        status: ResponseStatus.OK
    });
};

/**
 * Update draft event
 * @param event Event details to be updated as draft
 * @param teamId The LnD team ID
 */
export const getProjectMembersOverviewAsync = async (projectId: string,
    startDate: Date,
    endDate: Date) => {
    return Promise.resolve({
        data: memberOverview,
        status: ResponseStatus.OK
    });
};

/**
 * The API which handles request to create new tasks.
 * @param tasks The details of tasks to be created.
 */
export const createTasksAsync = async (
    tasks: Array<IProjectTaskOverview>) => {
    for (var i = 0; i < tasks.length; i++) {
        tasks[i].id = Guid.createEmpty().toString();
        projectTaskOverview.push(tasks[i]);
    }
    return Promise.resolve({
        data: true,
        status: ResponseStatus.OK
    });
};

/**
 * The API which handles request to update task.
 * @param tasks The details of tasks to be updated.
 */
export const updateTasksAsync = async (
    tasks: Array<IProjectTaskOverview>) => {
    return Promise.resolve({
        data: true,
        status: ResponseStatus.OK
    });
};

/**
 * Get approved and active project tasks overview between date range.
 * @param projectId The project Id of which details to fetch.
 * @param startDate The start date of the date range.
 * @param endDate The end date of the date range.
 */
export const getProjectTasksOverviewAsync = async (
    projectId: string,
    startDate: Date,
    endDate: Date) => {
    return Promise.resolve({
        data: projectTaskOverview,
        status: ResponseStatus.OK
    });
};