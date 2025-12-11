namespace AiGroupChat.Application.DTOs.Common;

/// <summary>
/// Paginated response wrapper.
/// </summary>
/// <typeparam name="T">Type of items in the response.</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// Items for the current page.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    /// <example>1</example>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    /// <example>50</example>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    /// <example>127</example>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    /// <example>3</example>
    public int TotalPages { get; set; }

    /// <summary>
    /// Whether there's a next page available.
    /// </summary>
    /// <example>true</example>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Whether there's a previous page available.
    /// </summary>
    /// <example>false</example>
    public bool HasPreviousPage { get; set; }
}