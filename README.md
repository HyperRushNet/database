# RDB - Lightweight JSON File Database

RDB (Recursive Database) is a **lightweight, type-agnostic JSON file database** designed for simplicity, speed, and minimal resource usage. It allows you to store, retrieve, and manage structured data entirely via JSON files, without the need for a traditional database engine.

## Features

- **Type-Agnostic**: Store any type of data, such as `users`, `products`, `orders`, or `events`.
- **File-Based Storage**: Each item is stored as a JSON file on disk, organized in subfolders to optimize file system performance.
- **RESTful API**: Simple HTTP endpoints to create, read, and delete items using query parameters.
- **Low Resource Usage**: Designed for small environments (e.g., 0.1 CPU, <500MB RAM), suitable for cloud deployments.
- **Concurrency Safe**: Supports multiple simultaneous requests with in-memory indexing and thread-safe operations.
- **Raw JSON Access**: Retrieve full JSON payloads for inspection or processing.
- **CORS Enabled**: Can be accessed from frontend apps or dashboards.

## Directory Structure

/data <br>
├─ users/ <br>
│ ├─ f2/78/f278a836652c4e8497dc77a135640e67.json <br>
├─ products/ <br>
│ ├─ 41/9e/419e8c9cf8e2441381b060060460c904.json <br>
├─ orders/ <br>
│ └─ ... <br>
├─ events/ <br>
│ └─ ... <br>
<br>
Each JSON file contains:

```json
{
  "id": "1234abc",
  "type": "products",
  "createdAt": "2025-08-28T21:10:43.885Z",
  "payload": {
    "name": "Mini Product"
  },
  "relativePath": "products/12/34/1234abc.json",
  "sizeBytes": 123
}

```

## API Endpoints

POST /database?type=<type> → Add a new item of the specified type

GET /database/items?type=<type> → List all items of the specified type

GET /database/item?type=<type>&id=<id> → Retrieve a specific item by ID

DELETE /database/item?type=<type>&id=<id> → Delete a specific item by ID

## Usage

### Add an item:
```text
curl -X POST "https://[your-app].onrender.com/database?type=users" \
-H "Content-Type: application/json" \
-d '{"name": "your-name", "email": "your-name@example.com"}'
```

### Retrieve all items of a type:

curl "https://[your-app].onrender.com/database/items?type=users"


### Retrieve a single item:

curl "https://[your-app].onrender.com/database/item?type=users&id=<item-id>"


### Delete an item:

curl -X DELETE "https://[your-app].onrender.com/database/item?type=users&id=<item-id>"

## Advantages

- Transparent, human-readable storage
- Extremely lightweight and fast
- Easy to debug and migrate
- Flexible and expandable for any data type
