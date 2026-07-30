# HandWStat — OpenAPI Client Generation Guide

This document describes how to generate a typed C# client from the HandballManager API OpenAPI specification, and how to integrate it correctly in the HandWStat client.

---

## Source File

The OpenAPI specification is maintained at:

```
docs/openapi/handballmanager-api-v1-v2.json
```

This file is regenerated from the running API using `scripts/openapi/Export-OpenApi.ps1`. It is the single source of truth for all endpoint contracts, schemas, and operation IDs.

---

## Generating the C# Smoke Client (CI)

The repository includes a pre-configured generation script for the smoke client used in CI contract drift detection.

**Command:**

```powershell
pwsh -File scripts/openapi/Generate-SmokeClient.ps1 `
  -SwaggerPath docs/openapi/handballmanager-api-v1-v2.json `
  -OutputPath tools/HandballManagerAPI.OpenApiSmokeClient/Generated/HandballManagerApiClient.g.cs
```

**Generated output:**

```
tools/HandballManagerAPI.OpenApiSmokeClient/Generated/HandballManagerApiClient.g.cs
```

**IMPORTANT: Do NOT edit the generated file.** It is overwritten on every generation run. Any manual changes will be lost. If you need to extend the client, create a separate partial class or wrapper.

---

## Updating the Snapshot

When the API contract changes (new endpoint, modified schema, new operation ID):

1. Regenerate the OpenAPI spec:
   ```powershell
   pwsh -File scripts/openapi/Export-OpenApi.ps1
   ```

2. Regenerate the C# smoke client:
   ```powershell
   pwsh -File scripts/openapi/Generate-SmokeClient.ps1 `
     -SwaggerPath docs/openapi/handballmanager-api-v1-v2.json `
     -OutputPath tools/HandballManagerAPI.OpenApiSmokeClient/Generated/HandballManagerApiClient.g.cs
   ```

3. Build the smoke client project to confirm no compilation errors.

4. Commit both the updated `handballmanager-api-v1-v2.json` and the updated `HandballManagerApiClient.g.cs`.

---

## Generating a HandWStat Client (NSwag or Kiota)

For the HandWStat mobile/web client, generate a typed client from the same swagger.json using NSwag or Kiota.

### NSwag (recommended for .NET / MAUI)

```bash
nswag openapi2csclient \
  /input:docs/openapi/handballmanager-api-v1-v2.json \
  /namespace:HandWStat.Api \
  /output:src/HandWStat.Api/Generated/ApiClient.cs \
  /generateClientClasses:true \
  /generateDtoTypes:true \
  /nullableReferenceTypes:true
```

### Kiota (alternative, supports multiple languages)

```bash
kiota generate \
  --openapi docs/openapi/handballmanager-api-v1-v2.json \
  --language CSharp \
  --namespace-name HandWStat.Api \
  --output src/HandWStat.Api/Generated
```

As with the smoke client, **do not edit generated files**. Regenerate whenever the API spec changes.

---

## Error Handling in Generated Client

The generated client returns raw HTTP responses. Always apply these rules before deserializing:

### Check success before deserializing

```csharp
var response = await _httpClient.GetAsync(url, cancellationToken);

if (!response.IsSuccessStatusCode)
{
    var problemDetails = await response.Content
        .ReadFromJsonAsync<ProblemDetails>(cancellationToken: cancellationToken);

    var correlationId = problemDetails?.Extensions?
        .GetValueOrDefault("correlationId")?.ToString();

    _logger.LogError(
        "API error {Status} on {Url}. CorrelationId: {CorrelationId}. Detail: {Detail}",
        (int)response.StatusCode,
        url,
        correlationId,
        problemDetails?.Detail);

    // Handle by status code — see HANDWSTAT_ERROR_AND_FALLBACK_RULES.md
    return null;
}

var result = await response.Content
    .ReadFromJsonAsync<LeaguePlayerAnalyticsResponse>(cancellationToken: cancellationToken);
```

### ProblemDetails schema

```csharp
public class ProblemDetails
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int? Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
    public Dictionary<string, object?>? Extensions { get; set; }
}
```

The `correlationId` is in `Extensions["correlationId"]`. Always log it on any 4xx or 5xx response.

---

## CI Integration

The smoke client is built in every CI run to detect contract drift between the committed OpenAPI spec and the generated client. A build failure in the smoke client project indicates that the spec or the generated code is out of sync with the API implementation.

CI step (excerpt):

```yaml
- name: Build OpenAPI smoke client
  run: dotnet build tools/HandballManagerAPI.OpenApiSmokeClient/HandballManagerAPI.OpenApiSmokeClient.csproj
```

If this step fails after an API change, regenerate the spec and client as described above and commit the updated files.
