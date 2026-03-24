# Architecture & Design Decisions Explained

## 🎯 Why N-Tier Architecture?

### Problem
Without proper architecture, applications become:
- Hard to maintain (everything mixed together)
- Hard to test (tight coupling)
- Hard to scale (can't separate components)
- Hard to understand (no clear organization)

### Solution: N-Tier Architecture
Divides the application into **three separate layers**, each with a specific responsibility.

---

## 📦 Layer-by-Layer Explanation

### **Core Layer** (Business Logic)

**What goes here?**
- Entities (Movie, ShowTime, Ticket, User)
- Enums (TicketStatus, UserRole)
- Interfaces (Repository contracts)

**What does NOT go here?**
- Database code (EF Core)
- HTTP code (Controllers)
- External libraries

**Why?**
1. **Independence** - Core doesn't depend on any technology
2. **Reusability** - Can use same entities in web, desktop, or mobile apps
3. **Testability** - Easy to test business logic without database or HTTP
4. **Clarity** - All business rules in one place

**Example**:
```csharp
// Core/Entities/Movie.cs - Pure business entity
public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int DurationInMinutes { get; set; }
    // No database attributes, no HTTP concerns
}
```

---

### **Infrastructure Layer** (Data Access)

**What goes here?**
- ApplicationDbContext (EF Core)
- Repository implementations
- Database migrations
- External service implementations (Password hashing, JWT)

**Why separate from Core?**
1. **Technology Freedom** - Can switch from SQL Server to PostgreSQL without changing Core
2. **Testability** - Can mock repositories to test Core logic
3. **Encapsulation** - Database details hidden from other layers
4. **Single Responsibility** - Only handles data persistence

**Example**:
```csharp
// Infrastructure/Repositories/MovieRepository.cs
public class MovieRepository : IMovieRepository
{
    private readonly ApplicationDbContext _context;
    
    public async Task<Movie> AddAsync(Movie movie)
    {
        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();
        return movie;
    }
}
```

**Note**: Infrastructure implements interfaces from Core, not the other way around.

---

### **API Layer** (Presentation)

**What goes here?**
- Controllers (HTTP endpoints)
- DTOs (Data Transfer Objects)
- Dependency Injection setup
- Configuration (appsettings.json)

**Why separate?**
1. **Flexibility** - Can add GraphQL, gRPC, or SignalR without changing Core/Infrastructure
2. **Security** - Input validation at the boundary
3. **Format Control** - DTOs control what data is exposed
4. **Testing** - Can test HTTP behavior separately

**Example**:
```csharp
// API/Controllers/MoviesController.cs
[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _movieRepository;
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var movies = await _movieRepository.GetAllAsync();
        return Ok(movies);
    }
}
```

---

## 🔄 How Data Flows Between Layers

### **Example: User Books a Ticket**

```
┌─────────────┐
│   Client    │ (Postman, Browser, Mobile App)
│  (HTTP)     │
└──────┬──────┘
       │ POST /api/tickets + JWT Token
       │
┌──────▼──────────────────────────────┐
│      API Layer (Controllers)        │
│  • Validates JWT token              │
│  • Extracts user ID from token      │
│  • Validates input (DTO)            │
│  • Calls repository                 │
└──────┬──────────────────────────────┘
       │
┌──────▼──────────────────────────────┐
│  Infrastructure Layer (Repository)  │
│  • Checks showtime exists           │
│  • Checks available seats           │
│  • Creates ticket entity            │
│  • Uses EF Core to save             │
└──────┬──────────────────────────────┘
       │
┌──────▼──────────────────────────────┐
│      Database (SQL Server)          │
│  • Inserts ticket record            │
│  • Updates available seats          │
│  • Returns saved ticket             │
└──────┬──────────────────────────────┘
       │
       │ Returns ticket entity
       ▼
     Response back to client
```

**Key Points**:
1. **API Layer** never talks to database directly
2. **Infrastructure Layer** doesn't know about HTTP
3. **Core Layer** defines contracts (interfaces)
4. Each layer only knows about the layer below it

---

## 🔐 Authentication & Authorization Explained

### **Why Simple Authentication (No ASP.NET Identity)?**

**Reasons**:
1. **Learning** - Easier to understand how authentication works
2. **Control** - Full control over user model and logic
3. **Simplicity** - No unnecessary complexity
4. **Interview-Friendly** - Can explain every line of code

### **How Authentication Works**

#### **1. User Registration**
```
User Input: { email, password, fullName, role }
       ↓
Password Hashing (PBKDF2):
  • Salt generated (16 bytes random)
  • Password + Salt → Hash (10,000 iterations)
  • Store: Salt + Hash as Base64 string
       ↓
Save to Database: { email, passwordHash, fullName, role }
       ↓
Generate JWT Token:
  • Claims: UserId, Email, FullName, Role
  • Sign with secret key
  • Set expiration (24 hours)
       ↓
Return: { token, email, fullName, role }
```

#### **2. User Login**
```
User Input: { email, password }
       ↓
Find User by Email in Database
       ↓
Verify Password:
  • Extract salt from stored hash
  • Hash input password with same salt
  • Compare hashes
       ↓
If Valid → Generate JWT Token
       ↓
Return: { token, email, fullName, role }
```

#### **3. Making Authenticated Requests**
```
Client sends:
  Headers: Authorization: Bearer {token}
       ↓
ASP.NET Core Middleware:
  • Validates token signature
  • Checks expiration
  • Extracts claims (UserId, Role)
       ↓
If Valid → Populates HttpContext.User
       ↓
Controller can access:
  • User.FindFirstValue(ClaimTypes.NameIdentifier) → UserId
  • User.IsInRole("Admin") → Role check
```

---

### **How Authorization Works**

#### **Role-Based Authorization**

```csharp
// Only Admins can access
[Authorize(Roles = "Admin")]
public class MoviesController : ControllerBase
{
    // All actions require Admin role
}

// Mix of roles
public class ShowTimesController : ControllerBase
{
    [AllowAnonymous]  // Anyone
    public async Task<IActionResult> GetAll() { }
    
    [Authorize(Roles = "Admin")]  // Only Admin
    public async Task<IActionResult> Create() { }
}
```

#### **Authorization Flow**

```
Request with JWT Token
       ↓
Middleware validates token
       ↓
Extracts role from claims: "Admin" or "User"
       ↓
Controller has [Authorize(Roles = "Admin")]
       ↓
Middleware checks: Does user have Admin role?
       ↓
   YES → Request proceeds
   NO  → 403 Forbidden
```

---

## 🔑 Key Design Patterns Used

### **1. Repository Pattern**

**Why?**
- Abstracts data access
- Easy to mock for testing
- Can switch databases without changing business logic

**Example**:
```csharp
// Core defines contract
public interface IMovieRepository
{
    Task<Movie> GetByIdAsync(int id);
}

// Infrastructure implements
public class MovieRepository : IMovieRepository
{
    public async Task<Movie> GetByIdAsync(int id)
    {
        return await _context.Movies.FindAsync(id);
    }
}

// API uses interface (doesn't care about implementation)
public class MoviesController
{
    private readonly IMovieRepository _repo;
    
    public MoviesController(IMovieRepository repo)
    {
        _repo = repo;
    }
}
```

---

### **2. Dependency Injection**

**Why?**
- Loose coupling
- Easy testing (can inject mocks)
- Centralized configuration

**Example**:
```csharp
// Program.cs - Register services
builder.Services.AddScoped<IMovieRepository, MovieRepository>();

// Controller - Receives via constructor
public class MoviesController
{
    private readonly IMovieRepository _repo;
    
    public MoviesController(IMovieRepository repo)
    {
        _repo = repo; // Injected automatically
    }
}
```

---

### **3. DTO (Data Transfer Object) Pattern**

**Why?**
- Separate internal entities from external representation
- Control what data is exposed
- Validation at API boundary

**Example**:
```csharp
// Entity (Core)
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }  // Sensitive!
}

// DTO (API)
public class RegisterRequest
{
    public string Email { get; set; }
    public string Password { get; set; }  // Plain text (will be hashed)
}

public class AuthResponse
{
    public string Token { get; set; }
    public string Email { get; set; }
    // NO password hash exposed!
}
```

---

## 🗄️ Database Design Decisions

### **Code First Approach**

**Why Code First?**
1. **Version Control** - Database schema in code (migrations)
2. **Team Collaboration** - Everyone has same schema
3. **Automatic Updates** - Run migration command, database updates
4. **Refactoring** - Easy to change entities, migrations track changes

### **Relationships**

```
Movie (1) ──→ (Many) ShowTime
  • One movie can have multiple showtimes
  • DeleteBehavior.Cascade - Delete movie → delete showtimes

ShowTime (1) ──→ (Many) Ticket
  • One showtime can have multiple tickets
  • DeleteBehavior.Restrict - Cannot delete showtime with tickets

User (1) ──→ (Many) Ticket
  • One user can book multiple tickets
  • DeleteBehavior.Restrict - Cannot delete user with active tickets
```

### **Primary Keys**

All tables use auto-incremented integer IDs:
```csharp
public int Id { get; set; }
```

**Why?**
- Simple and efficient
- Database generates automatically
- Easy to reference (foreign keys)

---

## 🎯 Benefits of This Architecture

### **For Development**

1. **Clear Structure** - Know exactly where to put new code
2. **Easy Navigation** - Controllers → Repositories → Database
3. **Fast Development** - Reuse repositories, no duplicate code
4. **Team Friendly** - Multiple developers can work without conflicts

### **For Testing**

1. **Unit Tests** - Test business logic (Core) without database
2. **Integration Tests** - Test repositories with in-memory database
3. **API Tests** - Test endpoints with mocked repositories

### **For Maintenance**

1. **Bug Fixes** - Easy to locate issue (which layer?)
2. **Updates** - Change database? Only Infrastructure layer affected
3. **New Features** - Add endpoint? Only API layer affected

### **For Scaling**

1. **Horizontal Scaling** - Can run multiple API instances
2. **Caching** - Add caching layer in Infrastructure
3. **Microservices** - Can extract layers into separate services

---

## 📊 Comparison: With vs Without N-Tier

### **Without N-Tier (All in Controllers)**

```csharp
public class MoviesController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Database code mixed with HTTP code
        var connString = "Server=...";
        using var conn = new SqlConnection(connString);
        var cmd = new SqlCommand("SELECT * FROM Movies", conn);
        conn.Open();
        var reader = await cmd.ExecuteReaderAsync();
        // Messy, hard to test, hard to change
    }
}
```

**Problems**:
- Can't test without real database
- Can't reuse code
- Hard to maintain
- SQL injection risk
- No clear structure

---

### **With N-Tier (Clean Separation)**

```csharp
// API Layer
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _repo;
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var movies = await _repo.GetAllAsync();
        return Ok(movies);
    }
}

// Infrastructure Layer
public class MovieRepository : IMovieRepository
{
    public async Task<IEnumerable<Movie>> GetAllAsync()
    {
        return await _context.Movies.ToListAsync();
    }
}
```

**Benefits**:
- Easy to test (mock repository)
- Reusable code
- Clear structure
- Safe (EF Core parameterized queries)
- Easy to maintain

---

## 🎤 Interview Talking Points

### **"Why did you use N-Tier Architecture?"**

"I chose N-Tier architecture for several reasons:

1. **Separation of Concerns** - Each layer has one responsibility. Core handles business logic, Infrastructure handles data access, and API handles HTTP communication.

2. **Maintainability** - When I need to fix a bug, I know exactly which layer to look at. Database issue? Infrastructure layer. HTTP issue? API layer.

3. **Testability** - I can test each layer independently. Core layer can be tested without a database, and repositories can be mocked in controller tests.

4. **Scalability** - If I need to add a mobile app, I can reuse Core and Infrastructure layers, only adding a new presentation layer."

---

### **"How does your authentication work?"**

"I implemented JWT-based authentication from scratch:

1. **Registration** - User provides email and password. I hash the password using PBKDF2 with 10,000 iterations and a random salt, then store it in the database.

2. **Login** - User provides credentials. I retrieve the user, verify the password hash, and if valid, generate a JWT token with claims (user ID, email, role).

3. **Authentication** - Client sends the JWT token in the Authorization header. ASP.NET Core middleware validates the token signature and expiration, then populates the User context.

4. **Authorization** - Controllers use `[Authorize(Roles = "Admin")]` attribute. Middleware checks if the user's role claim matches the required role."

---

### **"Why didn't you use ASP.NET Identity?"**

"I wanted to understand authentication at a low level. By implementing it myself:

1. I learned how password hashing works (PBKDF2)
2. I learned how JWT tokens are structured and validated
3. I have full control over the user model
4. The code is simpler and easier to explain
5. It's perfect for learning and interviews

For production at scale, I would consider ASP.NET Identity for features like 2FA, password reset, and email confirmation."

---

### **"How does data flow in your application?"**

"Let me walk through a ticket booking example:

1. **Client** sends POST /api/tickets with JWT token
2. **API Layer** validates the token, extracts user ID, and validates input
3. **Repository** checks if showtime exists and has available seats
4. **EF Core** creates SQL query and executes it
5. **Database** inserts ticket record and updates seat count
6. **Response** flows back through the layers to the client

Each layer only talks to the layer directly below it, maintaining clean separation."

---

## 🎓 Summary

This architecture demonstrates:

✅ **Clean Code** - Each file has one purpose  
✅ **SOLID Principles** - Single responsibility, dependency inversion  
✅ **Industry Standards** - Repository pattern, dependency injection  
✅ **Scalability** - Easy to extend and maintain  
✅ **Security** - JWT authentication, password hashing, role-based authorization  
✅ **Interview-Ready** - Easy to explain and defend design decisions

Perfect for understanding professional software development practices!
