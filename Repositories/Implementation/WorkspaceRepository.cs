using Microsoft.EntityFrameworkCore;
using SpaceFlow.Data;
using SpaceFlow.Repositories.Interfaces;
using SpaceFlow.Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpaceFlow.Repositories.Implementation
{
    public class WorkspaceRepository : IWorkspaceRepository
    {
        private readonly ApplicationDbContext _context;

        public WorkspaceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Workspace>> GetAllWorkspacesAsync()
        {
            return await _context.Workspaces.ToListAsync();
        }

        public async Task<Workspace?> GetWorkspaceByIdAsync(int id)
        {
            return await _context.Workspaces.FindAsync(id);
        }

        public async Task<Workspace> CreateWorkspaceAsync(Workspace workspace)
        {
            _context.Workspaces.Add(workspace);
            await _context.SaveChangesAsync();
            return workspace;
        }

        public async Task UpdateWorkspaceAsync(Workspace workspace)
        {
            workspace.UpdatedDate = DateTime.UtcNow;
            _context.Workspaces.Update(workspace);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteWorkspaceAsync(Workspace workspace)
        {
            _context.Workspaces.Remove(workspace);
            await _context.SaveChangesAsync();
        }
    }
}