CREATE TABLE IF NOT EXISTS profile
(
    id uuid not null primary key,
    first_name varchar(50) not null,
    last_name varchar (50) not null,
    profile_image varchar(300),
    profile_settings jsonb not null default '{}'::jsonb,
    encryption_key varchar(40) not null,
    created_at timestamp with time zone not null default now(),
    constraint "fk_profile_user_id" foreign key (id) references "user" (id)
)