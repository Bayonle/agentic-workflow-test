---
feature: Creating a Todo
status: draft
created: 2026-02-02
author: PM Agent
task_id: task-001
priority: P2
---

# Creating a Todo

## Problem Statement

The TodoApi project currently has JWT authentication infrastructure but lacks the core todo management functionality that users need. Users need the ability to create, read, update, and delete todo items to manage their tasks effectively.

## Context

**Current State:**
- .NET 9 Web API with Swagger documentation
- JWT authentication with ASP.NET Core Identity
- SQLite database with Entity Framework Core
- Authentication endpoints (register, login)
- No Todo model or endpoints yet

**Gap:**
The API is named "TodoApi" but has no todo functionality. This feature will implement the core CRUD operations for todo items.

## User Stories

### Story 1: User creates a new todo
**As a** authenticated user
**I want** to create a new todo item
**So that** I can track tasks I need to complete

**Acceptance Criteria:**
- [ ] User can POST to `/api/todos` with title and optional description
- [ ] Todo is created with authenticated user as owner
- [ ] Todo has unique ID, creation timestamp, and completion status
- [ ] API returns 201 Created with the new todo
- [ ] API returns 401 Unauthorized if not authenticated
- [ ] API returns 400 Bad Request if title is missing or invalid

### Story 2: User views their todos
**As a** authenticated user
**I want** to view all my todo items
**So that** I can see what tasks I have

**Acceptance Criteria:**
- [ ] User can GET `/api/todos` to list all their todos
- [ ] Response includes only todos owned by the authenticated user
- [ ] Todos are ordered by creation date (newest first)
- [ ] API returns 401 Unauthorized if not authenticated

### Story 3: User updates a todo
**As a** authenticated user
**I want** to update a todo item
**So that** I can modify task details or mark it complete

**Acceptance Criteria:**
- [ ] User can PUT to `/api/todos/{id}` with updated fields
- [ ] User can update title, description, and completion status
- [ ] User can only update their own todos
- [ ] API returns 200 OK with updated todo
- [ ] API returns 404 Not Found if todo doesn't exist
- [ ] API returns 403 Forbidden if user doesn't own the todo

### Story 4: User deletes a todo
**As a** authenticated user
**I want** to delete a todo item
**So that** I can remove completed or cancelled tasks

**Acceptance Criteria:**
- [ ] User can DELETE `/api/todos/{id}` to remove a todo
- [ ] User can only delete their own todos
- [ ] API returns 204 No Content on success
- [ ] API returns 404 Not Found if todo doesn't exist
- [ ] API returns 403 Forbidden if user doesn't own the todo

## Functional Requirements

### Must Have (P0)
1. **Todo Model** with properties:
   - Id (Guid, primary key)
   - Title (string, required, max 200 characters)
   - Description (string, optional, max 1000 characters)
   - IsCompleted (bool, default false)
   - CreatedAt (DateTime, auto-set)
   - UpdatedAt (DateTime, auto-updated)
   - UserId (string, foreign key to IdentityUser)

2. **REST API Endpoints**:
   - `POST /api/todos` - Create todo
   - `GET /api/todos` - List user's todos
   - `GET /api/todos/{id}` - Get single todo
   - `PUT /api/todos/{id}` - Update todo
   - `DELETE /api/todos/{id}` - Delete todo

3. **Authorization**: All endpoints require authentication and enforce user ownership

4. **Validation**:
   - Title is required and 1-200 characters
   - Description is optional and max 1000 characters

### Should Have (P1)
1. **Swagger Documentation** for all todo endpoints with examples
2. **DTOs** for request/response to decouple from domain model
3. **Filtering**: Query parameter to filter by completion status

### Could Have (P2)
1. Pagination for large todo lists
2. Search by title or description
3. Due dates for todos
4. Priority levels

## Non-Functional Requirements

### Performance
- List todos response time < 200ms for typical user (< 100 todos)
- Create/update/delete operations < 100ms

### Security
- All endpoints require JWT authentication
- Users can only access their own todos (enforce UserId filtering)
- Validate all inputs to prevent injection attacks
- Use parameterized queries (Entity Framework Core handles this)

### Usability
- Clear error messages for validation failures
- RESTful URL structure
- Consistent response format with existing auth endpoints
- Swagger documentation with examples

### Maintainability
- Follow existing project structure and conventions
- Use repository pattern if other entities use it
- Comprehensive unit tests for business logic
- Integration tests for API endpoints

## Technical Considerations

