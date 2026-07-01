namespace SystemDyplomow.Models;

public class Uzytkownik
{
    public int    UzytkownikId { get; set; }
    public string Pesel        { get; set; } = "";
    public string Imie         { get; set; } = "";
    public string Nazwisko     { get; set; } = "";
    public string Email        { get; set; } = "";
    public string? Telefon     { get; set; }
    public string Funkcja      { get; set; } = "";

    public string PelneNazwisko => $"{Imie} {Nazwisko}";
}

public class StatystykaPromotora
{
    public string   PromotorImieNazwisko { get; set; } = "";
    public string   PromotorEmail        { get; set; } = "";
    public int      LiczbaPrac           { get; set; }
    public decimal? SredniaOcena         { get; set; }
}
