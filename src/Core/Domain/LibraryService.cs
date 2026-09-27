using System;

namespace Core.Domain;

public sealed class LibraryService
{
    private const int MaxOpenLoans = 5;

    public void IssueBook(
        Reader reader,
        BookCopy book)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(book);

        if (reader.OpenLoans >= MaxOpenLoans)
        {
            throw new InvalidOperationException(
                $"Не можна видати книгу: " +
                $"читач {reader.Name} вже має " +
                $"{reader.OpenLoans} відкритих видач. " +
                $"Максимум — {MaxOpenLoans}.");
        }

        book.Issue();
        reader.AddLoan();
    }

    public void ReturnBook(
        Reader reader,
        BookCopy book)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(book);

        book.Return();
        reader.CloseLoan();
    }
}