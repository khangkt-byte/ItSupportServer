using Microsoft.EntityFrameworkCore;
using ItSupportServer.Data;
using static ItSupportServer.src.Modules.Configurations.ModelNameEnum;

namespace ItSupportServer.Data.Seeds
{
    public static class ConfigurationSeeders
    {
        public static void SeedConfigurations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Configuration>().HasData(
                new Configuration
                {
                    Id = MODEL.Sizes.ToString(),
                },
                new Configuration
                {
                    Id = MODEL.Accept_Users.ToString(),
                }
                );
        }
    }
}
