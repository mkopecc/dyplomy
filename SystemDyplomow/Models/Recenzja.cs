namespace SystemDyplomow.Models;

public class Recenzja
{
    public int      RecenzjaId            { get; set; }
    public int      PracaId               { get; set; }
    public string   TytulPracy            { get; set; } = "";
    public string   RecenzentPesel        { get; set; } = "";
    public string   RecenzentImieNazwisko { get; set; } = "";
    public string   RecenzentEmail        { get; set; } = "";
    public string?  Tresc                 { get; set; }
    public decimal  Ocena                 { get; set; }
    public DateTime DataWystawienia       { get; set; }
}
