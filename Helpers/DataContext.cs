using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using WebApi.Entities;
using WebApi.Models;

namespace WebApi.Helpers
{
    public class DataContext : IdentityDbContext<ApplicationUser, ApplicationRole,int>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }

        public DbSet<ApplicationRole> ApplicationRole { get; set; }

    }
}