# Autonomous Agent Orchestrator

**Trigger**: `/work`
**Purpose**: Main entry point to kick off the autonomous agent team on a feature

---

## Overview

The `/work` command starts the entire autonomous development workflow. It creates a feature request in ClickUp and triggers the PM agent to begin discovery, which then cascades through the entire team until the feature is deployed to production.

## How It Works

```
You: /work "Build user authentication with 2FA"
         ↓
   PM Agent starts discovery
         ↓
   PM writes PRD → You approve
         ↓
   Architect creates plan → You approve
         ↓
   Engineers implement (autonomous)
         ↓
   Security reviews (autonomous)
         ↓
   QA tests (autonomous)
         ↓
   DevOps deploys → You approve
         ↓
   Feature is LIVE ✅
```

**You only approve 3 times**: PRD, Plan, and Production Deploy

---

## Usage

### Basic Usage

```
/work "Build user notifications - email and in-app, real-time updates"
```

### With Priority

```
/work "Add export to CSV feature" --priority P1
```

### With Category

```
/work "Implement JWT authentication" --category security --priority P0
```

---

## What This Skill Does

### Step 1: Create ClickUp Task

```python
from agents.src.clickup_client import get_client
import os

client = get_client()

# Parse user request
feature_request = parse_request(user_input)

# Create task in ClickUp
task = client.create_task(
    list_id=os.getenv('CLICKUP_LIST_ID'),
    name=feature_request.title,
    description=f"""
## Feature Request
{feature_request.description}

## Requested By
User via /work command

## Status
Waiting for PM Agent to begin discovery...
""",
    status='Backlog',
    priority=feature_request.priority or 3,  # Default: normal
    tags=['feature-request', feature_request.category]
)

print(f"✅ Feature created: {task.url}")
print(f"📋 Task ID: {task.id}")
```

### Step 2: Notify PM Agent

```python
# Add comment to trigger PM agent
client.add_comment(
    task.id,
    """
@pm-agent New feature request ready for discovery.

Please:
1. Research requirements
2. Write PRD
3. Request approval

Feature details in task description above.
"""
)

print("✅ PM Agent notified - discovery will begin soon")
```

### Step 3: Show Dashboard

```python
# Show user what's happening
print(f"""
🚀 Autonomous Team Activated

**Feature**: {task.name}
**ClickUp Task**: {task.url}
**Status**: Backlog → PM Agent will begin discovery

**What happens next**:
1. PM Agent researches and writes PRD (30-60 min)
2. You'll be notified to approve PRD
3. After approval, Architect creates plan (30-60 min)
4. You'll be notified to approve plan
5. After approval, Engineers implement (2-5 days)
6. QA tests automatically
7. You'll be notified to approve deployment
8. Feature goes live!

**Your approvals needed**: 3 (PRD, Plan, Deploy)
**Estimated timeline**: 3-7 days

I'll keep you updated on progress. You can check status anytime:
/work status {task.id}
""")
```

### Step 4: Monitor and Report

The orchestrator monitors progress and provides updates:

```python
# Background monitoring
while task.status != 'Deployed':
    current_task = client.get_task(task.id)

    # Check for approval requests
    comments = client.get_comments(task.id)
    approval_needed = check_for_approval_requests(comments)

    if approval_needed:
        notify_user(approval_needed)

    # Check for escalations
    escalations = check_for_escalations(comments)
    if escalations:
        notify_user_urgent(escalations)

    # Periodic status updates
    if hours_since_last_update() >= 4:
        send_status_update(task)

    time.sleep(300)  # Check every 5 minutes
```

---

## Status Commands

### Check Status

```
/work status [task-id]
```

Shows current status:

```
📊 Feature Status: User Notifications

**Status**: In Progress (Day 3 of ~5)
**Progress**: 60% (6/10 tasks complete)

**Completed**:
✓ PRD written and approved
✓ Technical plan created and approved
✓ Notification API implemented
✓ WebSocket server implemented
✓ Frontend UI implemented
✓ Backend tests passing

**In Progress**:
→ Frontend integration testing (@frontend)
→ End-to-end testing (@qa)

**Pending**:
- Security review
- Final deployment

**Blockers**: None

**Next Approval Needed**: None (waiting for QA to finish)

**ClickUp**: https://app.clickup.com/...
```

