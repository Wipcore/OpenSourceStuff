# edit with: powershell_ise.exe
# run with: powershell -ExecutionPolicy RemoteSigned -File testports.ps1

<#
   Notice: Powershell remoting requires at least one of three things, all is
   disabled by default but on a corporate network you might have luck with #1:
1. That your computer where you start this script is joined to the same AD
   domain as the computers to where Powershell remoting logins (source below).
2. That you are using https, with certs and stuff. This is the approach Azure
   uses, but requires generating certs on all servers and to open another
   port. On the client it's however possible to ignore invalid ssl certs,
   although less safe. (google how).
3. That you make changes to the global powershell client settings on your
   computer, disabling security and allow sending credentials to any rogue
   domain. You do this by adding TrustedHosts. (google how).

Powershell default tcp ports:
5985 (unencrypted traffic - when using http)
5986 (encrypted traffic - when using https)
#>

Set-StrictMode -v latest
$ErrorActionPreference = "Stop"

function Main()
{
    [Hashtable[]] $tryconnectports = @(@{
        source = "testserver1.wipcore.se"
        target = "testserver2.wipcore.se"
        port = 80
    },@{
        source = "testserver1.wipcore.se"
        target = "NotAValidServer.wipcore.se"
        port = 80
    },@{
        source = "testserver2.wipcore.se"
        target = "testserver1.wipcore.se"
        port = 4321 # probably not open
    },@{
        source = "testserver2.wipcore.se"
        target = "testserver3.wipcore.se"
        port = 80
    })


    [string] $domain = $env:UserDnsDomain.ToLower()
    if (!$domain)
    {
        $domain = "mydomain.tld"
    }
    [string] $username = $env:username
    [string] $fullusername = $username + "@" + $domain
    $cred = Get-Credential $fullusername


    [string[]] $servers = $tryconnectports | % { $_.source } | select -unique

    Write-Host ([Net.Dns]::GetHostName() + ": Connecting to " + $servers.Count + " servers: '" + ($servers -join "', '") + "'")

    Invoke-Command -ComputerName $servers -cred $cred -args $tryconnectports {
        Set-StrictMode -v latest
        $ErrorActionPreference = "Stop"

        [Hashtable[]] $tryconnectports = $args

        $tryconnectports |
            Where-Object { [Net.Dns]::GetHostName() -eq $_.source.Split(".")[0] } |
            ForEach-Object {

            [string] $source = $_.source
            [string] $target = $_.target
            [int] $port = $_.port

            Write-Host ($source + " -> " + $target + ":" + $port + ": Trying to connect...")
            $socket = New-Object Net.Sockets.TcpClient
            try
            {
                $socket.Connect($target, $port)
                if ($socket.Connected)
                {
                    $socket.Close()
                    Write-Host ($source + " -> " + $target + ":" + $port + ": Port is open.") -f Green
                }
                else
                {
                    Write-Host ($source + " -> " + $target + ":" + $port + ": Port is blocked.") -f Red
                }
            }
            catch
            {
                Write-Host ($source + " -> " + $target + ":" + $port + ": Port is blocked.") -f Red
            }
            $socket = $null
        }
    }

    Write-Host ([Net.Dns]::GetHostName() + ": Done!")
    Read-Host
}

Main
