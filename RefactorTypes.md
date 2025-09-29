# Type Collection Architecture Fix

**IMPORTANT: Collections go in CONCRETE projects, Base types go in ABSTRACTIONS projects**

## ServiceType Collections (ServiceTypeCollectionGenerator)

### 1. ConnectionTypes
- [ ] **Collection:** Update `src/FractalDataWorks.Services.Connections/ConnectionTypes.cs` (CONCRETE project)
  - Attribute: `[ServiceTypeCollection(typeof(ConnectionTypeBase<,,>), typeof(IConnectionType), typeof(ConnectionTypes))]`
  - Inheritance: `: ServiceTypeCollectionBase<ConnectionTypeBase<IGenericConnection, IConnectionConfiguration, IConnectionFactory<IGenericConnection, IConnectionConfiguration>>, IConnectionType, IGenericConnection, IConnectionConfiguration, IConnectionFactory<IGenericConnection, IConnectionConfiguration>>`
  - Remove custom Register methods, make empty partial class
- [ ] **Option:** Update `src/FractalDataWorks.Services.Connections.Rest/RestConnectionType.cs`
  - Update attribute to: `[ServiceTypeOption(typeof(ConnectionTypes), "Rest")]`

### 2. DataStoreTypes
- [ ] **Collection:** Update `src/FractalDataWorks.DataStores/DataStoreTypes.cs` (CONCRETE project)
  - Attribute: `[ServiceTypeCollection(typeof(DataStoreTypeBase<,,>), typeof(IDataStoreType), typeof(DataStoreTypes))]`
  - Inheritance: `: ServiceTypeCollectionBase<DataStoreTypeBase<IDataStoreService, IDataStoreConfiguration, IDataStoreFactory<IDataStoreService, IDataStoreConfiguration>>, IDataStoreType, IDataStoreService, IDataStoreConfiguration, IDataStoreFactory<IDataStoreService, IDataStoreConfiguration>>`
  - Remove custom Register methods, make empty partial class

### 3. SecretManagementTypes
- [ ] **Collection:** Create `src/FractalDataWorks.Services.SecretManagement/SecretManagementTypes.cs` (CONCRETE project)
  - Attribute: `[ServiceTypeCollection(typeof(SecretManagementTypeBase<,,>), typeof(ISecretManagementType), typeof(SecretManagementTypes))]`
  - Inheritance: `: ServiceTypeCollectionBase<SecretManagementTypeBase<ISecretManagementService, ISecretManagementConfiguration, ISecretManagementFactory<ISecretManagementService, ISecretManagementConfiguration>>, ISecretManagementType, ISecretManagementService, ISecretManagementConfiguration, ISecretManagementFactory<ISecretManagementService, ISecretManagementConfiguration>>`

### 4. DataGatewayTypes
- [ ] **Collection:** Create `src/FractalDataWorks.Services.DataGateway/DataGatewayTypes.cs` (CONCRETE project)
  - Attribute: `[ServiceTypeCollection(typeof(DataGatewayTypeBase<,,>), typeof(IDataGatewayType), typeof(DataGatewayTypes))]`
  - Inheritance: `: ServiceTypeCollectionBase<DataGatewayTypeBase<IDataGatewayService, IDataGatewayConfiguration, IDataGatewayFactory<IDataGatewayService, IDataGatewayConfiguration>>, IDataGatewayType, IDataGatewayService, IDataGatewayConfiguration, IDataGatewayFactory<IDataGatewayService, IDataGatewayConfiguration>>`
- [ ] **Option:** Update `src/FractalDataWorks.Services.DataGateway/SqlDataGatewayServiceType.cs`
  - Update attribute to: `[ServiceTypeOption(typeof(DataGatewayTypes), "SqlDataGateway")]`

### 5. AuthenticationTypes
- [ ] **Collection:** Create `src/FractalDataWorks.Services.Authentication/AuthenticationTypes.cs` (CONCRETE project)
  - Attribute: `[ServiceTypeCollection(typeof(AuthenticationTypeBase<,,>), typeof(IAuthenticationType), typeof(AuthenticationTypes))]`
  - Inheritance: `: ServiceTypeCollectionBase<AuthenticationTypeBase<IAuthenticationService, IAuthenticationConfiguration, IAuthenticationFactory<IAuthenticationService, IAuthenticationConfiguration>>, IAuthenticationType, IAuthenticationService, IAuthenticationConfiguration, IAuthenticationFactory<IAuthenticationService, IAuthenticationConfiguration>>`
