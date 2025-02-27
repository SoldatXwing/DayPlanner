# DayPlanner

[![Firebase](https://img.shields.io/badge/Firebase-Auth-success)](https://firebase.google.com/)
[![Firebase](https://img.shields.io/badge/Firebase-Firestore-success)](https://firebase.google.com/)

DayPlanner is a web application that helps users organize their daily tasks and appointments. It uses Firebase Authentication for user login and Firestore for storing user data.

## Features

- User authentication with Firebase Auth
- Real-time database with Firestore
- Create, update, and delete tasks
- Schedule events and reminders

## Planned Features
- AI integration
- Third party imports
- Mobile (Andorid and ios) support

## Swagger Documentation
![Swagger Badge](https://img.shields.io/badge/Swagger-Interactive%20API-blue?style=flat-square)

This API supports and uses Swagger for interactive documentation and testing.

### Feature flags

Swagger can be enabled by these two boolean configuration keys:
`Swagger:Enabled`
`Swagger:UiEnabled`

`Enabled` controls the availability of the documentation endpoints. The general scheme of these endpoints is `/swagger/{version}/swagger.json` where version is for example *v1*.

`UiEnabled` enables the interactive UI that is available at `/swagger/index.html`.

By using a JSON configuration to enable both swagger in general in the UI you can use the following configuration:
```
{
    "Swagger": {
        "Enabled": true,
        "UiEnabled": true
    }
}
```

If the key `Enabled` is set to `false`, `UiEnabled` will be ignored.

## AI Support

This solution includes support for a locally hosted AI model, enabled by default. The AI feature is configured via the `appsettings.json` file in the `DayPlanner.AppHost` project. 

### Configuration
AI settings are defined in the following files:

- **AppHost Configuration** ([`DayPlanner.AppHost/appsettings.json`](https://github.com/SoldatXwing/DayPlanner/blob/master/DayPlanner.AppHost/appsettings.json)):
  ```json
  ...
  "AiSettings": {
    "IsEnabled": true,
    "Model": "phi3.5"
  }
  ```
  - `IsEnabled`: Set to `true` to activate AI support (default).
  - `Model`: Specifies the local LLM model (e.g., `"phi3.5"`).

- **API Configuration** ([`DayPlanner.Api/appsettings.json`](https://github.com/SoldatXwing/DayPlanner/blob/master/DayPlanner.Api/appsettings.json)):
  ```json
  ...
  "AiSettings": {
    "IsEnabled": false
  }
  ```
  - Override the `IsEnabled` setting here to disable AI support if needed.

### Disabling AI Support
If the host machine lacks the capability to run a local Large Language Model (LLM), you can disable AI support by setting `"IsEnabled": false` in either configuration file. The API-level setting takes precedence if it differs from the AppHost setting.

### Requirements
When `"IsEnabled": true`:
- Ensure **Docker** is installed and running on the host machine, as the local LLM depends on it.
- Verify that the specified `Model` (e.g., `"phi3.5"`) is compatible and available in your Docker environment.

### Tested models
- phi3.5
- deepseek-r1:8b

Based on the tested models, i personally think ```phi3.5``` is the better perfomance/output model.
