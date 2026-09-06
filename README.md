# CrossApp

Наскрізний проєкт з крос-платформного програмування.

## Предметна область

**Склад**

### Сутності

* `Product` — товар;
* `StockBatch` — партія товару;
* `Warehouse` — склад;
* `Movement` — рух товару.

### Призначення

Програма призначена для обліку залишків товарів по партіях та контролю їх руху між складами.

## Технології

* .NET 8
* C#
* Console Application
* Git
* GitHub

## Структура проєкту

```text
CrossApp/
├── .git/
├── .gitignore
├── CrossApp.sln
├── README.md
└── src/
    └── Cli/
        ├── Cli.csproj
        └── Program.cs
```

## Перевірка середовища

Використано:

* OS: Windows 10
* Architecture: x64
* .NET SDK: 8.0.403
* .NET Runtime: 8.0.10
* Git: 2.51.0.windows.1

## Запуск

Для запуску консольного застосунку:

```powershell
dotnet run --project src/Cli
```

Програма виводить інформацію про:

* операційну систему;
* середовище виконання;
* архітектуру процесу;
* версію .NET;
* каталог застосунку;
* поточний каталог;
* предметну область;
* сутності проєкту.

## Додаткове завдання 1 — Self-contained публікація

Виконано публікацію застосунку без залежності від попередньо встановленого .NET Runtime.

### Windows x64

```powershell
dotnet publish src/Cli -c Release -r win-x64 --self-contained true
```

Розмір каталогу публікації:

**70,65 MB**

### Linux x64

```powershell
dotnet publish src/Cli -c Release -r linux-x64 --self-contained true
```

Розмір каталогу публікації:

**70,57 MB**

### Порівняння

| Runtime Identifier |   Розмір |
| ------------------ | -------: |
| `win-x64`          | 70,65 MB |
| `linux-x64`        | 70,57 MB |

Різниця становить приблизно **0,08 MB**, тому розміри self-contained публікацій для Windows і Linux є практично однаковими.

## Додаткове завдання 2 — JSON-режим

До консольного застосунку додано підтримку аргументу `--json`.

Звичайний запуск:

```powershell
dotnet run --project src/Cli
```

Запуск у JSON-режимі:

```powershell
dotnet run --project src/Cli -- --json
```

У JSON-режимі вся інформація виводиться одним рядком у форматі JSON за допомогою `System.Text.Json`.

Приклад:

```json
{
  "Application": "CrossApp",
  "Student": "Дмитрієв Гліб Олександрович",
  "Group": "ФЕІ-36",
  "ProcessArchitecture": "X64",
  "DotNetVersion": "8.0.10",
  "Runtime": ".NET 8.0.10",
  "Domain": "Склад",
  "Entities": [
    "Product",
    "StockBatch",
    "Warehouse",
    "Movement"
  ]
}
```

## Git

Проєкт зберігається у Git-репозиторії.

Основна гілка:

```text
main
```

Репозиторій GitHub:

`https://github.com/dmytriievhlib/CrossApp.git`

Перший коміт:

```text
lab01: solution CrossApp, Cli, вибір домену
```

Другий коміт:

```text
lab01: self-contained publish and JSON output
```

## Результат

У межах лабораторної роботи створено початковий каркас крос-платформного проєкту `CrossApp`, налаштовано середовище .NET 8, Git-репозиторій та консольний застосунок. Перевірено компіляцію і запуск програми, виконано self-contained публікацію для Windows та Linux, а також реалізовано додатковий JSON-режим.
