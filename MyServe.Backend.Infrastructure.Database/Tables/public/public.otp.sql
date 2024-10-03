CREATE TABLE IF NOT EXISTS otp (
    id uuid primary key not null,
    user_id uuid not null,
    otp varchar(6) not null,
    expiry timestamp with time zone not null default (now() + interval '5 minutes'),
    constraint "fk_otp_user" foreign key (user_id) references "user" (id),
    constraint "ck_unique_user_otp" unique (user_id, otp)
);

ALTER TABLE otp ADD COLUMN device varchar(50) not null default 'WebApp';