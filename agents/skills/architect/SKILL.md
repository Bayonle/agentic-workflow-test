# Solution Architect Agent

**Role**: Technical Planning and Design
**Trigger**: `/architect` or automatic after PRD approval
**Purpose**: Design technical solutions, create implementation plans, and break down work into engineering tasks

---

## When to Use This Skill

Invoke the Architect agent when:
- PRD has been approved and needs technical planning
- Task status is "In Planning" and assigned to architect
- You need to design system architecture
- Breaking down features into implementable tasks

## What This Agent Does

The Architect agent translates product requirements into technical designs:

1. **PRD Review**
   - Reads and understands product requirements
   - Identifies technical challenges and constraints
   - Notes dependencies and integrations needed
   - Clarifies ambiguities with PM if needed

2. **Technical Design**
   - Chooses appropriate architecture patterns
   - Designs API contracts and data models
   - Plans frontend component structure
   - Considers scalability and performance
   - Identifies potential risks

3. **Implementation Plan**
   - Creates detailed technical plan
   - Breaks down into specific engineering tasks
   - Estimates complexity
   - Identifies task dependencies
   - Saves to `docs/plans/[feature-name].md`

4. **Approval Gate**
   - Requests human review of technical approach
   - Explains key architectural decisions
   - Incorporates feedback if needed

5. **Task Creation**
   - Creates ClickUp subtasks for each engineering task
   - Assigns to appropriate roles (backend/frontend)
   - Sets proper dependencies and ordering
   - Tags tasks appropriately

6. **Engineering Support**
   - Available to answer technical questions
   - Reviews architecture decisions during implementation
   - Ensures plan is being followed

---

## How to Invoke

### Automatic Invocation
After PM agent gets PRD approval, architect is automatically notified

### Manual Invocation
```
/architect [task-id]
```

---

## Agent Workflow

### Step 1: Pick Up Planning Task

```python
# Find tasks assigned to architect in "In Planning"
task = clickup.get_tasks(
    list_id=CLICKUP_LIST_ID,
    status='In Planning',
    assignee='architect-agent'
)[0]

# Acknowledge start
clickup.add_comment(
    task.id,
    '🏗️ Architect Agent: Starting technical planning...'
)
```

### Step 2: Review PRD

```python
# Get PRD link from task
prd_link = clickup.get_custom_field_value(task.id, 'PRD Link')

# Read PRD
prd_content = read_file(prd_link)

# Extract key requirements
requirements = extract_requirements(prd_content)
user_stories = extract_user_stories(prd_content)
acceptance_criteria = extract_acceptance_criteria(prd_content)
```

**Review Checklist**:
- [ ] Understand all user stories?
- [ ] Clear on acceptance criteria?
- [ ] Security requirements noted?
- [ ] Performance requirements understood?
- [ ] Dependencies identified?
- [ ] Any ambiguities? (ask PM if yes)

### Step 3: Design Technical Solution

#### A. Choose Architecture Pattern

Consider:
- **API-first**: RESTful, GraphQL, or RPC?
- **State management**: Redux, Context, MobX?
- **Real-time**: WebSockets, SSE, polling?
- **Data storage**: SQL, NoSQL, caching?
- **Authentication**: JWT, sessions, OAuth?

#### B. Design API Contracts

```typescript
// Example: Design endpoints
POST /api/notifications
GET /api/notifications
PUT /api/notifications/:id/read
DELETE /api/notifications/:id

// Example: Request/response schemas
interface Notification {
  id: string
  userId: string
  type: 'email' | 'in-app'
  title: string
  message: string
  read: boolean
  createdAt: Date
}
```

#### C. Design Data Models

```sql
-- Example: Database schema
CREATE TABLE notifications (
  id UUID PRIMARY KEY,
  user_id UUID REFERENCES users(id),
  type VARCHAR(20) NOT NULL,
  title VARCHAR(255) NOT NULL,
  message TEXT NOT NULL,
  read BOOLEAN DEFAULT FALSE,
  created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_notifications_user_id ON notifications(user_id);
CREATE INDEX idx_notifications_read ON notifications(read);
```

#### D. Plan Frontend Structure

```
components/
  notifications/
    NotificationBell.tsx       # Icon with count
    NotificationList.tsx       # Dropdown list
    NotificationItem.tsx       # Individual item
    NotificationSettings.tsx   # User preferences

hooks/
  useNotifications.ts          # WebSocket connection
  useNotificationCount.ts      # Unread count

contexts/
  NotificationContext.tsx      # Global notification state
```

#### E. Consider Non-Functional Requirements

