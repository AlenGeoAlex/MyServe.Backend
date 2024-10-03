CREATE TABLE IF NOT EXISTS refresh_token
(
    id uuid primary key not null,
    user_id uuid not null,
    created_at timestamp with time zone not null,
    expiry timestamp with time zone not null,
    constraint "fk_refresh_token_user" foreign key (user_id) references "user" (id)
)