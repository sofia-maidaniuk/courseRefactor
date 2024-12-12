use BankingAplication

--створення адміна
CREATE LOGIN admin_bank WITH PASSWORD = 'admin1234';
 
 use BankingAplication
 CREATE USER owner_bank FOR LOGIN admin_bank;

 USE [master];
ALTER SERVER ROLE sysadmin ADD MEMBER admin_bank;


SELECT IS_SRVROLEMEMBER('sysadmin', 'admin_bank');

-- автоматичне нарахування коштів для депозиту
CREATE TABLE Deposits (
    ID_Deposit INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    depositAmount DECIMAL(18, 2) NOT NULL,
    depositDate DATE NOT NULL,
    interestRate DECIMAL(5, 2)
);

ALTER TABLE Deposits ADD termInMonths INT NOT NULL DEFAULT 3;
ALTER TABLE Deposits ADD isProcessed BIT DEFAULT 0;

INSERT INTO Deposits (depositAmount, depositDate, interestRate, ID_Klient, termInMonths, isProcessed) 
VALUES (3000.00, '2024-09-10', 10.00, 1, 3, 0);



