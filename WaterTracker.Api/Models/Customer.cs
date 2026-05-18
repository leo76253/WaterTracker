using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WaterTracker.Api.Models
{
    //Opted to use IdentityUser as it provides a lot of the features we need out of box, such as password hashing.
    public class Customer : IdentityUser
    {
        //Note: there will most likely use a Full name which combines these.
        public string Forename { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        //Last login for security purposes but also useful for solving account issues.
        public DateTime LastLogin { get; set; }
        //Nice to have to see how long a user has been a 'member' for.
        public DateTime CreatedAt { get; set; }
        // Again, mostly for security purposes but also useful for solving account issues.
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}