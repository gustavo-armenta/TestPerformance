# Introduction
A 4.7.2 .NET web application with EntityFramework 6.2.0 and SqlAzureExecutionStrategy running on-premise completes http requests in less than 1.2 seconds. The same application deployed to azure Service Fabric Windows 1803 with Containers and azure sql database premium, the performance drops as some requests complete in less than 4 seconds, others under 30 seconds, and a few under 90 seconds. Looking through the application logs, we see errors, retries, and delays opening sql connections and running sql queries.

## Open Issues
* Opening a sql connection can take 3 seconds to complete
* Opening a sql connection can take 20 seconds to fail with System.ComponentModel.Win32Exception (0x80004005): The semaphore timeout period has expired. We believe SqlConnection.Open() method should be modified to fail fast in less than a second and allow the retry strategy to be useful. 
* Psping shows some delays of 3 seconds because host does not ack the tcp packet and the container has to retransmit.

## About the repro test utilities
* TestEntityFramework472 creates the database schema, tables, and rows
* TestSqlDatabase472 opens sql connections and execute queries without a retry strategy. It is expected that every operation completes or fails in less than 1 second. Otherwise, the retry strategy will add compund delays and a simple operation that usually completes in less than 10 milliseconds would complete in a worst scenario of 2 minutes and 30 seconds.
* TestHttpPerformance472 is used to measure the performance of the full application, not included in this repository.

## Repro in a machine just created
```
powershell
Get-NetIPInterface -AddressFamily IPv4
c:\test\test.ps1 -tag vm_mtu1500
c:\test\docker-compose.exe up -d
docker exec -it test_webapp_1 powershell -Command "c:\test\test.ps1 -tag container_mtu1500"
c:\test\docker-compose.exe down
```

## Repro setting MTU=1440
```
Get-NetIPInterface -AddressFamily IPv4 -NlMtuBytes 1500 | Set-NetIPInterface -NlMtuBytes 1440
Get-NetIPInterface -AddressFamily IPv4
Restart-Computer
powershell
Get-NetIPInterface -AddressFamily IPv4
c:\test\test.ps1 -tag vm_mtu1440
c:\test\docker-compose.exe up -d
docker exec -it test_webapp_1 powershell -Command "c:\test\test.ps1 -tag container_mtu1440"
c:\test\docker-compose.exe down
Get-NetIPInterface -AddressFamily IPv4 -NlMtuBytes 1440 | Set-NetIPInterface -NlMtuBytes 1500
Get-NetIPInterface -AddressFamily IPv4
Restart-Computer
```

## Repro setting MTU=1440, PortChunkSize=100
```
Get-NetIPInterface -AddressFamily IPv4 -NlMtuBytes 1500 | Set-NetIPInterface -NlMtuBytes 1440
Get-NetIPInterface -AddressFamily IPv4
Set-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Services\WinNat -Name PortChunkSize -Value 100
Get-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Services\WinNat -Name PortChunkSize
Restart-Computer
powershell
Get-NetIPInterface -AddressFamily IPv4
Get-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Services\WinNat -Name PortChunkSize
c:\test\test.ps1 -tag vm_mtu1440_portchunk100
c:\test\docker-compose.exe up -d
docker exec -it test_webapp_1 powershell -Command "c:\test\test.ps1 -tag container_mtu1440_portchunk100"
c:\test\docker-compose.exe down
Get-NetIPInterface -AddressFamily IPv4 -NlMtuBytes 1440 | Set-NetIPInterface -NlMtuBytes 1500
Get-NetIPInterface -AddressFamily IPv4
Remove-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Services\WinNat -Name PortChunkSize
Get-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Services\WinNat -Name PortChunkSize
Restart-Computer
```
