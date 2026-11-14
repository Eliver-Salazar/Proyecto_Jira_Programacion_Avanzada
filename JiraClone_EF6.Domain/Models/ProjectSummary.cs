using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JiraClone_EF6.Domain.Models
{
    //Transporta los datos básicos del proyecto sin depender de Entity Framework.
    public sealed class ProjectSummary
    {
        public int ProjectId { get; set; }
        public string ProjectKey { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
