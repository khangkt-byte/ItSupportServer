using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.Data.Seeds
{
    public static class DataSeeders
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            UserSeeders.SeedUsers(modelBuilder);
            PermissionSeeders.SeedPermissions(modelBuilder);
            ConfigurationSeeders.SeedConfigurations(modelBuilder);
        }
    }
}
