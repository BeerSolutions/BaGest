namespace BaGest.Core;

/// <summary>
/// The order of search results. This is a BaGest extension: the NuGet V3 search protocol does not define an order.
/// The orders and their <c>sortBy</c> values are those of nuget.org's gallery search, see <see cref="SearchSortOrders"/>.
/// </summary>
public enum SearchSortOrder
{
    /// <summary>
    /// The default order, used when no other order is requested. BaGest does not score results,
    /// so packages are ordered by ID.
    /// </summary>
    Relevance,

    /// <summary>
    /// Most recently changed first. BaGest does not record metadata edits, so this is the same
    /// as <see cref="Published"/>.
    /// </summary>
    LastEdited,

    /// <summary>
    /// Most recently published first, by the newest version of each package.
    /// </summary>
    Published,

    /// <summary>
    /// By title in ascending case-insensitive order. The ID is used for packages without a title.
    /// </summary>
    TitleAscending,

    /// <summary>
    /// By title in descending case-insensitive order. The ID is used for packages without a title.
    /// </summary>
    TitleDescending,

    /// <summary>
    /// Least recently published first, by the newest version of each package.
    /// </summary>
    CreatedAscending,

    /// <summary>
    /// Most recently published first, by the newest version of each package.
    /// </summary>
    CreatedDescending,

    /// <summary>
    /// Fewest downloads across all versions first.
    /// </summary>
    TotalDownloadsAscending,

    /// <summary>
    /// Most downloads across all versions first.
    /// </summary>
    TotalDownloadsDescending,
}
