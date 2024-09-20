# entra-id-auth

Code of my talk "Passierschein, bitte - Sichere und skalierbare Authentifizierung mit Microsoft Entra ID"

## Entra ID app registration setup

See [Entra ID app registration setup](docs/entra-id-app-registration-setup.md)

## Groups configuration

- Create a security group to access the api
    - Name the group something like `Demo Users`
- Add your user to the group

## Run applications

> It's best practices to configure the applications in development via [User secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets).  
> You can use the script `scripts/apply-usersecrets.sh` to configure user secrets based on files `secret.*.example.json` files in project root directory. Create your own copy of the exaples as required and use them.

### Backend

Set the following values in app configuration:

| Key | Value |
| --- | ----- |
| `AzureAd:TenantId` | ID of the Entra ID Tenant |
| `AzureAd:Audience` | App ID URI of [Backend API App Registration](docs/entra-id-app-registration-setup.md#backend-api-app-registration) |
| `Groups:WeatherUsers` | Group of [`Demo Users`](#groups-configuration) |
| `AppRoles:WeatherGet` | `Weather.Get` based on [Backend API App Roles configuration](docs/entra-id-app-registration-setup.md#backend-api-app-registration) |

### Frontend

Set the following values in app configuration:

| Key | Value |
| --- | ----- |
| `AzureAd:TenantId` | ID of the Entra ID Tenant |
| `AzureAd:ClientId` | App ID of [Frontend App Registration](docs/entra-id-app-registration-setup.md#frontend-app-registration) |
| `AzureAd:ClientSecret` | Secret of [Frontend App Registration](docs/entra-id-app-registration-setup.md#frontend-app-registration) |
| `Backend.Scopes` | Scopes to access the backend API as configured in [Backend API App Registration](docs/entra-id-app-registration-setup.md#backend-api-app-registration). It needs to be the full scope, e.g. `api://00000000-0000-0000-0000-000000000000/Weather.Read`. |

### Login Flow Console

Set the following values in app configuration:

| Key | Value |
| --- | ----- |
| `AzureAd:TenantId` | ID of the Entra ID Tenant |
| `AzureAd:ClientId` | App ID of [Frontend App Registration](docs/entra-id-app-registration-setup.md#frontend-app-registration) |
| `AzureAd:ClientSecret` | Secret of [Frontend App Registration](docs/entra-id-app-registration-setup.md#frontend-app-registration) |
| `AzureAd:Scopes` | Scopes to request for authentication. Defaults to `openid profile offline_access`. |

## Feature configuration

### Backend

The backend has a feature set. Configure as described below via app configuration.

| Key | Description |
| --- | ----------- |
| `Features:UseGroupAuthorization` | Enable to do a group authorization. It checks if the accessing user is part of the group configured in `Groups:WeatherUsers` by checking the *groups* claim. |
| `Features:UseAppRoleAuthorization` | Enable to do an app role authorization. It checks if the accessing user has the role `Weather.Get` based on [Backend API App Roles configuration](docs/entra-id-app-registration-setup.md#backend-api-app-registration) by checking the *roles* claim. |

## Slides

- [Passierschein, bitte - sichere und skalierbare Authentifizierung mit Microsoft Entra ID](https://speakerdeck.com/daniellindemann/passierschein-bitte-sichere-und-skalierbare-authentifizierung-mit-microsoft-entra-id)

## Demos

See [Demos](Demos.md)


