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

## Лабораторна робота 2

### Структура проєкту

```text
CrossApp/
├── CrossApp.sln
├── README.md
├── .gitignore
└── src/
    ├── Core/
    │   ├── Core.csproj
    │   └── EnvironmentInfo.cs
    └── Cli/
        ├── Cli.csproj
        └── Program.cs
```

### Архітектура

Проєкт складається з двох частин:

* `Core` — бібліотека класів, що містить спільну логіку отримання інформації про середовище виконання.
* `Cli` — консольний застосунок, який використовує бібліотеку `Core`.

Залежність між проєктами одностороння:

```text
Cli → Core
```

`Core` не залежить від `Cli`.

### Multi-targeting

Обидва проєкти налаштовані для компіляції під:

* `net8.0`
* `net10.0`

Збірка обох TFM виконана успішно.

Для демонстрації умовної компіляції використано:

```csharp
#if NET10_0_OR_GREATER
private const string BuildNote = "збірка під net10.0";
#else
private const string BuildNote = "збірка під net8.0";
#endif
```

Перевірка:

```text
net8.0  → збірка під net8.0
net10.0 → збірка під net10.0
```

### Публікація

| Режим                         |           Результат |
| ----------------------------- | ------------------: |
| Framework-dependent `win-x64` |            0,168 МБ |
| Self-contained `win-x64`      |            70,66 МБ |
| Self-contained + SingleFile   |   64,40 МБ, 3 файли |
| Self-contained + Trimmed      | 18,12 МБ, 28 файлів |
| Self-contained `linux-x64`    |            70,53 МБ |

Framework-dependent публікація має значно менший розмір, оскільки .NET Runtime не включається до публікації. Для її запуску на цільовій системі потрібен відповідний встановлений runtime.

Self-contained публікація містить необхідний .NET Runtime, тому має значно більший розмір, але не потребує окремого встановлення .NET Runtime.

### Single-file

Для публікації SingleFile використано параметр:

```text
-p:PublishSingleFile=true
```

Отримано 3 файли загальним розміром 64,40 МБ. Основним виконуваним файлом є `Cli.exe`.

Публікація успішно запускається у Windows.

### Trimming

Для оптимізації розміру використано:

```text
-p:PublishTrimmed=true
```

Отримано 28 файлів загальним розміром 18,12 МБ.

Застосунок після trimming успішно запускається. Під час збірки попереджень та помилок не виникло.

Trimming може створювати проблеми для застосунків, які активно використовують reflection або інший динамічний виклик коду, оскільки тример може видалити код, який не був виявлений як необхідний.

### Linux-публікація

Застосунок було опубліковано для:

```text
linux-x64
```

у режимі self-contained.

Розмір публікації — 70,53 МБ.

Linux-збірку було запущено в Docker-контейнері:

```text
mcr.microsoft.com/dotnet/runtime-deps:10.0
```

Результат запуску:

```text
ОС                  : Ubuntu 24.04.4 LTS
Runtime              : .NET 8.0.31
Архітектура          : X64
RID (визначено)      : linux-x64
RID (від .NET)       : linux-x64
Каталог              : /app/
Примітка збірки      : збірка під net8.0
```

Таким чином, Linux-публікація, створена у Windows, успішно працює в Linux-середовищі Docker.

### Висновок

У лабораторній роботі створено бібліотеку `Core` та консольний застосунок `Cli`, налаштовано залежність `Cli → Core` і multi-targeting для `net8.0` та `net10.0`. Досліджено framework-dependent, self-contained, SingleFile та trimmed режими публікації. Також виконано крос-платформну публікацію `linux-x64` та підтверджено її роботу в Docker-контейнері.
