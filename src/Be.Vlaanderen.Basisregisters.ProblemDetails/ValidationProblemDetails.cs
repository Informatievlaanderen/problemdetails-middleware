namespace Be.Vlaanderen.Basisregisters.BasicApiProblem
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using System.Xml.Serialization;
    using FluentValidation;
    using FluentValidation.Results;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

    /// <summary>
    /// Implementation of Problem Details for HTTP APIs https://tools.ietf.org/html/rfc7807 with additional Validation Errors
    /// </summary>
    [DataContract(Name = "ProblemDetails", Namespace = "")]
    public class ValidationProblemDetails : StatusCodeProblemDetails
    {
        /// <summary>
        /// Uitgebreide omschrijving van de validatiefout(en).
        /// </summary>
        [XmlIgnore]
        [IgnoreDataMember]
        [JsonProperty("validationErrors", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [Description("Uitgebreide omschrijving van de validatiefout(en).")]
        public Dictionary<string, Errors>? ValidationErrors { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [XmlElement("ValidationErrors")]
        [DataMember(Name = "ValidationErrors", Order = 600, EmitDefaultValue = false)]
        public ValidationErrorDetails? ValidationErrorsProxy
        {
            get => new ValidationErrorDetails(ValidationErrors);
            set => ValidationErrors = value;
        }

        [CollectionDataContract(ItemName = "ValidationError", KeyName = "Field", ValueName = "Errors", Namespace = "")]
        public class ValidationErrorDetails : Dictionary<string, Errors>
        {
            // WARNING: If you remove this ctor, the serializer is angry
            public ValidationErrorDetails() { }

            public ValidationErrorDetails(Dictionary<string, Errors> dictionary)
                : base(dictionary) { }
        }

        /// <summary>
        /// Veldnaam van waar de validatiefout zich bevindt.
        /// </summary>
        [CollectionDataContract(ItemName = "Error", Namespace = "")]
        public class Errors : Collection<ValidationError>
        {
            // WARNING: If you remove this ctor, the serializer is angry
            public Errors() { }

            public Errors(IList<ValidationError> list) : base(list) { }
        }

        // Here to make DataContractSerializer happy
        public ValidationProblemDetails()
            : base(StatusCodes.Status400BadRequest)
        {
            Title = ProblemDetailsExtensions.DefaultTitle;
            Detail = "Validatie mislukt!"; // TODO: Localize
            Instance = ProblemDetailsExtensions.GetProblemNumber();
            Type = ProblemDetailsExtensions.GetTypeUriFor<ValidationException>();
        }

        public ValidationProblemDetails(ValidationException exception) : this()
        {
            ValidationErrors = exception.Errors
                .GroupBy(x => x.PropertyName, x => x)
                .ToDictionary(x => x.Key, x => new Errors(x.Select(y => new ValidationError(y)).ToList()));
        }
    }

    /// <summary>
    /// Veldnaam van waar de validatiefout zich bevindt.
    /// </summary>
    [DataContract(Name = "ValidationError", Namespace = "")]
    public class ValidationError
    {
        /// <summary>
        /// Unieke code die de validatiefout beschrijft.
        /// </summary>
        [DataMember(Name = "Code", EmitDefaultValue = false)]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Code { get; set; }

        /// <summary>
        /// Omschrijving die de validatiefout beschrijft.
        /// </summary>
        [DataMember(Name = "Reason")]
        [JsonProperty("reason", Required = Required.DisallowNull)]
        public string Reason { get; set; } = "";

        public ValidationError()
        { }

        public ValidationError(string reason)
        {
            Reason = reason;
        }

        public ValidationError(string code, string reason)
        {
            Code = code;
            Reason = reason;
        }

        public ValidationError(ValidationFailure failure)
        {
            Code = failure.ErrorCode;
            Reason = failure.ErrorMessage;
        }
    }
}
