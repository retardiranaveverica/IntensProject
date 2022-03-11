using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projekat.Models
{
    public class JobCandidateDatabaseSettings : IJobCandidateDatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string SkillCollectionName { get; set; }
    }

    public interface IJobCandidateDatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string SkillCollectionName { get; set; }
    }


}
