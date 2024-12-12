use BankingAplication

--використані процедури
CREATE PROCEDURE GetTotalCounts
AS
BEGIN
    -- Повернення кількості клієнтів за ID
    SELECT COUNT(DISTINCT Klient.ID_Klient) AS TotalCountKlient
    FROM Klient;

    -- Повернення кількості банківських карт
    SELECT COUNT(BankingCard.ID_Card) AS TotalCountBankingCard
    FROM BankingCard;

    -- Повернення кількості депозитів
    SELECT COUNT(Deposits.ID_Deposit) AS TotalCountDeposits
    FROM Deposits;

    -- Повернення кількості кредитів
    SELECT COUNT(Credits.ID_Credit) AS TotalCountCredits
    FROM Credits;
END;

EXEC GetTotalCounts;

--вставка в таблицю клієнта 
CREATE PROCEDURE insert_klient
    @last_Name NVARCHAR(20),
    @first_Name NVARCHAR(20),
    @surname NVARCHAR(20),
    @passport_Number CHAR(9),
    @phone_Number CHAR(13),
    @id_kod CHAR(10),
    @birthday DATE,
    @password_user VARCHAR(50),
    @registration_Date DATE,
    @photoData VARBINARY(MAX)
AS
BEGIN
    INSERT INTO Klient (last_Name, first_Name, surname, passport_Number, phone_Number, id_kod, birthday, password_user, registration_Date, photoData)
    VALUES (@last_Name, @first_Name, @surname, @passport_Number, @phone_Number, @id_kod, @birthday, @password_user, @registration_Date, @photoData);
END;

-- вставка в таблицю банк карти
CREATE PROCEDURE insert_bankingCard
    @cardType NVARCHAR(50),
    @cardNumber NVARCHAR(16),
    @cvvCode NVARCHAR(3),
    @balance DECIMAL(18, 2),
    @currency NVARCHAR(10),
    @paySystem NVARCHAR(50),
    @cardDate DATE,
    @pin INT,
    @ID_Klient INT
AS
BEGIN
    INSERT INTO BankingCard (cardType, cardNumber, cvvCode, balance, currency, paySystem, cardDate, pin, ID_Klient)
    VALUES (@cardType, @cardNumber, @cvvCode, @balance, @currency, @paySystem, @cardDate, @pin, @ID_Klient);
END;

--вставка транзакцій
CREATE PROCEDURE insert_transaction
    @transactionType NVARCHAR(50),
    @transactionDestination NVARCHAR(200),
    @transactionDate DATE,
    @transactionNumber NVARCHAR(50),
    @transactionValue DECIMAL(18, 2),
    @ID_Card INT
AS
BEGIN
    INSERT INTO Transactions (transactionType, transactionDestination, transactionDate, transactionNumber, transactionValue, ID_Card)
    VALUES (@transactionType, @transactionDestination, @transactionDate, @transactionNumber, @transactionValue, @ID_Card);
END;

--вставка сервісів
CREATE PROCEDURE insert_service
    @serviceName NVARCHAR(100),
    @serviceBalance DECIMAL(18, 2),
    @serviceType NVARCHAR(100)
AS
BEGIN
    INSERT INTO Services (serviceName, serviceBalance, serviceType)
    VALUES (@serviceName, @serviceBalance, @serviceType);
END;

--вставка кредитів
CREATE PROCEDURE insert_credit
    @creditTotalSum DECIMAL(18, 2),
    @creditSum DECIMAL(18, 2),
    @creditDate DATE,
    @creditStatus BIT,
    @repaymentDate DATE,
    @repaymentSum DECIMAL(18, 2),
    @ID_Card INT
AS
BEGIN
    INSERT INTO Credits (creditTotalSum, creditSum, creditDate, creditStatus, repaymentDate, repaymentSum, ID_Card)
    VALUES (@creditTotalSum, @creditSum, @creditDate, @creditStatus, @repaymentDate, @repaymentSum, @ID_Card);
END;

--вставка депозитів
CREATE PROCEDURE insert_deposit
    @depositAmount DECIMAL(18, 2),
    @depositDate DATE,
    @interestRate DECIMAL(5, 2),
    @termInMonths INT = 3,
    @isProcessed BIT = 0,
    @ID_Klient INT
AS
BEGIN
    INSERT INTO Deposits (depositAmount, depositDate, interestRate, termInMonths, isProcessed, ID_Klient)
    VALUES (@depositAmount, @depositDate, @interestRate, @termInMonths, @isProcessed, @ID_Klient);
END;






