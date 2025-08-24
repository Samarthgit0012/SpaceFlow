using SpaceFlow.Repositories.Interfaces;
using SpaceFlow.Repositories.Models;
using SpaceFlow.Services.Interfaces;
using SpaceFlow.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpaceFlow.Services.Implementation
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly IWorkspaceRepository _workspaceRepository;

        public WorkspaceService(IWorkspaceRepository workspaceRepository)
        {
            _workspaceRepository = workspaceRepository;
        }

        public async Task<IEnumerable<Workspace>> GetWorkspacesAsync()
        {
            return await _workspaceRepository.GetAllWorkspacesAsync();
        }

        public async Task<Workspace?> GetWorkspaceByIdAsync(int id)
        {
            return await _workspaceRepository.GetWorkspaceByIdAsync(id);
        }

        public async Task<Workspace> CreateWorkspaceAsync(CreateWorkspaceDto workspaceDto)
        {
            var newWorkspace = new Workspace
            {
                Name = workspaceDto.Name,
                Type = workspaceDto.Type,
                Capacity = workspaceDto.Capacity,
                PricePerHour = workspaceDto.PricePerHour,
                Amenities = workspaceDto.Amenities,
                IsAvailable = workspaceDto.IsAvailable,
                ImageUrl = workspaceDto.ImageUrl
            };

            return await _workspaceRepository.CreateWorkspaceAsync(newWorkspace);
        }

        public async Task<Workspace?> UpdateWorkspaceAsync(int id, UpdateWorkspaceDto workspaceDto)
        {
            var existingWorkspace = await _workspaceRepository.GetWorkspaceByIdAsync(id);
            if (existingWorkspace == null)
            {
                return null;
            }

            existingWorkspace.Name = workspaceDto.Name;
            existingWorkspace.Type = workspaceDto.Type;
            existingWorkspace.Capacity = workspaceDto.Capacity;
            existingWorkspace.PricePerHour = workspaceDto.PricePerHour;
            existingWorkspace.Amenities = workspaceDto.Amenities;
            existingWorkspace.IsAvailable = workspaceDto.IsAvailable;
            existingWorkspace.ImageUrl = workspaceDto.ImageUrl;

            await _workspaceRepository.UpdateWorkspaceAsync(existingWorkspace);
            return existingWorkspace;
        }

        public async Task<bool> DeleteWorkspaceAsync(int id)
        {
            var workspaceToDelete = await _workspaceRepository.GetWorkspaceByIdAsync(id);
            if (workspaceToDelete == null)
            {
                return false;
            }

            await _workspaceRepository.DeleteWorkspaceAsync(workspaceToDelete);
            return true;
        }
    }
}