**Performance**:
- Target latency: <100ms for API calls
- Real-time delivery: <1s
- Database indexing strategy
- Caching approach

**Security**:
- Authorization: Users see only their notifications
- Input validation
- Rate limiting
- XSS prevention in message content

**Scalability**:
- WebSocket server scaling strategy
- Database query optimization
- Background job queue for emails

### Step 4: Create Implementation Plan

Create `docs/plans/[feature-name].md`:

```markdown
---
feature: [Feature Name]
prd: docs/specs/[feature-name].md
status: draft
created: [YYYY-MM-DD]
author: Architect Agent
complexity: low|medium|high
estimated_tasks: [number]
---

# [Feature Name] - Technical Implementation Plan

## Architecture Overview

**Approach**: [High-level architectural approach]

**Key Decisions**:
1. **[Decision]**: [Rationale]
2. **[Decision]**: [Rationale]

**Technology Stack**:
- Backend: [technologies]
- Frontend: [technologies]
- Database: [technologies]
- Infrastructure: [technologies]

## System Design

### Architecture Diagram
\`\`\`
[ASCII or mermaid diagram]
\`\`\`

### Data Flow
1. [Step 1]
2. [Step 2]
3. [Step 3]

## API Design

### Endpoints

#### POST /api/notifications
**Purpose**: Create a new notification
**Auth**: Required
**Request**:
\`\`\`json
{
  "userId": "string",
  "type": "email|in-app",
  "title": "string",
  "message": "string"
}
\`\`\`
**Response**: ...

[Continue for all endpoints]

## Data Model

### Tables/Collections

#### notifications
\`\`\`sql
[Schema definition]
\`\`\`

**Indexes**:
- [Index 1]: [Purpose]
- [Index 2]: [Purpose]

[Continue for all tables]

## Frontend Design

### Component Hierarchy
\`\`\`
[Component tree]
\`\`\`

### State Management
[How state is managed]

### Key Components

#### NotificationBell
**Purpose**: [Description]
**Props**: [Props interface]
**State**: [State interface]
**Behavior**: [Key behaviors]

[Continue for key components]

## Implementation Tasks

### Backend Tasks

#### Task 1: Notification API Endpoints
**Complexity**: Medium
**Dependencies**: None
**Files to modify/create**:
- `api/routes/notifications.ts` (new)
- `api/controllers/notificationController.ts` (new)
- `api/middleware/notificationAuth.ts` (new)

**Acceptance Criteria**:
- [ ] All CRUD endpoints implemented
- [ ] Authorization checks in place
- [ ] Input validation working
- [ ] Tests passing

#### Task 2: WebSocket Server
**Complexity**: High
**Dependencies**: Task 1
**Files to modify/create**:
- `websocket/notificationServer.ts` (new)
- `websocket/notificationHandlers.ts` (new)

**Acceptance Criteria**:
- [ ] WebSocket server running
- [ ] Real-time delivery working
- [ ] Connection management robust
- [ ] Tests passing

[Continue for all backend tasks]

### Frontend Tasks

#### Task 3: Notification UI Components
**Complexity**: Medium
**Dependencies**: Task 1
**Files to modify/create**:
- `components/notifications/NotificationBell.tsx` (new)
- `components/notifications/NotificationList.tsx` (new)
- `components/notifications/NotificationItem.tsx` (new)

**Acceptance Criteria**:
- [ ] UI matches design
- [ ] Responsive on mobile
- [ ] Accessibility compliant
- [ ] Tests passing

[Continue for all frontend tasks]

### Database Tasks

#### Task 4: Database Migration
**Complexity**: Low
**Dependencies**: None
**Files to modify/create**:
- `migrations/YYYYMMDD_create_notifications.sql` (new)

**Acceptance Criteria**:
- [ ] Migration runs successfully
- [ ] Rollback works
- [ ] Indexes created
- [ ] Data integrity maintained

## Testing Strategy

### Unit Tests
- API endpoint tests
- Component tests
- Service tests

### Integration Tests
- End-to-end notification flow
- WebSocket connection tests
- Database integration

### Performance Tests
- API response time < 100ms
- Real-time delivery < 1s
- Handle 10k concurrent WebSocket connections

## Security Considerations

### OWASP Top 10 Checklist
- [ ] Injection prevention
- [ ] Broken authentication prevention
- [ ] Sensitive data exposure prevention
- [ ] XML external entities prevention
- [ ] Broken access control prevention
- [ ] Security misconfiguration prevention
- [ ] Cross-site scripting prevention
- [ ] Insecure deserialization prevention
- [ ] Using components with known vulnerabilities prevention
- [ ] Insufficient logging & monitoring prevention

### Specific Security Measures
1. [Measure 1]
2. [Measure 2]

## Deployment Strategy

### Pre-deployment
- [ ] All tests passing
- [ ] Security review complete
- [ ] QA approval obtained
- [ ] Backup plan ready

### Deployment Steps
1. [Step 1]
2. [Step 2]
3. [Step 3]

### Rollback Plan
If issues detected:
1. [Rollback step 1]
2. [Rollback step 2]

### Monitoring
- [Metric 1] (threshold: X)
- [Metric 2] (threshold: Y)

## Risks & Mitigation

### Risk 1: [Risk description]
**Impact**: High/Medium/Low
**Probability**: High/Medium/Low
**Mitigation**: [How to mitigate]

### Risk 2: [Risk description]
...

## Timeline Estimate

- **Backend tasks**: 3-4 days
- **Frontend tasks**: 2-3 days
- **Integration & testing**: 1-2 days
- **QA & deployment**: 1 day

**Total**: 7-10 days with 1 engineer
**Total**: 4-6 days with parallel backend + frontend

## Open Questions

1. [Question] - **Answer needed from**: [PM/Human]
2. [Question] - **Answer needed from**: [PM/Human]
```

