namespace SystemDyplomow.Models;

public class Wydzial
{
    public int     WydzialId           { get; set; }
    public string  Nazwa               { get; set; } = "";
    public int?    DziekanId           { get; set; }
    public string  DziekanImieNazwisko { get; set; } = "(brak)";
    public string? DziekanEmail        { get; set; }
}
