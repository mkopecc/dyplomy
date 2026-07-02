CREATE DATABASE system_dyplomow;
GO
USE system_dyplomow;
GO

CREATE TABLE funkcje (
    funkcja_id   INT          NOT NULL IDENTITY(1,1),
    nazwa        VARCHAR(30)  NOT NULL,
    CONSTRAINT PK_funkcje PRIMARY KEY (funkcja_id)
);
GO

CREATE TABLE statusy (
    status_id    INT          NOT NULL IDENTITY(1,1),
    nazwa        VARCHAR(30)  NOT NULL,
    CONSTRAINT PK_statusy PRIMARY KEY (status_id)
);
GO

CREATE TABLE uzytkownicy (
    uzytkownik_id  INT           NOT NULL IDENTITY(1,1),
    pesel          VARCHAR(11)   NOT NULL,
    imie           VARCHAR(20)   NOT NULL,
    nazwisko       VARCHAR(20)   NOT NULL,
    email          VARCHAR(100)  NOT NULL,
    telefon        VARCHAR(15)   NULL,
    funkcja        INT           NOT NULL,
    CONSTRAINT PK_uzytkownicy       PRIMARY KEY (uzytkownik_id),
    CONSTRAINT UQ_uzytkownicy_pesel UNIQUE (pesel),
    CONSTRAINT UQ_uzytkownicy_email UNIQUE (email),
    CONSTRAINT CHK_pesel CHECK (LEN(pesel) = 11 AND ISNUMERIC(pesel) = 1),
    CONSTRAINT CHK_email CHECK (email LIKE '%@%'),
    CONSTRAINT FK_uzytkownicy_funkcje
        FOREIGN KEY (funkcja) REFERENCES funkcje (funkcja_id)
        ON DELETE NO ACTION ON UPDATE CASCADE
);
GO

CREATE TABLE wydzialy (
    wydzial_id   INT          NOT NULL IDENTITY(1,1),
    nazwa        VARCHAR(50)  NOT NULL,
    dziekan      INT          NULL,
    CONSTRAINT PK_wydzialy PRIMARY KEY (wydzial_id),
    CONSTRAINT FK_wydzialy_dziekan
        FOREIGN KEY (dziekan) REFERENCES uzytkownicy (uzytkownik_id)
        ON DELETE SET NULL ON UPDATE CASCADE
);
GO

CREATE TABLE kierunki (
    kierunek_id  INT          NOT NULL IDENTITY(1,1),
    nazwa        VARCHAR(50)  NOT NULL,
    wydzial      INT          NOT NULL,
    stopien      VARCHAR(30)  NOT NULL,
    CONSTRAINT PK_kierunki PRIMARY KEY (kierunek_id),
    CONSTRAINT FK_kierunki_wydzialy
        FOREIGN KEY (wydzial) REFERENCES wydzialy (wydzial_id)
        ON DELETE NO ACTION ON UPDATE CASCADE
);
GO

CREATE TABLE studenci_kierunki (
    sk_id       INT  NOT NULL IDENTITY(1,1),
    student_id  INT  NOT NULL,
    kierunek_id INT  NOT NULL,
    CONSTRAINT PK_studenci_kierunki   PRIMARY KEY (sk_id),
    CONSTRAINT UQ_sk_student_kierunek UNIQUE (student_id, kierunek_id),
    CONSTRAINT FK_sk_student
        FOREIGN KEY (student_id) REFERENCES uzytkownicy (uzytkownik_id)
        ON DELETE CASCADE ON UPDATE NO ACTION,
    CONSTRAINT FK_sk_kierunek
        FOREIGN KEY (kierunek_id) REFERENCES kierunki (kierunek_id)
        ON DELETE CASCADE ON UPDATE NO ACTION
);
GO

CREATE TABLE prace_dyplomowe (
    praca_id       INT          NOT NULL IDENTITY(1,1),
    student_id     INT          NOT NULL,
    promotor_id    INT          NOT NULL,
    tytul          VARCHAR(50)  NOT NULL,
    data_zlozenia  DATE         NOT NULL,
    status         INT          NOT NULL DEFAULT 1,
    plik_pracy     VARCHAR(50)  NULL,
    CONSTRAINT PK_prace_dyplomowe  PRIMARY KEY (praca_id),
    CONSTRAINT UQ_prace_student    UNIQUE (student_id),
    CONSTRAINT CHK_prace_rozne_osoby CHECK (student_id <> promotor_id),
    CONSTRAINT FK_prace_student
        FOREIGN KEY (student_id) REFERENCES uzytkownicy (uzytkownik_id)
        ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_prace_promotor
        FOREIGN KEY (promotor_id) REFERENCES uzytkownicy (uzytkownik_id)
        ON DELETE NO ACTION ON UPDATE NO ACTION,
    CONSTRAINT FK_prace_status
        FOREIGN KEY (status) REFERENCES statusy (status_id)
        ON DELETE NO ACTION ON UPDATE CASCADE
);
GO

