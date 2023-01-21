﻿using car_racing_tournament_api.DTO;
using car_racing_tournament_api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace car_racing_tournament_api.Controllers
{
    [Route("api/team")]
    [ApiController]
    public class TeamController : Controller
    {
        private ITeam _teamService;
        private IUserSeason _userSeasonService;

        public TeamController(ITeam teamService, IUserSeason userSeasonService)
        {
            _teamService = teamService;
            _userSeasonService = userSeasonService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var resultGet = await _teamService.GetTeamById(id);
            if (!resultGet.IsSuccess)
                return NotFound(resultGet.ErrorMessage);

            return Ok(resultGet.Team);
        }

        [HttpPut("{id}"), Authorize]
        public async Task<IActionResult> Put(Guid id, [FromBody] TeamDto teamDto)
        {
            if (User.Identity?.Name == null)
                return Unauthorized();

            var resultGet = await _teamService.GetTeamById(id);
            if (!resultGet.IsSuccess)
                return NotFound(resultGet.ErrorMessage);

            if (!_userSeasonService.HasPermission(new Guid(User.Identity.Name), resultGet.Team.SeasonId))
                return Forbid();

            var resultUpdate = await _teamService.UpdateTeam(id, teamDto);
            if (!resultUpdate.IsSuccess)
                return BadRequest(resultUpdate.ErrorMessage);

            return NoContent();
        }

        [HttpDelete("{id}"), Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (User.Identity?.Name == null)
                return Unauthorized();

            var resultGet = await _teamService.GetTeamById(id);
            if (!resultGet.IsSuccess)
                return NotFound(resultGet.ErrorMessage);

            if (!_userSeasonService.HasPermission(new Guid(User.Identity.Name), resultGet.Team.SeasonId))
                return Forbid();

            var resultDelete = await _teamService.DeleteTeam(id);
            if (!resultDelete.IsSuccess)
                return BadRequest(resultDelete.ErrorMessage);

            return NoContent();
        }
    }
}
