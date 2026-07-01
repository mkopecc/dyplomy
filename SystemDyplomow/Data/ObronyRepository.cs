using Microsoft.Data.SqlClient;
using SystemDyplomow.Models;

namespace SystemDyplomow.Data;

public class ObronyRepository
{
    public List<Obrona> GetAll()
    {
        var list = new List<Obrona>();
        using var conn = Database.GetConnection();
        conn.Open();
        var sql = """
            SELECT o.obrona_id, o.praca_id, p.tytul, o.data_obrony, o.sala, o.wynik_koncowy
            FROM obrony o
            JOIN prace_dyplomowe p ON p.praca_id = o.praca_id
            ORDER BY o.data_obrony DESC
            """;
        using var cmd = new SqlCommand(sql, conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new Obrona
            {
                ObronaId     = r.GetInt32(0),
                PracaId      = r.GetInt32(1),
                TytulPracy   = r.GetString(2),
                DataObrony   = r.GetDateTime(3),
                Sala         = r.GetString(4),
                WynikKoncowy = r.GetDecimal(5)
            });
        }
        return list;
    }

    public void DodajObrone(int pracaId, DateTime data, string sala, decimal wynik)
    {
        using var conn = Database.GetConnection();
        conn.Open();
        var sql = "INSERT INTO obrony (praca_id, data_obrony, sala, wynik_koncowy) VALUES (@p, @d, @s, @w)";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@p", pracaId);
        cmd.Parameters.AddWithValue("@d", data);
        cmd.Parameters.AddWithValue("@s", sala);
        cmd.Parameters.AddWithValue("@w", wynik);
        cmd.ExecuteNonQuery();
    }

    public List<StatystykaPromotora> GetStatystyki()
    {
        var list = new List<StatystykaPromotora>();
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(
            "SELECT promotor_imie_nazwisko, promotor_email, liczba_prac, srednia_ocena FROM vw_statystyki_promotora ORDER BY liczba_prac DESC", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new StatystykaPromotora
            {
                PromotorImieNazwisko = r.GetString(0),
                PromotorEmail        = r.GetString(1),
                LiczbaPrac           = r.GetInt32(2),
                SredniaOcena         = r.IsDBNull(3) ? null : r.GetDecimal(3)
            });
        }
        return list;
    }
}
