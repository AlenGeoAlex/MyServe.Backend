CREATE OR REPLACE FUNCTION public.cleanup()
    RETURNS void AS $$
BEGIN
    DELETE FROM refresh_token
    WHERE expiry < NOW();
END;
$$ LANGUAGE plpgsql;