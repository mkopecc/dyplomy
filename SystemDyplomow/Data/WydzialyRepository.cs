using Microsoft.Data.SqlClient;
using SystemDyplomow.Models;

namespace SystemDyplomow.Data;

public class WydzialyRepository
{
    public List<Wydzial> GetAll()
    {
        var list = new List<Wydzial>();
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("SELECT wydzial_id, wydzial_nazwa, dziekan_id, dziekan_imie_nazwisko, dziekan_email FROM vw_wydzialy ORDER BY wydzial_nazwa", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new Wydzial
            {
                WydzialId           = r.GetInt32(0),
                Nazwa               = r.GetString(1),
                DziekanId           = r.IsDBNull(2) ? null : r.GetInt32(2),
                DziekanImieNazwisko = r.IsDBNull(3) ? "(brak)" : r.GetString(3),
                DziekanEmail        = r.IsDBNull(4) ? null : r.GetString(4)
            });
        return list;
    }

    public List<Uzytkownik> GetPracownicy()
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

    public void DodajWydzial(string nazwa, int? dziekanId)
    {
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("sp_dodaj_wydzial", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@nazwa",      nazwa);
        cmd.Parameters.AddWithValue("@dziekan_id", (object?)dziekanId ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public void AktualizujWydzial(int id, string nazwa, int? dziekanId)
    {
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "UPDATE wydzialy SET nazwa = @nazwa, dziekan = @dziekan WHERE wydzial_id = @id", conn);
        cmd.Parameters.AddWithValue("@id",     id);
        cmd.Parameters.AddWithValue("@nazwa",  nazwa);
        cmd.Parameters.AddWithValue("@dziekan", (object?)dziekanId ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public void UsunWydzial(int id)
    {
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "DELETE FROM wydzialy WHERE wydzial_id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }
}
