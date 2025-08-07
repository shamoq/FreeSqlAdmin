namespace Simple.AdminApplication.SysMng.Entities
{
    public class SysJobLog : DefaultTenantEntity
    {
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public string Duration { get; set; }
        public DateTime? NextRun { get; set; }
    }
}