use BankingAplication

-- ���������� ���������
-- ��������� 1
CREATE PROCEDURE GetTotalCounts
AS
BEGIN
    -- ���������� ������� �볺��� �� ID
    SELECT COUNT(DISTINCT Klient.ID_Klient) AS TotalCountKlient
    FROM Klient;

    -- ���������� ������� ���������� ����
    SELECT COUNT(BankingCard.ID_Card) AS TotalCountBankingCard
    FROM BankingCard;

    -- ���������� ������� ��������
    SELECT COUNT(Deposits.ID_Deposit) AS TotalCountDeposits
    FROM Deposits;

    -- ���������� ������� �������
    SELECT COUNT(Credits.ID_Credit) AS TotalCountCredits
    FROM Credits;
END;

EXEC GetTotalCounts;

-- ��������� 2.1
-- ������� � ������� �볺��� 
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

-- ��������� 2.2
-- ������� � ������� ���� �����
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

-- ��������� 2.3
--������� ����������
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

-- ��������� 2.4
--������� ������
CREATE PROCEDURE insert_service
    @serviceName NVARCHAR(100),
    @serviceBalance DECIMAL(18, 2),
    @serviceType NVARCHAR(100)
AS
BEGIN
    INSERT INTO Services (serviceName, serviceBalance, serviceType)
    VALUES (@serviceName, @serviceBalance, @serviceType);
END;

-- ��������� 2.5
--������� �������
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

-- ��������� 2.6
--������� ��������
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

-- ��������� 3.1
-- ��������� ���� � ������� �볺��
CREATE PROCEDURE delete_klient
    @id INT
AS
BEGIN
    DELETE FROM Klient WHERE ID_Klient = @id;
END;

-- ��������� 3.2
-- ��������� ���� � ������� ��������� �����
CREATE PROCEDURE delete_bankingCard
    @id INT
AS
BEGIN
    DELETE FROM BankingCard WHERE ID_Card = @id;
END;

-- ��������� 3.3
-- ��������� ���� � ������� ����������
CREATE PROCEDURE delete_transactions
    @id INT
AS
BEGIN
    DELETE FROM Transactions WHERE ID_transaction = @id;
END;

DROP PROCEDURE delete_credit

-- ��������� 3.4
-- ��������� ���� � ������� �������
CREATE PROCEDURE delete_credits
    @id INT
AS
BEGIN
    DELETE FROM Credits WHERE ID_Credit = @id;
END;

-- ��������� 3.5
-- ��������� ���� � ������� ��������
CREATE PROCEDURE delete_deposits
    @id INT
AS
BEGIN
    DELETE FROM Deposits WHERE ID_Deposit = @id;
END;

-- ��������� 3.6
-- ��������� ���� � ������� ������
CREATE PROCEDURE delete_services
    @id INT
AS
BEGIN
    DELETE FROM Services WHERE ID_Service = @id;
END;


-- ��������� 4.1
-- ��������� ���� � ������� �볺���
CREATE PROCEDURE update_klient
    @ID_Klient INT,
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
    UPDATE Klient
    SET last_Name = @last_Name,
        first_Name = @first_Name,
        surname = @surname,
        passport_Number = @passport_Number,
        phone_Number = @phone_Number,
        id_kod = @id_kod,
        birthday = @birthday,
        password_user = @password_user,
        registration_Date = @registration_Date,
        photoData = @photoData
    WHERE ID_Klient = @ID_Klient;
END;

-- ��������� 4.2
-- ��������� ���� � ������� ����
CREATE PROCEDURE update_bankingcard
    @ID_Card INT,
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
    UPDATE BankingCard
    SET cardType = @cardType,
        cardNumber = @cardNumber,
        cvvCode = @cvvCode,
        balance = @balance,
        currency = @currency,
        paySystem = @paySystem,
        cardDate = @cardDate,
        pin = @pin,
        ID_Klient = @ID_Klient
    WHERE ID_Card = @ID_Card;
END;

-- ��������� 4.3
-- ��������� ���� � ������� ����������
CREATE PROCEDURE update_transactions
    @ID_transaction INT,
    @transactionType NVARCHAR(50),
    @transactionDestination NVARCHAR(200),
    @transactionDate DATE,
    @transactionNumber NVARCHAR(50),
    @transactionValue DECIMAL(18, 2),
    @ID_Card INT
AS
BEGIN
    UPDATE Transactions
    SET transactionType = @transactionType,
        transactionDestination = @transactionDestination,
        transactionDate = @transactionDate,
        transactionNumber = @transactionNumber,
        transactionValue = @transactionValue,
        ID_Card = @ID_Card
    WHERE ID_transaction = @ID_transaction;
END;

-- ��������� 4.4
-- ��������� ���� � ������� ������
CREATE PROCEDURE update_services
    @ID_Service INT,
    @serviceName NVARCHAR(100),
    @serviceBalance DECIMAL(18, 2),
    @serviceType NVARCHAR(100)
AS
BEGIN
    UPDATE Services
    SET serviceName = @serviceName,
        serviceBalance = @serviceBalance,
        serviceType = @serviceType
    WHERE ID_Service = @ID_Service;
END;

-- ��������� 4.5
-- ��������� ���� � ������� �������
CREATE PROCEDURE update_credits
    @ID_Credit INT,
    @creditTotalSum DECIMAL(18, 2),
    @creditSum DECIMAL(18, 2),
    @creditDate DATE,
    @creditStatus BIT,
    @repaymentDate DATE,
    @repaymentSum DECIMAL(18, 2),
    @ID_Card INT
