using SpaceFlow.Repositories.Models;
using SpaceFlow.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpaceFlow.Services.Interfaces
{
    public interface IWorkspaceService
    {
        Task<IEnumerable<Workspace>> GetWorkspacesAsync();
        Task<Workspace?> GetWorkspaceByIdAsync(int id);
        Task<Workspace> CreateWorkspaceAsync(CreateWorkspaceDto workspaceDto);
        Task<Workspace?> UpdateWorkspaceAsync(int id, UpdateWorkspaceDto workspaceDto);
        Task<bool> DeleteWorkspaceAsync(int id);
    }
}