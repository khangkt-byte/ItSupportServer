using ItSupportServer.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ItSupportServer.Data.Seeds
{
    /// <summary>
    /// Device and device type seeders
    /// Pattern: IT asset inventory seed data
    /// Use: Pre-populate common device types and sample devices
    /// Reference: ITIL Asset Management, ServiceNow CMDB
    /// </summary>
    public static class DeviceSeeders
    {
        private static readonly DateTime SeedDate = new(2025, 10, 30, 9, 38, 50, DateTimeKind.Utc);

        public static void SeedDevices(ModelBuilder modelBuilder)
        {
            SeedDeviceTypes(modelBuilder);
            SeedSampleDevices(modelBuilder);
        }

        /// <summary>
        /// Seed common IT device types
        /// Pattern: ITIL Configuration Item Types
        /// </summary>
        private static void SeedDeviceTypes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DeviceTypes>().HasData(
                new DeviceTypes
                {
                    DeviceTypeId = 1,
                    Name = "Máy tính để bàn",
                    Description = "Desktop computer, PC, workstation",
                    CreatedAt = SeedDate
                },
                new DeviceTypes
                {
                    DeviceTypeId = 2,
                    Name = "Laptop",
                    Description = "Máy tính xách tay",
                    CreatedAt = SeedDate
                },
                new DeviceTypes
                {
                    DeviceTypeId = 3,
                    Name = "Máy in",
                    Description = "Printer, máy in văn phòng",
                    CreatedAt = SeedDate
                },
                new DeviceTypes
                {
                    DeviceTypeId = 4,
                    Name = "Máy photocopy",
                    Description = "Máy photocopy, scan đa chức năng",
                    CreatedAt = SeedDate
                },
                new DeviceTypes
                {
                    DeviceTypeId = 5,
                    Name = "Màn hình",
                    Description = "Monitor, màn hình máy tính",
                    CreatedAt = SeedDate
                },
                new DeviceTypes
                {
                    DeviceTypeId = 6,
                    Name = "Switch mạng",
                    Description = "Network switch, thiết bị chuyển mạch",
                    CreatedAt = SeedDate
                },
                new DeviceTypes
                {
                    DeviceTypeId = 7,
                    Name = "Router",
                    Description = "Bộ định tuyến, router wifi",
                    CreatedAt = SeedDate
                },
                new DeviceTypes
                {
                    DeviceTypeId = 8,
                    Name = "Server",
                    Description = "Máy chủ, server vật lý",
                    CreatedAt = SeedDate
                },
                new DeviceTypes
                {
                    DeviceTypeId = 9,
                    Name = "UPS",
                    Description = "Bộ lưu điện, uninterruptible power supply",
                    CreatedAt = SeedDate
                },
                new DeviceTypes
                {
                    DeviceTypeId = 10,
                    Name = "Điện thoại IP",
                    Description = "IP Phone, điện thoại nội bộ",
                    CreatedAt = SeedDate
                }
            );
        }

        /// <summary>
        /// Seed sample devices for testing/demo
        /// Pattern: Realistic IT inventory
        /// </summary>
        private static void SeedSampleDevices(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Devices>().HasData(
                // Desktop computers
                new Devices
                {
                    DeviceId = 1,
                    DeviceTypeId = 1,
                    Name = "PC-IT-001",
                    Brand = "Dell",
                    Model = "OptiPlex 7090",
                    SerialNumber = "DELL-PC-001-2024",
                    Notes = "Máy trưởng phòng CNTT",
                    CreatedAt = SeedDate
                },
                new Devices
                {
                    DeviceId = 2,
                    DeviceTypeId = 1,
                    Name = "PC-KD-001",
                    Brand = "HP",
                    Model = "EliteDesk 800 G8",
                    SerialNumber = "HP-PC-001-2024",
                    Notes = "Máy phòng kinh doanh",
                    CreatedAt = SeedDate
                },

                // Laptops
                new Devices
                {
                    DeviceId = 3,
                    DeviceTypeId = 2,
                    Name = "LT-IT-001",
                    Brand = "Lenovo",
                    Model = "ThinkPad X1 Carbon",
                    SerialNumber = "LN-LT-001-2024",
                    Notes = "Laptop IT Support",
                    CreatedAt = SeedDate
                },
                new Devices
                {
                    DeviceId = 4,
                    DeviceTypeId = 2,
                    Name = "LT-KD-001",
                    Brand = "Dell",
                    Model = "Latitude 5420",
                    SerialNumber = "DELL-LT-001-2024",
                    Notes = "Laptop nhân viên kinh doanh",
                    CreatedAt = SeedDate
                },

                // Printers
                new Devices
                {
                    DeviceId = 5,
                    DeviceTypeId = 3,
                    Name = "PRINTER-T1",
                    Brand = "HP",
                    Model = "LaserJet Pro MFP M428fdw",
                    SerialNumber = "HP-PR-001-2024",
                    Notes = "Máy in tầng 1",
                    CreatedAt = SeedDate
                },
                new Devices
                {
                    DeviceId = 6,
                    DeviceTypeId = 3,
                    Name = "PRINTER-T3",
                    Brand = "Canon",
                    Model = "imageRUNNER 2425",
                    SerialNumber = "CN-PR-001-2024",
                    Notes = "Máy in phòng CNTT tầng 3",
                    CreatedAt = SeedDate
                },

                // Network equipment
                new Devices
                {
                    DeviceId = 7,
                    DeviceTypeId = 6,
                    Name = "SW-CORE-01",
                    Brand = "Cisco",
                    Model = "Catalyst 2960X",
                    SerialNumber = "CISCO-SW-001-2024",
                    Notes = "Core switch tầng 3 - Server room",
                    CreatedAt = SeedDate
                },
                new Devices
                {
                    DeviceId = 8,
                    DeviceTypeId = 7,
                    Name = "ROUTER-MAIN",
                    Brand = "Cisco",
                    Model = "ISR 4331",
                    SerialNumber = "CISCO-RT-001-2024",
                    Notes = "Router chính kết nối internet",
                    CreatedAt = SeedDate
                },

                // Server
                new Devices
                {
                    DeviceId = 9,
                    DeviceTypeId = 8,
                    Name = "SRV-APP-01",
                    Brand = "Dell",
                    Model = "PowerEdge R750",
                    SerialNumber = "DELL-SRV-001-2024",
                    Notes = "Application server - Production",
                    CreatedAt = SeedDate
                },
                new Devices
                {
                    DeviceId = 10,
                    DeviceTypeId = 8,
                    Name = "SRV-DB-01",
                    Brand = "HP",
                    Model = "ProLiant DL380 Gen10",
                    SerialNumber = "HP-SRV-001-2024",
                    Notes = "Database server - PostgreSQL",
                    CreatedAt = SeedDate
                }
            );
        }
    }
}