### Database Schema
```sql
CREATE TABLE Todos (
    Id TEXT PRIMARY KEY,
    Title TEXT NOT NULL,
    Description TEXT,
    IsCompleted INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    UserId TEXT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Todos_UserId ON Todos(UserId);
CREATE INDEX IX_Todos_CreatedAt ON Todos(CreatedAt);
```

### Technology Stack
- **.NET 9** - Framework version
- **Entity Framework Core 9** - ORM for database access
- **SQLite** - Database (existing)
- **ASP.NET Core Identity** - User management (existing)
- **Swagger/OpenAPI** - API documentation (existing)

### Design Patterns
- **Controller-Service-Repository**: Follow if established in codebase
- **DTOs**: Use for API contracts (CreateTodoDto, UpdateTodoDto, TodoResponseDto)
- **AutoMapper**: Consider if complex mapping is needed (optional)

## Edge Cases & Error Handling

1. **Empty/Whitespace Title**: Return 400 Bad Request with clear message
2. **Title Too Long**: Return 400 Bad Request with character limit
3. **Invalid Todo ID**: Return 404 Not Found
4. **Accessing Another User's Todo**: Return 403 Forbidden
5. **Unauthenticated Request**: Return 401 Unauthorized
6. **Database Constraint Violation**: Return 500 with generic error (don't expose details)
7. **Concurrent Updates**: Last write wins (simple approach for MVP)

## Dependencies

### External
- None (all packages already in project)

### Internal
- **ApplicationDbContext**: Add Todos DbSet
- **Authentication**: Use existing JWT middleware
- **User Management**: Link to IdentityUser via UserId

## Out of Scope

The following are explicitly out of scope for this feature:

- Shared todos or collaboration features
- Todo categories or tags
- Recurring todos
- Attachments or file uploads
- Real-time notifications
- Mobile app UI (API only)
- Todo templates
- Bulk operations
- Export/import functionality

## Success Metrics

1. **Functionality**: All CRUD operations work as specified
2. **Security**: Users cannot access other users' todos
3. **Testing**:
   - Unit tests with >80% code coverage for services
   - Integration tests for all API endpoints
   - All tests pass in CI/CD
4. **Documentation**: Swagger UI shows all endpoints with examples
5. **Performance**: API responses meet SLA targets
6. **Code Quality**: Passes code review without major issues

## API Examples

### Create Todo
```http
POST /api/todos HTTP/1.1
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "title": "Buy groceries",
  "description": "Milk, eggs, bread"
}

Response: 201 Created
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Buy groceries",
  "description": "Milk, eggs, bread",
  "isCompleted": false,
  "createdAt": "2026-02-02T17:00:00Z",
  "updatedAt": "2026-02-02T17:00:00Z"
}
```

### Get Todos
```http
GET /api/todos HTTP/1.1
Authorization: Bearer <jwt-token>

Response: 200 OK
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Buy groceries",
    "description": "Milk, eggs, bread",
    "isCompleted": false,
    "createdAt": "2026-02-02T17:00:00Z",
    "updatedAt": "2026-02-02T17:00:00Z"
  }
]
```

### Update Todo
```http
PUT /api/todos/3fa85f64-5717-4562-b3fc-2c963f66afa6 HTTP/1.1
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "title": "Buy groceries",
  "description": "Milk, eggs, bread, butter",
  "isCompleted": true
}

Response: 200 OK
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Buy groceries",
  "description": "Milk, eggs, bread, butter",
  "isCompleted": true,
  "createdAt": "2026-02-02T17:00:00Z",
  "updatedAt": "2026-02-02T17:05:00Z"
}
```

### Delete Todo
```http
DELETE /api/todos/3fa85f64-5717-4562-b3fc-2c963f66afa6 HTTP/1.1
Authorization: Bearer <jwt-token>

Response: 204 No Content
```

## Open Questions

1. **Filtering**: Do we need to filter by completion status in MVP? (Suggested: Yes, via query param `?completed=true/false`)
2. **Ordering**: Default order by createdAt desc, or allow customization? (Suggested: Simple default for MVP)
3. **Soft Delete**: Should deletes be soft (mark as deleted) or hard (remove from DB)? (Suggested: Hard delete for MVP)
4. **Pagination**: How many todos before pagination is needed? (Suggested: Defer to P1 unless user base is large)

## Next Steps

After PRD approval:
1. **Architect** designs technical implementation (data layer, service layer, API layer)
2. **Engineer** implements according to plan
3. **QA** tests functionality, security, and edge cases
4. **DevOps** deploys to staging/production

---

**Status**: Ready for human approval
**Reviewer**: @human
