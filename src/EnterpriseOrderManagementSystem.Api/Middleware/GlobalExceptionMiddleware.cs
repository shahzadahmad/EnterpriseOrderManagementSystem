using EnterpriseOrderManagementSystem.Api.Common.Models;
using EnterpriseOrderManagementSystem.Application.Common.Exceptions;
using EnterpriseOrderManagementSystem.Domain.Common.Exceptions;
using FluentValidation;
using System.Text.Json;

namespace EnterpriseOrderManagementSystem.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(
                context,
                exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var traceId = context.TraceIdentifier;

        var response = exception switch
        {
            ValidationException validationException =>
                CreateValidationResponse(
                    validationException,
                    traceId),

            NotFoundException =>
                CreateResponse(
                    StatusCodes.Status404NotFound,
                    exception.Message,
                    "NOT_FOUND",
                    traceId),

            ConflictException =>
                CreateResponse(
                    StatusCodes.Status409Conflict,
                    exception.Message,
                    "CONFLICT",
                    traceId),

            BusinessRuleViolationException =>
                CreateResponse(
                    StatusCodes.Status400BadRequest,
                    exception.Message,
                    "BUSINESS_RULE_VIOLATION",
                    traceId),

            DomainException =>
                CreateResponse(
                    StatusCodes.Status400BadRequest,
                    exception.Message,
                    "DOMAIN_ERROR",
                    traceId),

            ArgumentException =>
                CreateResponse(
                    StatusCodes.Status400BadRequest,
                    exception.Message,
                    "INVALID_ARGUMENT",
                    traceId),

            _ =>
                CreateUnexpectedErrorResponse(
                    exception,
                    traceId)
        };

        context.Response.StatusCode = response.StatusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }

    private static ErrorResponse CreateValidationResponse(
        ValidationException exception,
        string traceId)
    {
        var errors = exception.Errors
            .Select(error => error.ErrorMessage)
            .Distinct()
            .ToArray();

        return new ErrorResponse(
            StatusCodes.Status400BadRequest,
            "One or more validation errors occurred.",
            "VALIDATION_ERROR",
            errors,
            traceId);
    }

    private static ErrorResponse CreateResponse(
        int statusCode,
        string message,
        string errorCode,
        string traceId)
    {
        return new ErrorResponse(
            statusCode,
            message,
            errorCode,
            null,
            traceId);
    }

    private ErrorResponse CreateUnexpectedErrorResponse(
        Exception exception,
        string traceId)
    {
        _logger.LogError(
            exception,
            "Unhandled exception. TraceId: {TraceId}",
            traceId);

        return new ErrorResponse(
            StatusCodes.Status500InternalServerError,
            "An unexpected error occurred.",
            "INTERNAL_SERVER_ERROR",
            null,
            traceId);
    }
}