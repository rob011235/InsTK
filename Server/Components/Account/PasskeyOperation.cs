// <copyright file="PasskeyOperation.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Components.Account
{
    /// <summary>
    /// Represents the type of passkey operation being performed.
    /// </summary>
    public enum PasskeyOperation
    {
        /// <summary>
        /// Indicates a passkey creation operation.
        /// </summary>
        Create = 0,

        /// <summary>
        /// Indicates a passkey request operation.
        /// </summary>
        Request = 1,
    }
}
