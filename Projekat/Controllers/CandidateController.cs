using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Projekat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Projekat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        
        public CandidateController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //Get all JobCandidates
        [HttpGet]
        public JsonResult Get()
        {
            MongoClient mongoClient = new MongoClient(_configuration.GetConnectionString("CandidateConn"));
            var dbList = mongoClient.GetDatabase("JobCandidateDatabase").GetCollection<Candidate>("Candidate").AsQueryable();
                        
            return new JsonResult(dbList);
        }

        //Get all JobCandidates with given name
        [HttpGet("{name}")]
        public JsonResult GetByName(string name)
        {
            MongoClient mongoClient = new MongoClient(_configuration.GetConnectionString("CandidateConn"));
            var dbList = mongoClient.GetDatabase("JobCandidateDatabase").GetCollection<Candidate>("Candidate").AsQueryable();
            List<Candidate> candidates = new List<Candidate>();

            foreach (var obj in dbList)
            {
                if (obj.FirstName == name)
                    candidates.Add(obj);
            }

            if (candidates.Count == 0)
                return new JsonResult("There is no candidate with given name");

            return new JsonResult(candidates);
        }

        //Adding new candidate
        [HttpPost]
        public JsonResult Post(int Id, string FirstName, string LastName, string Email, DateTime BirthDate, string PhoneNumber, string skill)
        {

            if (FirstName != null && LastName != null && Email != null && BirthDate <= DateTime.Now && PhoneNumber != null && skill != null)
            {
                Candidate candidate = new Candidate();
                candidate.skills = new List<Skill>();
                MongoClient mongoClient = new MongoClient(_configuration.GetConnectionString("CandidateConn"));
                var candidates = mongoClient.GetDatabase("JobCandidateDatabase").GetCollection<Candidate>("Candidate").AsQueryable();
                var skills = mongoClient.GetDatabase("JobCandidateDatabase").GetCollection<Skill>("Skill").AsQueryable();

                candidate.FirstName = FirstName;
                candidate.LastName = LastName;

                if (!IsValidEmail(Email))
                {
                    return new JsonResult("Email adress  is inccorect");
                }

                candidate.Email = Email;

                candidate.BirthDate = BirthDate;

                if (!CheckPhoneNum(PhoneNumber))
                {
                    return new JsonResult("Phone number is inccorect");
                }

                candidate.PhoneNumber = PhoneNumber;
               
                foreach (var obje in candidates)
                {
                    if (obje.Id == Id)
                        return new JsonResult("Already have candidate with given Id");
                }

                candidate.Id = Id;
             
                foreach(var obj in skills)
                {
                    if(obj.Name == skill)
                    {
                        candidate.skills.Add(obj);
                    }
                }

                if(candidate.skills.Count == 0)
                {
                    return new JsonResult("Skill not found");

                }

                mongoClient.GetDatabase("JobCandidateDatabase").GetCollection<Candidate>("Candidate").InsertOne(candidate);
                return new JsonResult("Added successfuly");
            }
            return new JsonResult("All fields are required");
        }

        //Updating (adding) skill to candidate
        [HttpPut("{id}")]
        public JsonResult PutSkillToCandidate(int id, string skill)
        {
            MongoClient mongoClient = new MongoClient(_configuration.GetConnectionString("CandidateConn"));
            var candidates = mongoClient.GetDatabase("JobCandidateDatabase").GetCollection<Candidate>("Candidate").AsQueryable();
            var skills = mongoClient.GetDatabase("JobCandidateDatabase").GetCollection<Skill>("Skill").AsQueryable();

            Candidate candidate = new Candidate();
            Skill skill1 = new Skill();


            foreach (var obj in candidates)
            {
                if (obj.Id == id)
                    candidate = obj;
            }

            foreach(var obj in skills)
            {
                if (obj.Name == skill)
                    skill1 = obj;
                
            }

            
            foreach(var obj in candidate.skills)
            {
                if (obj.Name == skill1.Name)
                    return new JsonResult("Candidate already have given skill");
            }

            var filter = Builders<Candidate>.Filter.Eq("Id", id);
            candidate.skills.Add(skill1);

            mongoClient.GetDatabase("JobCandidateDatabase").GetCollection<Candidate>("Candidate").ReplaceOne(filter, candidate);
            return new JsonResult("Successfuly update");
        }

        //Removing skill from candidate
        [HttpPut]
        public JsonResult RemoveSkillFromCandidate(int id, string skill)
        {
            MongoClient mongoClient = new MongoClient(_configuration.GetConnectionString("CandidateConn"));
            var candidates = mongoClient.GetDatabase("JobCandidateDatabase").GetCollection<Candidate>("Candidate").AsQueryable();
            Candidate candidate = new Candidate();
            Skill skill1 = new Skill();

            foreach (var obj in candidates)
            {
                if (obj.Id == id)
                    candidate = obj;
            }

            foreach (var obj in candidate.skills)
            {
                if (obj.Name == skill)
                    skill1 = obj;
            }

            var filter = Builders<Candidate>.Filter.Eq("Id", id);

            candidate.skills.Remove(skill1);

            mongoClient.GetDatabase("JobCandidateDatabase").GetCollection<Candidate>("Candidate").ReplaceOne(filter, candidate);
            return new JsonResult("Successfuly update");

        }

        //Deleting candidate
        [HttpDelete]
        public JsonResult Delete(int id)
        {
            Candidate candidate = new Candidate();
            MongoClient mongoClient = new MongoClient(_configuration.GetConnectionString("CandidateConn"));
            var can = mongoClient.GetDatabase("JobCandidateDatabase").GetCollection<Candidate>("Candidate").AsQueryable();

            foreach(var obj in can)
            {
                if (obj.Id == id)
                    candidate = obj;
            }

            if(candidate == null)
                return new JsonResult("Not found candidate with given Id");

            var filter = Builders<Candidate>.Filter.Eq("Id", id);
            mongoClient.GetDatabase("JobCandidateDatabase").GetCollection<Candidate>("Candidate").DeleteOneAsync(filter);

            return new JsonResult("Deleted successfuly");
        }

        #region privateFunctions
        private bool CheckPhoneNum(string number)
        { 
            if (number.All(char.IsDigit))
            {
                return true;
            }
            else
                return false;

        }

        private bool IsValidEmail(string email)
        {
            string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|" + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)" + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }
        #endregion

    }
}