- [ ] **Options:**
  - Update `src/FractalDataWorks.Services.Authentication.AzureEntra/AzureEntraAuthenticationType.cs`
    - Update attribute to: `[ServiceTypeOption(typeof(AuthenticationTypes), "AzureEntra")]`
  - Update `src/FractalDataWorks.Services.Authentication.AzureEntra/AzureEntraAuthenticationServiceType.cs`
    - Update attribute to: `[ServiceTypeOption(typeof(AuthenticationTypes), "AzureEntraService")]`

### 6. TransformationTypes
- [ ] **Collection:** Create `src/FractalDataWorks.Services.Transformations/TransformationTypes.cs` (CONCRETE project)
  - Attribute: `[ServiceTypeCollection(typeof(TransformationTypeBase<,,>), typeof(ITransformationType), typeof(TransformationTypes))]`
  - Inheritance: `: ServiceTypeCollectionBase<TransformationTypeBase<ITransformationService, ITransformationConfiguration, ITransformationFactory<ITransformationService, ITransformationConfiguration>>, ITransformationType, ITransformationService, ITransformationConfiguration, ITransformationFactory<ITransformationService, ITransformationConfiguration>>`
- [ ] **Option:** Update `src/FractalDataWorks.Services.Transformations/StandardTransformationServiceType.cs`
  - Update attribute to: `[ServiceTypeOption(typeof(TransformationTypes), "StandardTransformation")]`

### 7. SchedulerTypes
- [ ] **Collection:** Create `src/FractalDataWorks.Services.Scheduling/SchedulerTypes.cs` (CONCRETE project)
  - Attribute: `[ServiceTypeCollection(typeof(SchedulerTypeBase<,,>), typeof(ISchedulerType), typeof(SchedulerTypes))]`
  - Inheritance: `: ServiceTypeCollectionBase<SchedulerTypeBase<ISchedulerService, ISchedulerConfiguration, ISchedulerFactory<ISchedulerService, ISchedulerConfiguration>>, ISchedulerType, ISchedulerService, ISchedulerConfiguration, ISchedulerFactory<ISchedulerService, ISchedulerConfiguration>>`

## TypeCollection Collections (TypeCollectionGenerator)

### 8. SecurityMethods
- [ ] **Collection:** Update `src/FractalDataWorks.Web.Http.Abstractions/Security/SecurityMethods.cs`
  - Change: `[TypeCollection(typeof(SecurityMethodBase), typeof(ISecurityMethod), typeof(SecurityMethods))]`
  - To: `[TypeCollection(typeof(SecurityMethodBase), "SecurityMethods")]`
  - Add inheritance: `: TypeCollectionBase<SecurityMethodBase, ISecurityMethod>`
- [ ] **Options:** Update all files to `[TypeOption(typeof(SecurityMethods), "Name")]`:
  - `src/FractalDataWorks.Web.Http.Abstractions/Security/None.cs` → "None"
  - `src/FractalDataWorks.Web.Http.Abstractions/Security/JWT.cs` → "JWT"
  - `src/FractalDataWorks.Web.Http.Abstractions/Security/ApiKey.cs` → "ApiKey"
  - `src/FractalDataWorks.Web.Http.Abstractions/Security/OAuth2.cs` → "OAuth2"
  - `src/FractalDataWorks.Web.Http.Abstractions/Security/Certificate.cs` → "Certificate"

### 9. EndpointTypes
- [ ] **Collection:** Update `src/FractalDataWorks.Web.Http.Abstractions/EndPoints/EndpointTypes.cs`
  - Change: `[TypeCollection(typeof(EndpointTypeBase), typeof(IEndpointType), typeof(EndpointTypes))]`
  - To: `[TypeCollection(typeof(EndpointTypeBase), "EndpointTypes")]`
  - Add inheritance: `: TypeCollectionBase<EndpointTypeBase, IEndpointType>`
