namespace SqlSecAuditor.Models
{
    // 1. Klasa reprezentująca dane pojedynczego SQL Servera
    public class SqlInstance
    {
        public string ServerName { get; set; }
        public string GeneralInfo { get; set; }
        public string PermissionsInfo { get; set; }
    }
}