@ECHO OFF

dotnet tool restore
dotnet build -- %*

AddToPath "CentralPackageManagement\bin\Debug"
