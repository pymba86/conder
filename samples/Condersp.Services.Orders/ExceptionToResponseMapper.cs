using System;
using System.Net;
using Conder.WebApi.Exceptions;

namespace Conder.Samples.Services.Orders
{
    public class ExceptionToResponseMapper : IExceptionToResponseMapper
    {
        public ExceptionResponse Map(Exception exception)
            => new ExceptionResponse(new
                {
                    code = "error", message = exception.Message
                },
                HttpStatusCode.BadRequest);
    }
}