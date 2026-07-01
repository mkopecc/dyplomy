using Microsoft.Data.SqlClient;

namespace SystemDyplomow.Data;

public static class Database
{
    private const string ConnectionString =
        @"Server=DESKTOP-39Q2RAV\SQLEXPRESS;Database=system_dyplomow;Integrated Security=True;TrustServerCertificate=True;";

    public static SqlConnection GetConnection() => new(ConnectionString);
}
