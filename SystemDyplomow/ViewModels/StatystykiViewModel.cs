using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SystemDyplomow.Data;
using SystemDyplomow.Models;

namespace SystemDyplomow.ViewModels;

public class StatystykiViewModel : BaseViewModel
{
    private readonly ObronyRepository _repo = new();

    public ObservableCollection<StatystykaPromotora> Statystyki { get; } = [];

    private int _liczbaPrac;
    public int LiczbaPrac { get => _liczbaPrac; set => Set(ref _liczbaPrac, value); }

    private int _liczbaObron;
    public int LiczbaObron { get => _liczbaObron; set => Set(ref _liczbaObron, value); }

    private int _liczbaRecenzji;
    public int LiczbaRecenzji { get => _liczbaRecenzji; set => Set(ref _liczbaRecenzji, value); }

    public ICommand OdswiezCommand { get; }

    public StatystykiViewModel()
    {
        OdswiezCommand = new RelayCommand(Odswiez);
        Odswiez();
    }

    private void Odswiez()
    {
        try
        {
            Statystyki.Clear();
            foreach (var s in _repo.GetStatystyki()) Statystyki.Add(s);
            ZaladujSumary();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ZaladujSumary()
    {
        using var conn = Data.Database.GetConnection();
        conn.Open();
        using var cmd = new Microsoft.Data.SqlClient.SqlCommand("""
            SELECT
                (SELECT COUNT(*) FROM prace_dyplomowe),
                (SELECT COUNT(*) FROM obrony),
                (SELECT COUNT(*) FROM recenzje)
            """, conn);
        using var r = cmd.ExecuteReader();
        if (r.Read())
        {
            LiczbaPrac     = r.GetInt32(0);
            LiczbaObron    = r.GetInt32(1);
            LiczbaRecenzji = r.GetInt32(2);
        }
    }
}
