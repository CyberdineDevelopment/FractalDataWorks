# FractalDataWorks SOAP Connection Sample

Complete sample application demonstrating the FractalDataWorks SOAP web service connection implementation.

## Overview

This sample demonstrates:
- ✅ SOAP 1.1 and 1.2 support
- ✅ XML envelope construction and parsing
- ✅ SOAP method invocation with parameters
- ✅ SOAPAction header management
- ✅ SOAP fault handling
- ✅ Custom headers and namespaces
- ✅ Automatic retry logic
- ✅ Error handling and timeout management

## Prerequisites

- **.NET 10.0 SDK or later**
- **Internet connection** for accessing public SOAP services

## Quick Start

```bash
dotnet run
```

The sample uses a public calculator SOAP service for demonstration.

## Configuration

### appsettings.json

```json
{
  "SoapConnection": {
    "EndpointUrl": "http://www.dneonline.com/calculator.asmx",
    "SoapVersion": "1.1",
    "XmlNamespace": "http://tempuri.org/",
    "SoapAction": "http://tempuri.org/Add",
    "TimeoutSeconds": 30,
    "RetryCount": 3
  }
}
```

### SoapConnectionConfiguration Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| EndpointUrl | string | Required | SOAP endpoint URL |
| SoapVersion | string | "1.2" | SOAP version (1.1 or 1.2) |
| XmlNamespace | string | "" | XML namespace for methods |
| SoapAction | string | "" | SOAPAction header value |
| TimeoutSeconds | int | 30 | Request timeout |
| RetryCount | int | 3 | Retry attempts |
| UseMtom | bool | false | Use MTOM for binary data |
| ValidateSslCertificate | bool | true | Validate SSL |

## Code Examples

### Example 1: Simple SOAP Call

```csharp
var parameters = new Dictionary<string, object>
{
    ["intA"] = 10,
    ["intB"] = 20
};

var command = new SoapConnectionCommand(
    "Add",
    parameters,
    "http://tempuri.org/Add"
);

var result = await service.Execute<string>(command, CancellationToken.None);
```

### Example 2: Multiple Parameters

```csharp
var parameters = new Dictionary<string, object>
{
    ["firstName"] = "John",
    ["lastName"] = "Doe",
    ["age"] = 30
};

var command = new SoapConnectionCommand("RegisterUser", parameters);
```

### Example 3: Custom Headers

```csharp
var headers = new Dictionary<string, string>
{
    ["X-Request-ID"] = Guid.NewGuid().ToString()
};

var command = new SoapConnectionCommand(
    "GetData",
    parameters,
    null, // Use configuration SoapAction
    headers
);
```

## SOAP Versions

### SOAP 1.1

- Content-Type: `text/xml; charset=utf-8`
- Requires SOAPAction header
- Namespace: `http://schemas.xmlsoap.org/soap/envelope/`

```csharp
var config = new SoapConnectionConfiguration
{
    SoapVersion = "1.1",
    SoapAction = "http://tempuri.org/MethodName"
};
```

### SOAP 1.2

- Content-Type: `application/soap+xml; charset=utf-8`
- SOAPAction optional (can be in Content-Type)
- Namespace: `http://www.w3.org/2003/05/soap-envelope`

```csharp
var config = new SoapConnectionConfiguration
{
    SoapVersion = "1.2"
};
```

## Error Handling

### SOAP Fault Handling

```csharp
var result = await service.Execute<string>(command, cancellationToken);

if (!result.IsSuccess)
{
    // SOAP faults are returned as error messages
    Console.WriteLine($"SOAP Fault: {result.CurrentMessage}");
}
```

## Authentication

### WS-Security

For WS-Security authentication, add to Headers:

```csharp
var config = new SoapConnectionConfiguration
{
    AuthenticationType = "WssSecurity",
    Headers = new Dictionary<string, string>
    {
        ["Authorization"] = "Basic base64credentials"
    }
};
```

## Dependency Injection

```csharp
services.AddHttpClient<SoapService>();
services.AddSingleton(soapConfig);
services.AddScoped<SoapService>();
```

## Common SOAP Operations

### Invoking Methods

```csharp
var command = new SoapConnectionCommand(methodName, parameters);
var result = await service.Execute<TResponse>(command, cancellationToken);
```

### Handling Complex Types

For complex response types, the service returns XML as string. Parse using XDocument:

```csharp
var result = await service.Execute<string>(command, cancellationToken);
var xml = XDocument.Parse(result.Value);
```

## License

This sample is part of the FractalDataWorks project.
