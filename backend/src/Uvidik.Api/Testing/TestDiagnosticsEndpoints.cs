using FluentValidation.Results;
using Uvidik.Application.Exceptions;

namespace Uvidik.Api.Testing;

// Only mapped when ASPNETCORE_ENVIRONMENT=Testing (see Program.cs) — exercises the global
// exception handler's status-code mapping over real HTTP, never reachable otherwise.
internal static class TestDiagnosticsEndpoints
{
    public static void MapTestDiagnosticsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/_test/throws");
        group.MapGet("/validation", (Func<IResult>)(() => throw new FluentValidation.ValidationException(
            [new ValidationFailure("field", "must not be empty")])));
        group.MapGet("/not-found", (Func<IResult>)(() => throw new NotFoundException("thing not found")));
        group.MapGet("/conflict", (Func<IResult>)(() => throw new ConflictException("duplicate")));
        group.MapGet("/forbidden", (Func<IResult>)(() => throw new ForbiddenException("not allowed")));
        group.MapGet("/unexpected", (Func<IResult>)(() => throw new InvalidOperationException("boom")));
    }
}
