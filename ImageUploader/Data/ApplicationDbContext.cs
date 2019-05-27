using System;
using ImageUploader.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ImageUploader.Data
{
    /*Inherit from identity db context will give us methods propperties of identify
    DB context class. Making it easy to communicate with out DataBase. */
    public class ApplicationDbContext : IdentityDbContext
    {
        //Pass the options to the constructor of class. (Connection string that connects to database 
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        //Added as propperties to our application
        /* Now when we create our database using code first approach, we will see two tables created */
        public DbSet<Gallery> Galleries { get; set; }
        public DbSet<GalleryImage> GalleryImages { get; set; }
    }
}
