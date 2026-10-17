using Microsoft.EntityFrameworkCore;

namespace Uvidik.Infrastructure.Persistence;

public sealed class UvidikDbContext(DbContextOptions<UvidikDbContext> options) : DbContext(options);
