create table Klient
(
    ID_Klient int identity(1,1) not null primary key,
    last_Name nvarchar(20) check (last_Name like N'[Р-пр-џ]%' and last_Name not like N'%[0-9]%'),
    first_Name nvarchar(20) check (first_Name like N'[Р-пр-џ]%' and first_Name not like N'%[0-9]%'),
    surname nvarchar(20) check (surname like N'[Р-пр-џ]%' and surname not like N'%[0-9]%'),
    passport_Number char(9) check (passport_Number like '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'),
	phone_Number char(13),
	id_kod char(10) check (id_kod like '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'),
	birthday date not null,
	password_user varchar(50) not null,
    registration_Date date
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

update BankingCard set balance = 50000 where ID_Card=3