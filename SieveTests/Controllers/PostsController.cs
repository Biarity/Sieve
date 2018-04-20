using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using SieveTests.Entities;

namespace SieveTests.Controllers
{
    [Route("api/[controller]/[action]")]
    public class PostsController : Controller
    {
        private ISieveProcessor _sieveProcessor;
        private ApplicationDbContext _dbContext;

        public PostsController(ISieveProcessor sieveProcessor,
            ApplicationDbContext dbContext)
        {
            _sieveProcessor = sieveProcessor;
            _dbContext = dbContext;
        }

        [HttpGet]
        public JsonResult GetAllWithSieve(SieveModel sieveModel)
        {
            var result = _dbContext.Posts.AsNoTracking();

            result = _sieveProcessor.Apply(sieveModel, result);

            return Json(result.ToList());
        }

        [HttpGet]
        public JsonResult Create(int number = 10)
        {
            for (int i = 0; i < number; i++)
            {
                _dbContext.Posts.Add(new Post());
            }

            _dbContext.SaveChanges();

            return Json(_dbContext.Posts.ToList());
        }

        [HttpGet]
        public JsonResult GetAll()
        {
            return Json(_dbContext.Posts.ToList());
        }
    }
}
