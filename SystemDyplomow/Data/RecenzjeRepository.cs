using Microsoft.Data.SqlClient;
using SystemDyplomow.Models;

namespace SystemDyplomow.Data;

public class RecenzjeRepository
{
    public List<Recenzja> GetAll()
    {
        var list = new List<Recenzja>();
        using var conn = Database.GetConnection();
        conn.Open();
        // 0=recenzja_id 1=praca_id 2=tytul 3=pesel 4=imie_nazwisko 5=email 6=tresc 7=ocena 8=data
        var sql = """
            SELECT r.recenzja_id, r.praca_id, p.tytul,
                   u.pesel,
                   u.imie + ' ' + u.nazwisko,
                   u.email,
                   r.tresc, r.ocena, r.data_wystawienia
            FROM recenzje r
            JOIN prace_dyplomowe p ON p.praca_id      = r.praca_id
            JOIN uzytkownicy u     ON u.uzytkownik_id  = r.recenzent_id
            ORDER BY r.data_wystawienia DESC
            """;
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Recenzja
            {
                RecenzjaId            = reader.GetInt32(0),
                PracaId               = reader.GetInt32(1),
                TytulPracy            = reader.GetString(2),
                RecenzentPesel        = reader.GetString(3),
                RecenzentImieNazwisko = reader.GetString(4),
                RecenzentEmail        = reader.GetString(5),
                Tresc                 = reader.IsDBNull(6) ? null : reader.GetString(6),
                Ocena                 = reader.GetDecimal(7),
                DataWystawienia       = reader.GetDateTime(8)
            });
        }
        return list;
    }

    public List<Uzytkownik> GetRecenzenci()
    {
        var list = new List<Uzytkownik>();
        using var conn = Database.GetConnection();
        conn.Open();
        var sql = """
            SELECT u.uzytkownik_id, u.imie, u.nazwisko, u.email, u.pesel, f.nazwa
            FROM uzytkownicy u
            JOIN funkcje f ON f.funkcja_id = u.funkcja
            WHERE f.nazwa = 'pracownik uczelni'
            ORDER BY u.nazwisko, u.imie
            """;
        using var cmd = new SqlCommand(sql, conn);
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

    public List<PracaDyplomowa> GetPrace()
    {
        var list = new List<PracaDyplomowa>();
        using var conn = Database.GetConnection();
        conn.Open();
        // Pobieramy też promotor_email i student_email — potrzebne do SprawdzDuplikat()
        var sql = """
            SELECT praca_id, tytul,
                   student_email, promotor_email
            FROM vw_prace
            ORDER BY tytul
            """;
        using var cmd = new SqlCommand(sql, conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new PracaDyplomowa
            {
                PracaId      = r.GetInt32(0),
                Tytul        = r.GetString(1),
                StudentEmail = r.GetString(2),
                PromotorEmail = r.GetString(3)
            });
        return list;
    }

    public void DodajRecenzje(int pracaId, int recenzentId, string? tresc, decimal ocena, DateTime data)
    {
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("sp_dodaj_recenzje", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@praca_id",         pracaId);
        cmd.Parameters.AddWithValue("@recenzent_id",     recenzentId);
        cmd.Parameters.AddWithValue("@tresc",            (object?)tresc ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ocena",            ocena);
        cmd.Parameters.AddWithValue("@data_wystawienia", data);
        cmd.ExecuteNonQuery();
    }
}
