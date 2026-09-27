using System;

namespace Core.Domain;

public sealed class Reader
{
    private int _openLoans;

    public string Id { get; }
    public string Name { get; }
    public int OpenLoans => _openLoans;

    private Reader(
        string id,
        string name)
    {
        Id = id;
        Name = name;
    }

    public static Reader Create(
        string id,
        string name)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException(
                "Ідентифікатор читача обов'язковий",
                nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Ім'я читача не може бути порожнім",
                nameof(name));

        return new Reader(
            id.Trim(),
            name.Trim());
    }

    internal void AddLoan()
    {
        _openLoans++;
    }

    internal void CloseLoan()
    {
        if (_openLoans <= 0)
            throw new InvalidOperationException(
                "У читача немає відкритих видач");

        _openLoans--;
    }

    public override string ToString() =>
        $"{Id}: {Name}, відкритих видач: {OpenLoans}";
}