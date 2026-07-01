using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SystemDyplomow.Data;
using SystemDyplomow.Models;

namespace SystemDyplomow.ViewModels;

public class WydzialyViewModel : BaseViewModel
{
    private readonly WydzialyRepository _repo = new();

    public ObservableCollection<Wydzial>    Wydzialy   { get; } = [];
    public ObservableCollection<Uzytkownik> Pracownicy { get; } = [];

    // --- Zaznaczony wiersz w tabeli ---
    private Wydzial? _selectedRow;
    public Wydzial? SelectedRow
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

    private Uzytkownik? _selectedDziekan;
    public Uzytkownik? SelectedDziekan
    {
        get => _selectedDziekan;
        set => Set(ref _selectedDziekan, value);
    }

    // --- Tryb edycji ---
    public bool   IsEditing   => SelectedRow != null;
    public string ButtonLabel => IsEditing ? "Zapisz zmiany" : "Dodaj";

    // --- Komendy ---
    public ICommand OdswiezCommand   { get; }
    public ICommand ZapiszCommand    { get; }   // Dodaj lub Zapisz zmiany
    public ICommand UsunCommand      { get; }
    public ICommand AnulujCommand    { get; }

    public WydzialyViewModel()
    {
        OdswiezCommand = new RelayCommand(Odswiez);
        ZapiszCommand  = new RelayCommand(Zapisz, () => !string.IsNullOrWhiteSpace(Nazwa));
        UsunCommand    = new RelayCommand(Usun,   () => SelectedRow != null);
        AnulujCommand  = new RelayCommand(Anuluj, () => SelectedRow != null);
        Zaladuj();
    }

    private void WypelnijFormularz(Wydzial w)
    {
        Nazwa           = w.Nazwa;
        SelectedDziekan = w.DziekanId.HasValue
            ? Pracownicy.FirstOrDefault(p => p.UzytkownikId == w.DziekanId.Value)
            : null;
    }

    private void Anuluj()
    {
        SelectedRow     = null;
        Nazwa           = "";
        SelectedDziekan = null;
    }

    private void Zaladuj()
    {
        try
        {
            foreach (var p in _repo.GetPracownicy()) Pracownicy.Add(p);
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
            Wydzialy.Clear();
            foreach (var w in _repo.GetAll()) Wydzialy.Add(w);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Zapisz()
    {
        if (string.IsNullOrWhiteSpace(Nazwa))
        {
            MessageBox.Show("Podaj nazwę wydziału.", "Nieprawidłowe dane",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            if (IsEditing)
            {
                _repo.AktualizujWydzial(SelectedRow!.WydzialId, Nazwa.Trim(), SelectedDziekan?.UzytkownikId);
                MessageBox.Show("Wydział zaktualizowany.", "Sukces",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                _repo.DodajWydzial(Nazwa.Trim(), SelectedDziekan?.UzytkownikId);
                MessageBox.Show($"Dodano wydział: {Nazwa.Trim()}.", "Sukces",
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
            $"Usunąć wydział \"{SelectedRow.Nazwa}\"?\n" +
            "Uwaga: usunięcie wydziału usunie też wszystkie przypisane kierunki.",
            "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (wynik != MessageBoxResult.Yes) return;
        try
        {
            _repo.UsunWydzial(SelectedRow.WydzialId);
            Anuluj();
            Odswiez();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd bazy danych", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
