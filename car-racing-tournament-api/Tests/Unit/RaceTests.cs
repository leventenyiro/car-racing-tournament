﻿using AutoMapper;
using car_racing_tournament_api.Data;
using car_racing_tournament_api.DTO;
using car_racing_tournament_api.Models;
using car_racing_tournament_api.Profiles;
using car_racing_tournament_api.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace car_racing_tournament_api.Tests.Unit
{
    [TestFixture]
    public class RaceTests
    {
        private CarRacingTournamentDbContext? _context;
        private RaceService? _raceService;
        private Guid _raceId;
        
        [SetUp]
        public void Init()
        {
            var options = new DbContextOptionsBuilder<CarRacingTournamentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new CarRacingTournamentDbContext(options);

            _raceId = Guid.NewGuid();

            Team team = new Team
            {
                Id = Guid.NewGuid(),
                Name = "First team",
                Color = "123123"
            };

            _context.Races.Add(new Race
            {
                Id = _raceId,
                Name = "My first race",
                DateTime = new DateTime(2023, 1, 1, 18, 0, 0),
                Results = new List<Result>
                {
                    new Result
                    {
                        Id = Guid.NewGuid(),
                        Driver = new Driver
                        {
                            Id = Guid.NewGuid(),
                            Name = "FirstDriver",
                            RealName = "First Driver",
                            Number = 1,
                            ActualTeam = team
                        },
                        Points = 25,
                        Position = 1,
                        Team = team
                    }
                },
                Season = new Season
                {
                    Id = Guid.NewGuid(),
                    Name = "First season",
                    IsArchived = false,
                    Drivers = new List<Driver>
                    {
                        new Driver
                        {
                            Id = Guid.NewGuid(),
                            Name = "SecondDriver",
                            Number = 2,
                            ActualTeam = team
                        }
                    },
                    Teams = new List<Team> { team }
                }
            });
            _context.SaveChanges();

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutoMapperConfig());
            });
            var mapper = mockMapper.CreateMapper();

            _raceService = new RaceService(_context, mapper);
        }

        [Test]
        public async Task GetRaceByIdSuccess()
        {
            var result = await _raceService!.GetRaceById(_raceId);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.ErrorMessage);

            Race race = result.Race!;
            Assert.AreEqual(race.Id, _context!.Races.First().Id);
            Assert.AreEqual(race.Name, _context!.Races.First().Name);
            Assert.AreEqual(race.DateTime, _context!.Races.First().DateTime);
        }

        [Test]
        public async Task GetRaceByIdNotFound()
        {
            var result = await _raceService!.GetRaceById(Guid.NewGuid());
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotEmpty(result.ErrorMessage);
            Assert.IsNull(result.Race);
        }

        [Test]
        public async Task UpdateRaceSuccess()
        {
            var race = new RaceDto
            {
                Name = "test tournament",
                DateTime = new DateTime(2022, 12, 12, 12, 0, 0)
            };

            var result = await _raceService!.UpdateRace(_raceId, race);
            Assert.IsTrue(result.IsSuccess);

            var findRace = _context!.Races.FirstAsync().Result;
            Assert.AreEqual(findRace.Name, race.Name);
            Assert.AreEqual(findRace.DateTime, race.DateTime);
        }

        [Test]
        public async Task UpdateRaceMissingName()
        {
            var race = new RaceDto
            {
                Name = "",
                DateTime = new DateTime(2022, 12, 12, 12, 0, 0)
            };

            var result = await _raceService!.UpdateRace(_raceId, race);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotEmpty(result.ErrorMessage);
        }

        [Test]
        public async Task DeleteRaceSuccess()
        {
            Assert.IsNotEmpty(_context!.Races);

            var result = await _raceService!.DeleteRace(_raceId);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.ErrorMessage);

            Assert.IsEmpty(_context.Races);
        }

        [Test]
        public async Task DeleteRaceWrongId()
        {
            var result = await _raceService!.DeleteRace(Guid.NewGuid());
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotEmpty(result.ErrorMessage);
        }

        [Test]
        public async Task GetResultsByRaceIdSuccess()
        {
            var result = await _raceService!.GetResultsByRaceId(_raceId);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.ErrorMessage);
            Assert.IsNotNull(result.Results);
            Assert.AreEqual(result.Results!.Count, 1);
        }

        [Test]
        public async Task GetResultsByRaceIdWrongId()
        {
            var result = await _raceService!.GetResultsByRaceId(Guid.NewGuid());
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotEmpty(result.ErrorMessage);
            Assert.IsNull(result.Results);
        }

        [Test]
        public async Task AddResultSuccess()
        {
            var driver = _context!.Drivers.Where(x => x.Number == 2).FirstOrDefaultAsync().Result;
            var resultDto = new ResultDto
            {
                Points = 18,
                Position = 2,
                DriverId = driver!.Id,
                TeamId = (Guid)driver.ActualTeamId!
            };

            var result = await _raceService!.AddResult(_raceId, resultDto);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual(_context!.Results.ToListAsync().Result.Count, 2);
        }

        [Test]
        public async Task AddResultMissingIds()
        {
            var driver = _context!.Drivers.Where(x => x.Number == 2).FirstOrDefaultAsync().Result;
            var resultDto = new ResultDto
            {
                Points = 18,
                Position = 2,
                DriverId = Guid.Empty,
                TeamId = (Guid)driver!.ActualTeamId!
            };

            var result = await _raceService!.AddResult(_raceId, resultDto);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotEmpty(result.ErrorMessage);
            Assert.AreEqual(_context!.Results.ToListAsync().Result.Count, 1);

            resultDto.DriverId = driver!.Id;
            resultDto.TeamId = Guid.Empty;
            result = await _raceService!.AddResult(_raceId, resultDto);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotEmpty(result.ErrorMessage);
            Assert.AreEqual(_context!.Results.ToListAsync().Result.Count, 1);
        }

        [Test]
        public async Task AddResultNotPositivePosition()
        {
            var driver = _context!.Drivers.Where(x => x.Number == 2).FirstOrDefaultAsync().Result;
            var resultDto = new ResultDto
            {
                Points = 18,
                Position = -1,
                DriverId = driver!.Id,
                TeamId = (Guid)driver.ActualTeamId!
            };

            var result = await _raceService!.AddResult(_raceId, resultDto);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotEmpty(result.ErrorMessage);
            Assert.AreEqual(_context!.Results.ToListAsync().Result.Count, 1);

            resultDto.Position = 0;
            result = await _raceService!.AddResult(_raceId, resultDto);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotEmpty(result.ErrorMessage);
            Assert.AreEqual(_context!.Results.ToListAsync().Result.Count, 1);
        }

        [Test]
        public async Task AddResultMinusPoints()
        {
            var driver = _context!.Drivers.Where(x => x.Number == 2).FirstOrDefaultAsync().Result;
            var resultDto = new ResultDto
            {
                Points = -2,
                Position = 2,
                DriverId = driver!.Id,
                TeamId = (Guid)driver.ActualTeamId!
            };
            
            var result = await _raceService!.AddResult(_raceId, resultDto);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotEmpty(result.ErrorMessage);
            Assert.AreEqual(_context!.Results.ToListAsync().Result.Count, 1);
        }

        /*[Test] ADD RESULT WITH ANOTHER SEASON ID
        public async Task AddDriverWithAnotherSeasonTeam()
        {
            var anotherSeasonId = Guid.NewGuid();
            _context!.Seasons.Add(new Season
            {
                Id = anotherSeasonId,
                Name = "Test Season",
                Description = "This is our test season",
                IsArchived = false,
                UserSeasons = new List<UserSeason>()
                {
                    new UserSeason
                    {
                        Id = Guid.NewGuid(),
                        UserId = _userId,
                        Permission = UserSeasonPermission.Admin
                    }
                },
                Teams = new List<Team>()
                {
                    new Team
                    {
                        Id = Guid.NewGuid(),
                        Name = "Test Team",
                        Color = "FF0000",
                        Drivers = new List<Driver>(),
                        Results = new List<Result>(),
                        SeasonId = anotherSeasonId
                    }
                },
                Drivers = new List<Driver>(),
                Races = new List<Race>()
            });
            _context.SaveChanges();

            var driverDto = new DriverDto
            {
                Name = "AddDriver1",
                RealName = "Add Driver",
                Number = 2,
                ActualTeamId = _context.Seasons
                    .Where(x => x.Id == anotherSeasonId)
                    .FirstOrDefaultAsync().Result!.Teams!
                    .FirstOrDefault()!.Id
            };

            var result = await _seasonService!.AddDriver(_seasonId, driverDto);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotEmpty(result.ErrorMessage);
        }*/

        //[Test]
        //public async Task AddResultResultExists() // a driver cannot reach more result on a race
    }
}