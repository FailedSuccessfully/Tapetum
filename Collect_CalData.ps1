$g
$a
$m
$timeStart = Get-Date
$timeNow = $timeStart
$timeEnd = $timeStart.AddSeconds(30)
$port= new-Object System.IO.Ports.SerialPort COM3,9600,None,8,one
$port.Open()
while ($timeNow -le $timeEnd){
    Write-Host $port.ReadLine()
    $timeNow = Get-Date
}
$port.Close()
