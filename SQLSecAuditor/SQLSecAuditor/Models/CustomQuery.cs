namespace SqlSecAuditor.Models
{
    public sealed class CustomQuery
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string Sql { get; set; } = string.Empty;
    }
}
