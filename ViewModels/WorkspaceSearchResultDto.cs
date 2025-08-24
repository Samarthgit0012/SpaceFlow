using SpaceFlow.Repositories.Models;
using System.Collections.Generic;

namespace SpaceFlow.ViewModels
{
    public class WorkspaceSearchResultDto
    {
        public List<Workspace> Workspaces { get; set; } = new List<Workspace>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage => PageNumber * PageSize < TotalCount;
        public bool HasPreviousPage => PageNumber > 1;
        public List<string> AvailableTypes { get; set; } = new List<string>();
        public List<string> AvailableAmenities { get; set; } = new List<string>();
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
    }
}
