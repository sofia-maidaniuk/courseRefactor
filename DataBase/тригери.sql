use BankingAplication

-- використані тригери
-- ТРИГЕР 1
-- Автоматичне створення запису транзакції після створення нового депозиту AFTER INSERT
CREATE TRIGGER LogDepositTransaction
ON Deposits
AFTER INSERT
AS
BEGIN
    DECLARE @ID_Deposit INT, @DepositAmount DECIMAL(18, 2);

    -- Отримання даних про новий депозит із таблиці INSERTED
    SELECT @ID_Deposit = ID_Deposit, @DepositAmount = depositAmount
    FROM INSERTED;

    -- Додавання запису про транзакцію для депозиту в таблицю Transactions
    INSERT INTO Transactions (transactionType, transactionDestination, transactionDate, transactionNumber, transactionValue)
    VALUES ('Оформлення депозиту', 'LaBank', GETDATE(), CONCAT('DP-', @ID_Deposit), @DepositAmount);
END;

SELECT name, is_disabled
FROM sys.triggers
WHERE name = 'LogDepositTransaction';

-- ТРИГЕР 2
-- Тригер для обробки кредиту після вставки у вікні адміністрування
CREATE TRIGGER trg_UpdateCardBalanceOnCredit
ON Credits
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @creditTotalSum DECIMAL(18, 2),
            @creditSum DECIMAL(18, 2),
            @repaymentSum DECIMAL(18, 2),
            @ID_Card INT,
            @isInsert BIT;

    -- Отримуємо значення з INSERTED
    SELECT TOP 1
        @creditTotalSum = ISNULL(INSERTED.creditTotalSum, 0),
        @creditSum = ISNULL(INSERTED.creditSum, 0),
        @repaymentSum = ISNULL(INSERTED.repaymentSum, 0),
        @ID_Card = INSERTED.ID_Card
    FROM INSERTED;

    -- Визначаємо, це INSERT чи UPDATE
    SELECT TOP 1 @isInsert = CASE WHEN DELETED.ID_Credit IS NULL THEN 1 ELSE 0 END
    FROM INSERTED
    LEFT JOIN DELETED ON INSERTED.ID_Credit = DELETED.ID_Credit;

    IF @isInsert = 1
    BEGIN
        -- Новий запис: додаємо загальну суму кредиту мінус виплати
        UPDATE BankingCard
        SET balance = ISNULL(balance, 0) + (@creditTotalSum - @repaymentSum)
        WHERE ID_Card = @ID_Card;
    END
    ELSE
    BEGIN
        -- Оновлення запису: враховуємо зміни у значеннях
        DECLARE @prevCreditTotalSum DECIMAL(18, 2),
                @prevRepaymentSum DECIMAL(18, 2);

        SELECT TOP 1 
            @prevCreditTotalSum = ISNULL(DELETED.creditTotalSum, 0),
            @prevRepaymentSum = ISNULL(DELETED.repaymentSum, 0)
        FROM DELETED;

        DECLARE @change DECIMAL(18, 2);
        SET @change = ((@creditTotalSum - @repaymentSum) - (@prevCreditTotalSum - @prevRepaymentSum));

        UPDATE BankingCard
        SET balance = ISNULL(balance, 0) + @change
        WHERE ID_Card = @ID_Card;
    END
END;



-- ТРИГЕР 3
-- автоматично списує кошти з банківської картки клієнта, коли створюється новий депозит у вікні адміна
CREATE TRIGGER DeductFromCardOnDeposit
ON Deposits
AFTER INSERT
AS
BEGIN
    DECLARE @DepositAmount DECIMAL(18, 2), @ID_Klient INT, @ID_Card INT, @Currency NVARCHAR(10), @AvailableBalance DECIMAL(18, 2);

    -- Отримуємо дані про новий депозит із таблиці INSERTED
    SELECT @DepositAmount = depositAmount, @ID_Klient = ID_Klient
    FROM INSERTED;

    -- Шукаємо картку клієнта з валютою UAH і достатнім балансом
    SELECT TOP 1 @ID_Card = ID_Card, @AvailableBalance = balance
    FROM BankingCard
    WHERE ID_Klient = @ID_Klient
      AND currency = 'UAH'
      AND balance >= @DepositAmount
    ORDER BY balance DESC; -- Вибираємо картку з найбільшим балансом

    -- Якщо картка знайдена, списуємо кошти
    IF @ID_Card IS NOT NULL
    BEGIN
        UPDATE BankingCard
        SET balance = balance - @DepositAmount
        WHERE ID_Card = @ID_Card;

        -- Позначаємо депозит як оброблений
        UPDATE Deposits
        SET isProcessed = 1
        WHERE ID_Deposit IN (SELECT ID_Deposit FROM INSERTED);
    END
    ELSE
    BEGIN
        RAISERROR('У клієнта недостатньо коштів для списання з картки.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;

-- ТРИГЕР 4
-- тригер, щоб під час видалення клієнта усі дані по'язані з ним автоматично видалялися 
CREATE TRIGGER trg_DeleteKlient
ON Klient
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Видаляємо депозити, пов'язані з клієнтом
    DELETE FROM Deposits
    WHERE ID_Klient IN (SELECT ID_Klient FROM deleted);

    -- Видаляємо банківські карти, пов'язані з клієнтом
    DELETE FROM BankingCard
    WHERE ID_Klient IN (SELECT ID_Klient FROM deleted);

    -- Видаляємо самого клієнта
    DELETE FROM Klient
    WHERE ID_Klient IN (SELECT ID_Klient FROM deleted);
END;


-- тригер, щоб під час видалення клієнта усі дані по'язані з ним автоматично видалялися 
CREATE TRIGGER trg_DeleteBankingCard
ON BankingCard
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Видаляємо транзакції, пов'язані з банківською картою
    DELETE FROM Transactions
    WHERE ID_Card IN (SELECT ID_Card FROM deleted);

    -- Видаляємо кредити, пов'язані з банківською картою
    DELETE FROM Credits
    WHERE ID_Card IN (SELECT ID_Card FROM deleted);

    -- Видаляємо банківську карту
    DELETE FROM BankingCard
    WHERE ID_Card IN (SELECT ID_Card FROM deleted);
END;

