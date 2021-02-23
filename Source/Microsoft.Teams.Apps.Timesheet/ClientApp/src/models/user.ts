// <copyright file="user.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

export default interface IUser {
    // User's display name.
    displayName: string;

    // User's email address.
    email: string;

    // User AAD object identifier.
    id: string;
}
