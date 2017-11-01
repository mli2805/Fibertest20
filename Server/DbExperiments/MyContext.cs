﻿using System;
using System.Data.Entity;

namespace DbExperiments
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsEmailActivated { get; set; }
        public int Role { get; set; }
    }

    public class Measurement
    {
        public int Id { get; set; }
        public byte[] Meas { get; set; }
    }

    public class MonitoringResult
    {
        public int Id { get; set; }
        public Guid RtuId { get; set; }
        public Guid TraceId { get; set; }

        public MoniResultBlob Data { get; set; }
    }

    public class MoniResultBlob
    {
        public bool IsFailed { get; set; }

        public double DistanceToFirstBreak { get; set; }

        // and so on...

        public byte[] SorBytes { get; set; }
    }

    public class MyContext : DbContext
    {
        public MyContext() : base("mydb") { }
        public DbSet<User> Users { get; set; }
        public DbSet<Measurement> Measurements { get; set; }
        public DbSet<MonitoringResult> MonitoringResults { get; set; }
    }
}
