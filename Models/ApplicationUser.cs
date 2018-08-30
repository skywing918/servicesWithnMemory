using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class ApplicationUser: IdentityUser<int>
    {
        // Extended Properties

        public string UserId { get; set; }
        public string FullName { get; set; }

        public string Status { get; set; }
        public string Photo { get; set; }

        public DateTime Registered { get; set; }
    }
}
