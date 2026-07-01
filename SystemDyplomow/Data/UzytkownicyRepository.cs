using Microsoft.Data.SqlClient;
using SystemDyplomow.Models;

namespace SystemDyplomow.Data;

public class UzytkownicyRepository
{
    public List<Uzytkownik> GetAll(string? funkcjaFilter = null)
    {
        var list = new List<Uzytkownik>();
        using var conn = Database.GetConnection();
        conn.Open();
        var sql = """
            SELECT u.uzytkownik_id, u.pesel, u.imie, u.nazwisko,
                   u.email, ISNULL(u.telefon, ''), f.nazwa
            FROM uzytkownicy u
            JOIN funkcje f ON f.funkcja_id = u.funkcja
            """;
        if (!string.IsNullOrEmpty(funkcjaFilter))
            sql += " WHERE f.nazwa = @fn";
        sql += " ORDER BY u.nazwisko, u.imie";

        using var cmd = new SqlCommand(sql, conn);
        if (!string.IsNullOrEmpty(funkcjaFilter))
            cmd.Parameters.AddWithValue("@fn", funkcjaFilter);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new Uzytkownik
            {
                UzytkownikId = r.GetInt32(0),
                Pesel        = r.GetString(1),
                Imie         = r.GetString(2),
                Nazwisko     = r.GetString(3),
                Email        = r.GetString(4),
                Telefon      = r.GetString(5),
                Funkcja      = r.GetString(6)
            });
        return list;
    }

    public List<string> GetFunkcje()
    {
        var list = new List<string>();
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("SELECT nazwa FROM funkcje ORDER BY funkcja_id", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(r.GetString(0));
        return list;
    }

    public void DodajUzytkownika(string pesel, string imie, string nazwisko,
                                  string email, string telefon, string funkcja)
    {
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("sp_dodaj_uzytkownika", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@pesel",    pesel);
        cmd.Parameters.AddWithValue("@imie",     imie);
        cmd.Parameters.AddWithValue("@nazwisko", nazwisko);
        cmd.Parameters.AddWithValue("@email",    email);
        cmd.Parameters.AddWithValue("@telefon",  telefon);
        cmd.Parameters.AddWithValue("@funkcja",  funkcja);
        cmd.ExecuteNonQuery();
    }

    public void UsunUzytkownika(int id)
    {
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "DELETE FROM uzytkownicy WHERE uzytkownik_id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    // ------------------------------------------------------------------
    // Przypisania student ↔ kierunek
    // ------------------------------------------------------------------

    public List<Kierunek> GetStudentKierunki(int studentId)
    {
        var list = new List<Kierunek>();
        using var conn = Database.GetConnection();
        conn.Open();
        var sql = """
            SELECT k.kierunek_id, k.nazwa, k.stopien, w.wydzial_id, w.nazwa
            FROM studenci_kierunki sk
            JOIN kierunki k ON k.kierunek_id = sk.kierunek_id
            JOIN wydzialy w ON w.wydzial_id  = k.wydzial
            WHERE sk.student_id = @id
            ORDER BY k.nazwa
            """;
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", studentId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new Kierunek
            {
                KierunekId   = r.GetInt32(0),
                Nazwa        = r.GetString(1),
                Stopien      = r.GetString(2),
                WydzialId    = r.GetInt32(3),
                WydzialNazwa = r.GetString(4)
            });
        return list;
    }

    public List<Kierunek> GetWszystkieKierunki()
    {
        var list = new List<Kierunek>();
        using var conn = Database.GetConnection();
        conn.Open();
        var sql = """
            SELECT k.kierunek_id, k.nazwa, k.stopien, w.wydzial_id, w.nazwa
            FROM kierunki k
            JOIN wydzialy w ON w.wydzial_id = k.wydzial
            ORDER BY k.nazwa
            """;
        using var cmd = new SqlCommand(sql, conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new Kierunek
            {
                KierunekId   = r.GetInt32(0),
                Nazwa        = r.GetString(1),
                Stopien      = r.GetString(2),
                WydzialId    = r.GetInt32(3),
                WydzialNazwa = r.GetString(4)
            });
        return list;
    }

    public void PrzypiszKierunek(int studentId, int kierunekId)
    {
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "INSERT INTO studenci_kierunki (student_id, kierunek_id) VALUES (@sid, @kid)", conn);
        cmd.Parameters.AddWithValue("@sid", studentId);
        cmd.Parameters.AddWithValue("@kid", kierunekId);
        cmd.ExecuteNonQuery();
    }

    public void UsunZKierunku(int studentId, int kierunekId)
    {
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "DELETE FROM studenci_kierunki WHERE student_id = @sid AND kierunek_id = @kid", conn);
        cmd.Parameters.AddWithValue("@sid", studentId);
        cmd.Parameters.AddWithValue("@kid", kierunekId);
        cmd.ExecuteNonQuery();
    }
}
