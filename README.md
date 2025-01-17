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

### Account Management

#### Get Account Information
```http
GET /v1/account
```

| Parameter  | Type      | Description                |
| :--------- | :-------- | :------------------------- |
| `api_key`  | `string`  | **Required**. Your API key |

**Response Codes:**
- `200 OK` - Returns the user account information.
- `404 Not Found` - User not found.

---

#### Login
```http
POST /v1/account/login
```

| Parameter      | Type     | Description                       |
| :------------- | :------- | :-------------------------------- |
| `email`        | `string` | **Required**. User email address  |
| `password`     | `string` | **Required**. User password       |

**Response Codes:**
- `200 OK` - Returns the bearer access token.
- `400 Bad Request` - Invalid email or password.

---

#### Validate Token
```http
POST /v1/account/validate
```

| Parameter          | Type     | Description                       |
| :----------------- | :------- | :-------------------------------- |
| `authorization`    | `string` | **Required**. Bearer token header |

**Response Codes:**
- `200 OK` - Returns the user ID associated with the token.
- `401 Unauthorized` - Token is invalid.

---

#### Register
```http
POST /v1/account/register
```

| Parameter      | Type     | Description                              |
| :------------- | :------- | :--------------------------------------- |
| `email`        | `string` | **Required**. Email address of the user |
| `password`     | `string` | **Required**. User password             |

**Response Codes:**
- `200 OK` - Returns the newly created user.
- `400 Bad Request` - Invalid data provided.

---

### Appointments Management

#### Get All Appointments
```http
GET /v1/appointments
```

| Parameter  | Type   | Description                           |
| :--------- | :----- | :------------------------------------ |
| `page`     | `int`  | Page number (default: 1)              |
| `pageSize` | `int`  | Items per page (default: 10)          |

**Response Codes:**
- `200 OK` - Returns a paginated list of appointments.

---

#### Get Appointments by Date Range
```http
GET /v1/appointments/range
```

| Parameter  | Type      | Description                               |
| :--------- | :-------- | :---------------------------------------- |
| `start`    | `datetime`| **Required**. Start date of the range    |
| `end`      | `datetime`| **Required**. End date of the range      |

**Response Codes:**
- `200 OK` - Returns appointments within the range.
- `400 Bad Request` - Invalid date range provided.

---

#### Create Appointment
```http
POST /v1/appointments
```

| Parameter      | Type      | Description                              |
| :------------- | :-------- | :--------------------------------------- |
| `title`        | `string`  | **Required**. Title of the appointment  |
| `start`        | `datetime`| **Required**. Start time of appointment |
| `end`          | `datetime`| **Required**. End time of appointment   |

**Response Codes:**
- `201 Created` - Appointment created successfully.
- `400 Bad Request` - Invalid request attributes.

---

#### Get Appointment by ID
```http
GET /v1/appointments/{appointmentId}
```

| Parameter        | Type     | Description                           |
| :--------------- | :------- | :------------------------------------ |
| `appointmentId`  | `string` | **Required**. ID of the appointment   |

**Response Codes:**
- `200 OK` - Returns the appointment.
- `404 Not Found` - Appointment not found.

---

#### Update Appointment
```http
PUT /v1/appointments/{appointmentId}
```

| Parameter        | Type      | Description                            |
| :--------------- | :-------- | :------------------------------------- |
| `appointmentId`  | `string`  | **Required**. ID of the appointment    |
| `title`          | `string`  | **Required**. Title of the appointment |
| `start`          | `datetime`| **Required**. Start time of appointment|
| `end`            | `datetime`| **Required**. End time of appointment  |

**Response Codes:**
- `200 OK` - Appointment updated successfully.
- `400 Bad Request` - Invalid request attributes.
- `403 Forbidden` - Unauthorized access.

---

#### Delete Appointment
```http
DELETE /v1/appointments/{appointmentId}
```

| Parameter        | Type     | Description                           |
| :--------------- | :------- | :------------------------------------ |
| `appointmentId`  | `string` | **Required**. ID of the appointment   |

**Response Codes:**
- `204 No Content` - Appointment deleted successfully.
- `403 Forbidden` - Unauthorized access.
- `404 Not Found` - Appointment not found.

---

### Google Calendar Integration

#### Google Login URL
```http
GET /v1/googlecalendar/login
```

**Response Codes:**
- `200 OK` - Returns the authorization URL for Google OAuth2 login.

---

#### Callback for Google OAuth2
```http
GET /v1/googlecalendar/callback
```

| Parameter | Type     | Description                     |
| :-------- | :------- | :------------------------------ |
| `code`    | `string` | **Required**. Google auth code  |
| `state`   | `string` | **Required**. User ID           |

**Response Codes:**
- `200 OK` - Token exchanged successfully.
- `400 Bad Request` - Invalid code provided.
- `404 Not Found` - User not found.

---

#### Sync Appointments
```http
POST /v1/googlecalendar/sync
```

**Response Codes:**
- `204 No Content` - Sync completed successfully.
- `403 Forbidden` - Unauthorized access or missing token.


