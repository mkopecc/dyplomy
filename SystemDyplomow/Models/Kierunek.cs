namespace SystemDyplomow.Models;

public class Kierunek
{
    public int    KierunekId   { get; set; }
    public string Nazwa        { get; set; } = "";
    public string Stopien      { get; set; } = "";
    public int    WydzialId    { get; set; }
    public string WydzialNazwa { get; set; } = "";
}
