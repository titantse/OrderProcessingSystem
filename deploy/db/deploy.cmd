@echo off
SET DB_SERVER=%1

IF NOT DEFINED DB_SERVER ( SET DB_SERVER=localhost)

@echo create database
sqlcmd.exe -S %DB_SERVER% -b -E -d OrderDB -i "%~dp0db.sql" >>%~dp0Setup.log || goto ERROR

@echo create table
sqlcmd.exe -S %DB_SERVER% -b -E -d OrderDB -i "%~dp0table.sql" >>%~dp0Setup.log || goto ERROR

@echo create sp
sqlcmd -S %DB_SERVER% -b -E -d OrderDB -o "%~dp0Setup.log" -i "%~dp0sp.sql" -v DBName="%DB_NAME%" || goto ERROR

@echo Deploy succeeded!
exit /b 0

:ERROR
@echo Something error happened during setup, please check!
exit /b 1



