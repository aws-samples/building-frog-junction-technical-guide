CREATE TABLE inventory (
    item_uid SERIAL PRIMARY KEY,
    owner_id TEXT,
    type TEXT,
    in_pocket BOOLEAN,
    wearable BOOLEAN,
    worn BOOLEAN,
    droppable BOOLEAN,
    dropped BOOLEAN,
    location_x NUMERIC,
    location_y NUMERIC
);