# Links Platform | Платформа Связей
## Repository | Репозиторий
https://github.com/Konard/LinksPlatform

## Description | Описание
Holistic system for storage and transformation of information based on associative model of data. Целостная система для хранения и обработки информации основная на ассоциативной модели данных.

## How to run | Как запустить
### Prerequisites | Требования
* Install .NET Core SDK https://www.microsoft.com/net
* Установить .NET Core SDK https://www.microsoft.com/net

### Run Tests | Запустить тесты
* `cd Platform`
* `dotnet restore Platform.dotnet.sln`
* `dotnet build Platform.dotnet.sln`
* `cd Platform.Tests`
* `rm Platform.Tests.Library.csproj`
* `dotnet xunit -parallel none`
