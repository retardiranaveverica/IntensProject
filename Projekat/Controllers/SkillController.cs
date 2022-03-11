using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Projekat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projekat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public SkillController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public JsonResult Get()
        {
            MongoClient mongoClient = new MongoClient(_configuration.GetConnectionString("CandidateConn"));
            var dbList = mongoClient.GetDatabase("JobCandidateDatabase").GetCollection<Skill>("Skill").AsQueryable();

            return new JsonResult(dbList);
        }

        [HttpPost]
        public JsonResult Post(string name)
        {
            Skill skill = new Skill();
            if(name == null)
            {
                return new JsonResult("Field is required");
            }
            MongoClient mongoClient = new MongoClient(_configuration.GetConnectionString("CandidateConn"));
            
            var skills = mongoClient.GetDatabase("JobCandidateDatabase").GetCollection<Skill>("Skill").AsQueryable();

            foreach (var obj in skills)
                if (obj.Name == name)
                    return new JsonResult("Skill already exist");

            skill.Name = name;
            mongoClient.GetDatabase("JobCandidateDatabase").GetCollection<Skill>("Skill").InsertOne(skill);
             return new JsonResult("Added successfuly");
        }
    }
}
