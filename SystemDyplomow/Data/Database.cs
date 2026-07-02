using System.IO;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace SystemDyplomow.Data;

public static class Database
{
    private static readonly string _connectionString = LoadConnectionString();

    private static string LoadConnectionString()
    {
        // Szukaj dyplomy.cfg obok .exe
        string cfgPath = Path.Combine(AppContext.BaseDirectory, "dyplomy.cfg");

        if (File.Exists(cfgPath))
        {
            var cfg = JsonSerializer.Deserialize<DbConfig>(File.ReadAllText(cfgPath))
                      ?? throw new InvalidOperationException("Nieprawidłowy plik dyplomy.cfg");

            var builder = new SqlConnectionStringBuilder
            {
                DataSource         = cfg.Server,
                InitialCatalog     = cfg.Database,
                TrustServerCertificate = true
            };

            if (cfg.IntegratedSecurity)
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.UserID   = cfg.UserId;
                builder.Password = cfg.Password;
            }

            return builder.ConnectionString;
        }

        // Fallback – domyślne połączenie lokalne
        return @"Server=localhost\SQLEXPRESS;Database=system_dyplomow;Integrated Security=True;TrustServerCertificate=True;";
    }

    public static SqlConnection GetConnection() => new(_connectionString);

    private sealed class DbConfig
    {
        public string Server            { get; set; } = @"localhost\SQLEXPRESS";
        public string Database          { get; set; } = "system_dyplomow";
        public bool   IntegratedSecurity { get; set; } = true;
        public string UserId            { get; set; } = "";
        public string Password          { get; set; } = "";
    }
}
