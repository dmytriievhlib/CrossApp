CrossApp

Наскрізний проєкт з крос-платформного програмування.

Предметна область

Склад

Сутності:

Product — товар;
StockBatch — партія товару;
Warehouse — склад;
Movement — переміщення товару.

Призначення: застосунок призначений для обліку залишків товарів по партіях та контролю їх переміщення між складами.

Запуск
dotnet build
dotnet run --project src/Cli
Середовище
.NET SDK 8.0
C#
Git
Visual Studio
ОС: Windows
Архітектура: x64
Структура проєкту
CrossApp/
├── .gitignore
├── CrossApp.sln
├── README.md
└── src/
    └── Cli/
        ├── Cli.csproj
        └── Program.cs