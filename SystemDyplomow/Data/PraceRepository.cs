using Microsoft.Data.SqlClient;
using SystemDyplomow.Models;

namespace SystemDyplomow.Data;

public class PraceRepository
{
    public List<PracaDyplomowa> GetAll(string? statusFilter = null)
    {
        var list = new List<PracaDyplomowa>();
        using var conn = Database.GetConnection();
        conn.Open();

        // kolumny: 0=praca_id 1=tytul 2=plik_pracy 3=student_pesel 4=student_imie_nazwisko 5=student_email
        //          6=promotor_pesel 7=promotor_imie_nazwisko 8=promotor_email 9=status 10=data_zlozenia
        var sql = """
            SELECT praca_id, tytul, plik_pracy,
                   student_pesel, student_imie_nazwisko, student_email,
                   promotor_pesel, promotor_imie_nazwisko, promotor_email,
                   status, data_zlozenia
            FROM vw_prace
            """;
        if (!string.IsNullOrEmpty(statusFilter))
            sql += " WHERE status = @status";
        sql += " ORDER BY data_zlozenia DESC";

        using var cmd = new SqlCommand(sql, conn);
        if (!string.IsNullOrEmpty(statusFilter))
            cmd.Parameters.AddWithValue("@status", statusFilter);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new PracaDyplomowa
            {
                PracaId              = reader.GetInt32(0),
                Tytul                = reader.GetString(1),
                PlikPracy            = reader.IsDBNull(2) ? null : reader.GetString(2),
                StudentPesel         = reader.GetString(3),
                StudentImieNazwisko  = reader.GetString(4),
                StudentEmail         = reader.GetString(5),
                PromotorPesel        = reader.GetString(6),
                PromotorImieNazwisko = reader.GetString(7),
                PromotorEmail        = reader.GetString(8),
                Status               = reader.GetString(9),
                DataZlozenia         = reader.GetDateTime(10)
            });
        }
        return list;
    }

    public List<string> GetStatusy()
    {
        var list = new List<string> { "(wszystkie)" };
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("SELECT nazwa FROM statusy ORDER BY status_id", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(r.GetString(0));
        return list;
    }

    public List<Uzytkownik> GetUzytkownicy(string funkcja)
    {
        var list = new List<Uzytkownik>();
        using var conn = Database.GetConnection();
        conn.Open();
        var sql = """
            SELECT u.uzytkownik_id, u.imie, u.nazwisko, u.email, u.pesel, f.nazwa
            FROM uzytkownicy u
            JOIN funkcje f ON f.funkcja_id = u.funkcja
            WHERE f.nazwa = @fn
            ORDER BY u.nazwisko, u.imie
            """;
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@fn", funkcja);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new Uzytkownik
            {
                UzytkownikId = r.GetInt32(0),
                Imie         = r.GetString(1),
                Nazwisko     = r.GetString(2),
                Email        = r.GetString(3),
                Pesel        = r.GetString(4),
                Funkcja      = r.GetString(5)
            });
        return list;
    }

    public void DodajPrace(int studentId, int promotorId, string tytul, DateTime data, string? plikPracy = null)
    {
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("sp_dodaj_prace", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@student_id",    studentId);
        cmd.Parameters.AddWithValue("@promotor_id",   promotorId);
        cmd.Parameters.AddWithValue("@tytul",         tytul);
        cmd.Parameters.AddWithValue("@data_zlozenia", data);
        cmd.Parameters.AddWithValue("@plik_pracy",    (object?)plikPracy ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public void ZmienStatus(int pracaId, string nowyStatus)
    {
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("sp_zmien_status", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@praca_id",    pracaId);
        cmd.Parameters.AddWithValue("@nowy_status", nowyStatus);
        cmd.ExecuteNonQuery();
    }

    public List<PracaDyplomowa> GetDoObrony()
    {
        var list = new List<PracaDyplomowa>();
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            """
            SELECT praca_id, tytul,
                   student_imie_nazwisko, student_email,
                   promotor_imie_nazwisko, promotor_email,
                   data_zlozenia
            FROM vw_prace_do_obrony
            """, conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new PracaDyplomowa
            {
                PracaId              = r.GetInt32(0),
                Tytul                = r.GetString(1),
                StudentImieNazwisko  = r.GetString(2),
                StudentEmail         = r.GetString(3),
                PromotorImieNazwisko = r.GetString(4),
                PromotorEmail        = r.GetString(5),
                DataZlozenia         = r.GetDateTime(6)
            });
        }
        return list;
    }
}
