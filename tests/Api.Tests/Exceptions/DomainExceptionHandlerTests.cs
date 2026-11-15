
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using Api.Exceptions;
using Core;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Api.Tests.Exceptions;

public class DomainExceptionHandlerTests
{
    [Fact]
    public async Task DomainException_Returns409Conflict()
    {
        // Arrange
        var handler = new DomainExceptionHandler();
        var context = new DefaultHttpContext();

        context.Response.Body = new MemoryStream();

        var exception = new DomainException(
            "Товар із таким SKU вже існує.");

        // Act
        bool handled = await handler.TryHandleAsync(
            context,
            exception,
            CancellationToken.None);

        // Assert
        Assert.True(handled);

        Assert.Equal(
            StatusCodes.Status409Conflict,
            context.Response.StatusCode);

        context.Response.Body.Position = 0;

        using var reader = new StreamReader(context.Response.Body);
        string responseBody = await reader.ReadToEndAsync();

        Assert.Contains(
            "Товар із таким SKU вже існує.",
            responseBody);
    }
}