CREATE TABLE obrony (
    obrona_id      INT           NOT NULL IDENTITY(1,1),
    praca_id       INT           NOT NULL,
    data_obrony    DATE          NOT NULL,
    sala           VARCHAR(10)   NOT NULL,
    wynik_koncowy  DECIMAL(3,1)  NOT NULL,
    CONSTRAINT PK_obrony       PRIMARY KEY (obrona_id),
    CONSTRAINT UQ_obrony_praca UNIQUE (praca_id),
    CONSTRAINT CHK_wynik_koncowy CHECK (wynik_koncowy IN (2.0, 3.0, 3.5, 4.0, 4.5, 5.0)),
    CONSTRAINT FK_obrony_praca
        FOREIGN KEY (praca_id) REFERENCES prace_dyplomowe (praca_id)
        ON DELETE CASCADE ON UPDATE CASCADE
);
GO

CREATE TABLE recenzje (
    recenzja_id       INT           NOT NULL IDENTITY(1,1),
    praca_id          INT           NOT NULL,
    recenzent_id      INT           NOT NULL,
    tresc             NVARCHAR(MAX) NULL,
    ocena             DECIMAL(3,1)  NOT NULL,
    data_wystawienia  DATE          NOT NULL,
    CONSTRAINT PK_recenzje      PRIMARY KEY (recenzja_id),
    CONSTRAINT UQ_recenzje_para UNIQUE (praca_id, recenzent_id),
    CONSTRAINT CHK_ocena CHECK (ocena IN (2.0, 3.0, 3.5, 4.0, 4.5, 5.0)),
    CONSTRAINT FK_recenzje_praca
        FOREIGN KEY (praca_id) REFERENCES prace_dyplomowe (praca_id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT FK_recenzje_recenzent
        FOREIGN KEY (recenzent_id) REFERENCES uzytkownicy (uzytkownik_id)
        ON DELETE NO ACTION ON UPDATE CASCADE
);
GO

-- Triggers

CREATE OR ALTER TRIGGER trg_prace_sprawdz_role
ON prace_dyplomowe AFTER INSERT, UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM inserted i
        JOIN uzytkownicy u ON u.uzytkownik_id = i.promotor_id
        JOIN funkcje f     ON f.funkcja_id    = u.funkcja
        WHERE f.nazwa <> 'pracownik uczelni'
    ) BEGIN RAISERROR('Promotor musi byc pracownikiem uczelni.', 16, 1); ROLLBACK TRANSACTION; RETURN; END
    IF EXISTS (
        SELECT 1 FROM inserted i
        JOIN uzytkownicy u ON u.uzytkownik_id = i.student_id
        JOIN funkcje f     ON f.funkcja_id    = u.funkcja
        WHERE f.nazwa <> 'student'
    ) BEGIN RAISERROR('Student musi miec przypisana funkcje student.', 16, 1); ROLLBACK TRANSACTION; RETURN; END
END;
GO

CREATE OR ALTER TRIGGER trg_recenzje_sprawdz_recenzenta
ON recenzje AFTER INSERT, UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM inserted i
        JOIN prace_dyplomowe p ON p.praca_id = i.praca_id
        WHERE i.recenzent_id = p.promotor_id
    ) BEGIN RAISERROR('Recenzent nie moze byc promotorem tej pracy.', 16, 1); ROLLBACK TRANSACTION; RETURN; END
    IF EXISTS (
        SELECT 1 FROM inserted i
        JOIN prace_dyplomowe p ON p.praca_id = i.praca_id
        WHERE i.recenzent_id = p.student_id
    ) BEGIN RAISERROR('Recenzent nie moze byc autorem recenzowanej pracy.', 16, 1); ROLLBACK TRANSACTION; RETURN; END
END;
GO

CREATE OR ALTER TRIGGER trg_obrony_sprawdz_date
ON obrony AFTER INSERT, UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM inserted i
        JOIN prace_dyplomowe p ON p.praca_id = i.praca_id
        WHERE i.data_obrony < p.data_zlozenia
    ) BEGIN RAISERROR('Data obrony nie moze byc wczesniejsza niz data zlozenia pracy.', 16, 1); ROLLBACK TRANSACTION; RETURN; END
END;
GO

CREATE OR ALTER TRIGGER trg_obrony_aktualizuj_status
ON obrony AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE prace_dyplomowe
    SET status = (SELECT status_id FROM statusy WHERE nazwa = 'obroniona')
    WHERE praca_id IN (SELECT praca_id FROM inserted);
