cd artifacts\bin\Platform.WindowsAPI\Release
nuget push Platform.WindowsAPI.*.nupkg
nuget push Platform.WindowsAPI.*.symbols.nupkg
cd ..\..\..\..
cd artifacts\bin\Platform.Helpers\Release
nuget push Platform.Helpers.*.nupkg
nuget push Platform.Helpers.*.symbols.nupkg
cd ..\..\..\..
cd artifacts\bin\Platform.Memory\Release
nuget push Platform.Memory.*.nupkg
nuget push Platform.Memory.*.symbols.nupkg
cd ..\..\..\..
cd artifacts\bin\Platform.Communication\Release
nuget push Platform.Communication.*.nupkg
nuget push Platform.Communication.*.symbols.nupkg
cd ..\..\..\..
cd artifacts\bin\Platform.Data.Core\Release
nuget push Platform.Data.Core.*.nupkg
nuget push Platform.Data.Core.*.symbols.nupkg
cd ..\..\..\..
cd artifacts\bin\Platform.Tests\Release
nuget push Platform.Tests.*.nupkg
nuget push Platform.Tests.*.symbols.nupkg
cd ..\..\..\..