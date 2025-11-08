using System;
using Microsoft.AspNetCore.Http;

namespace BookingCareManagement.Application.Common.Exceptions;

public class AuthException : Exception
{
    public int Status { get; }
    public AuthException(string message, int status = StatusCodes.Status401Unauthorized)
        : base(message) => Status = status;
}
