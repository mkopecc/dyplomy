namespace SystemDyplomow.Models;

public class Obrona
{
    public int ObronaId { get; set; }
    public int PracaId { get; set; }
    public string TytulPracy { get; set; } = "";
    public DateTime DataObrony { get; set; }
    public string Sala { get; set; } = "";
    public decimal WynikKoncowy { get; set; }
}
