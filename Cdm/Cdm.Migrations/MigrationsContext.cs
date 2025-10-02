namespace Cdm.Migrations;

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Cdm.Data.Common.Models;

public class MigrationsContext : DbContext
{
    public MigrationsContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<ACharacter> ACharacter { get; set; }
    public DbSet<User> Users { get; set; }
}
