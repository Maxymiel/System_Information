rmdir Publish /S /Q

dotnet clean
dotnet clean -c Release

dotnet publish ConverterHTML\ConverterHTML.csproj -c Release -f net40 -o Publish\Net40\ConverterHTML
dotnet publish System_Information\System_Information.csproj -c Release -f net40 -o Publish\Net40\GenerateInformation
xcopy /E HTML Publish\Net40\HTML\

dotnet publish ConverterHTML\ConverterHTML.csproj -c Release -f net48 -o Publish\Net48\ConverterHTML
dotnet publish System_Information\System_Information.csproj -c Release -f net48 -o Publish\Net48\GenerateInformation
xcopy /E HTML Publish\Net48\HTML\