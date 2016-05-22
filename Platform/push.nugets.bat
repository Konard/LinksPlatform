cd Platform.WindowsAPI
dotnet pack -c Release
cd bin\Release
nuget push *.symbols.nupkg
del *.symbols.nupkg
nuget push *.nupkg
del *.nupkg
cd ..\..\..
cd Platform.Helpers
dotnet pack -c Release
cd bin\Release
nuget push *.symbols.nupkg
del *.symbols.nupkg
nuget push *.nupkg
del *.nupkg
cd ..\..\..
cd Platform.Memory
dotnet pack -c Release
cd bin\Release
nuget push *.symbols.nupkg
del *.symbols.nupkg
nuget push *.nupkg
del *.nupkg
cd ..\..\..
cd Platform.Communication
dotnet pack -c Release
cd bin\Release
nuget push *.symbols.nupkg
del *.symbols.nupkg
nuget push *.nupkg
del *.nupkg
cd ..\..\..
cd Platform.Data.Core
dotnet pack -c Release
cd bin\Release
nuget push *.symbols.nupkg
del *.symbols.nupkg
nuget push *.nupkg
del *.nupkg
cd ..\..\..
cd Platform.Tests
dotnet pack -c Release
cd bin\Release
nuget push *.symbols.nupkg
del *.symbols.nupkg
nuget push *.nupkg
del *.nupkg
cd ..\..\..