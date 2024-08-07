﻿namespace NexusMods.Abstractions.Loadouts.Mods;

/// <summary>
/// Status of a given Mod
/// </summary>
[Obsolete(message: "This will be removed with Loadout Items")]
public enum ModStatus
{
    /// <summary>
    /// The mod is installed.
    /// </summary>
    Installed,
    /// <summary>
    /// The mod is currently being installed.
    /// </summary>
    Installing,
    /// <summary>
    /// The mod install has failed.
    /// </summary>
    Failed
}
