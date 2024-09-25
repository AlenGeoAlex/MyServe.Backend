-- Create Table Profile

CREATE OR REPLACE FUNCTION update_profile_email()
RETURNS TRIGGER AS $$
BEGIN
    -- Update the email in the public.profile table
UPDATE public.profile
SET email = NEW.email
WHERE id = NEW.id;  -- Assuming 'id' is the common identifier

RETURN NEW;  -- Return the new record
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER users_email_update
    AFTER UPDATE OF email ON auth.users
    FOR EACH ROW
    WHEN (OLD.email IS DISTINCT FROM NEW.email) 
EXECUTE FUNCTION update_profile_email();