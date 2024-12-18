use BankingAplication

-- ���������� �������
-- ������ 1
-- ����������� ��������� ������ ���������� ���� ��������� ������ �������� AFTER INSERT
CREATE TRIGGER LogDepositTransaction
ON Deposits
AFTER INSERT
AS
BEGIN
    DECLARE @ID_Deposit INT, @DepositAmount DECIMAL(18, 2);

    -- ��������� ����� ��� ����� ������� �� ������� INSERTED
    SELECT @ID_Deposit = ID_Deposit, @DepositAmount = depositAmount
    FROM INSERTED;

    -- ��������� ������ ��� ���������� ��� �������� � ������� Transactions
    INSERT INTO Transactions (transactionType, transactionDestination, transactionDate, transactionNumber, transactionValue)
    VALUES ('Deposit Created', 'Bank', GETDATE(), CONCAT('DP-', @ID_Deposit), @DepositAmount);
END;

SELECT name, is_disabled
FROM sys.triggers
WHERE name = 'LogDepositTransaction';

-- ������ 2
-- ������ ��� ������� ������� ���� ������� � ��� �������������
CREATE TRIGGER trg_UpdateCardBalanceOnCredit
ON Credits
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- �������� ���� ��� �������
    DECLARE @creditTotalSum DECIMAL(18, 2),
            @creditSum DECIMAL(18, 2),
            @repaymentSum DECIMAL(18, 2),
            @ID_Card INT,
            @creditStatus BIT,
            @isInsert BIT;

    -- �������� �������� � ����������� ��� ���������� ������
    SELECT TOP 1
        @creditTotalSum = INSERTED.creditTotalSum,
        @creditSum = INSERTED.creditSum,
        @repaymentSum = INSERTED.repaymentSum,
        @ID_Card = INSERTED.ID_Card,
        @creditStatus = INSERTED.creditStatus
    FROM INSERTED;

    -- ��������, �� �� ����� ����� (INSERT) �� ��������� (UPDATE)
    SELECT TOP 1 @isInsert = CASE WHEN DELETED.ID_Credit IS NULL THEN 1 ELSE 0 END
    FROM INSERTED
    LEFT JOIN DELETED ON INSERTED.ID_Credit = DELETED.ID_Credit;

    -- ���� ����� ����� (INSERT)
    IF @isInsert = 1
    BEGIN
        -- ������ �� ������� ����� ���� ������� �� ������������ ��������� �������
        UPDATE BankingCard
        SET balance = balance + (@creditTotalSum - ISNULL(@repaymentSum, 0))
        WHERE ID_Card = @ID_Card;
    END
    ELSE
    BEGIN
        -- ���� ��������� ������ (UPDATE)
        DECLARE @prevCreditTotalSum DECIMAL(18, 2),
                @prevRepaymentSum DECIMAL(18, 2);

        -- �������� �������� ��������
        SELECT 
            @prevCreditTotalSum = DELETED.creditTotalSum,
            @prevRepaymentSum = DELETED.repaymentSum
        FROM DELETED
        WHERE DELETED.ID_Credit = @ID_Card;

        -- ���������� ���� � �������� ��� �� ��������
        DECLARE @totalChange DECIMAL(18, 2);
        SET @totalChange = (@creditTotalSum - ISNULL(@repaymentSum, 0)) 
                           - (@prevCreditTotalSum - ISNULL(@prevRepaymentSum, 0));

        -- ��������� ������ ����� � ����������� ���
        UPDATE BankingCard
        SET balance = balance + @totalChange
        WHERE ID_Card = @ID_Card;
    END
END;

-- ������ 3
-- ����������� ����� ����� � ��������� ������ �볺���, ���� ����������� ����� ������� � ��� �����
CREATE TRIGGER DeductFromCardOnDeposit
ON Deposits
AFTER INSERT
AS
BEGIN
    DECLARE @DepositAmount DECIMAL(18, 2), @ID_Klient INT, @ID_Card INT, @Currency NVARCHAR(10), @AvailableBalance DECIMAL(18, 2);

    -- �������� ��� ��� ����� ������� �� ������� INSERTED
    SELECT @DepositAmount = depositAmount, @ID_Klient = ID_Klient
    FROM INSERTED;

    -- ������ ������ �볺��� � ������� UAH � �������� ��������
    SELECT TOP 1 @ID_Card = ID_Card, @AvailableBalance = balance
    FROM BankingCard
    WHERE ID_Klient = @ID_Klient
      AND currency = 'UAH'
      AND balance >= @DepositAmount
    ORDER BY balance DESC; -- �������� ������ � ��������� ��������

    -- ���� ������ ��������, ������� �����
    IF @ID_Card IS NOT NULL
    BEGIN
        UPDATE BankingCard
        SET balance = balance - @DepositAmount
        WHERE ID_Card = @ID_Card;

        -- ��������� ������� �� ����������
        UPDATE Deposits
        SET isProcessed = 1
        WHERE ID_Deposit IN (SELECT ID_Deposit FROM INSERTED);
    END
    ELSE
    BEGIN
        RAISERROR('� �볺��� ����������� ����� ��� �������� � ������.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;

-- ������ 4
-- ������, ��� �� ��� ��������� �볺��� �� ��� ��'���� � ��� ����������� ���������� 
CREATE TRIGGER trg_DeleteKlient
ON Klient
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- ��������� ��������, ���'���� � �볺����
    DELETE FROM Deposits
    WHERE ID_Klient IN (SELECT ID_Klient FROM deleted);

    -- ��������� �������� �����, ���'���� � �볺����
    DELETE FROM BankingCard
    WHERE ID_Klient IN (SELECT ID_Klient FROM deleted);

    -- ��������� ������ �볺���
    DELETE FROM Klient
    WHERE ID_Klient IN (SELECT ID_Klient FROM deleted);
END;


-- ������, ��� �� ��� ��������� �볺��� �� ��� ��'���� � ��� ����������� ���������� 
CREATE TRIGGER trg_DeleteBankingCard
ON BankingCard
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- ��������� ����������, ���'���� � ���������� ������
    DELETE FROM Transactions
    WHERE ID_Card IN (SELECT ID_Card FROM deleted);

    -- ��������� �������, ���'���� � ���������� ������
    DELETE FROM Credits
    WHERE ID_Card IN (SELECT ID_Card FROM deleted);

    -- ��������� ��������� �����
    DELETE FROM BankingCard
    WHERE ID_Card IN (SELECT ID_Card FROM deleted);
END;


DELETE FROM Klient
WHERE ID_Klient = 13;

