using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NJsonSchema.CodeGeneration;
using NSwag;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.CSharp;
using NJsonSchema.CodeGeneration.CSharp;

namespace CrmDataGeneration.OpenApi
{
    public class SwaggerGenerator
    {

        public const string SwaggerReferenceClientsPath = "..\\..\\..\\CrmDataGeneration\\OpenApi\\Reference\\SwaggerReferenceClients.cs";
        public const string SwaggerReferenceEntitiesPath = "..\\..\\..\\CrmDataGeneration\\OpenApi\\Reference\\SwaggerReferenceEntities.cs";
        public static SwaggerToCSharpClientGeneratorSettings GenerationSettings => new SwaggerToCSharpClientGeneratorSettings
        {
            ExceptionClass = "AcumaticaSwaggerException",
            GenerateOptionalParameters = true,
            ClientBaseClass = nameof(OpenApiBaseClient),
            ConfigurationClass = nameof(OpenApiState),
            UseHttpClientCreationMethod = true,
            DisposeHttpClient = false,
            GenerateBaseUrlProperty = false,
            UseBaseUrl = false,
            CSharpGeneratorSettings =
            {
                ClassStyle = CSharpClassStyle.Poco,
                Namespace = "CrmDataGeneration.OpenApi.Reference",
                ArrayBaseType = "System.Collections.Generic.List",
                ArrayType = "System.Collections.Generic.List",
            }
        };

        // Generate client and replace existing
        public static void GenerateClient(string swaggerUrl)
        {
            var generator = new SwaggerToCSharpClientGenerator(SwaggerDocument.FromUrlAsync(swaggerUrl).Result, GenerationSettings);
            var clients = generator.GenerateFile(ClientGeneratorOutputType.Implementation);
            var entities = generator.GenerateFile(ClientGeneratorOutputType.Contracts);
            File.WriteAllText(SwaggerReferenceClientsPath, clients);
            File.WriteAllText(SwaggerReferenceEntitiesPath, entities);
        }
    }
}