- [ ] **Options:** Update all files to `[TypeOption(typeof(EndpointTypes), "Name")]`:
  - `src/FractalDataWorks.Web.Http.Abstractions/EndPoints/CRUD.cs` → "CRUD"
  - `src/FractalDataWorks.Web.Http.Abstractions/EndPoints/Query.cs` → "Query"
  - `src/FractalDataWorks.Web.Http.Abstractions/EndPoints/Command.cs` → "Command"
  - `src/FractalDataWorks.Web.Http.Abstractions/EndPoints/Event.cs` → "Event"
  - `src/FractalDataWorks.Web.Http.Abstractions/EndPoints/File.cs` → "File"
  - `src/FractalDataWorks.Web.Http.Abstractions/EndPoints/Health.cs` → "Health"

### 10. RateLimitPolicies
- [ ] **Collection:** Update `src/FractalDataWorks.Web.Http.Abstractions/EnhancedEnums/Policies/RateLimitPolicies.cs`
  - Attribute: `[TypeCollection(typeof(RateLimitPolicyBase), "RateLimitPolicies")]`
  - Inheritance: `: TypeCollectionBase<RateLimitPolicyBase, IRateLimitPolicy>`
- [ ] **Options:** Update all files to `[TypeOption(typeof(RateLimitPolicies), "Name")]`:
  - `src/FractalDataWorks.Web.Http.Abstractions/EnhancedEnums/Policies/None.cs` → "None"
  - `src/FractalDataWorks.Web.Http.Abstractions/EnhancedEnums/Policies/FixedWindow.cs` → "FixedWindow"
  - `src/FractalDataWorks.Web.Http.Abstractions/EnhancedEnums/Policies/SlidingWindow.cs` → "SlidingWindow"
  - `src/FractalDataWorks.Web.Http.Abstractions/EnhancedEnums/Policies/TokenBucket.cs` → "TokenBucket"
  - `src/FractalDataWorks.Web.Http.Abstractions/EnhancedEnums/Policies/Concurrency.cs` → "Concurrency"

### 11. HealthStatuses
- [ ] **Collection:** Update `src/FractalDataWorks.Services.SecretManagement.Abstractions/HealthStatuses.cs`
  - Change: `[EnumCollection(CollectionName = "HealthStatuses")]`
  - To: `[TypeCollection(typeof(HealthStatus), "HealthStatuses")]`
  - Add inheritance: `: TypeCollectionBase<HealthStatus, IHealthStatus>`
- [ ] **Options:** Update all files to `[TypeOption(typeof(HealthStatuses), "Name")]`:
  - `src/FractalDataWorks.Services.SecretManagement.Abstractions/UnknownHealthStatus.cs` → "Unknown"
  - `src/FractalDataWorks.Services.SecretManagement.Abstractions/HealthyHealthStatus.cs` → "Healthy"
  - `src/FractalDataWorks.Services.SecretManagement.Abstractions/UnhealthyHealthStatus.cs` → "Unhealthy"
  - `src/FractalDataWorks.Services.SecretManagement.Abstractions/WarningHealthStatus.cs` → "Warning"
  - `src/FractalDataWorks.Services.SecretManagement.Abstractions/DegradedHealthStatus.cs` → "Degraded"
  - `src/FractalDataWorks.Services.SecretManagement.Abstractions/CriticalHealthStatus.cs` → "Critical"

## Files to Delete (Collections in Wrong Locations)
- [ ] Delete `src/FractalDataWorks.Services.Connections.Abstractions/ConnectionTypes.cs` (wrong location)
- [ ] Delete `src/FractalDataWorks.Services.DataGateway.Abstractions/DataGatewayTypes.cs` (wrong location)
- [ ] Delete `src/FractalDataWorks.Services.Authentication.Abstractions/AuthenticationTypes.cs` (wrong location)
- [ ] Delete `src/FractalDataWorks.Services.Transformations.Abstractions/TransformationTypes.cs` (wrong location)
- [ ] Delete `src/FractalDataWorks.Services.SecretManagement.Abstractions/SecretManagementTypes.cs` (wrong location)
- [ ] Delete `src/FractalDataWorks.Services.Scheduling.Abstractions/SchedulerTypes.cs` (wrong location)

## Notes
- **Collections** belong in **CONCRETE** projects (Services.Connections, not Services.Connections.Abstractions)
- **Base types** belong in **ABSTRACTIONS** projects (ConnectionTypeBase, ServiceTypeBase)
- **ServiceType collections** use 3-parameter attributes and complex inheritance
- **TypeCollection collections** use 2-parameter attributes and simpler inheritance
- All collections should be empty partial classes that get source-generated content