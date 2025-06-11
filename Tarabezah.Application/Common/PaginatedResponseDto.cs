using System.Collections.Generic;

namespace Tarabezah.Application.Common;

/// <summary>
/// Generic wrapper for paginated responses
/// </summary>
/// <typeparam name="T">Type of the data being paginated</typeparam>
public class PaginatedResponseDto<T>
{
    /// <summary>
    /// The actual data for the current page
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPrevious => PageNumber > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNext => PageNumber < TotalPages;
} 