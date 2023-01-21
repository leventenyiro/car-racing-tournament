﻿using AutoMapper;
using car_racing_tournament_api.Data;
using car_racing_tournament_api.DTO;
using car_racing_tournament_api.Interfaces;
using car_racing_tournament_api.Models;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace car_racing_tournament_api.Services
{
    public class SeasonService : ISeason
    {
        private readonly FormulaDbContext _formulaDbContext;
        private IMapper _mapper;

        public SeasonService(FormulaDbContext formulaDbContext, IMapper mapper)
        {
            _formulaDbContext = formulaDbContext;
            _mapper = mapper;
        }

        public async Task<(bool IsSuccess, List<SeasonDto> Seasons, string ErrorMessage)> GetSeasons()
        {
            List<SeasonDto> seasons = _formulaDbContext.Seasons.Include(x => x.UserSeasons).Select(x => new SeasonDto
            {
                Id = x.Id,
                Name = x.Name,
                UserSeasons = x.UserSeasons.Select(x => new UserSeasonDto
                {
                    Username = x.User.Username,
                    Permission = x.Permission
                }).ToList()
            }).ToList();

            if (seasons != null)
            {
                return (true, seasons, null);
            }
            return (false, null, "No seasons found");
        }

        public async Task<(bool IsSuccess, SeasonDto Season, string ErrorMessage)> GetSeasonById(Guid id)
        {
            SeasonDto season = _formulaDbContext.Seasons
                .Include(x => x.UserSeasons)
                .Where(x => x.Id == id)
                .Select(x => new SeasonDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    UserSeasons = x.UserSeasons.Select(x => new UserSeasonDto
                    {
                        Username = x.User.Username,
                        Permission = x.Permission
                    }).ToList()
                }).First();
            if (season != null)
            {
                return (true, season, null);
            }
            return (false, null, "Season not found");
        }

        public async Task<(bool IsSuccess, Guid SeasonId, string ErrorMessage)> AddSeason(string name, Guid userId)
        {
            if (!string.IsNullOrEmpty(name))
            {
                UserSeason userSeason = new UserSeason { Id = new Guid(), Permission = UserSeasonPermission.Admin, UserId = userId };
                List<UserSeason> userSeasons = new List<UserSeason> { userSeason }; 

                var seasonObj = new Season { Id = Guid.NewGuid(), Name = name, UserSeasons = userSeasons };
                _formulaDbContext.Seasons.Add(seasonObj);
                _formulaDbContext.SaveChanges();
                return (true, seasonObj.Id, null);
            }
            return (false, Guid.Empty, "Please provide the season data");
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> UpdateSeason(Guid id, string name)
        {
            var seasonObj = _formulaDbContext.Seasons.Where(e => e.Id == id).FirstOrDefault();
            if (seasonObj != null)
            {
                seasonObj.Name = name;
                _formulaDbContext.Seasons.Update(seasonObj);
                _formulaDbContext.SaveChanges();
                return (true, null);
            }
            return (false, "Season not found");
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> DeleteSeason(Guid id)
        {
            var season = _formulaDbContext.Seasons.Where(e => e.Id == id).FirstOrDefault();
            if (season != null)
            {
                _formulaDbContext.Seasons.Remove(season);
                _formulaDbContext.SaveChanges();
                return (true, null);
            }
            return (false, "Season not found");
        }

        public async Task<(bool IsSuccess, List<SeasonDto> Seasons, string ErrorMessage)> GetSeasonsByUserSeasonList(List<Guid> userSeasons)
        {
            List<SeasonDto> seasons = _formulaDbContext.Seasons
                .Include(x => x.UserSeasons)
                .Where(x => userSeasons.Contains(x.Id))
                .Select(x => new SeasonDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    UserSeasons = x.UserSeasons.Select(x => new UserSeasonDto
                    {
                        Username = x.User.Username,
                        Permission = x.Permission
                    }).ToList()
                }).ToList();
            if (seasons != null)
            {
                return (true, seasons, null);
            }
            return (false, null, "No seasons found");
        }

        public async Task<(bool IsSuccess, List<Driver> Drivers, string ErrorMessage)> GetDriversBySeasonId(Guid seasonId)
        {
            var drivers = _formulaDbContext.Drivers.Where(x => x.SeasonId == seasonId).ToList();
            if (drivers != null)
            {
                return (true, drivers, null);
            }
            return (false, null, "No drivers found");
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> AddDriver(Guid seasonId, DriverDto driverDto)
        {
            if (driverDto != null)
            {
                var driver = _mapper.Map<Driver>(driverDto);
                driver.Id = Guid.NewGuid();
                _formulaDbContext.Add(driver);
                _formulaDbContext.SaveChanges();
                return (true, null);
            }
            return (false, "Please provide the driver data");
        }

        public async Task<(bool IsSuccess, List<Team> Teams, string ErrorMessage)> GetTeamsBySeasonId(Guid seasonId)
        {
            var teams = _formulaDbContext.Teams.Where(x => x.SeasonId == seasonId).ToList();
            if (teams != null)
            {
                return (true, teams, null);
            }
            return (false, null, "No teams found");
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> AddTeam(Guid seasonId, TeamDto team)
        {
            if (team != null)
            {
                Team teamObj = new Team
                {
                    Id = Guid.NewGuid(),
                    Name = team.Name,
                    Season = _formulaDbContext.Seasons.Where(e => e.Id == seasonId).First(),
                };
                try
                {
                    ColorTranslator.FromHtml(team.Color);
                    teamObj.Color = team.Color;
                }
                catch (Exception)
                {
                    return (false, "Incorrect color code");
                }
                _formulaDbContext.Add(teamObj);
                _formulaDbContext.SaveChanges();
                return (true, null);
            }
            return (false, "Please provide the team data");
        }

        public async Task<(bool IsSuccess, List<Race> Races, string ErrorMessage)> GetRacesBySeasonId(Guid seasonId)
        {
            var races = _formulaDbContext.Races.Where(x => x.SeasonId == seasonId).ToList();
            if (races != null)
            {
                return (true, races, null);
            }
            return (false, null, "No races found");
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> AddRace(Guid seasonId, RaceDto raceDto)
        {
            if (raceDto != null)
            {
                var race = _mapper.Map<Race>(raceDto);
                race.Id = Guid.NewGuid();
                _formulaDbContext.Add(race);
                _formulaDbContext.SaveChanges();
                return (true, null);
            }
            return (false, "Please provide the race data");
        }
    }
}