use BankingAplication

--створення адміна
CREATE LOGIN admin_bank WITH PASSWORD = 'admin1234';
 
use BankingAplication
CREATE USER owner_bank FOR LOGIN admin_bank;

USE [master];
ALTER SERVER ROLE sysadmin ADD MEMBER admin_bank;


SELECT IS_SRVROLEMEMBER('sysadmin', 'admin_bank');




