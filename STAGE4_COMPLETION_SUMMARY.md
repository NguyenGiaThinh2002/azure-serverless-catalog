# Stage 4: Authentication - Implementation Complete ✅

## What Has Been Implemented

### 1. **Authentication Infrastructure** ✅
- Added Supabase client package for authentication
- Created `AuthService` for JWT token validation
- Implemented user role-based authorization (Admin/Viewer)

### 2. **JWT Authentication Middleware** ✅
- Created `JwtAuthMiddleware` that intercepts all HTTP requests
- Validates JWT tokens from Authorization header
- Extracts user information and adds to function context
- Skips authentication for health check and Swagger endpoints

### 3. **Authentication Endpoints** ✅
- `GET /api/auth/me` - Get current user information
- `POST /api/auth/validate` - Validate a JWT token

### 4. **Authorization Helpers** ✅
- Created `AuthorizationHelper` class with utility methods
- Helper methods for checking user roles
- Helper methods for creating unauthorized/forbidden responses

### 5. **Row Level Security (RLS)** ✅
- Created SQL script for RLS policies (`database/rls_policies.sql`)
- Policies for Admin and Viewer roles
- User profile sync with Supabase Auth
- Automatic profile creation on user signup

### 6. **User Entity** ✅
- Created `User` entity in `Catalog.Core.Entities`
- Includes Id, Email, Name, and Role properties

## Configuration Required

### Step 1: Get Supabase Credentials

1. Go to your Supabase dashboard: https://supabase.com/dashboard
2. Select your project
3. Go to **Settings** → **API**
4. Copy:
   - **Project URL** → Use as `SUPABASE_URL`
   - **anon public key** → Use as `SUPABASE_ANON_KEY`

### Step 2: Update local.settings.json

Update `src/backend/Catalog.Api/local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "Functions:Worker:HostEndpoint": "http://localhost:7071",
    "SUPABASE_CONNECTION_STRING": "Host=db.pkcndnqftkspbixbyjtl.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=Thinhlatoi2;SSL Mode=Require;Trust Server Certificate=true",
    "SUPABASE_URL": "https://pkcndnqftkspbixbyjtl.supabase.co",
    "SUPABASE_ANON_KEY": "YOUR_ANON_KEY_FROM_SUPABASE_DASHBOARD"
  }
}
```

### Step 3: Set Up Row Level Security

1. Go to Supabase dashboard → **SQL Editor**
2. Run the script: `database/rls_policies.sql`
3. This will:
   - Enable RLS on categories and products tables
   - Create policies for Admin and Viewer roles
   - Set up user profile synchronization

### Step 4: Set Up Microsoft Entra ID OAuth (Optional but Recommended)

Follow the detailed guide in `database/auth_setup_guide.md` to:
1. Create an Azure AD app registration
2. Configure OAuth in Supabase
3. Set up redirect URIs

## How Authentication Works

### Request Flow:
1. Client sends request with `Authorization: Bearer <token>` header
2. `JwtAuthMiddleware` intercepts the request
3. Token is extracted and validated by `AuthService`
4. User information is extracted from JWT claims
5. User object is added to function context
6. Function can access user via `AuthorizationHelper.GetUserFromContext()`

### Role-Based Access:
- **Admin**: Can perform all operations (CRUD on all resources)
- **Viewer**: Can only read data (SELECT operations)

### Protected Endpoints:
- All endpoints except `/api/health` and `/swagger/*` require authentication
- Functions can check user role using `AuthorizationHelper.IsAuthorized()`

## Testing Authentication

### 1. Test Token Validation:

```bash
curl -X POST http://localhost:7071/api/auth/validate \
  -H "Content-Type: application/json" \
  -d '{"token": "your-jwt-token-here"}'
```

### 2. Test User Info Endpoint:

```bash
curl http://localhost:7071/api/auth/me \
  -H "Authorization: Bearer your-jwt-token-here"
```

### 3. Test Protected Endpoint (without token):

```bash
curl http://localhost:7071/api/products
# Should return 401 Unauthorized
```

### 4. Test Protected Endpoint (with token):

```bash
curl http://localhost:7071/api/products \
  -H "Authorization: Bearer your-jwt-token-here"
# Should return data if user has Viewer or Admin role
```

## Next Steps

### Immediate Actions:
1. ✅ Add `SUPABASE_URL` and `SUPABASE_ANON_KEY` to `local.settings.json`
2. ✅ Run `database/rls_policies.sql` in Supabase SQL Editor
3. ✅ Restore NuGet packages: `dotnet restore`
4. ✅ Test the authentication endpoints

### For Stage 5 (Backend APIs):
- You'll need to update Product and Category functions to:
  - Use `AuthorizationHelper` to get current user
  - Check roles before allowing modifications
  - Return appropriate error messages

### Example Function Pattern:

```csharp
[Function("GetProducts")]
public async Task<HttpResponseData> GetProducts(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequestData req,
    FunctionContext context)
{
    // User is already validated by middleware
    var user = AuthorizationHelper.GetUserFromContext(context);
    
    // Viewer and Admin can read
    if (user == null)
    {
        return await AuthorizationHelper.CreateUnauthorizedResponseAsync(req);
    }
    
    // Your business logic here
    // ...
}
```

## Files Created/Modified

### New Files:
- `src/backend/Catalog.Core/Entities/User.cs`
- `src/backend/Catalog.Core/Services/IAuthService.cs`
- `src/backend/Catalog.Infrastructure/Services/AuthService.cs`
- `src/backend/Catalog.Api/Middleware/JwtAuthMiddleware.cs`
- `src/backend/Catalog.Api/Helpers/AuthorizationHelper.cs`
- `src/backend/Catalog.Api/Functions/AuthFunctions.cs`
- `database/rls_policies.sql`
- `database/auth_setup_guide.md`

### Modified Files:
- `src/backend/Catalog.Infrastructure/Catalog.Infrastructure.csproj` - Added Supabase and JWT packages
- `src/backend/Catalog.Api/Catalog.Api.csproj` - Added JWT Bearer package
- `src/backend/Catalog.Infrastructure/ServiceCollectionExtensions.cs` - Added Supabase client and AuthService
- `src/backend/Catalog.Api/Program.cs` - Registered JWT middleware
- `src/backend/Catalog.Api/local.settings.json` - Added Supabase URL and key placeholders

## Security Notes

⚠️ **Important Security Considerations:**

1. **Never commit** `SUPABASE_ANON_KEY` or connection strings to version control
2. The `SUPABASE_ANON_KEY` is safe for frontend use (it's public)
3. Always validate tokens on the backend (already implemented)
4. RLS policies provide an additional security layer at the database level
5. Use HTTPS in production (required for OAuth)

## Troubleshooting

### "SUPABASE_URL not found" error:
- Ensure `SUPABASE_URL` is in `local.settings.json` Values section
- Restart the Azure Functions app after adding

### "Invalid token" errors:
- Verify token is from the correct Supabase project
- Check that token hasn't expired
- Ensure `SUPABASE_ANON_KEY` matches your project

### RLS blocking queries:
- Verify RLS policies are enabled in Supabase
- Check user role is set correctly in user metadata
- Ensure JWT contains role claim

## Ready for Stage 5! 🚀

Your authentication infrastructure is complete. You can now proceed to Stage 5: Backend APIs - Full CRUD Endpoints, where you'll implement the Product and Category endpoints with proper authentication and authorization.

