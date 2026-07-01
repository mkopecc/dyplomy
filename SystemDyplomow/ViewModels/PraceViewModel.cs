using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SystemDyplomow.Data;
using SystemDyplomow.Models;

namespace SystemDyplomow.ViewModels;

public class PraceViewModel : BaseViewModel
{
    private readonly PraceRepository _repo = new();

    // --- Lista prac ---
    private ObservableCollection<PracaDyplomowa> _prace = [];
    public ObservableCollection<PracaDyplomowa> Prace
    {
        get => _prace;
        set => Set(ref _prace, value);
    }

    private PracaDyplomowa? _selectedPraca;
    public PracaDyplomowa? SelectedPraca
    {
        get => _selectedPraca;
        set => Set(ref _selectedPraca, value);
    }

    // --- Filtr statusu (z "(wszystkie)") i lista do zmiany statusu (bez "(wszystkie)") ---
    public ObservableCollection<string> Statusy       { get; } = [];
    public ObservableCollection<string> StatusyZmiany { get; } = [];

    private string _selectedStatus = "(wszystkie)";
    public string SelectedStatus
    {
        get => _selectedStatus;
        set { Set(ref _selectedStatus, value); OdswiezPrace(); }
    }

    // --- Formularz nowej pracy ---
    public ObservableCollection<Uzytkownik> Studenci  { get; } = [];
    public ObservableCollection<Uzytkownik> Promotorzy { get; } = [];

    private Uzytkownik? _selectedStudent;
    public Uzytkownik? SelectedStudent
    {
        get => _selectedStudent;
        set => Set(ref _selectedStudent, value);
    }

    private Uzytkownik? _selectedPromotor;
    public Uzytkownik? SelectedPromotor
    {
        get => _selectedPromotor;
        set => Set(ref _selectedPromotor, value);
    }

    private string _tytul = "";
    public string Tytul
    {
        get => _tytul;
        set => Set(ref _tytul, value);
    }

    private string _plikPracy = "";
    public string PlikPracy
    {
        get => _plikPracy;
        set => Set(ref _plikPracy, value);
    }

    private DateTime _dataZlozenia = DateTime.Today;
    public DateTime DataZlozenia
    {
        get => _dataZlozenia;
        set => Set(ref _dataZlozenia, value);
    }

    // --- Zmiana statusu ---
    private string _nowyStatus = "";
    public string NowyStatus
    {
        get => _nowyStatus;
        set => Set(ref _nowyStatus, value);
    }

    // --- Komendy ---
    public ICommand OdswiezCommand    { get; }
    public ICommand DodajPraceCommand { get; }
    public ICommand ZmienStatusCommand { get; }

    public PraceViewModel()
    {
        OdswiezCommand     = new RelayCommand(OdswiezPrace);
        DodajPraceCommand  = new RelayCommand(DodajPrace);
        ZmienStatusCommand = new RelayCommand(ZmienStatus, () => SelectedPraca != null && !string.IsNullOrEmpty(NowyStatus));

        Zaladuj();
    }

    private void Zaladuj()
    {
        try
        {
            foreach (var s in _repo.GetStatusy())
            {
                Statusy.Add(s);
                if (s != "(wszystkie)") StatusyZmiany.Add(s);
            }
            foreach (var u in _repo.GetUzytkownicy("student"))           Studenci.Add(u);
            foreach (var u in _repo.GetUzytkownicy("pracownik uczelni")) Promotorzy.Add(u);
            OdswiezPrace();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd połączenia z bazą:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OdswiezPrace()
    {
        try
        {
            var filter = SelectedStatus == "(wszystkie)" ? null : SelectedStatus;
            Prace = new ObservableCollection<PracaDyplomowa>(_repo.GetAll(filter));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ------------------------------------------------------------------
    // Walidacja przed zapisem do bazy
    // ------------------------------------------------------------------
    private string? WalidujPrace()
    {
        if (string.IsNullOrWhiteSpace(Tytul))
            return "Tytuł pracy nie może być pusty.";

        if (Tytul.Length > 50)
            return $"Tytuł pracy jest za długi ({Tytul.Length}/50 znaków). Skróć go.";

        if (SelectedStudent == null || SelectedPromotor == null)
            return "Wybierz studenta i promotora.";

        if (SelectedStudent.UzytkownikId == SelectedPromotor.UzytkownikId)
            return "Student i promotor muszą być różnymi osobami.";

        if (Prace.Any(p => p.StudentEmail == SelectedStudent.Email))
            return $"{SelectedStudent.PelneNazwisko} ma już złożoną pracę dyplomową.";

        return null;
    }

    private static string TlumaczBladSQL(string msg)
    {
        if (msg.Contains("CHK_prace_rozne_osoby"))
            return "Student i promotor muszą być różnymi osobami.";
        if (msg.Contains("UQ_prace_student"))
            return "Ten student ma już złożoną pracę dyplomową.";
        if (msg.Contains("CHECK constraint"))
            return "Dane nie spełniają reguł bazy danych. Sprawdź poprawność pól.";
        return msg;
    }

    private void DodajPrace()
    {
        var blad = WalidujPrace();
        if (blad != null)
        {
            MessageBox.Show(blad, "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            var plik = string.IsNullOrWhiteSpace(PlikPracy) ? null : PlikPracy.Trim();
            _repo.DodajPrace(SelectedStudent!.UzytkownikId, SelectedPromotor!.UzytkownikId, Tytul, DataZlozenia, plik);
            MessageBox.Show("Praca dodana pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            Tytul = ""; PlikPracy = "";
            OdswiezPrace();
        }
        catch (Exception ex)
        {
            MessageBox.Show(TlumaczBladSQL(ex.Message), "Błąd bazy danych", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ZmienStatus()
    {
        if (SelectedPraca == null || string.IsNullOrEmpty(NowyStatus)) return;
        try
        {
            _repo.ZmienStatus(SelectedPraca.PracaId, NowyStatus);
            MessageBox.Show($"Status zmieniony na: {NowyStatus}", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            OdswiezPrace();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd bazy danych", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
