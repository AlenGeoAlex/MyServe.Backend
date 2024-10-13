CREATE OR REPLACE FUNCTION files."delete_recursive"(fileId uuid)
    RETURNS TABLE(id uuid, name varchar(100), type files.filetype, target_url varchar(255)) AS $$
DECLARE
    child RECORD;
BEGIN
    -- Check if the file exists and is not already deleted
    IF EXISTS(SELECT 1 FROM files."file" f WHERE f.is_deleted = false AND f.id = delete_recursive.fileId) THEN
        -- If it is a directory, iterate through its children
        IF EXISTS(SELECT 1 FROM files."file" f WHERE f.id = delete_recursive.fileId AND f.type = 'dir') THEN
            FOR child IN
                SELECT *
                FROM files."file"
                WHERE parent = delete_recursive.fileId AND is_deleted = false
                LOOP
                    -- Recursively call the function for the child
                    RETURN QUERY SELECT * FROM files."delete_recursive"(child.id);
                END LOOP;
        END IF;

        -- Mark the current file as deleted
        UPDATE files."file" f
        SET is_deleted = true
        WHERE f.id = delete_recursive.fileId;

        -- Return the current file's details
        RETURN QUERY SELECT f.id, f.name, f.type, f.target_url
                     FROM files."file" f
                     WHERE f.id = delete_recursive.fileId;
    ELSE
        RAISE EXCEPTION 'Non existing';
    END IF;
END;
$$ LANGUAGE plpgsql;
