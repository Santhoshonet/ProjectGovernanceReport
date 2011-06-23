@ echo off

xcopy bin\* "C:\Inetpub\wwwroot\wss\VirtualDirectories\80\bin" /s /c /q /h /r /y

xcopy features\* "C:\Program Files\Common Files\Microsoft Shared\web server extensions\12\TEMPLATE\FEATURES" /s /c /q /h /r /y

xcopy layouts\* "C:\Program Files\Common Files\Microsoft Shared\web server extensions\12\TEMPLATE\LAYOUTS" /s /c /q /h /r /y

pause