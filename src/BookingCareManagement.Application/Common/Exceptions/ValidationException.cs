using System;

namespace BookingCareManagement.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
