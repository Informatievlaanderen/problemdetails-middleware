namespace Be.Vlaanderen.Basisregisters.BasicApiProblem
{
    using System;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Microsoft.AspNetCore.Mvc;

    public class ProblemDetailsException : Exception
    {
        public ProblemDetails Details { get; }

        public ProblemDetailsException(ProblemDetails details) : base($"{details.Type} : {details.Title}")
            => Details = details;

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"Type    : {Details.Type}");
            stringBuilder.AppendLine($"Title   : {Details.Title}");
            stringBuilder.AppendLine($"Status  : {Details.Status}");
            stringBuilder.AppendLine($"Detail  : {Details.Detail}");
            stringBuilder.AppendLine($"Instance: {Details.Instance}");

            return stringBuilder.ToString();
        }
    }
}