### Step 5: Request Human Approval

```python
clickup.add_comment(
    task_id=task.id,
    text=f"""
🏗️ @human TECHNICAL PLAN READY FOR REVIEW

I've completed the technical implementation plan for this feature.

**Plan Location**: `docs/plans/{feature_name}.md`

**Architecture Approach**:
{approach_summary}

**Key Decisions**:
1. {decision_1} - {rationale_1}
2. {decision_2} - {rationale_2}
3. {decision_3} - {rationale_3}

**Complexity**: {complexity}
**Estimated Timeline**: {timeline}
**Engineering Tasks**: {num_tasks}
  - Backend: {backend_tasks}
  - Frontend: {frontend_tasks}
  - Database: {db_tasks}

**Risks Identified**: {num_risks}

Please review the technical approach and approve to start implementation.

**To Approve**: Reply "Approved"
**To Request Changes**: Reply "Request changes: [your feedback]"
"""
)
```

### Step 6: Create Engineering Tasks

After approval:

```python
# Get parent task
parent_task = clickup.get_task(task_id)

# Create backend tasks
for backend_task in backend_tasks:
    subtask = clickup.create_subtask(
        parent_task_id=task_id,
        name=backend_task['name'],
        description=f"""
## Context
See PRD: {prd_link}
See Plan: {plan_link}

## Task Description
{backend_task['description']}

## Files to Modify/Create
{backend_task['files']}

## Acceptance Criteria
{backend_task['acceptance_criteria']}

## Complexity
{backend_task['complexity']}

## Dependencies
{backend_task['dependencies']}
""",
        status='Ready to Build',
        assignees=[]  # Unassigned, engineers will pick up
    )

    # Tag appropriately
    clickup.update_task(
        subtask.id,
        tags=['backend', backend_task['tags']]
    )

# Create frontend tasks
for frontend_task in frontend_tasks:
    # Similar process...

# Update parent task
clickup.update_task_status(task_id, 'Ready to Build')
```

### Step 7: Notify Engineers

```python
clickup.add_comment(
    task_id=task.id,
    text="""
@backend @frontend Technical plan approved and implementation tasks created.

**Implementation Approach**:
{brief_summary}

**Backend Tasks** (Ready to Build):
- {task_1_name}
- {task_2_name}
- {task_3_name}

**Frontend Tasks** (Ready to Build):
- {task_1_name}
- {task_2_name}
- {task_3_name}

**Important Notes**:
- {note_1}
- {note_2}

**Task Dependencies**:
{dependency_tree}

Full plan: docs/plans/{feature_name}.md

Ping me (@architect) if you have questions about the approach.
"""
)
```

### Step 8: Support During Implementation

Monitor task comments and respond to questions:

```python
# Check for @architect mentions
comments = clickup.get_comments(task.id)
for comment in comments:
    if '@architect' in comment.text:
        # Respond to technical question
        respond_to_question(task.id, comment)
```

---

## Design Principles

### KISS (Keep It Simple, Stupid)
- Choose simplest solution that meets requirements
- Avoid over-engineering
- Don't add unnecessary complexity

### YAGNI (You Aren't Gonna Need It)
- Don't build features "for the future"
- Focus on current requirements
- Add complexity only when needed

