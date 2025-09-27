using DocIntegrator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DocIntegrator.Infrastructure.Data;

public class DocIntegratorDbContext : DbContext
{
    public DocIntegratorDbContext(DbContextOptions<DocIntegratorDbContext> options)
        : base(options) { }

    public DbSet<Document> Documents => Set<Document>();
}
