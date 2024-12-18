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
CREATE PROCEDURE delete_transaction
    @id INT
AS
BEGIN
    DELETE FROM Transactions WHERE ID_transaction = @id;
END;

-- ��������� 3.4
-- ��������� ���� � ������� �������
CREATE PROCEDURE delete_credit
    @id INT
AS
BEGIN
    DELETE FROM Credits WHERE ID_Credit = @id;
END;

-- ��������� 3.4
-- ��������� ���� � ������� ��������
CREATE PROCEDURE delete_deposits
    @id INT
AS
BEGIN
    DELETE FROM Deposits WHERE ID_Deposit = @id;
END;

-- ��������� 3.5
-- ��������� ���� � ������� ������
CREATE PROCEDURE delete_services
    @id INT
AS
BEGIN
    DELETE FROM Services WHERE ID_Service = @id;
END;


SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
WHERE TABLE_NAME = 'Deposits';








