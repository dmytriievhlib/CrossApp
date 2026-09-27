using System;

namespace Core.Domain;

public sealed class BookCopy
{
    public string Id { get; }
    public string Title { get; }
    public bool IsIssued { get; private set; }

    private BookCopy(
        string id,
        string title)
    {
        Id = id;
        Title = title;
    }

    public static BookCopy Create(
        string id,
        string title)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException(
                "Ідентифікатор примірника обов'язковий",
                nameof(id));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(
                "Назва книги не може бути порожньою",
                nameof(title));

        return new BookCopy(
            id.Trim(),
            title.Trim());
    }

    internal void Issue()
    {
        if (IsIssued)
            throw new InvalidOperationException(
                "Примірник уже виданий");

        IsIssued = true;
    }

    internal void Return()
    {
        if (!IsIssued)
            throw new InvalidOperationException(
                "Примірник не був виданий");

        IsIssued = false;
    }

    public override string ToString() =>
        $"{Id}: {Title}, виданий: {IsIssued}";
}