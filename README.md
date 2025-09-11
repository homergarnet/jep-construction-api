# barangay-crime-compliant-api
---------------------------------------------------------------------------------------------------------------
How to publish dotnet core using CLI
dotnet publish -c release
---------------------------------------------------------------------------------------------------------------
for dotnet models
//FOR windows
dotnet ef dbcontext scaffold "Server=Homer\MSSQLSERVER02;Database=Jep_Construction;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -o Models --force

//FOR macbook IOS
dotnet ef dbcontext scaffold "Server=localhost;Database=Thesis;User ID=sa;Password=Password!1234; TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o Models --force
---------------------------------------------------------------------------------------------------------------
to access swagger
http://localhost:8001/swagger/index.html
---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
useRoles
ProfileCode	ProfileName
1	Admin
2	Employee
3   Client