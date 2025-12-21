-- 100_seed_data.sql

INSERT INTO
    users (email, password, full_name)
VALUES (
        'admin@test.com',
        '123456',
        'Admin User'
    ),
    (
        'user@test.com',
        '123456',
        'Normal User'
    );

INSERT INTO
    products (name, price)
VALUES ('Book', 100000),
    ('Laptop', 15000000);

INSERT INTO orders (user_id, total) VALUES (1, 15100000);