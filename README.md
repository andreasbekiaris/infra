# Position Tracking API

A simple **ASP.NET Core Web API** for storing geographic positions in **SQL Server** and calculating the distance between one position and all the others using the **Haversine formula**.

This project was created as a solution for the Infralabs position-tracking challenge. The task requires a Web API with raw SQL access, input validation, error handling, logging, and unit tests. The uploaded task specification defines three required endpoints, SQL Server storage, validation rules, logging, and tests fileciteturn0file1. The current implementation includes a `PositionsController`, a `Position`/`Distance` model, a `DistanceCalculator` helper, and unit tests for validation, distance calculation, and error handling fileciteturn0file0 fileciteturn0file5 fileciteturn0file6 fileciteturn0file4 fileciteturn0file3 fileciteturn0file2.

---

## Features

- **GET `/api/positions`**  
  Returns all saved positions ordered alphabetically by name.

- **POST `/api/positions`**  
  Validates and inserts a new position into the database using **raw SQL**.

- **GET `/api/positions/{name}/distance`**  
  Finds the selected position and calculates its distance from every other stored position.

- **Input validation** for:
  - null or empty names
  - whitespace-only names
  - invalid latitude and longitude ranges
  - duplicate position names

- **Error handling** for:
  - database failures
  - invalid input
  - duplicate insertion attempts

- **Logging** using built-in .NET logging for:
  - successful retrievals
  - successful inserts
  - successful distance calculations
  - runtime/database errors

- **Unit tests** with **xUnit** for:
  - Haversine calculation
  - validation logic
  - database failure handling

---

## Technologies Used

- **.NET 8 Web API**
- **ASP.NET Core MVC**
- **SQL Server**
- **Raw SQL with `Microsoft.Data.SqlClient`**
- **xUnit** for testing
- **Built-in .NET logging**

---

## Project Structure

```text
.
├── PositionsController.cs       # Main API controller
├── Position.cs                  # Position and Distance models
├── geopos.cs                    # Haversine distance helper
├── PositionsControllerTests.cs  # Validation tests
├── ErrorHandlingTests.cs        # Error handling tests
├── geohelptests.cs              # Distance formula test
└── README.md
```

The controller reads the database connection string from configuration, uses parameterized SQL queries, and logs important events such as successful operations and errors fileciteturn0file0. The model contains a nullable `Name` plus `Lat` and `Lon`, and also defines the `Distance` response object with `Name` and `Distance_km` fileciteturn0file5. The distance calculation is implemented in a separate `DistanceCalculator` class using the Haversine formula fileciteturn0file6.

---

## Database Schema

Use the following SQL Server table:

```sql
CREATE TABLE positions (
    pos_name NVARCHAR(64) PRIMARY KEY UNIQUE NOT NULL,
    pos_lat FLOAT NOT NULL,
    pos_lon FLOAT NOT NULL
);
```

This schema matches the task specification fileciteturn0file1.

---

## API Endpoints

### **1. Get all positions**

**Request**

```http
GET /api/positions
```

**Behavior**

- Opens a SQL connection
- Reads all rows from the `positions` table
- Maps each row to a `Position` object
- Returns the results ordered by `pos_name`

**Success Response**

```json
[
  {
    "name": "Athens",
    "lat": 37.98,
    "lon": 23.72
  },
  {
    "name": "Patras",
    "lat": 38.24,
    "lon": 21.73
  }
]
```

---

### **2. Insert a new position**

**Request**

```http
POST /api/positions
Content-Type: application/json
```

**Body**

```json
{
  "name": "Athens",
  "lat": 37.98,
  "lon": 23.72
}
```

**Validation rules**

- `Name` must not be null, empty, or whitespace
- `Lat` must be between `-90` and `90`
- `Lon` must be between `-180` and `180`
- `Name` must be unique in the database

**Possible responses**

- `200 OK` on success
- `400 BadRequest` for invalid input
- `409 Conflict` if the name already exists
- `500 Internal Server Error` for database/runtime failures

The POST method explicitly checks for null input, invalid names, invalid coordinates, and duplicate names before inserting with parameterized SQL commands fileciteturn0file0.

---

### **3. Get distances from one position to all others**

**Request**

```http
GET /api/positions/{name}/distance
```

**Example**

```http
GET /api/positions/Athens/distance
```

**Behavior**

