﻿using car_racing_tournament_api.DTO;
using car_racing_tournament_api.Interfaces;
using car_racing_tournament_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace car_racing_tournament_api.Controllers
{
    [Route("api/season")]
    [ApiController]
    public class SeasonController : Controller
    {
        private ISeason _seasonService;
        private IUserSeason _userSeasonService;
        private IUser _userService;

        public SeasonController(ISeason seasonService, IUserSeason userSeasonService, IUser userService)
        {
            _seasonService = seasonService;
            _userSeasonService = userSeasonService;
            _userService = userService;
        }

        [HttpGet, EnableQuery]
        public async Task<IActionResult> Get()
        {
            var result = await _seasonService.GetSeasons();
            if (!result.IsSuccess)
                return NotFound(result.ErrorMessage);

            return Ok(result.Seasons);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _seasonService.GetSeasonById(id);
            if (!result.IsSuccess)
                return NotFound(result.ErrorMessage);
            
            return Ok(result.Season);
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> Post([FromBody] SeasonCreateDto seasonDto)
        {
            var resultAdd = await _seasonService.AddSeason(seasonDto, new Guid(User.Identity!.Name!));
            if (!resultAdd.IsSuccess)
                return BadRequest(resultAdd.ErrorMessage);

            return StatusCode(StatusCodes.Status201Created, resultAdd.SeasonId);
        }

        [HttpPut("{id}"), Authorize]
        public async Task<IActionResult> Put(Guid id, [FromBody] SeasonUpdateDto seasonDto)
        {
            if (!await _userSeasonService.IsAdmin(new Guid(User.Identity!.Name!), id))
                return Forbid();

            var resultUpdate = await _seasonService.UpdateSeason(id, seasonDto);
            if (!resultUpdate.IsSuccess)
                return BadRequest(resultUpdate.ErrorMessage);
            
            return NoContent();
        }

        [HttpPut("{id}/archive"), Authorize]
        public async Task<IActionResult> Archive(Guid id)
        {
            if (!await _userSeasonService.IsAdmin(new Guid(User.Identity!.Name!), id))
                return Forbid();

            var resultArchive = await _seasonService.ArchiveSeason(id);
            if (!resultArchive.IsSuccess)
                return BadRequest(resultArchive.ErrorMessage);

            return NoContent();
        }

        [HttpDelete("{id}"), Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!await _userSeasonService.IsAdmin(new Guid(User.Identity!.Name!), id))
                return Forbid();

            var resultDelete = await _seasonService.DeleteSeason(id);
            if (!resultDelete.IsSuccess)
                return BadRequest(resultDelete.ErrorMessage);
            
            return NoContent();
        }

        [HttpGet("user"), Authorize]
        public async Task<IActionResult> GetByUserId()
        {
            /*var resultGetUserSeasons = await _userSeasonService.GetSeasonsByUserId(new Guid(User.Identity!.Name!));
            if (!resultGetUserSeasons.IsSuccess)
                return NotFound(resultGetUserSeasons.ErrorMessage);
            
            var resultGetSeasons = await _seasonService.GetSeasonsByUserSeasonList(resultGetUserSeasons.UserSeasons!.Select(x => x.SeasonId).ToList());
            if (!resultGetSeasons.IsSuccess)
                return NotFound(resultGetSeasons.ErrorMessage);*/

            var resultGetSeasons = await _seasonService.GetSeasonsByUserId(new Guid(User.Identity!.Name!));
            if (!resultGetSeasons.IsSuccess)
                return NotFound(resultGetSeasons.ErrorMessage);

            return Ok(resultGetSeasons.Seasons);
        }

        [HttpPost("{seasonId}/user-season"), Authorize]
        public async Task<IActionResult> Post(Guid seasonId, [FromForm] string usernameEmail)
        {
            if (!await _userSeasonService.IsAdmin(new Guid(User.Identity!.Name!), seasonId))
                return Forbid();

            var resultGet = await _userService.GetUserByUsernameEmail(usernameEmail);
            if (!resultGet.IsSuccess)
                return NotFound(resultGet.ErrorMessage);

            var resultAdd = await _userSeasonService.AddModerator(new Guid(User.Identity!.Name!), resultGet.User!.Id, seasonId);
            if (!resultAdd.IsSuccess)
                return BadRequest(resultAdd.ErrorMessage);

            return NoContent();
        }

        [HttpGet("{seasonId}/driver")]
        public async Task<IActionResult> GetDriversBySeasonId(Guid seasonId)
        {
            var resultGet = await _seasonService.GetDriversBySeasonId(seasonId);
            if (!resultGet.IsSuccess)
                return NotFound(resultGet.ErrorMessage);

            return Ok(resultGet.Drivers);
        }

        [HttpPost("{seasonId}/driver"), Authorize]
        public async Task<IActionResult> PostDriver(Guid seasonId, [FromBody] DriverDto driverDto)
        {
            if (!await _userSeasonService.IsAdminModerator(new Guid(User.Identity!.Name!), seasonId))
                return Forbid();

            var resultAdd = await _seasonService.AddDriver(seasonId, driverDto);
            if (!resultAdd.IsSuccess)
                return BadRequest(resultAdd.ErrorMessage);
            
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpGet("{seasonId}/team")]
        public async Task<IActionResult> GetTeamsBySeasonId(Guid seasonId)
        {
            var resultGet = await _seasonService.GetTeamsBySeasonId(seasonId);
            if (!resultGet.IsSuccess)
                return NotFound(resultGet.ErrorMessage);

            return Ok(resultGet.Teams);
        }

        [HttpPost("{seasonId}/team"), Authorize]
        public async Task<IActionResult> PostTeam(Guid seasonId, [FromBody] TeamDto team)
        {
            if (!await _userSeasonService.IsAdminModerator(new Guid(User.Identity!.Name!), seasonId))
                return Forbid();

            var resultAdd = await _seasonService.AddTeam(seasonId, team);
            if (!resultAdd.IsSuccess)
                return BadRequest(resultAdd.ErrorMessage);
            
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpGet("{seasonId}/race")]
        public async Task<IActionResult> GetRacesBySeasonId(Guid seasonId)
        {
            var resultGet = await _seasonService.GetRacesBySeasonId(seasonId);
            if (!resultGet.IsSuccess)
                return NotFound(resultGet.ErrorMessage);

            return Ok(resultGet.Races);
        }

        [HttpPost("{seasonId}/race"), Authorize]
        public async Task<IActionResult> PostRace(Guid seasonId, [FromBody] RaceDto raceDto)
        {
            if (!await _userSeasonService.IsAdminModerator(new Guid(User.Identity!.Name!), seasonId))
                return Forbid();
            
            var resultAdd = await _seasonService.AddRace(seasonId, raceDto);
            if (!resultAdd.IsSuccess)
                return BadRequest(resultAdd.ErrorMessage);

            return StatusCode(StatusCodes.Status201Created);
        }
    }
}
