# Demos

## Vor Demo

Apps erstellen: [Entra ID App Registration Setup](docs/entra-id-app-registration-setup.md)

## App erstellen

- Öffne Entra ID
- Gehe zu *App registrations*
- App registration konfigurieren für Benutzung mit *LoginFlowConsole* Projekt
    - Name: `My new app`
    - Wähle `Accounts in this organizational directory only (Single tenant)` als *Supported account type*
    - Redirect URI: `http://localhost:53298`
        > URL von *LoginFlowConsole* Browser
- Secrets setzen
    - In `src/Demo.EntraIdAuth.Helper.LoginFlowConsole/appsettings.json`
    - Mit `scripts/apply-usersecrets.sh` script
        Beispiel für *basta* secrets file:
        
        ```bash
        ./scripts/apply-usersecrets.sh --secrets-file secrets.helper.loginflowconsole.basta.json --project-file src/Demo.EntraIdAuth.Helper.LoginFlowConsole/Demo.EntraIdAuth.Helper.LoginFlowConsole.csproj
        ```

- Run `LoginFlowConsole - .NET Core Launch (console)`
- Login in via Browser (link in console output)
- Show console output
    - Claims
    - Access token

## Frontend Zugriff auf API

- Scopes konfigurieren
- *[Backend API App Registration](docs/entra-id-app-registration-setup.md#backend-api-app-registration) erklären*
    - Im *Expose an API* Menü
- *[Frontend App Registration](docs/entra-id-app-registration-setup.md#frontend-app-registration) erklären*
    - Im *API permissions* Menü
    - Hinzufügen von Permissions
        - Tab *My APIs*
        - *[Backend API App Registration](docs/entra-id-app-registration-setup.md#backend-api-app-registration)* auswählen
        - *Permission `Weather.Read` hinzufügen zeigen*
- Frontend `appsettings.json` Config anpassen
    - `Backend:Scopes`
    - *Erklärung: Frontend fragt nach Permissions*
- Code im Frontend
    - *`Program.cs` durchgehen*

        ```csharp
        builder.AddAuthentication(...)
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddDownstreamApi("Demo.EntraIdAuth.Backend", builder.Configuration.GetSection("Backend"))
            .AddInMemoryTokenCaches();
        ```
    
    - *`Pages/Index.cshtml.cs`, `Services/BackendViaDownstreamApiService.cs` und `Services/BackendWithoutHelperClassService.cs` durchgehen*
        - Jeweils: Wie werden API-Calls gemacht
- Backend konfigurieren für Scope-only überprüfung
    - `Features:UseGroupAuthorization` auf `false` setzen
    - `Features:UseAppRoleAuthorization` auf `false` setzen
- Code im Backend
    - *`Program.cs` durchgehen*
        - Authentication Config

            ```csharp
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
            ```
        
        - Authorization Config mit Scope-Policy `Scope:Weather.Read`

            ```csharp
            builder.Services.AddAuthorization(options =>
            {
                // check scope
                options.AddPolicy("Scope:Weather.Read", policy => policy.RequireScope("Weather.Read"));
            });
            ```

- Run Backend und Frontend
    - Gerne in Private Mode Browser
    - Gerne Debug zeigen und einzeln durchgehen
- Gegentest
    - im Backend den Scope ändern und starten
    - Ergebnis im Frontend ist 403 (Forbidden)

## Absicherung durch Benutzerberechtigungen

### Groups

- *[Backend API App Registration](docs/entra-id-app-registration-setup.md#backend-api-app-registration) erklären*
    - Im Menü unter `Token configuration`
    - `Add group claims`
    - *Erklären Claims*: ID, Access, SAML mit `Group ID`
    - ***Ergebnis**: Token enthält automatisch Gruppen*
- *Gruppen zeigen*
    - Gruppe aus [Groups configuration](README.md#groups-configuration) öffnen
    - *Erklären, dass Benutzer hinzugefügt ist*
- Backend konfigurieren
    - `Features:UseGroupAuthorization` auf `true` setzen
    - `Groups:WeatherUsers` auf Group ID der Benutzergruppe setzen
- *Code in Backend erklären*
    - `JwtBearerOptions` mit Role Claim

        ```csharp
        options.TokenValidationParameters.RoleClaimType = "groups";
        ```
    
    - Authorization mit Group check

        ```csharp
        builder.Services.AddAuthorization(options =>
        {
            var weatherUsersGroupObjectId = builder.Configuration.GetValue<string>("Groups:WeatherUsers")!;
            options.AddPolicy("WeaterUsersGroupRequired", policy => policy.RequireRole([weatherUsersGroupObjectId]));
        });
        ```
    
    - Zuweisung von Authorization Policies für Endpunkt

        ```csharp
        var authorizationPolicies = new List<string>()
        {
            "Scope:Weather.Read",
        };
        if(features.UseGroupAuthorization)
        {
            authorizationPolicies.Add("WeaterUsersGroupRequired");
        }

        app.MapGet(...)
        .RequireAuthorization(authorizationPolicies.ToArray());
        ```

- Run Backend und Frontend
- Gegentest
    - im Backend die *Group ID* ändern und starten
    - Ergebnis im Frontend ist 403 (Forbidden)

### App Roles

- *[Backend API App Registration](docs/entra-id-app-registration-setup.md#backend-api-app-registration) erklären*
    - Im Menü unter `App roles`
    - `Create app role`
    - *Erklären App role Felder*: Display name, Type, Value, Description
- Enterprise App konfigurieren
    - Unter *Users and groups* Role Assignments hinzufügen
- Backend konfigurieren
    - `Features:UseGroupAuthorization` auf `false` setzen
    - `Features:UseAppRoleAuthorization` auf `true` setzen
    - `AppRoles:WeatherGet` auf vorhandene App Role setzen; z.B. `Weather.Get`
- *Code in Backend erklären*
    - `JwtBearerOptions` mit Role Claim

        ```csharp
        options.TokenValidationParameters.RoleClaimType = "roles";
        ```
    
    - Authorization mit App Role check

        ```csharp
        builder.Services.AddAuthorization(options =>
        {
            var weatherGetAppRole = builder.Configuration.GetValue<string>("AppRoles:WeatherGet")!;
            options.AddPolicy("WeatherGetAppRoleRequired", policy => policy.RequireRole([weatherGetAppRole]));
        });
        ```

    - Zuweisung von Authorization Policies für Endpunkt

        ```csharp
        var authorizationPolicies = new List<string>()
        {
            "Scope:Weather.Read",
        };
        if(features.UseAppRoleAuthorization)
        {
            authorizationPolicies.Add("WeatherGetAppRoleRequired");
        }

        app.MapGet(...)
        .RequireAuthorization(authorizationPolicies.ToArray());
        ```

- Run Backend und Frontend
- Gegentest
    - im Backend die *AppRoles* ändern und starten
    - Ergebnis im Frontend ist 403 (Forbidden)