- Validates the route parameter
- Reads the coordinates of the requested position
- Loads all positions from the database
- Skips the selected position itself
- Calculates distance in kilometers to every other position
- Returns a list of `Distance` objects

**Example response**

```json
[
  {
    "name": "Patras",
    "distance_km": 177.31
  },
  {
    "name": "Thessaloniki",
    "distance_km": 302.54
  }
]
```

The distance endpoint first searches for the requested position, returns `404 NotFound` if it does not exist, then calculates distances using `DistanceCalculator.CalculateDistance(...)` fileciteturn0file0 fileciteturn0file6.

---

## Haversine Formula

The project uses the Haversine formula to calculate the great-circle distance between two latitude/longitude points. The helper implementation is placed in `DistanceCalculator` and returns the distance in kilometers fileciteturn0file6.

```csharp
public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
{
    const double EarthRadiusKm = 6371;

    var dLat = (lat2 - lat1) * (Math.PI / 180);
    var dLon = (lon2 - lon1) * (Math.PI / 180);

    var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(lat1 * (Math.PI / 180)) * Math.Cos(lat2 * (Math.PI / 180)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

    return EarthRadiusKm * c;
}
```

---

## Logging

The controller uses `ILogger<PositionsController>` and records important events such as:

- number of retrieved positions
- successful inserts
- successful distance calculations
- exceptions during reads, inserts, and calculations

This is visible directly in the controller code through `LogInformation(...)` and `LogError(...)` calls fileciteturn0file0.

---

## Security and Code Quality Notes

### **What is done well**

- Uses **dependency injection** for configuration and logging
- Reads the connection string from configuration instead of hardcoding it
- Uses **parameterized SQL queries**, which helps prevent SQL injection
- Separates distance logic into a helper class
- Includes tests for important scenarios

### **Possible improvements**

- Use `async/await` with `OpenAsync`, `ExecuteReaderAsync`, and `ExecuteNonQueryAsync`
- Move database logic into a separate service/repository for cleaner architecture
- Add more unit tests for valid insertions and edge cases in distance calculations
- Add integration tests against a test database
- Add Docker support if required by the bonus section
- Confirm Swagger/OpenAPI is enabled in `Program.cs` for API documentation, since that setup is not shown in the uploaded files

---

## Setup Instructions

### **1. Clone the repository**

```bash
git clone <your-repository-url>
cd <your-project-folder>
```

### **2. Configure the connection string**

Add your SQL Server connection string to `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=InfralabsDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

The controller expects a connection string named `DefaultConnection` and throws an exception if it is missing fileciteturn0file0.

### **3. Create the database table**

Run this SQL script in SQL Server:

```sql
CREATE TABLE positions (
    pos_name NVARCHAR(64) PRIMARY KEY UNIQUE NOT NULL,
    pos_lat FLOAT NOT NULL,
    pos_lon FLOAT NOT NULL
);
```

### **4. Restore dependencies**

```bash
dotnet restore
```

### **5. Run the API**

```bash
dotnet run
```

### **6. Run the tests**

```bash
dotnet test
```

---

## Unit Tests

The uploaded tests currently cover the following areas:

### **Validation tests**

`PositionsControllerTests.cs` checks that invalid names return `BadRequest`, including:

- empty string
- null
- whitespace-only string

These tests validate the controller's input checks for POST requests fileciteturn0file4.

### **Error handling tests**

`ErrorHandlingTests.cs` creates a controller with an invalid SQL Server connection string and verifies that the API returns `500 Internal Server Error` for:

- `GetPositions()`
- `PostPositions(...)`
- `Getposdistance(...)`

This confirms that exceptions are caught and translated into consistent error responses fileciteturn0file2.

### **Haversine test**

`GeoHelperTests` verifies that the distance between the same point and itself is zero, which is a basic but important correctness test for the formula fileciteturn0file3.

---

## Example Workflow

### Insert a position

```http
POST /api/positions
```

```json
{
  "name": "Athens",
  "lat": 37.9838,
  "lon": 23.7275
}
```

### Insert another position

```http
POST /api/positions
```

```json
{
  "name": "Patras",
  "lat": 38.2466,
  "lon": 21.7346
}
```

### Get all positions

```http
GET /api/positions
```

### Calculate distances from Athens

```http
GET /api/positions/Athens/distance
```




