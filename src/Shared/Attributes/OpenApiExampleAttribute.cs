using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace ItSupportServer.src.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OpenApiExampleAttribute : Attribute
    {
        public string Value { get; }
        public OpenApiExampleAttribute(string value)
        {
            Value = value;
        }
    }

    public class ExampleSchemaTransformer : IOpenApiSchemaTransformer
    {
        public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
        {
            // Kiểm tra nếu schema này đang map tới một Property của class
            if (context.JsonPropertyInfo?.AttributeProvider is ICustomAttributeProvider attributeProvider)
            {
                // Tìm xem property có gắn [OpenApiExample] không
                var exampleAttr = attributeProvider
                    .GetCustomAttributes(typeof(OpenApiExampleAttribute), false)
                    .FirstOrDefault() as OpenApiExampleAttribute;

                if (exampleAttr != null)
                {
                    // Gán giá trị vào Example
                    // Lưu ý: Cần convert string sang OpenApiString hoặc kiểu tương ứng
                    schema.Example = new OpenApiString(exampleAttr.Value);
                }
            }
            return Task.CompletedTask;
        }
    }
}
