CREATE TABLE IF NOT EXISTS profile
(
    id uuid not null primary key,
    first_name varchar(30) not null,
    last_name varchar (30) not null,
    profile_image varchar(100),
    profile_settings jsonb not null default '{}'::jsonb,
    created timestamp with time zone not null default now(),
    constraint "fk_profile_user_id" foreign key (id) references "user" (id)
)