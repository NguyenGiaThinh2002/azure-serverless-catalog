# Supabase Setup and Testing Guide

This guide will help you set up and test the Supabase connection for the Catalog API.

## Prerequisites

1. A Supabase account (sign up at https://supabase.com)
2. A Supabase project created
3. .NET 8 SDK installed

## Step 1: Get Your Supabase Connection String

1. Log in to your Supabase dashboard
2. Go to your project settings
3. Navigate to **Database** → **Connection string**
4. Select **URI** format
5. Copy the connection string. It should look like:
   ```
   postgresql://postgres:[YOUR-PASSWORD]@db.[YOUR-PROJECT-REF].supabase.co:5432/postgres
   ```

## Step 2: Convert to Npgsql Connection String Format

Convert the Supabase URI to Npgsql format:

```
Host=db.[YOUR-PROJECT-REF].supabase.co;Port=5432;Database=postgres;Username=postgres;Password=[YOUR-PASSWORD];SSL Mode=Require;Trust Server Certificate=true
```

**Example:**
```
Host=db.abcdefghijklmnop.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your-super-secret-password;SSL Mode=Require;Trust Server Certificate=true
```

## Step 3: Set Up the Database Schema

1. In your Supabase dashboard, go to **SQL Editor**
2. Open the file `database/schema.sql` from this project
3. Copy and paste the entire SQL script into the SQL Editor
4. Click **Run** to execute the script
5. Verify that the `categories` and `products` tables were created

## Step 4: Configure the Connection String

### For Local Development:

1. Open `src/backend/Catalog.Api/local.settings.json`
2. Update the `SUPABASE_CONNECTION_STRING` value with your connection string:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "Functions:Worker:HostEndpoint": "http://localhost:7071",
    "SUPABASE_CONNECTION_STRING": "Host=db.your-project.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your-password;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

### For Production:

Set the `SUPABASE_CONNECTION_STRING` environment variable in your Azure Functions app settings.

## Step 5: Restore Dependencies

Run the following command to restore the new NuGet packages:

```bash
cd src/backend/Catalog.Api
dotnet restore
```

## Step 6: Test the Connection

### Method 1: Using the Health Check Endpoint

1. Start the Azure Functions app:
   ```bash
   cd src/backend/Catalog.Api
   dotnet run
   ```

2. The API should start on `http://localhost:7071`

3. Test the health check endpoint:
   ```bash
   curl http://localhost:7071/api/health
   ```

   Or open in browser: `http://localhost:7071/api/health`

4. You should see a response indicating that the Supabase health check is healthy:
   ```json
   {
     "status": "Healthy",
     "checks": {
       "basic_health": "Healthy",
       "catalog_supabase": "Healthy"
     }
   }
   ```

### Method 2: Using a Simple Test Script

Create a test file `test-connection.cs`:

```csharp
using Npgsql;

var connectionString = Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING")
    ?? "Host=db.your-project.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your-password;SSL Mode=Require;Trust Server Certificate=true";

try
{
    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    Console.WriteLine("✅ Successfully connected to Supabase!");
    
    var command = new NpgsqlCommand("SELECT version();", connection);
    var version = await command.ExecuteScalarAsync();
    Console.WriteLine($"PostgreSQL Version: {version}");
    
    // Test if tables exist
    var tableCheck = new NpgsqlCommand(
        "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name IN ('categories', 'products');", 
        connection);
    var reader = await tableCheck.ExecuteReaderAsync();
    
    Console.WriteLine("\nTables found:");
    while (await reader.ReadAsync())
    {
        Console.WriteLine($"  - {reader.GetString(0)}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Connection failed: {ex.Message}");
}
```

Run it:
```bash
dotnet script test-connection.cs
```

### Method 3: Using Supabase Dashboard

1. Go to your Supabase dashboard
2. Navigate to **Database** → **Table Editor**
3. You should see the `categories` and `products` tables
4. Try inserting a test record manually to verify the schema

## Step 7: Test CRUD Operations

Once the connection is verified, test the API endpoints:

1. **Create a Category:**
   ```bash
   curl -X POST http://localhost:7071/api/categories \
     -H "Content-Type: application/json" \
     -d '{"name": "Electronics", "description": "Electronic products"}'
   ```

2. **Get All Categories:**
   ```bash
   curl http://localhost:7071/api/categories
   ```

3. **Create a Product:**
   ```bash
   curl -X POST http://localhost:7071/api/products \
     -H "Content-Type: application/json" \
     -d '{"name": "Laptop", "description": "Gaming laptop", "price": 999.99, "categoryId": "[category-id]", "stock": 10}'
   ```

## Troubleshooting

### Connection Timeout
- Verify your connection string is correct
- Check if your IP is allowed in Supabase (Settings → Database → Connection Pooling)
- Ensure SSL Mode is set to `Require`

### Authentication Failed
- Double-check your password in the connection string
- Verify the username is `postgres` (default)

### Table Not Found
- Make sure you ran the `database/schema.sql` script
- Check the table names match: `categories` and `products` (lowercase)

### SSL/TLS Errors
- Ensure `Trust Server Certificate=true` is in your connection string
- For production, consider using proper SSL certificates

## Security Notes

- **Never commit** your connection string with passwords to version control
- Use environment variables or Azure Key Vault for production
- Consider using connection pooling for better performance
- Rotate your database passwords regularly

## Additional Resources

- [Supabase Documentation](https://supabase.com/docs)
- [Npgsql Documentation](https://www.npgsql.org/doc/)
- [Dapper Documentation](https://github.com/DapperLib/Dapper)




