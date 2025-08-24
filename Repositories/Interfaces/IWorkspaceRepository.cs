using SpaceFlow.Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpaceFlow.Repositories.Interfaces
{
    public interface IWorkspaceRepository
    {
        Task<IEnumerable<Workspace>> GetAllWorkspacesAsync();
        Task<Workspace?> GetWorkspaceByIdAsync(int id);
        Task<Workspace> CreateWorkspaceAsync(Workspace workspace);
        Task UpdateWorkspaceAsync(Workspace workspace);
        Task DeleteWorkspaceAsync(Workspace workspace);
    }
}