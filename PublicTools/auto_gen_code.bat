
SET IMPORT_PATH=../proto/
SET CSHARP_PATH=../src/csharp

pushd ToolBin

protoc.exe ../proto/*.proto --csharp_out=%CSHARP_PATH% -I=%IMPORT_PATH%
IF not %ERRORLEVEL% == 0 exit  %ERRORLEVEL%

PServicePugin.exe dir:../proto file:*.proto saveto:%CSHARP_PATH% version:0.0.1
IF not %ERRORLEVEL% == 0 exit  %ERRORLEVEL%

ExcelOut.exe dir:../econfigs namespace:EConfig exportJson:../src/json/ exportCs:%CSHARP_PATH%/ExcelConfig.cs ex:*.xlsx debug:false
IF not %ERRORLEVEL% == 0 exit  %ERRORLEVEL%
popd

C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\csc  /lib:%cd%/ToolBin /warn:1 /target:library /out:%cd%\src\output\Xsoft.Proto.dll /doc:%cd%\src\output\Xsoft.Proto.xml /reference:Google.Protobuf.dll %cd%\src\csharp\*.cs
IF not %ERRORLEVEL% == 0 exit  %ERRORLEVEL%

Copy src\output\  ..\client\Assets\Plugins\CoreDll\ /Y
IF not %ERRORLEVEL% == 0 exit  %ERRORLEVEL%
Copy src\json\  ..\client\Assets\Resources\Json\  /Y
IF not %ERRORLEVEL% == 0 exit  %ERRORLEVEL%
Copy src\json\  ..\Server\Configs\ /Y
IF not %ERRORLEVEL% == 0 exit  %ERRORLEVEL%