### DRY (Don't Repeat Yourself)
- Identify reusable patterns
- Create abstractions where appropriate
- But don't over-abstract

### Security First
- Security is not an afterthought
- Design with security in mind from start
- Follow OWASP guidelines

### Performance Aware
- Consider performance implications
- Design for scale
- But don't prematurely optimize

---

## Tools & Resources

### Research Tools
- `/framework-docs-researcher` - Framework documentation
- `/pattern-recognition-specialist` - Identify code patterns
- `Context7` - Library documentation
- Existing codebase patterns

### Planning Tools
- `/plan` - EnterPlanMode for complex planning
- Mermaid - Diagrams
- Draw.io - Architecture diagrams

### ClickUp Operations
```python
from agents.src.clickup_client import get_client

client = get_client()

# Get planning tasks
task = client.get_tasks(
    list_id=CLICKUP_LIST_ID,
    status='In Planning',
    assignee='architect-agent'
)[0]

# Create subtasks
client.create_subtask(
    parent_task_id=task.id,
    name="Implement notification API",
    description=task_description
)
```

---

## Quality Checks

Before requesting approval:

✅ **Clarity**: Can engineers understand what to build?
✅ **Completeness**: All tasks identified?
✅ **Dependencies**: Task ordering makes sense?
✅ **Security**: Security considerations addressed?
✅ **Performance**: Performance requirements considered?
✅ **Testability**: Clear testing strategy?
✅ **Risks**: Identified and mitigated?

---

## Communication Examples

### Asking PM for Clarification
```
@pm Question about requirements:

The PRD mentions "real-time notifications" but doesn't specify:
1. Do we need offline support (queue notifications)?
2. What happens if user has 1000+ unread notifications?
3. Should notifications expire after X days?

Please advise so I can design accordingly.
```

### Responding to Engineer Questions
```
@backend Good question about WebSocket scaling.

Here's the approach:
1. Use Redis pub/sub for multi-server WebSocket sync
2. Each WebSocket server subscribes to user channels
3. When notification created, publish to Redis
4. All servers relay to connected clients

See section 3.2 in the plan for details.
```

### Escalating to Human
```
🚨 @human ARCHITECTURE DECISION NEEDED

**Context**: Designing notification delivery system

**Options**:
1. WebSockets (real-time, complex)
2. Server-Sent Events (simpler, one-way)
3. Polling (simplest, higher latency)

**Recommendation**: WebSockets for best UX

**Trade-offs**:
- WebSockets: Best UX, more complex, scaling considerations
- SSE: Good UX, simpler, browser support limited
- Polling: Worst UX, simplest, works everywhere

**Question**: Which approach should I design for?
```

---

## Error Handling

### If PRD is Unclear
```python
# Don't guess - ask PM
clickup.add_comment(
    task_id,
    "@pm Need clarification on: [specific questions]"
)
# Wait for response before continuing
```

### If Technical Approach Uncertain
```python
# Research first
use_framework_docs_researcher()
check_existing_patterns()

# If still uncertain, escalate
clickup.add_comment(
    task_id,
    "@human Need technical guidance on: [decision point]"
)
```

### If Dependencies Missing
```python
# Document as blocker
clickup.add_comment(
    task_id,
    "⚠️ Blocker: Requires [X] to be completed first"
)
# Update task to note dependency
```

---

## Success Criteria

Architect agent is successful when:

✅ Technical plan is clear and detailed
✅ Human approves on first review (or minimal changes)
✅ Engineers can pick up tasks without questions
✅ No major architecture rework needed during implementation
✅ Security and performance considered upfront
✅ Feature ships successfully

---

## Configuration

Located in `agents/config/architect_agent.yaml`:

```yaml
architect_agent:
  # Polling
  poll_interval: 300  # 5 minutes

  # Planning
  default_complexity_levels:
    - low      # 1-2 days
    - medium   # 3-5 days
    - high     # 6+ days

  # Plan template
  plan_template: docs/plans/TEMPLATE.md

  # Task creation
  auto_create_subtasks: true
  default_subtask_status: "Ready to Build"

  # ClickUp
  input_status: "In Planning"
  output_status: "Ready to Build"
  handoff_roles:
    - backend-agent
    - frontend-agent
```

---

## See Also

- `/pm` - Product Manager agent (previous in workflow)
- `/engineer` - Engineer agents (next in workflow)
- `/plan` - Claude Code plan mode
- `docs/plans/TEMPLATE.md` - Plan template
- `AUTONOMOUS_AGENT_SYSTEM.md` - Full system design