### List Active Features

```
/work list
```

Shows all active features:

```
📋 Active Features (3)

1. User Notifications
   Status: In Progress (60%)
   Team: Backend, Frontend, QA
   Next: QA testing

2. CSV Export
   Status: In Planning (20%)
   Team: Architect
   Next: Waiting for plan approval

3. Dark Mode
   Status: In Discovery (10%)
   Team: PM
   Next: Waiting for PRD approval
```

---

## Approval Workflow

When agents need approval, you'll be notified:

### PRD Approval

```
📋 APPROVAL NEEDED: User Notifications PRD

The PM Agent has completed the Product Requirements Document.

**Key Highlights**:
- 3 user stories
- Real-time and email notifications
- User preferences
- Notification history

**PRD**: docs/specs/notifications.md

**To Approve**:
/work approve {task-id}

**To Request Changes**:
/work reject {task-id} "Please add support for SMS notifications"

**View Full PRD**:
cat docs/specs/notifications.md
```

### Plan Approval

```
🏗️ APPROVAL NEEDED: User Notifications Technical Plan

The Architect has completed the implementation plan.

**Approach**: WebSockets + Email Queue
**Complexity**: Medium
**Timeline**: 3-5 days
**Tasks**: 10 (4 backend, 3 frontend, 2 QA, 1 deploy)

**Plan**: docs/plans/notifications.md

**To Approve**:
/work approve {task-id}

**View Full Plan**:
cat docs/plans/notifications.md
```

### Deploy Approval

```
🚀 APPROVAL NEEDED: Production Deployment

DevOps is ready to deploy User Notifications to production.

**Pre-Deploy Status**:
✓ All tests passing
✓ Security approved
✓ QA approved
✓ Rollback plan ready

**Risk**: Low
**Monitoring**: 1 hour active monitoring

**To Approve**:
/work approve {task-id}

**To Hold**:
/work reject {task-id} "Hold until Monday"
```

---

## Agent Coordination

The orchestrator ensures agents work together smoothly:

### Handling Escalations

```python
# Agent escalation format
if '@human' in comment and '🚨' in comment:
    # Parse escalation
    escalation = parse_escalation(comment)

    # Notify user immediately
    print(f"""
🚨 AGENT NEEDS HELP: {escalation.agent}

**Feature**: {task.name}
**Agent**: {escalation.agent}
**Issue**: {escalation.issue}

**What they tried**:
{escalation.attempts}

**What they need from you**:
{escalation.request}

**Task**: {task.url}

Please respond in ClickUp or use:
/work respond {task.id} "[your guidance]"
"""
)
```

### Coordinating Dependencies

```python
# Ensure correct ordering
if backend_not_complete and frontend_waiting:
    notify_frontend_agent(
        "Backend tasks not ready yet. "
        "You can start on UI mockups in parallel."
    )

# Prevent blockers
if qa_ready_but_no_engineer:
    alert_engineer_agent(
        "QA is blocked waiting for your completion. "
        "Please prioritize finishing."
    )
```

---

## Dashboard

Real-time view of autonomous team:

```
🤖 Autonomous Agent Team Dashboard

┌─────────────────────────────────────────────┐
│ Active Features: 3                           │
│ Pending Approvals: 1                         │
│ Blockers: 0                                  │
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│ Agent Status                                 │
├─────────────────────────────────────────────┤
│ 👨‍💼 PM Agent: Working on "Dark Mode" PRD     │
│ 🏗️  Architect: Idle                          │
│ 👨‍💻 Backend: Working on "Notifications API"  │
│ 👨‍💻 Frontend: Working on "Notification UI"   │
│ 🧪 QA: Testing "CSV Export"                  │
│ 🔒 Security: Reviewing PR #234               │
│ 🚀 DevOps: Idle                              │
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│ Pending Approvals (1)                        │
├─────────────────────────────────────────────┤
│ CSV Export - Plan Approval                   │
│ /work approve {task-id}                      │
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│ Recent Activity                              │
├─────────────────────────────────────────────┤
│ 2 min ago: QA approved "CSV Export"          │
│ 15 min ago: Backend completed "API endpoint" │
│ 1 hour ago: You approved "Notifications PRD" │
└─────────────────────────────────────────────┘
```

