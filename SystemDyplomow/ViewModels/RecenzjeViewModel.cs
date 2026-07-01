using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SystemDyplomow.Data;
using SystemDyplomow.Models;

namespace SystemDyplomow.ViewModels;

public class RecenzjeViewModel : BaseViewModel
{
    private readonly RecenzjeRepository _repo = new();

    public ObservableCollection<Recenzja>      Recenzje  { get; } = [];
    public ObservableCollection<Uzytkownik>    Recenzenci { get; } = [];
    public ObservableCollection<PracaDyplomowa> Prace    { get; } = [];

    public List<decimal> DostepneOceny { get; } = [2.0m, 3.0m, 3.5m, 4.0m, 4.5m, 5.0m];

    private Uzytkownik? _selectedRecenzent;
    public Uzytkownik? SelectedRecenzent
    {
        get => _selectedRecenzent;
        set { Set(ref _selectedRecenzent, value); SprawdzDuplikat(); }
    }

    private PracaDyplomowa? _selectedPraca;
    public PracaDyplomowa? SelectedPraca
    {
        get => _selectedPraca;
        set { Set(ref _selectedPraca, value); SprawdzDuplikat(); }
    }

    private string _tresc = "";
    public string Tresc
    {
        get => _tresc;
        set => Set(ref _tresc, value);
    }

    private decimal _ocena = 4.0m;
    public decimal Ocena
    {
        get => _ocena;
        set => Set(ref _ocena, value);
    }

    private DateTime _dataWystawienia = DateTime.Today;
    public DateTime DataWystawienia
    {
        get => _dataWystawienia;
        set => Set(ref _dataWystawienia, value);
    }

    private bool _isDuplikat;
    public bool IsDuplikat
    {
        get => _isDuplikat;
        set => Set(ref _isDuplikat, value);
    }

    private string _duplikatInfo = "";
    public string DuplikatInfo
    {
        get => _duplikatInfo;
        set => Set(ref _duplikatInfo, value);
    }

    public ICommand OdswiezCommand { get; }
    public ICommand DodajCommand   { get; }

    public RecenzjeViewModel()
    {
        OdswiezCommand = new RelayCommand(Odswiez);
        DodajCommand   = new RelayCommand(Dodaj,
            () => SelectedPraca != null && SelectedRecenzent != null && !IsDuplikat);
        Zaladuj();
    }

    private void SprawdzDuplikat()
    {
        if (SelectedPraca == null || SelectedRecenzent == null)
        {
            IsDuplikat   = false;
            DuplikatInfo = "";
            return;
        }

        // Recenzent jest promotorem tej pracy
        if (SelectedPraca.PromotorEmail == SelectedRecenzent.Email)
        {
            IsDuplikat   = true;
            DuplikatInfo = "Recenzent nie może być promotorem tej pracy.";
            return;
        }

        // Recenzent jest studentem tej pracy
        if (SelectedPraca.StudentEmail == SelectedRecenzent.Email)
        {
            IsDuplikat   = true;
            DuplikatInfo = "Recenzent nie może być autorem recenzowanej pracy.";
            return;
        }

        // Duplikat: ta para (praca + recenzent) już istnieje
        bool istnieje = Recenzje.Any(r =>
            r.PracaId        == SelectedPraca.PracaId &&
            r.RecenzentEmail == SelectedRecenzent.Email);

        IsDuplikat   = istnieje;
        DuplikatInfo = istnieje
            ? $"{SelectedRecenzent.PelneNazwisko} już ocenił(a) tę pracę."
            : "";
    }

    private void Zaladuj()
    {
        try
        {
            foreach (var r in _repo.GetRecenzenci()) Recenzenci.Add(r);
            foreach (var p in _repo.GetPrace())      Prace.Add(p);
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
            Recenzje.Clear();
            foreach (var r in _repo.GetAll()) Recenzje.Add(r);
            SprawdzDuplikat();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Dodaj()
    {
        if (SelectedPraca == null || SelectedRecenzent == null) return;
        try
        {
            var tresc = string.IsNullOrWhiteSpace(Tresc) ? null : Tresc.Trim();
            _repo.DodajRecenzje(SelectedPraca.PracaId, SelectedRecenzent.UzytkownikId, tresc, Ocena, DataWystawienia);
            MessageBox.Show("Recenzja dodana.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            Tresc = "";
            Odswiez();
            SelectedPraca     = null;
            SelectedRecenzent = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd bazy danych", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
