# Supabase Authentication Setup Guide

This guide will help you set up Supabase authentication with Microsoft Entra ID (Azure AD) OAuth integration.

## Step 1: Get Supabase Credentials

1. Go to your Supabase project dashboard
2. Navigate to **Settings** → **API**
3. Copy the following values:
   - **Project URL** (e.g., `https://xxxxx.supabase.co`)
   - **anon/public key** (this is your `SUPABASE_ANON_KEY`)

## Step 2: Configure Environment Variables

Add these to your `local.settings.json`:

```json
{
  "Values": {
    "SUPABASE_URL": "https://your-project-ref.supabase.co",
    "SUPABASE_ANON_KEY": "your-anon-key-here",
    "SUPABASE_CONNECTION_STRING": "Host=db.your-project.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your-password;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

## Step 3: Set Up Microsoft Entra ID OAuth Provider

### In Azure Portal:

1. Go to **Azure Active Directory** → **App registrations**
2. Click **New registration**
3. Configure:
   - **Name**: `Catalog E-Commerce App`
   - **Supported account types**: Accounts in any organizational directory and personal Microsoft accounts
   - **Redirect URI**: 
     - Type: Web
     - URI: `https://your-project-ref.supabase.co/auth/v1/callback`
4. After creation, note:
   - **Application (client) ID**
   - Go to **Certificates & secrets** → Create a new client secret → Note the **Value**

### In Supabase Dashboard:

1. Go to **Authentication** → **Providers**
2. Find **Azure (Microsoft)** and click **Configure**
3. Enable the provider
4. Enter:
   - **Client ID (for Azure AD)**: Your Application (client) ID from Azure
   - **Client Secret (for Azure AD)**: Your client secret value from Azure
   - **Redirect URL**: `https://your-project-ref.supabase.co/auth/v1/callback`
5. Click **Save**

## Step 4: Set Up Row Level Security (RLS)

1. Go to your Supabase dashboard
2. Navigate to **SQL Editor**
3. Run the `database/rls_policies.sql` script
4. This will:
   - Enable RLS on your tables
   - Create policies for Admin and Viewer roles
   - Set up user profile sync

## Step 5: Create Test Users

### Option 1: Through Supabase Dashboard

1. Go to **Authentication** → **Users**
2. Click **Add user** → **Create new user**
3. Set email and password
4. In the user metadata, add:
   ```json
   {
     "role": "Admin"
   }
   ```

### Option 2: Through Microsoft Entra ID OAuth

1. Users can sign in using their Microsoft account
2. First-time users will be assigned the "Viewer" role by default
3. Admins can update roles in the `user_profiles` table

## Step 6: Test Authentication

### Test Token Validation:

```bash
# Get a token from Supabase (you'll need to implement the frontend login first)
# Then test the validation endpoint:
curl -X POST http://localhost:7071/api/auth/validate \
  -H "Content-Type: application/json" \
  -d '{"token": "your-jwt-token-here"}'
```

### Test User Info:

```bash
curl http://localhost:7071/api/auth/me \
  -H "Authorization: Bearer your-jwt-token-here"
```

## Step 7: Update User Roles

To promote a user to Admin, run this SQL in Supabase:

```sql
UPDATE public.user_profiles
SET role = 'Admin'
WHERE email = 'user@example.com';
```

Or update the user metadata in Supabase Auth:

1. Go to **Authentication** → **Users**
2. Find the user
3. Click **Edit**
4. In **Raw App Meta Data**, add:
   ```json
   {
     "role": "Admin"
   }
   ```

## Security Notes

- **Never commit** your `SUPABASE_ANON_KEY` or connection strings to version control
- Use environment variables in production
- The `SUPABASE_ANON_KEY` is safe to use in frontend code (it's public)
- Always validate tokens on the backend
- RLS policies ensure data security at the database level

## Troubleshooting

### "Invalid token" errors:
- Verify your `SUPABASE_URL` and `SUPABASE_ANON_KEY` are correct
- Check that the token hasn't expired
- Ensure the token is from the correct Supabase project

### RLS blocking queries:
- Verify RLS policies are enabled
- Check that the JWT contains the correct role claim
- Ensure the user has the appropriate role assigned

### OAuth redirect not working:
- Verify the redirect URI in Azure matches exactly
- Check that the OAuth provider is enabled in Supabase
- Ensure HTTPS is used (required for OAuth)