END;
GO

-- Views

CREATE OR ALTER VIEW vw_prace AS
SELECT p.praca_id, p.tytul, p.plik_pracy,
       s_u.pesel AS student_pesel, s_u.imie + ' ' + s_u.nazwisko AS student_imie_nazwisko, s_u.email AS student_email,
       pr_u.pesel AS promotor_pesel, pr_u.imie + ' ' + pr_u.nazwisko AS promotor_imie_nazwisko, pr_u.email AS promotor_email,
       st.nazwa AS status, p.data_zlozenia
FROM prace_dyplomowe p
JOIN uzytkownicy s_u  ON s_u.uzytkownik_id  = p.student_id
JOIN uzytkownicy pr_u ON pr_u.uzytkownik_id = p.promotor_id
JOIN statusy st        ON st.status_id       = p.status;
GO

CREATE OR ALTER VIEW vw_prace_do_obrony AS
SELECT p.praca_id, p.tytul,
       s_u.imie + ' ' + s_u.nazwisko AS student_imie_nazwisko, s_u.email AS student_email,
       pr_u.imie + ' ' + pr_u.nazwisko AS promotor_imie_nazwisko, pr_u.email AS promotor_email,
       p.data_zlozenia
FROM prace_dyplomowe p
JOIN uzytkownicy s_u  ON s_u.uzytkownik_id  = p.student_id
JOIN uzytkownicy pr_u ON pr_u.uzytkownik_id = p.promotor_id
JOIN statusy st        ON st.status_id       = p.status
WHERE st.nazwa = 'zatwierdzona'
  AND NOT EXISTS (SELECT 1 FROM obrony o WHERE o.praca_id = p.praca_id);
GO

CREATE OR ALTER VIEW vw_statystyki_promotora AS
SELECT pr_u.uzytkownik_id AS promotor_id,
       pr_u.imie + ' ' + pr_u.nazwisko AS promotor_imie_nazwisko,
       pr_u.email AS promotor_email,
       COUNT(DISTINCT p.praca_id) AS liczba_prac,
       AVG(r.ocena) AS srednia_ocena
FROM uzytkownicy pr_u
JOIN prace_dyplomowe p ON p.promotor_id = pr_u.uzytkownik_id
LEFT JOIN recenzje r   ON r.praca_id    = p.praca_id
GROUP BY pr_u.uzytkownik_id, pr_u.imie, pr_u.nazwisko, pr_u.email;
GO

CREATE OR ALTER VIEW vw_wydzialy AS
SELECT w.wydzial_id, w.nazwa AS wydzial_nazwa,
       ISNULL(u.imie + ' ' + u.nazwisko, '(brak)') AS dziekan_imie_nazwisko,
       u.email AS dziekan_email, u.uzytkownik_id AS dziekan_id
FROM wydzialy w
LEFT JOIN uzytkownicy u ON u.uzytkownik_id = w.dziekan;
GO

CREATE OR ALTER VIEW vw_kierunki AS
SELECT k.kierunek_id, k.nazwa AS kierunek_nazwa, k.stopien,
       w.wydzial_id, w.nazwa AS wydzial_nazwa
FROM kierunki k
JOIN wydzialy w ON w.wydzial_id = k.wydzial;
GO

-- Stored procedures

CREATE OR ALTER PROCEDURE sp_dodaj_prace
    @student_id INT, @promotor_id INT, @tytul VARCHAR(50), @data_zlozenia DATE, @plik_pracy VARCHAR(50) = NULL
AS BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM prace_dyplomowe WHERE student_id = @student_id)
        BEGIN RAISERROR('Ten student ma juz zlozona prace dyplomowa.', 16, 1); RETURN; END
    IF @student_id = @promotor_id
        BEGIN RAISERROR('Student i promotor musza byc roznymi osobami.', 16, 1); RETURN; END
    BEGIN TRY
        INSERT INTO prace_dyplomowe (student_id, promotor_id, tytul, data_zlozenia, status, plik_pracy)
        VALUES (@student_id, @promotor_id, @tytul, @data_zlozenia, 1, NULLIF(@plik_pracy, ''));
    END TRY
    BEGIN CATCH
        DECLARE @msg3 NVARCHAR(2048) = ERROR_MESSAGE(), @num3 INT = ERROR_NUMBER();
        IF @num3 = 547 AND @msg3 LIKE '%CHK_prace_rozne_osoby%'
            RAISERROR('Student i promotor musza byc roznymi osobami.', 16, 1);
        ELSE IF @num3 = 2627 AND @msg3 LIKE '%UQ_prace_student%'
            RAISERROR('Ten student ma juz zlozona prace dyplomowa.', 16, 1);
        ELSE RAISERROR(@msg3, 16, 1);
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_zmien_status
    @praca_id INT, @nowy_status VARCHAR(30)
