using System;
using System.Collections.Generic;

namespace SpaceFlow.ViewModels
{
    public class WorkspaceSearchDto
    {
        public string? SearchTerm { get; set; }
        public string? Type { get; set; }
        public int? MinCapacity { get; set; }
        public int? MaxCapacity { get; set; }
        public decimal? MinPricePerHour { get; set; }
        public decimal? MaxPricePerHour { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string>? Amenities { get; set; }
        public bool? IsAvailable { get; set; } = true;
        public string SortBy { get; set; } = "Name"; // Name, Price, Capacity, Type
        public string SortOrder { get; set; } = "asc"; // asc, desc
        public bool SortDescending { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }
}
