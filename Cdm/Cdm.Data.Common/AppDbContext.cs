namespace Cdm.Data.Common;

using System;
using System.Collections.Generic;
using System.Text;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