AS BEGIN
    SET NOCOUNT ON;
    DECLARE @status_id INT;
    SELECT @status_id = status_id FROM statusy WHERE nazwa = @nowy_status;
    IF @status_id IS NULL BEGIN RAISERROR('Podany status nie istnieje.', 16, 1); RETURN; END
    UPDATE prace_dyplomowe SET status = @status_id WHERE praca_id = @praca_id;
    IF @@ROWCOUNT = 0 RAISERROR('Praca o podanym ID nie istnieje.', 16, 1);
END;
GO

CREATE OR ALTER PROCEDURE sp_dodaj_recenzje
    @praca_id INT, @recenzent_id INT, @tresc NVARCHAR(MAX), @ocena DECIMAL(3,1), @data_wystawienia DATE
AS BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO recenzje (praca_id, recenzent_id, tresc, ocena, data_wystawienia)
        VALUES (@praca_id, @recenzent_id, NULLIF(@tresc, ''), @ocena, @data_wystawienia);
    END TRY
    BEGIN CATCH
        DECLARE @msg1 NVARCHAR(2048) = ERROR_MESSAGE(), @num1 INT = ERROR_NUMBER();
        IF @num1 = 2627 RAISERROR('Ten recenzent juz ocenial te prace.', 16, 1);
        ELSE RAISERROR(@msg1, 16, 1);
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_dodaj_uzytkownika
    @pesel VARCHAR(11), @imie VARCHAR(20), @nazwisko VARCHAR(20),
    @email VARCHAR(100), @telefon VARCHAR(15), @funkcja VARCHAR(30)
AS BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @funkcja_id INT;
        SELECT @funkcja_id = funkcja_id FROM funkcje WHERE nazwa = @funkcja;
        IF @funkcja_id IS NULL BEGIN RAISERROR('Podana funkcja nie istnieje.', 16, 1); RETURN; END
        INSERT INTO uzytkownicy (pesel, imie, nazwisko, email, telefon, funkcja)
        VALUES (@pesel, @imie, @nazwisko, @email, NULLIF(@telefon, ''), @funkcja_id);
    END TRY
    BEGIN CATCH
        DECLARE @msg2 NVARCHAR(2048) = ERROR_MESSAGE(), @num2 INT = ERROR_NUMBER();
        IF @num2 = 2627 AND @msg2 LIKE '%pesel%'
            RAISERROR('Uzytkownik z takim PESEL juz istnieje.', 16, 1);
        ELSE IF @num2 = 2627
            RAISERROR('Uzytkownik z takim adresem email juz istnieje.', 16, 1);
        ELSE IF @num2 = 547 AND @msg2 LIKE '%CHK_pesel%'
            RAISERROR('Nieprawidlowy PESEL – musi zawierac dokladnie 11 cyfr.', 16, 1);
        ELSE IF @num2 = 547 AND @msg2 LIKE '%CHK_email%'
            RAISERROR('Nieprawidlowy adres email – musi zawierac znak @.', 16, 1);
        ELSE IF @num2 = 547
            RAISERROR('Dane naruszaja reguly walidacji bazy danych.', 16, 1);
        ELSE RAISERROR(@msg2, 16, 1);
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_dodaj_wydzial
    @nazwa VARCHAR(50), @dziekan_id INT = NULL
AS BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @dziekan_id IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM uzytkownicy u JOIN funkcje f ON f.funkcja_id = u.funkcja
            WHERE u.uzytkownik_id = @dziekan_id AND f.nazwa = 'pracownik uczelni'
        ) BEGIN RAISERROR('Dziekan musi byc pracownikiem uczelni.', 16, 1); RETURN; END
        INSERT INTO wydzialy (nazwa, dziekan) VALUES (@nazwa, @dziekan_id);
    END TRY
    BEGIN CATCH
        DECLARE @msg4 NVARCHAR(2048) = ERROR_MESSAGE();
        RAISERROR(@msg4, 16, 1);
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_dodaj_kierunek
    @nazwa VARCHAR(50), @wydzial_id INT, @stopien VARCHAR(30)
AS BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM wydzialy WHERE wydzial_id = @wydzial_id)
            BEGIN RAISERROR('Podany wydzial nie istnieje.', 16, 1); RETURN; END
        INSERT INTO kierunki (nazwa, wydzial, stopien) VALUES (@nazwa, @wydzial_id, @stopien);
    END TRY
    BEGIN CATCH
        DECLARE @msg5 NVARCHAR(2048) = ERROR_MESSAGE();
        RAISERROR(@msg5, 16, 1);
    END CATCH
END;
GO
