Param(
  [string]$tag
)

cd c:\test
.\psping64.exe -accepteula -n 60s my.database.windows.net:1433 > .\$tag.psping.txt
.\TestEntityFramework472.exe > .\$tag.ef.txt
Copy-Item -Path .\TestSqlDatabase472.exe.config.open -Destination TestSqlDatabase472.exe.config -Force
.\TestSqlDatabase472.exe > .\$tag.open.txt
Copy-Item -Path .\TestSqlDatabase472.exe.config.query -Destination TestSqlDatabase472.exe.config -Force
.\TestSqlDatabase472.exe > .\$tag.query.txt
