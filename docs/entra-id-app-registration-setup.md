# Entra ID App Registration Setup

## Backend API App registration

The backend API app registration is the representation of the backend API. It controls the allowed scopes, api roles and token claims required by the backend api.

Create app registration:

- Go the Azure portal and open *Entra ID*
- Select *App registrations* from the side menu
- Click *New registration*
- Choose a name like `Backend API`
- Select `Accounts in this organizational directory only (Single tenant)` as *Supported account ytpe*
- Click *Register* to create the app registration

Configure app registration:

- Add yourself as owner via *Owners* menu item
- Expose an API
    - Add *Application* ID URI
        - Click *Add* link beside the *Application ID URI* text
        - Use the default value, that will use the App ID `api://` placed in front
        - The value will look like `api://00000000-0000-0000-0000-000000000000`
    - Add scopes
        - Add `Weather.Read`
            - Scope name: `Weather.Read`
            - Who can consent: `Admins and users`
            - Admin consent display name: `Read the weather data`
            - Admin consent description: `Allows the user to read weather information`
            - User consent display name: `Read the weather data`
            - User consent description: `Allows the user to read weather information`
            - State: `Enabled`
- Token configuration
    - Click *Add groups claim* to add groups claim
    - For *ID* set `Group ID`
    - For *Access* set `Group ID`
    - For *SAML* set `Group ID`
- App roles
    - Add `Weather users`
        - Display name: `Weather users`
        - Allowed member types: `Users/Groups`
        - Value: `Weather.Get`
        - Description: `Read weather data`
        - Do you want to enable this app role?: `[x]`
    - Add `Some other app role`
        - Display name: `Some other app role`
        - Allowed member types: `Users/Groups`
        - Value: `Other.Action`
        - Description: `Do some other action`
        - Do you want to enable this app role?: `[x]`

## Frontend App registration

The frontend app registration is the representation of the frontend. It controls how the user can sign-in to the application.

Create app registration:

- Go the Azure portal and open *Entra ID*
- Select *App registrations* from the side menu
- Click *New registration*
- Choose a name like `Frontend`
- Select `Accounts in this organizational directory only (Single tenant)` as *Supported account ytpe*
- Click *Register* to create the app registration

Configure app registration:

- Add yourself as owner via *Owners* menu item
- Configure *Authentication*
    - Add platform *Web*
    - Redirect URI:
        - `https://localhost:7124/signin-oidc`
        - `http://localhost:53298`
    - Front-channel logout URL: `https://localhost:7124/signout-oidc`
    - Implicit grant and hybrid flows
        - [ ] Access tokens (used for implicit flows)
        - [x] ID tokens (used for implicit and hybrid flows)
- Certificates & secrets
    - Create a secret and store it for later app configuration
- API permissions
    - Add API permission for [backend API](#backend-api-app-registration)
        - Click *Add a permission*
        - Select tab *My APIs*
        - Choose backend api
        - Select *Delegated permissions*
        - Choose permissions:
            - `Weather.Read`

> *https://localhost:7124* is the frontend razor web application; *http://localhost:53298* is the login flow console application
