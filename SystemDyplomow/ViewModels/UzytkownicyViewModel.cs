using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SystemDyplomow.Data;
using SystemDyplomow.Models;

namespace SystemDyplomow.ViewModels;

public class UzytkownicyViewModel : BaseViewModel
{
    private readonly UzytkownicyRepository _repo = new();

    public ObservableCollection<Uzytkownik> Uzytkownicy { get; } = [];
    public List<string> Funkcje      { get; private set; } = [];
    public List<string> FunkcjeFiltr { get; private set; } = [];

    // Kierunki wybranego studenta
    public ObservableCollection<Kierunek> StudentKierunki    { get; } = [];
    public List<Kierunek>                 WszystkieKierunki  { get; private set; } = [];

    private Kierunek? _selectedKierunekDoDodania;
    public Kierunek? SelectedKierunekDoDodania
    {
        get => _selectedKierunekDoDodania;
        set => Set(ref _selectedKierunekDoDodania, value);
    }

    private Kierunek? _selectedKierunekDoUsuniecia;
    public Kierunek? SelectedKierunekDoUsuniecia
    {
        get => _selectedKierunekDoUsuniecia;
        set => Set(ref _selectedKierunekDoUsuniecia, value);
    }

    private string? _selectedFiltr;
    public string? SelectedFiltr
    {
        get => _selectedFiltr;
        set { Set(ref _selectedFiltr, value); Odswiez(); }
    }

    private Uzytkownik? _selected;
    public Uzytkownik? Selected
    {
        get => _selected;
        set
        {
            Set(ref _selected, value);
            OnPropertyChanged(nameof(IsStudentSelected));
            OdswiezKierunki();
        }
    }

    // Widoczność sekcji kierunków – tylko gdy zaznaczony jest student
    public bool IsStudentSelected => Selected?.Funkcja == "student";

    private string _pesel    = "";
    private string _imie     = "";
    private string _nazwisko = "";
    private string _email    = "";
    private string _telefon  = "";
    private string _funkcja  = "";

    public string Pesel    { get => _pesel;    set => Set(ref _pesel,    value); }
    public string Imie     { get => _imie;     set => Set(ref _imie,     value); }
    public string Nazwisko { get => _nazwisko; set => Set(ref _nazwisko, value); }
    public string Email    { get => _email;    set => Set(ref _email,    value); }
    public string Telefon  { get => _telefon;  set => Set(ref _telefon,  value); }
    public string Funkcja  { get => _funkcja;  set => Set(ref _funkcja,  value); }

    public ICommand OdswiezCommand          { get; }
    public ICommand DodajCommand            { get; }
    public ICommand UsunCommand             { get; }
    public ICommand PrzypiszKierunekCommand { get; }
    public ICommand UsunZKierunkuCommand    { get; }

    public UzytkownicyViewModel()
    {
        OdswiezCommand = new RelayCommand(Odswiez);
        DodajCommand   = new RelayCommand(Dodaj,
            () => !string.IsNullOrWhiteSpace(Pesel)
               && !string.IsNullOrWhiteSpace(Imie)
               && !string.IsNullOrWhiteSpace(Nazwisko)
               && !string.IsNullOrWhiteSpace(Email)
               && !string.IsNullOrWhiteSpace(Funkcja));
        UsunCommand = new RelayCommand(Usun, () => Selected != null);
        PrzypiszKierunekCommand = new RelayCommand(PrzypiszKierunek,
            () => IsStudentSelected && SelectedKierunekDoDodania != null);
        UsunZKierunkuCommand = new RelayCommand(UsunZKierunku,
            () => IsStudentSelected && SelectedKierunekDoUsuniecia != null);
        Zaladuj();
    }

    // ------------------------------------------------------------------
    // Walidacja przed zapisem do bazy
    // ------------------------------------------------------------------
    private string? Waliduj()
    {
        if (Pesel.Length != 11 || !Pesel.All(char.IsDigit))
            return "PESEL musi mieć dokładnie 11 cyfr (same cyfry, bez spacji).";

        if (!Email.Contains('@') || !Email.Contains('.'))
            return "Adres email musi mieć format: nazwa@domena.pl";

        if (!string.IsNullOrEmpty(Telefon)
            && !Telefon.All(c => char.IsDigit(c) || c is '+' or '-' or ' '))
            return "Telefon może zawierać tylko cyfry, +, - i spacje.";

        return null;
    }

    private void Zaladuj()
    {
        try
        {
            Funkcje           = _repo.GetFunkcje();
            FunkcjeFiltr      = ["(wszyscy)", .. Funkcje];
            WszystkieKierunki = _repo.GetWszystkieKierunki();
            OnPropertyChanged(nameof(Funkcje));
            OnPropertyChanged(nameof(FunkcjeFiltr));
            OnPropertyChanged(nameof(WszystkieKierunki));
            Funkcja = Funkcje.FirstOrDefault() ?? "";
            Odswiez();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OdswiezKierunki()
    {
        StudentKierunki.Clear();
        if (Selected == null || Selected.Funkcja != "student") return;
        try
        {
            foreach (var k in _repo.GetStudentKierunki(Selected.UzytkownikId))
                StudentKierunki.Add(k);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PrzypiszKierunek()
    {
        if (Selected == null || SelectedKierunekDoDodania == null) return;
        if (StudentKierunki.Any(k => k.KierunekId == SelectedKierunekDoDodania.KierunekId))
        {
            MessageBox.Show("Student jest już przypisany do tego kierunku.",
                            "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        try
        {
            _repo.PrzypiszKierunek(Selected.UzytkownikId, SelectedKierunekDoDodania.KierunekId);
            OdswiezKierunki();
            SelectedKierunekDoDodania = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd bazy danych", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UsunZKierunku()
    {
        if (Selected == null || SelectedKierunekDoUsuniecia == null) return;
        try
        {
            _repo.UsunZKierunku(Selected.UzytkownikId, SelectedKierunekDoUsuniecia.KierunekId);
            OdswiezKierunki();
            SelectedKierunekDoUsuniecia = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd bazy danych", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Odswiez()
    {
        try
        {
            var filtr = SelectedFiltr is null or "(wszyscy)" ? null : SelectedFiltr;
            Uzytkownicy.Clear();
            foreach (var u in _repo.GetAll(filtr)) Uzytkownicy.Add(u);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Dodaj()
    {
        var blad = Waliduj();
        if (blad != null)
        {
            MessageBox.Show(blad, "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            _repo.DodajUzytkownika(Pesel, Imie, Nazwisko, Email, Telefon, Funkcja);
            MessageBox.Show($"Dodano użytkownika: {Imie} {Nazwisko}.", "Sukces",
                            MessageBoxButton.OK, MessageBoxImage.Information);
            Pesel = Imie = Nazwisko = Email = Telefon = "";
            Odswiez();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd bazy danych", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Usun()
    {
        if (Selected == null) return;
        var wynik = MessageBox.Show(
            $"Usunąć użytkownika {Selected.PelneNazwisko}?\n" +
            "Uwaga: powiązane prace, recenzje i obrony mogą zostać usunięte kaskadowo.",
            "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (wynik != MessageBoxResult.Yes) return;
        try
        {
            _repo.UsunUzytkownika(Selected.UzytkownikId);
            Odswiez();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd bazy danych", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
