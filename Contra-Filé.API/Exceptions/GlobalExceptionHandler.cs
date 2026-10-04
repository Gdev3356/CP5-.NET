using Contra_Filé.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Contra_Filé.API.Exceptions;

/// <summary>
/// Intercepta todas as exceções não tratadas e retorna ProblemDetails (RFC 7807).
/// Registrar com AddExceptionHandler&lt;GlobalExceptionHandler&gt;() no Program.cs.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // traceId como propriedade nomeada do log (nível Error), para correlacionar com a
        // mesma requisição nos logs de controller/serviço e no ProblemDetails.Extensions abaixo.
        logger.LogError(
            exception,
            "Exceção não tratada. TraceId: {TraceId} - Mensagem: {Message}",
            httpContext.TraceIdentifier, exception.Message);

        var (statusCode, title, detail) = Map(exception);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Type    = "about:blank",
            Title   = title,
            Status  = statusCode,
            Detail  = detail,
            Instance = httpContext.Request.Path
        };

        if (environment.IsDevelopment())
            problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
    
    // Mapeamento: exceção - (HTTP status, título, detalhe)
    private (int Status, string Title, string? Detail) Map(Exception ex) => ex switch
    {
        ArgumentNullException e  => (400, "Requisição inválida",            e.Message),
        ArgumentException e      => (400, "Requisição inválida",            e.Message),
        DomainException e        => (400, "Regra de negócio violada",       e.Message),
        InvalidOperationException e => (400, "Operação inválida",           e.Message),
        KeyNotFoundException e   => (404, "Recurso não encontrado",         e.Message),
        UnauthorizedAccessException e => (401, "Não autorizado",            e.Message),
        _                        => MapUnhandled(ex)
    };

    private (int, string, string?) MapUnhandled(Exception ex)
    {
        if (environment.IsDevelopment())
            return (500, "Erro interno do servidor", ex.ToString());

        return (500, "Erro interno do servidor",
            "Ocorreu um erro inesperado. Tente novamente mais tarde.");
    }
}