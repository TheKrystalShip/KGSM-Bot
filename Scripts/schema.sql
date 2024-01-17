CREATE TABLE IF NOT EXISTS services (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    working_dir TEXT NOT NULL,
    installed_version TEXT DEFAULT '0',
    is_steam BOOLEAN DEFAULT 1
);
