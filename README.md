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

## API Reference

### Swagger Documentation
![Swagger Badge](https://img.shields.io/badge/Swagger-Interactive%20API-blue?style=flat-square)

This API supports and uses Swagger for interactive documentation and testing. To enable Swagger, ensure the following configurations are in place:

1. **Enable Swagger in the Application:**
   - Ensure the Swagger services are added in your `Program.cs` file:
     ```csharp
     builder.Services.AddSwaggerGen();
     ```

2. **Enable Swagger Middleware:**
   - Include the Swagger middleware in the HTTP request pipeline:
     ```csharp
     app.UseSwagger();
     app.UseSwaggerUI();
     ```





