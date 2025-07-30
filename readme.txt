russian: 
В ОС Windows существует проблема сохранения учетных данных для подключения по RDP (mstsc.exe)
Данная программа позволяет определить сетевые алиасы для хостов и связь с учетными данными,
а также предоставляет масксимально удобный способ сохранения учетных данных и настройки 
локальных записей dns (%windir%/system32/drivers/etc/hosts)
english:
Windows has a problem saving RDP connection credentials (mstsc.exe)
This program allows you to define network aliases for hosts and communication with credentials,
and provides a maximally convenient way to save credentials and settings
local dns records (% windir %/system32/drivers/etc/hosts)

build instruction:
    1. check installed NET SDK 9
    2. clone this repo
    3. open console in repo folder
    4. run `dotnet build -c release`
    5. after build if bin\release\net9.0-windows you find exe-file
    6. start it and enjoy!

example: 
if you have pc1 
and every time rdp request password when 
1. add record pc1 from Servers menu
2. add entry to hosts (some host alias as added in 1 and real ip)
3. add login via cmdkey from main menu
4. test as Servers/Connect

