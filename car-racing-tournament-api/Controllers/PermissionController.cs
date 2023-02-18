﻿using car_racing_tournament_api.DTO;
using car_racing_tournament_api.Interfaces;
using car_racing_tournament_api.Models;
using car_racing_tournament_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace car_racing_tournament_api.Controllers
{
    [Route("api/permission")]
    [ApiController]
    public class PermissionController : Controller
    {
        private IPermission _permissionService;

        public PermissionController(IPermission permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpPut("{id}"), Authorize]
        public async Task<IActionResult> Put(Guid id, [FromForm] PermissionType permissionType)
        {
            var resultGetPermission = await _permissionService.GetPermissionById(id);
            if (!resultGetPermission.IsSuccess)
                return NotFound(resultGetPermission.ErrorMessage);

            if (!await _permissionService.IsAdmin(new Guid(User.Identity!.Name!), resultGetPermission.Permission!.SeasonId))
                return Forbid();

            // if downgrade himself, oldest moderator

            var resultUpdate = await _permissionService.UpdatePermissionType(resultGetPermission.Permission, permissionType);
            if (!resultUpdate.IsSuccess)
                return BadRequest(resultUpdate.ErrorMessage);

            return NoContent();
        }

        [HttpDelete("id"), Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            var resultGet = await _permissionService.GetPermissionById(id);
            if (!resultGet.IsSuccess)
                return NotFound(resultGet.ErrorMessage);

            if (!await _permissionService.IsAdmin(new Guid(User.Identity!.Name!), resultGet.Permission!.SeasonId))
                return Forbid();

            // if delete himself, oldest moderator

            var resultDelete = await _permissionService.RemovePermission(resultGet.Permission);
            if (!resultDelete.IsSuccess)
                return BadRequest(resultDelete.ErrorMessage);

            return NoContent();
        }
    }
}
