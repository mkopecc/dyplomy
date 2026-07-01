using Microsoft.Data.SqlClient;
using SystemDyplomow.Models;

namespace SystemDyplomow.Data;

public class KierunkiRepository
{
    public List<Kierunek> GetAll()
    {
        var list = new List<Kierunek>();
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "SELECT kierunek_id, kierunek_nazwa, stopien, wydzial_id, wydzial_nazwa FROM vw_kierunki ORDER BY wydzial_nazwa, kierunek_nazwa", conn);
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

    public void DodajKierunek(string nazwa, int wydzialId, string stopien)
    {
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("sp_dodaj_kierunek", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@nazwa",      nazwa);
        cmd.Parameters.AddWithValue("@wydzial_id", wydzialId);
        cmd.Parameters.AddWithValue("@stopien",    stopien);
        cmd.ExecuteNonQuery();
    }

    public void AktualizujKierunek(int id, string nazwa, int wydzialId, string stopien)
    {
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "UPDATE kierunki SET nazwa = @nazwa, wydzial = @wydzial, stopien = @stopien WHERE kierunek_id = @id",
            conn);
        cmd.Parameters.AddWithValue("@id",      id);
        cmd.Parameters.AddWithValue("@nazwa",   nazwa);
        cmd.Parameters.AddWithValue("@wydzial", wydzialId);
        cmd.Parameters.AddWithValue("@stopien", stopien);
        cmd.ExecuteNonQuery();
    }

    public void UsunKierunek(int id)
    {
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "DELETE FROM kierunki WHERE kierunek_id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }
}
