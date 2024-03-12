namespace Be.Vlaanderen.Basisregisters.BasicApiProblem
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public class XmlProblemDetailsWriter : IProblemDetailsWriter
    {
        public bool CanWrite(ProblemDetailsContext context)
        {
            return context.HttpContext.Request.Headers["Accept"].ToString()
                .Contains("application/xml", StringComparison.OrdinalIgnoreCase);
        }

        public async ValueTask WriteAsync(ProblemDetailsContext context)
        {
            var problemDetails = context.ProblemDetails;
            var httpContext = context.HttpContext;
            httpContext.Response.ContentType = "application/problem+xml";

            var serializer = new DataContractSerializer(problemDetails.GetType());

            await using var writer = new MemoryStream();
            serializer.WriteObject(writer, problemDetails);
            writer.Position = 0;

            using var reader = new StreamReader(writer);
            var xml = await reader.ReadToEndAsync();

            await httpContext.Response.WriteAsync(xml, Encoding.UTF8);
        }
    }
}
