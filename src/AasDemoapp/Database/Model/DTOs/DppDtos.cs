namespace AasDemoapp.Database.Model.DTOs
{
    public class HandoverDocumentationDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SubmodelSummaryDto? Submodel { get; set; }
        public string? Error { get; set; }
    }

    public class SubmodelSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string? IdShort { get; set; }
        public string? Description { get; set; }
        public string? Version { get; set; }
        public string? Revision { get; set; }
        public int ElementCount { get; set; }
    }
}
