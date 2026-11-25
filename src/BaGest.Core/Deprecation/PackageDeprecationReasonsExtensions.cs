using System;
using System.Collections.Generic;
using System.Linq;

namespace BaGest.Core;

/// <summary>
/// Conversions between <see cref="PackageDeprecationReasons"/> and the reason strings of the NuGet protocol.
/// </summary>
public static class PackageDeprecationReasonsExtensions
{
    // The order in which reasons are reported by the NuGet protocol.
    private static readonly PackageDeprecationReasons[] ProtocolOrder =
    [
        PackageDeprecationReasons.Legacy,
        PackageDeprecationReasons.CriticalBugs,
        PackageDeprecationReasons.Other,
    ];

    /// <summary>
    /// The reason strings of the NuGet protocol, for example in error messages: "Legacy, CriticalBugs, Other".
    /// </summary>
    public static string AllowedProtocolValues { get; } = string.Join(", ", ProtocolOrder);

    /// <summary>
    /// Convert the reasons to the strings used by the NuGet protocol, for example "Legacy".
    /// </summary>
    /// <param name="reasons">The reasons to convert.</param>
    /// <returns>The protocol strings, or an empty list if the package is not deprecated.</returns>
    public static IReadOnlyList<string> ToProtocolValues(this PackageDeprecationReasons reasons)
    {
        return ProtocolOrder
            .Where(reason => reasons.HasFlag(reason))
            .Select(reason => reason.ToString())
            .ToList();
    }

    /// <summary>
    /// Parse a single NuGet protocol reason, for example "Legacy". Parsing is case-insensitive.
    /// </summary>
    /// <param name="value">The protocol value to parse.</param>
    /// <param name="reason">The parsed reason.</param>
    /// <returns>Whether the value is a known deprecation reason.</returns>
    public static bool TryParseProtocolValue(string value, out PackageDeprecationReasons reason)
    {
        var trimmed = value?.Trim();
        reason = ProtocolOrder.FirstOrDefault(r => string.Equals(r.ToString(), trimmed, StringComparison.OrdinalIgnoreCase));

        return reason != PackageDeprecationReasons.None;
    }
}
