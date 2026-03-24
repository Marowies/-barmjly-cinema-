# Quick Start Guide

## ⚡ Get Started in 5 Minutes

### Prerequisites
- ✅ .NET 9.0 SDK installed
- ✅ SQL Server (LocalDB, Express, or Full)

### Step 1: Update Connection String

Open `Cinema.API/appsettings.json` and update:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CinemaDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

**Note**: Replace `(localdb)\\mssqllocaldb` with your SQL Server instance.

### Step 2: Create Database

```bash
cd Cinema.API
dotnet ef database update --project ../Cinema.Infrastructure/Cinema.Infrastructure.csproj --startup-project Cinema.API.csproj
```

### Step 3: Run the API

```bash
dotnet run
```

### Step 4: Open Swagger

Browser: `https://localhost:5001/swagger`

---

## 🎯 Test with Swagger

### 1. Register Admin User

**POST** `/api/auth/register`

```json
{
  "email": "admin@cinema.com",
  "password": "Admin123!",
  "fullName": "Admin User",
  "role": "Admin"
}
```

**Response**: Copy the `token`

---

### 2. Authorize in Swagger

Click **Authorize** button (🔓 icon)

Enter: `Bearer {your_token_here}`

Click **Authorize**

---

### 3. Add a Movie (Admin)

**POST** `/api/movies`

```json
{
  "title": "Inception",
  "description": "A mind-bending thriller",
  "durationInMinutes": 148,
  "releaseDate": "2010-07-16",
  "genre": "Sci-Fi"
}
```

---

### 4. Add ShowTime (Admin)

**POST** `/api/showtimes`

```json
{
  "movieId": 1,
  "startTime": "2026-02-10T18:00:00",
  "endTime": "2026-02-10T20:30:00",
  "price": 12.50,
  "availableSeats": 100
}
```

---

### 5. Register Regular User

**POST** `/api/auth/register`

```json
{
  "email": "user@cinema.com",
  "password": "User123!",
  "fullName": "John Doe",
  "role": "User"
}
```

**Response**: Copy the `token`

---

### 6. Re-Authorize with User Token

Click **Authorize** button

Enter: `Bearer {user_token}`

Click **Authorize**

---

### 7. Book a Ticket (User)

**POST** `/api/tickets`

```json
{
  "showTimeId": 1,
  "seatNumber": 15
}
```

---

### 8. View My Tickets (User)

**GET** `/api/tickets/my-tickets`

---

### 9. Cancel Ticket (User)

**POST** `/api/tickets/1/cancel`

---

## 📋 Common Commands

### Build Solution
```bash
dotnet build CinemaBookingSystem.sln
```

### Run API
```bash
cd Cinema.API
dotnet run
```

### Create Migration
```bash
cd Cinema.API
dotnet ef migrations add MigrationName --project ../Cinema.Infrastructure/Cinema.Infrastructure.csproj --startup-project Cinema.API.csproj
```

### Update Database
```bash
cd Cinema.API
dotnet ef database update --project ../Cinema.Infrastructure/Cinema.Infrastructure.csproj --startup-project Cinema.API.csproj
```

### Drop Database
```bash
cd Cinema.API
dotnet ef database drop --project ../Cinema.Infrastructure/Cinema.Infrastructure.csproj --startup-project Cinema.API.csproj
```

---

## 🔍 Troubleshooting

### Connection Error
- Check SQL Server is running
- Verify connection string in `appsettings.json`
- Try: `Server=localhost;...` or `Server=(localdb)\\mssqllocaldb;...`

### JWT Error
- Token expired? Re-login to get new token
- Wrong token format? Use `Bearer {token}`
- Copy full token without spaces

### 401 Unauthorized
- Make sure you clicked **Authorize** in Swagger
- Check token is valid and not expired
- Verify you're using the correct role (Admin/User)

### 403 Forbidden
- You don't have permission for this endpoint
- Admin endpoints require Admin token
- User endpoints require User token

### Migration Error
- Make sure you're in `Cinema.API` folder
- Check both project paths are correct
- Try: `dotnet clean` then rebuild

---

## 🎯 Quick Test Flow

1. ✅ Register Admin → Get token
2. ✅ Authorize with Admin token
3. ✅ Add Movie
4. ✅ Add ShowTime
5. ✅ Register User → Get token
6. ✅ Authorize with User token
7. ✅ Book Ticket
8. ✅ View My Tickets
9. ✅ Cancel Ticket

---

## 📚 Documentation

- **README.md** - Complete project documentation
- **ARCHITECTURE_EXPLAINED.md** - Architecture deep dive
- **PROJECT_SUMMARY.md** - Project overview

---

## 🎉 You're Ready!

Your Cinema Booking System is now running with:
- ✅ N-Tier Architecture
- ✅ JWT Authentication
- ✅ Role-Based Authorization
- ✅ Complete CRUD Operations
- ✅ Database Migrations

Happy coding! 🚀
