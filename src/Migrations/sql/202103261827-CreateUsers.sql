CREATE TABLE AppUser(
    id serial PRIMARY KEY,
    email TEXT NOT NULL,
    password TEXT NOT NULL,
    last_logged_in TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    max_api_calls INT DEFAULT 10,
    address TEXT NOT NULL,
    name TEXT NOT NULL
);