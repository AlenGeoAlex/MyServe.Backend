CREATE TABLE IF NOT EXISTS "user"(
    id uuid primary key not null ,
    email_address varchar not null,
    "provider" jsonb default '[]'::jsonb,
    is_locked boolean default false not null,
    last_login timestamp with time zone null,
    constraint "uk_user_email_unique" unique (email_address)
)