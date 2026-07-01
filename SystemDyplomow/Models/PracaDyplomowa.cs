namespace SystemDyplomow.Models;

public class PracaDyplomowa
{
    public int      PracaId               { get; set; }
    public string   Tytul                 { get; set; } = "";
    public string   StudentPesel          { get; set; } = "";
    public string   StudentImieNazwisko   { get; set; } = "";
    public string   StudentEmail          { get; set; } = "";
    public string   PromotorPesel         { get; set; } = "";
    public string   PromotorImieNazwisko  { get; set; } = "";
    public string   PromotorEmail         { get; set; } = "";
    public string   Status                { get; set; } = "";
    public DateTime DataZlozenia          { get; set; }
    public string?  PlikPracy             { get; set; }
}
