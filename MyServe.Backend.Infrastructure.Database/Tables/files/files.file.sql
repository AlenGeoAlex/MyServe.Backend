CREATE TABLE IF NOT EXISTS "files"."file"(
    "id" uuid primary key not null,
    "name" varchar(100) not null,
    "parent" uuid null,
    "type" files.filetype not null,
    "owner" uuid not null,
    "created_at" timestamp with time zone not null default now(),
    "target_url" varchar(255) null,
    "target_size" bigint not null default 0,
    "mime_type" varchar(255),
    "created" uuid not null,
    "is_deleted" boolean not null default false,
    "favourite" boolean not null default false,
    constraint "fk_file_parent" foreign key (parent) references files.file (id),
    constraint "fk_file_created" foreign key (created) references public.profile (id),
    constraint "fk_file_owner" foreign key (owner) references public.profile (id),
    constraint "unq_file_name_parent_owner" unique ("name", "parent", "owner")
);

CREATE OR REPLACE FUNCTION check_file_validation()
RETURNS TRIGGER AS $$
DECLARE
    file_type files.filetype;
    is_soft_deleted boolean;
    parent_owner uuid;    
BEGIN
    IF NEW.parent IS NOT NULL THEN
        SELECT type, is_deleted, owner
        INTO file_type, is_soft_deleted, parent_owner
        FROM files.file
        WHERE id = NEW.parent;

        IF file_type <> 'dir' THEN
            RAISE EXCEPTION 'Parent is not a directory';
        END IF;

        IF parent_owner <> NEW.owner THEN
            RAISE EXCEPTION 'No permission in parent' ;
        END IF;

        IF is_soft_deleted THEN
            RAISE EXCEPTION 'Parent file is soft deleted';
        END IF;            
    END IF;
    
    IF NEW.type = 'obj' THEN
        IF NEW.target_url IS NULL THEN
            RAISE EXCEPTION 'Target Url should be present for files';
        END IF;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER validate_parent_type
    BEFORE INSERT OR UPDATE ON files.file
    FOR EACH ROW EXECUTE FUNCTION check_file_validation();