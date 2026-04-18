namespace Cdm.Web.Components.Shared;

/// <summary>
/// Represents a single item in a breadcrumb navigation trail.
/// </summary>
public record BreadcrumbItem(string Label, string Href = "#");
