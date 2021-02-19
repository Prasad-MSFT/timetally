// <copyright file="toast-notification.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { ActivityStatus } from "./activity-status";

export default interface IToastNotification {
    id: number
    message: string,
    type: ActivityStatus
}