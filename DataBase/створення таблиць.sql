create table Klient
(
    ID_Klient int identity(1,1) not null primary key,
    last_Name nvarchar(20) check (last_Name like N'[А-Яа-я]%' and last_Name not like N'%[0-9]%'),
    first_Name nvarchar(20) check (first_Name like N'[А-Яа-я]%' and first_Name not like N'%[0-9]%'),
    surname nvarchar(20) check (surname like N'[А-Яа-я]%' and surname not like N'%[0-9]%'),
    passport_Number char(9) check (passport_Number like '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'),
	phone_Number char(13),
	id_kod char(10) check (id_kod like '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'),
	birthday date not null,
	password_user varchar(50) not null,
    registration_Date date,
	photoData VARBINARY(MAX)
);

create table BankingCard (
    ID_Card int identity(1,1) not null primary key,
	cardType nvarchar(50) not null,
    cardNumber nvarchar(16) not null,
    cvvCode nvarchar(3) not null,
	balance DECIMAL(18, 2),
	currency nvarchar(10) not null,
	paySystem nvarchar(50) not null,
	cardDate date not null, 
	pin int not null
); 

alter table BankingCard add ID_Klient int
alter table BankingCard add foreign key (ID_Klient) references dbo.Klient(ID_Klient) on delete no action on update cascade

create table Transactions(
  ID_transaction int identity(1,1) not null primary key,
  transactionType nvarchar(50) not null,
  transactionDestination nvarchar(200) not null,
  transactionDate date not null, 
  transactionNumber nvarchar(50),
  transactionValue DECIMAL(18, 2)
);

alter table Transactions add ID_Card int
alter table Transactions add foreign key (ID_Card) references dbo.BankingCard(ID_Card) on delete no action on update cascade

create table Services(
	ID_Service int identity(1,1) not null primary key,
	serviceName nvarchar(100) not null,
	serviceBalance DECIMAL(18, 2),
	serviceType nvarchar(100) not null
);

INSERT INTO Services (serviceName, serviceType, serviceBalance) VALUES ('Газопостачання', 'communal', 0);
INSERT INTO Services (serviceName, serviceType, serviceBalance) VALUES ('Водопостачання', 'communal', 0);
INSERT INTO Services (serviceName, serviceType, serviceBalance) VALUES ('Вивезення ТВП', 'communal', 0);
INSERT INTO Services (serviceName, serviceType, serviceBalance) VALUES ('Енергопостачання', 'communal', 0);
INSERT INTO Services (serviceName, serviceType, serviceBalance) VALUES ('Квартплата', 'communal', 0);
INSERT INTO Services (serviceName, serviceType, serviceBalance) VALUES ('Оплата за тепло', 'communal', 0);
INSERT INTO Services (serviceName, serviceType, serviceBalance) VALUES ('Інтернет', 'internet', 0);
INSERT INTO Services (serviceName, serviceType, serviceBalance) VALUES ('Допомога', 'charity', 0);

create table Credits(
	ID_Credit int identity(1,1) not null primary key,
	creditTotalSum  DECIMAL(18, 2),
	creditSum  DECIMAL(18, 2),
	creditDate date not null, 
	creditStatus bit not null default 0,
	repaymentDate date, 
	repaymentSum DECIMAL(18, 2)
);

alter table Credits add ID_Card int
alter table Credits add foreign key (ID_Card) references dbo.BankingCard(ID_Card) on delete no action on update cascade


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



