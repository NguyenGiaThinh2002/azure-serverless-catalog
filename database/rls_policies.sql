-- Row Level Security (RLS) Policies for Supabase
-- Run this script in your Supabase SQL Editor after setting up authentication

-- Enable RLS on tables
ALTER TABLE categories ENABLE ROW LEVEL SECURITY;
ALTER TABLE products ENABLE ROW LEVEL SECURITY;

-- Create a function to get the current user's role from JWT
CREATE OR REPLACE FUNCTION auth.user_role()
RETURNS TEXT AS $$
BEGIN
  RETURN COALESCE(
    (current_setting('request.jwt.claims', true)::jsonb->>'role')::TEXT,
    'Viewer'
  );
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Create a function to get the current user ID from JWT
CREATE OR REPLACE FUNCTION auth.user_id()
RETURNS TEXT AS $$
BEGIN
  RETURN (current_setting('request.jwt.claims', true)::jsonb->>'sub')::TEXT;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Policy: Viewers can read all categories and products
CREATE POLICY "Viewers can read categories"
  ON categories FOR SELECT
  USING (true);

CREATE POLICY "Viewers can read products"
  ON products FOR SELECT
  USING (true);

-- Policy: Admins can do everything
CREATE POLICY "Admins can manage categories"
  ON categories FOR ALL
  USING (auth.user_role() = 'Admin');

CREATE POLICY "Admins can manage products"
  ON products FOR ALL
  USING (auth.user_role() = 'Admin');

-- Policy: Viewers cannot modify data (only Admins can)
-- This is implicit through the Admin-only policies above

-- Optional: Create a users table that syncs with Supabase auth.users
-- This allows you to store additional user metadata
CREATE TABLE IF NOT EXISTS public.user_profiles (
    id UUID PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
    email TEXT NOT NULL,
    name TEXT,
    role TEXT NOT NULL DEFAULT 'Viewer' CHECK (role IN ('Admin', 'Viewer')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ
);

-- Enable RLS on user_profiles
ALTER TABLE user_profiles ENABLE ROW LEVEL SECURITY;

-- Policy: Users can read their own profile
CREATE POLICY "Users can read own profile"
  ON user_profiles FOR SELECT
  USING (auth.uid()::TEXT = id::TEXT);

-- Policy: Admins can read all profiles
CREATE POLICY "Admins can read all profiles"
  ON user_profiles FOR SELECT
  USING (auth.user_role() = 'Admin');

-- Function to automatically create user profile on signup
CREATE OR REPLACE FUNCTION public.handle_new_user()
RETURNS TRIGGER AS $$
BEGIN
  INSERT INTO public.user_profiles (id, email, name, role)
  VALUES (
    NEW.id,
    NEW.email,
    COALESCE(NEW.raw_user_meta_data->>'full_name', NEW.email),
    COALESCE(NEW.raw_user_meta_data->>'role', 'Viewer')
  );
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Trigger to create profile on new user signup
DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
CREATE TRIGGER on_auth_user_created
  AFTER INSERT ON auth.users
  FOR EACH ROW EXECUTE FUNCTION public.handle_new_user();

-- Grant necessary permissions
GRANT USAGE ON SCHEMA public TO anon, authenticated;
GRANT ALL ON public.categories TO anon, authenticated;
GRANT ALL ON public.products TO anon, authenticated;
GRANT ALL ON public.user_profiles TO anon, authenticated;