---

## Configuration

`agents/config/orchestrator.yaml`:

```yaml
orchestrator:
  # ClickUp
  default_list_id: ${CLICKUP_LIST_ID}
  default_priority: 3  # Normal

  # Monitoring
  status_update_interval: 14400  # 4 hours
  check_interval: 300  # 5 minutes

  # Notifications
  notify_on_approval: true
  notify_on_escalation: true
  notify_on_completion: true

  # Agent coordination
  auto_assign_pm: true
  parallel_work_enabled: true
```

---

## Example: Full Workflow

### You Start Feature

```bash
/work "Build user authentication with 2FA"
```

### Orchestrator Response

```
🚀 Autonomous Team Activated

✅ Feature created in ClickUp
✅ PM Agent notified

**Feature**: Build user authentication with 2FA
**ClickUp**: https://app.clickup.com/t/abc123
**Status**: Backlog → PM will begin discovery

**Next Steps**:
1. PM researches and writes PRD (est. 1 hour)
2. You approve PRD
3. Architect creates plan (est. 1 hour)
4. You approve plan
5. Engineers implement (est. 3-5 days)
6. QA tests (est. 1 day)
7. You approve deployment
8. Feature goes live

I'll notify you when approvals are needed.

Track progress: /work status abc123
```

### 1 Hour Later: PRD Ready

```
📋 @human APPROVAL NEEDED

PM Agent completed PRD for: User Authentication

docs/specs/authentication.md

/work approve abc123
```

### You Approve

```bash
/work approve abc123
```

```
✅ PRD Approved

Architect Agent notified to begin technical planning.
```

### 1 Hour Later: Plan Ready

```
🏗️ @human APPROVAL NEEDED

Architect completed plan for: User Authentication

docs/plans/authentication.md

/work approve abc123
```

### You Approve

```bash
/work approve abc123
```

```
✅ Plan Approved

Engineering tasks created and ready to build.
Backend and Frontend agents will begin implementation.
```

### 3 Days Later: Daily Update

```
📊 Status Update: User Authentication

**Progress**: 80% (8/10 tasks complete)

**Completed Today**:
✓ JWT authentication implemented
✓ 2FA enrollment flow completed
✓ Backend tests passing

**In Progress**:
→ Frontend authentication UI (@frontend)
→ Integration testing (@qa)

**Next**: QA approval, then deployment

On track for deployment tomorrow!
```

### 4 Days Later: Ready to Deploy

```
🚀 @human APPROVAL NEEDED

DevOps ready to deploy: User Authentication

All quality gates passed.
Risk: Low
Monitoring: 1 hour

/work approve abc123
```

### You Approve

```bash
/work approve abc123
```

```
✅ Deploy Approved

DevOps executing deployment...
```

### 30 Minutes Later: Deployed

```
🎉 FEATURE DEPLOYED: User Authentication

Status: Live in production
Monitoring: All metrics nominal

Users can now authenticate with 2FA!

ClickUp task marked as Deployed.
```

---

## Success Criteria

Orchestrator is successful when:

✅ Features move smoothly through pipeline
✅ User only intervenes at approval gates
✅ Agents coordinate without conflicts
✅ Blockers are surfaced quickly
✅ Status is always clear

---

## See Also

- `/pm` - Product Manager Agent
- `/architect` - Solution Architect Agent
- `/engineer` - Engineer Agent
- `/qa` - QA Agent
- `/security-review` - Security Agent
- `/deploy` - DevOps Agent
- `AUTONOMOUS_AGENT_SYSTEM.md` - Complete system design
