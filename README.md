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