using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SystemDyplomow.Data;
using SystemDyplomow.Models;

namespace SystemDyplomow.ViewModels;

public class ObronyViewModel : BaseViewModel
{
    private readonly ObronyRepository _repo = new();
    private readonly PraceRepository  _praceRepo = new();

    public ObservableCollection<Obrona>          Obrony    { get; } = [];
    public ObservableCollection<PracaDyplomowa>  PraceDoObrony { get; } = [];
    public List<decimal> DostepneOceny { get; } = [2.0m, 3.0m, 3.5m, 4.0m, 4.5m, 5.0m];

    private PracaDyplomowa? _selectedPraca;
    public PracaDyplomowa? SelectedPraca
    {
        get => _selectedPraca;
        set => Set(ref _selectedPraca, value);
    }

    private DateTime _dataObrony = DateTime.Today;
    public DateTime DataObrony
    {
        get => _dataObrony;
        set => Set(ref _dataObrony, value);
    }

    private string _sala = "";
    public string Sala
    {
        get => _sala;
        set => Set(ref _sala, value);
    }

    private decimal _wynik = 4.0m;
    public decimal Wynik
    {
        get => _wynik;
        set => Set(ref _wynik, value);
    }

    public ICommand OdswiezCommand { get; }
    public ICommand DodajCommand   { get; }

    public ObronyViewModel()
    {
        OdswiezCommand = new RelayCommand(Odswiez);
        DodajCommand   = new RelayCommand(Dodaj, () => SelectedPraca != null && !string.IsNullOrWhiteSpace(Sala));
        Zaladuj();
    }

    private void Zaladuj()
    {
        try
        {
            foreach (var p in _praceRepo.GetDoObrony()) PraceDoObrony.Add(p);
            Odswiez();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Odswiez()
    {
        try
        {
            Obrony.Clear();
            foreach (var o in _repo.GetAll()) Obrony.Add(o);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ------------------------------------------------------------------
    // Walidacja przed zapisem do bazy
    // ------------------------------------------------------------------
    private string? WalidujObrone()
    {
        if (SelectedPraca == null)
            return "Wybierz pracę do obrony.";

        if (string.IsNullOrWhiteSpace(Sala))
            return "Podaj numer sali obrony.";

        if (DataObrony < SelectedPraca.DataZlozenia)
            return $"Data obrony ({DataObrony:dd.MM.yyyy}) nie może być " +
                   $"wcześniejsza niż data złożenia pracy " +
                   $"({SelectedPraca.DataZlozenia:dd.MM.yyyy}).";

        return null;
    }

    private void Dodaj()
    {
        var blad = WalidujObrone();
        if (blad != null)
        {
            MessageBox.Show(blad, "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            _repo.DodajObrone(SelectedPraca!.PracaId, DataObrony, Sala, Wynik);
            MessageBox.Show("Obrona zarejestrowana. Status pracy zaktualizowany automatycznie.", "Sukces",
                            MessageBoxButton.OK, MessageBoxImage.Information);
            PraceDoObrony.Remove(SelectedPraca);
            SelectedPraca = null;
            Sala = "";
            Odswiez();
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            if (msg.Contains("data_obrony") || msg.Contains("trg_obrony_sprawdz_date"))
                msg = "Data obrony nie może być wcześniejsza niż data złożenia pracy.";
            else if (msg.Contains("UQ_obrony_praca"))
                msg = "Ta praca ma już zarejestrowaną obronę.";
            MessageBox.Show(msg, "Błąd bazy danych", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
