using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace StoreVisitTracker.Infrastructure.Db
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Projenin bulunduğu dizin
                .AddJsonFile("appsettings.Development.json") // Konfigürasyonu yükle
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = configuration.GetConnectionString("mysql_connection");

            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}


/*
Yeni Migrations oluşturmak istediğimizde uygulama henüz başlamadığından Ef, AppDbContext'i dolduramaz ve Program.cs çalışmaz. 
Bu yüzden de "Unable to create a DbContext of type 'AppDbContext'" hatasını alırız. Bunu engellemenin bir yolu IDesignTimeDbContextFactory eklemektir.
*/