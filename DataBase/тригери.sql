use BankingAplication

--використані тригери
--Автоматичне створення запису транзакції після створення нового депозиту AFTER INSERT
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
    VALUES ('Deposit Created', 'Bank', GETDATE(), CONCAT('DP-', @ID_Deposit), @DepositAmount);
END;

SELECT name, is_disabled
FROM sys.triggers
WHERE name = 'LogDepositTransaction';


-- Тригер для обробки кредиту після вставки у вікні адміністрування
CREATE TRIGGER trg_UpdateCardBalanceOnCredit
ON Credits
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Тимчасові змінні для значень
    DECLARE @creditTotalSum DECIMAL(18, 2),
            @creditSum DECIMAL(18, 2),
            @repaymentSum DECIMAL(18, 2),
            @ID_Card INT,
            @creditStatus BIT,
            @isInsert BIT;

    -- Отримуємо значення з вставленого або оновленого запису
    SELECT TOP 1
        @creditTotalSum = INSERTED.creditTotalSum,
        @creditSum = INSERTED.creditSum,
        @repaymentSum = INSERTED.repaymentSum,
        @ID_Card = INSERTED.ID_Card,
        @creditStatus = INSERTED.creditStatus
    FROM INSERTED;

    -- Перевірка, чи це новий запис (INSERT) чи оновлення (UPDATE)
    SELECT TOP 1 @isInsert = CASE WHEN DELETED.ID_Credit IS NULL THEN 1 ELSE 0 END
    FROM INSERTED
    LEFT JOIN DELETED ON INSERTED.ID_Credit = DELETED.ID_Credit;

    -- Якщо новий запис (INSERT)
    IF @isInsert = 1
    BEGIN
        -- Додаємо до балансу повну суму кредиту за вирахуванням виплаченої частини
        UPDATE BankingCard
        SET balance = balance + (@creditTotalSum - ISNULL(@repaymentSum, 0))
        WHERE ID_Card = @ID_Card;
    END
    ELSE
    BEGIN
        -- Якщо оновлення запису (UPDATE)
        DECLARE @prevCreditTotalSum DECIMAL(18, 2),
                @prevRepaymentSum DECIMAL(18, 2);

        -- Отримуємо попередні значення
        SELECT 
            @prevCreditTotalSum = DELETED.creditTotalSum,
            @prevRepaymentSum = DELETED.repaymentSum
        FROM DELETED
        WHERE DELETED.ID_Credit = @ID_Card;

        -- Обчислюємо зміну в кредитній сумі та виплатах
        DECLARE @totalChange DECIMAL(18, 2);
        SET @totalChange = (@creditTotalSum - ISNULL(@repaymentSum, 0)) 
                           - (@prevCreditTotalSum - ISNULL(@prevRepaymentSum, 0));

        -- Оновлюємо баланс карти з урахуванням змін
        UPDATE BankingCard
        SET balance = balance + @totalChange
        WHERE ID_Card = @ID_Card;
    END
END;

--автоматично списує кошти з банківської картки клієнта, коли створюється новий депозит у вікні адміна
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

