using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FacebookAPI.Models
{
    public class FacebookAPIContext : DbContext
    {
        public FacebookAPIContext (DbContextOptions<FacebookAPIContext> options)
            : base(options)
        {
        }

        public DbSet<FacebookAPI.Models.Post> Post { get; set; }
    }
}
