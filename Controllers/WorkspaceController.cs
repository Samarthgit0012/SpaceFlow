using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceFlow.Repositories.Models;
using SpaceFlow.Services.Interfaces;
using SpaceFlow.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpaceFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkspacesController : ControllerBase
    {
        private readonly IWorkspaceService _workspaceService;

        public WorkspacesController(IWorkspaceService workspaceService)
        {
            _workspaceService = workspaceService;
        }

        // GET: api/Workspaces
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Workspace>>> GetWorkspaces()
        {
            var workspaces = await _workspaceService.GetWorkspacesAsync();
            return Ok(workspaces);
        }

        // GET: api/Workspaces/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Workspace>> GetWorkspace(int id)
        {
            var workspace = await _workspaceService.GetWorkspaceByIdAsync(id);

            if (workspace == null)
            {
                return NotFound();
            }

            return Ok(workspace);
        }

        // POST: api/Workspaces
        [HttpPost]
        [Authorize] // Example of protecting an endpoint
        public async Task<ActionResult<Workspace>> PostWorkspace(CreateWorkspaceDto workspaceDto)
        {
            var createdWorkspace = await _workspaceService.CreateWorkspaceAsync(workspaceDto);
            return CreatedAtAction(nameof(GetWorkspace), new { id = createdWorkspace.Id }, createdWorkspace);
        }

        // PUT: api/Workspaces/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutWorkspace(int id, UpdateWorkspaceDto workspaceDto)
        {
            var updatedWorkspace = await _workspaceService.UpdateWorkspaceAsync(id, workspaceDto);
            if (updatedWorkspace == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        // DELETE: api/Workspaces/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteWorkspace(int id)
        {
            var result = await _workspaceService.DeleteWorkspaceAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}