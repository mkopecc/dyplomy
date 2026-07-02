USE system_dyplomow;
GO

-- Funkcje
INSERT INTO funkcje (nazwa) VALUES ('student');         -- id = 1
INSERT INTO funkcje (nazwa) VALUES ('pracownik uczelni'); -- id = 2

-- Statusy prac
INSERT INTO statusy (nazwa) VALUES ('złożona');
INSERT INTO statusy (nazwa) VALUES ('zatwierdzona');
INSERT INTO statusy (nazwa) VALUES ('odrzucona');
INSERT INTO statusy (nazwa) VALUES ('obroniona');

-- Użytkownicy – studenci
INSERT INTO uzytkownicy (pesel, imie, nazwisko, email, telefon, funkcja)
VALUES
    ('00210187654', 'Anna',      'Kowalska',   'anna.kowalska@student.pl',   '501000001', 1),
    ('99120265432', 'Bartosz',   'Nowak',      'bartosz.nowak@student.pl',   '501000002', 1),
    ('01312187651', 'Celina',    'Wiśniewska', 'celina.wisn@student.pl',     '501000003', 1),
    ('02231598760', 'Dawid',     'Wójcik',     'dawid.wojcik@student.pl',    '501000004', 1),
    ('98091276543', 'Elżbieta',  'Kowalczyk',  'elzbieta.kow@student.pl',   '501000005', 1),
    ('03150887652', 'Filip',     'Zielinski',  'filip.ziel@student.pl',      '501000006', 1);

-- Użytkownicy – pracownicy uczelni
INSERT INTO uzytkownicy (pesel, imie, nazwisko, email, telefon, funkcja)
VALUES
    ('62041587612', 'Grzegorz',  'Adamski',    'g.adamski@uczelnia.pl',      '600100001', 2),
    ('55060398745', 'Halina',    'Baran',      'h.baran@uczelnia.pl',        '600100002', 2),
    ('70031265478', 'Ireneusz',  'Czajka',     'i.czajka@uczelnia.pl',       '600100003', 2),
    ('68052176523', 'Joanna',    'Dąbrowska',  'j.dabrowska@uczelnia.pl',    '600100004', 2),
    ('75091287634', 'Krzysztof', 'Ekiert',     'k.ekiert@uczelnia.pl',       '600100005', 2),
    ('60121398756', 'Lidia',     'Frankowska', 'l.frankowska@uczelnia.pl',   '600100006', 2);

-- Wydziały (dziekan id=7 = Grzegorz Adamski, pracownik uczelni)
INSERT INTO wydzialy (nazwa, dziekan) VALUES ('Wydział Informatyki i Telekomunikacji', 7);
INSERT INTO wydzialy (nazwa, dziekan) VALUES ('Wydział Zarządzania', NULL);

-- Kierunki
INSERT INTO kierunki (nazwa, wydzial, stopien) VALUES ('Informatyka',              1, 'inżynier');
INSERT INTO kierunki (nazwa, wydzial, stopien) VALUES ('Teleinformatyka',          1, 'magister');
INSERT INTO kierunki (nazwa, wydzial, stopien) VALUES ('Zarządzanie',              2, 'licencjat');
INSERT INTO kierunki (nazwa, wydzial, stopien) VALUES ('Finanse i Rachunkowość',   2, 'licencjat');

-- Przypisania studentów do kierunków
INSERT INTO studenci_kierunki (student_id, kierunek_id) VALUES (1, 1);
INSERT INTO studenci_kierunki (student_id, kierunek_id) VALUES (2, 1);
INSERT INTO studenci_kierunki (student_id, kierunek_id) VALUES (3, 2);
INSERT INTO studenci_kierunki (student_id, kierunek_id) VALUES (4, 3);
INSERT INTO studenci_kierunki (student_id, kierunek_id) VALUES (5, 3);
INSERT INTO studenci_kierunki (student_id, kierunek_id) VALUES (6, 4);

-- Prace dyplomowe
INSERT INTO prace_dyplomowe (student_id, promotor_id, tytul, data_zlozenia, status, plik_pracy)
VALUES
    (1, 8,  'Analiza algorytmów sortowania',       '2024-10-01', 2, NULL),
    (2, 9,  'Systemy rekomendacyjne w e-commerce', '2024-10-15', 1, NULL),
    (3, 10, 'Bezpieczeństwo sieci lokalnych',      '2024-11-01', 2, NULL),
    (4, 11, 'Optymalizacja procesów logistycznych','2024-11-20', 1, NULL),
    (5, 12, 'Analiza rynku kryptowalut',           '2024-12-05', 4, 'praca_analiza.pdf'),
    (6, 7,  'Chatboty w obsłudze klienta',         '2025-01-10', 1, NULL);

-- Obrony
INSERT INTO obrony (praca_id, data_obrony, sala, wynik_koncowy)
VALUES (5, '2025-02-15', 'A204', 4.5);

-- Recenzje
INSERT INTO recenzje (praca_id, recenzent_id, tresc, ocena, data_wystawienia)
VALUES
    (1, 10, 'Praca napisana starannie, temat dobrze opracowany.', 4.5, '2024-12-10'),
    (3, 8,  'Solidna analiza zagrożeń, wnioski trafne.',          4.0, '2024-12-20'),
    (5, 9,  'Obszerna analiza z ciekawymi wnioskami.',            5.0, '2025-01-30');
GO
