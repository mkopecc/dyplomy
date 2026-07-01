using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SystemDyplomow.Data;
using SystemDyplomow.Models;

namespace SystemDyplomow.ViewModels;

public class KierunkiViewModel : BaseViewModel
{
    private readonly KierunkiRepository _repo     = new();
    private readonly WydzialyRepository _wydzRepo = new();

    public ObservableCollection<Kierunek> Kierunki { get; } = [];
    public ObservableCollection<Wydzial>  Wydzialy { get; } = [];

    public List<string> Stopnie { get; } = ["licencjat", "inżynier", "magister", "magister inżynier"];

    // --- Zaznaczony wiersz w tabeli ---
    private Kierunek? _selectedRow;
    public Kierunek? SelectedRow
    {
        get => _selectedRow;
        set
        {
            Set(ref _selectedRow, value);
            OnPropertyChanged(nameof(IsEditing));
            OnPropertyChanged(nameof(ButtonLabel));
            if (value != null) WypelnijFormularz(value);
        }
    }

    // --- Formularz ---
    private string _nazwa = "";
    public string Nazwa
    {
        get => _nazwa;
        set => Set(ref _nazwa, value);
    }

    private string _stopien = "inżynier";
    public string Stopien
    {
        get => _stopien;
        set => Set(ref _stopien, value);
    }

    private Wydzial? _selectedWydzial;
    public Wydzial? SelectedWydzial
    {
        get => _selectedWydzial;
        set => Set(ref _selectedWydzial, value);
    }

    // --- Tryb edycji ---
    public bool   IsEditing   => SelectedRow != null;
    public string ButtonLabel => IsEditing ? "Zapisz zmiany" : "Dodaj";

    // --- Komendy ---
    public ICommand OdswiezCommand { get; }
    public ICommand ZapiszCommand  { get; }
    public ICommand UsunCommand    { get; }
    public ICommand AnulujCommand  { get; }

    public KierunkiViewModel()
    {
        OdswiezCommand = new RelayCommand(Odswiez);
        ZapiszCommand  = new RelayCommand(Zapisz,
            () => !string.IsNullOrWhiteSpace(Nazwa) && SelectedWydzial != null);
        UsunCommand    = new RelayCommand(Usun,   () => SelectedRow != null);
        AnulujCommand  = new RelayCommand(Anuluj, () => SelectedRow != null);
        Zaladuj();
    }

    private void WypelnijFormularz(Kierunek k)
    {
        Nazwa           = k.Nazwa;
        Stopien         = k.Stopien;
        SelectedWydzial = Wydzialy.FirstOrDefault(w => w.WydzialId == k.WydzialId);
    }

    private void Anuluj()
    {
        SelectedRow     = null;
        Nazwa           = "";
        Stopien         = "inżynier";
        SelectedWydzial = null;
    }

    private void Zaladuj()
    {
        try
        {
            foreach (var w in _wydzRepo.GetAll()) Wydzialy.Add(w);
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
            Kierunki.Clear();
            foreach (var k in _repo.GetAll()) Kierunki.Add(k);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Zapisz()
    {
        if (string.IsNullOrWhiteSpace(Nazwa) || SelectedWydzial == null)
        {
            MessageBox.Show("Podaj nazwę kierunku i wybierz wydział.", "Nieprawidłowe dane",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            if (IsEditing)
            {
                _repo.AktualizujKierunek(SelectedRow!.KierunekId, Nazwa.Trim(), SelectedWydzial.WydzialId, Stopien);
                MessageBox.Show("Kierunek zaktualizowany.", "Sukces",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                _repo.DodajKierunek(Nazwa.Trim(), SelectedWydzial.WydzialId, Stopien);
                MessageBox.Show($"Dodano kierunek: {Nazwa.Trim()} ({Stopien}).", "Sukces",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            Anuluj();
            Odswiez();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd bazy danych", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Usun()
    {
        if (SelectedRow == null) return;
        var wynik = MessageBox.Show(
            $"Usunąć kierunek \"{SelectedRow.Nazwa} ({SelectedRow.Stopien})\"?\n" +
            "Uwaga: usunięcie kierunku usunie też wszystkie przypisania studentów do niego.",
            "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (wynik != MessageBoxResult.Yes) return;
        try
        {
            _repo.UsunKierunek(SelectedRow.KierunekId);
            Anuluj();
            Odswiez();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd bazy danych", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