AS
BEGIN
    UPDATE Credits
    SET creditTotalSum = @creditTotalSum,
        creditSum = @creditSum,
        creditDate = @creditDate,
        creditStatus = @creditStatus,
        repaymentDate = @repaymentDate,
        repaymentSum = @repaymentSum,
        ID_Card = @ID_Card
    WHERE ID_Credit = @ID_Credit;
END;

-- ��������� 4.6
-- ��������� ���� � ������� ��������
CREATE PROCEDURE update_deposits
    @ID_Deposit INT,
    @depositAmount DECIMAL(18, 2),
    @depositDate DATE,
    @interestRate DECIMAL(5, 2),
    @termInMonths INT,
    @isProcessed BIT,
    @ID_Klient INT
AS
BEGIN
    UPDATE Deposits
    SET depositAmount = @depositAmount,
        depositDate = @depositDate,
        interestRate = @interestRate,
        termInMonths = @termInMonths,
        isProcessed = @isProcessed,
        ID_Klient = @ID_Klient
    WHERE ID_Deposit = @ID_Deposit;
END;

-- ��������� 5
-- ��������� ���������� ��� �볺���
CREATE PROCEDURE GetClientDetails
    @ID_Klient INT
AS
BEGIN
    SET NOCOUNT ON;

    -- �������� ��� �볺���
    SELECT 
        k.ID_Klient,
        k.last_Name,
        k.first_Name,
        k.surname,
        k.passport_Number,
        k.phone_Number,
        k.id_kod,
        k.birthday,
        k.registration_Date
    FROM Klient k
    WHERE k.ID_Klient = @ID_Klient;

    -- �������� ��� ���������� ���� �볺���
    SELECT 
        b.ID_Card,
        b.cardType,
        b.cardNumber,
        b.balance,
        b.currency
    FROM BankingCard b
    WHERE b.ID_Klient = @ID_Klient;

    -- �������� ��� �������� �볺���
    SELECT 
        d.ID_Deposit,
        d.depositAmount,
        d.depositDate,
        d.interestRate,
        d.termInMonths,
        d.isProcessed
    FROM Deposits d
    WHERE d.ID_Klient = @ID_Klient;

    -- �������� ��� �������, ��������� �� ������� �볺���
    SELECT 
        c.ID_Credit,
        c.creditTotalSum,
        c.creditSum,
        c.creditDate,
        c.creditStatus,
        c.repaymentDate
    FROM Credits c
    INNER JOIN BankingCard b ON c.ID_Card = b.ID_Card
    WHERE b.ID_Klient = @ID_Klient;

	-- �������� ��� ����������, ��������� �� ������� �볺���
    SELECT 
        t.ID_transaction,
        t.transactionType,
        t.transactionDestination,
        t.transactionDate,
        t.transactionNumber,
        t.transactionValue,
        t.ID_Card
    FROM Transactions t
    INNER JOIN BankingCard b ON t.ID_Card = b.ID_Card
    WHERE b.ID_Klient = @ID_Klient;
END;

DROP PROCEDURE SearchClients;


-- ��������� 6
-- ����� �볺���
CREATE PROCEDURE SearchClients
    @SearchTerm NVARCHAR(50)
AS
BEGIN
    SELECT ID_Klient, last_Name, first_Name, surname, phone_Number
    FROM Klient
    WHERE 
        last_Name LIKE @SearchTerm OR
        first_Name LIKE @SearchTerm OR
        surname LIKE @SearchTerm
    ORDER BY last_Name, first_Name, surname;
END;

-- ��������� 7
-- ��������� �볺��� �� ������ �����
CREATE PROCEDURE GetNewClientsByDateRange
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    SELECT 
        ID_Klient,
		last_Name,
        first_Name,
        surname,
        registration_Date
    FROM Klient
    WHERE registration_Date BETWEEN @StartDate AND @EndDate
    ORDER BY registration_Date ASC;
END;

-- ��������� 8
-- ������� �������� � ������� transactionType ��� ���������� ComboBox
CREATE PROCEDURE GetTransactionTypes
AS
BEGIN
    SELECT DISTINCT transactionType FROM Transactions;
END;

-- ��������� 9
-- ��������� ���������� �� ��������� ����������� �� �����
CREATE PROCEDURE GetTransactions
    @TransactionType NVARCHAR(50) = NULL -- NULL ������, �� ���������� �� ���������������
AS
BEGIN
    IF @TransactionType IS NULL
        SELECT * FROM Transactions
    ELSE
        SELECT * FROM Transactions WHERE transactionType = @TransactionType;
END;

-- ��������� 10
-- ���������� ��� ����������
CREATE PROCEDURE GetTransactionStatistics
AS
BEGIN
    SET NOCOUNT ON;

    -- �������������� ��� ����������
    SELECT TOP 1 transactionType, COUNT(*) AS TransactionCount
    FROM Transactions
    GROUP BY transactionType
    ORDER BY COUNT(*) DESC;

    -- ������� ���������� ��� ����������
    SELECT TOP 1 transactionType, COUNT(*) AS TransactionCount
    FROM Transactions
    GROUP BY transactionType
    ORDER BY COUNT(*) ASC;
END;





