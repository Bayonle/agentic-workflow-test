# Autonomous Agent Orchestrator (File System)

**Trigger**: `/work`
**Purpose**: Main entry point for the file-system based autonomous workflow

---

## Overview

The `/work` command creates tasks in the file system and coordinates the autonomous agent team. No external APIs, just markdown files and directories.

---

## Usage

```bash
# Start new feature
/work "Build user authentication with 2FA"

# Check status
/work status task-001

# List active work
/work list

# Daily summary
/work standup
```

---

## What This Does

### `/work "feature description"`

1. Creates task in `workspace/tasks/inbox/`
2. Logs activity to `workspace/activity.log`
3. Returns task ID for tracking

**Example:**
```python
import sys
sys.path.append('agents/src')

from task_manager import create_task
from activity import log_activity

# Create task
task = create_task(
    title=user_input,
    description=f"Feature request from user: {user_input}",
    priority="P2",
    tags=["feature-request"]
)

# Log it
log_activity('system', f"Created {task.id}: {task.title}")

# Return info
print(f"""
🚀 Feature Created: {task.id}

**Title:** {task.title}
**Status:** inbox (PM will pick up)
**Location:** workspace/tasks/inbox/{task.id}.md

**Next Steps:**
1. PM agent will research and write PRD
2. You'll be asked to approve PRD
3. Architect will create technical plan
4. You'll be asked to approve plan
5. Engineers implement
6. QA tests
7. You'll be asked to approve deployment

**Track progress:**
- View task: cat workspace/tasks/*/{task.id}.md
- Check activity: tail -f workspace/activity.log
- Daily summary: /work standup
""")
```

---

### `/work status [task-id]`

Shows current status of a task:

```python
from task_manager import get_task_manager

tm = get_task_manager('workspace')
task = tm.find_task(task_id)

if not task:
    print(f"❌ Task {task_id} not found")
else:
    # Calculate progress
    all_tasks = tm.list_tasks()
    related = [t for t in all_tasks if task_id in t.id or t.id in task.thread]
    completed = [t for t in related if t.status == 'deployed']
    progress = len(completed) / len(related) * 100 if related else 0

    print(f"""
📊 Task Status: {task.title}

**ID:** {task.id}
**Status:** {task.status}
**Priority:** {task.priority}
**Progress:** {progress:.0f}%

**Assigned:** {', '.join(task.assigned) if task.assigned else 'None'}
**Subscribers:** {', '.join(task.subscribers)}

**Links:**
- PRD: {task.prd or 'Not yet created'}
- Plan: {task.plan or 'Not yet created'}
- PR: {task.pr or 'Not yet created'}

**Recent Activity:**
""")

    # Show recent comments
    for comment in task.thread[-5:]:
        print(f"  {comment['timestamp']} - {comment['agent']}")
        print(f"    {comment['message'][:80]}...")

    print(f"""
**Next Steps:**
{get_next_steps(task.status)}

**View full task:** cat workspace/tasks/{task.status}/{task.id}.md
""")
```

---

### `/work list`

Shows all active tasks:

```python
from task_manager import get_task_manager

tm = get_task_manager('workspace')

# Get tasks by status
active_statuses = [
    'inbox', 'in-discovery', 'in-planning',
    'ready-to-build', 'in-progress', 'ready-for-testing',
    'in-qa', 'ready-to-deploy'
]

print("📋 Active Work\n")

for status in active_statuses:
    tasks = tm.list_tasks(status)
    if tasks:
        print(f"\n## {status.replace('-', ' ').title()} ({len(tasks)})")
        for task in tasks:
            assignee = ', '.join(task.assigned) if task.assigned else 'unassigned'
            print(f"  - {task.id}: {task.title} ({assignee})")

# Show blocked separately
blocked = tm.list_tasks('blocked')
if blocked:
    print(f"\n## 🚫 Blocked ({len(blocked)})")
    for task in blocked:
        print(f"  - {task.id}: {task.title}")
        if task.thread:
            last = task.thread[-1]
            print(f"    Blocker: {last['message'][:60]}...")
```

---

### `/work standup`

Generates and shows daily standup:

```python
from daily_standup import generate_standup

report = generate_standup('workspace')
print(report)

# Optionally save
write_file('workspace/standup.md', report)
```

---

## Approval Workflow

### When PRD Ready

The PM agent will add a comment:
```
@human PRD ready for review: docs/specs/feature.md
```

**You review:**
```bash
cat workspace/docs/specs/feature.md
```

**To approve:**
```python
from task_manager import get_task_manager

tm = get_task_manager('workspace')
tm.add_comment(task_id, 'human', 'PRD approved. Proceed with planning.')
tm.move_task(task_id, 'in-planning')
tm.assign_task(task_id, 'architect')
```

Or just tell me: `/work approve task-001`

### When Plan Ready

Similar flow - architect requests approval, you review and approve.

### When Ready to Deploy

DevOps requests approval, you review deployment plan and approve.

---

## Activity Monitoring

### Real-time Activity Feed

```bash
# Watch live
tail -f workspace/activity.log

# Today's activity
grep $(date +%Y-%m-%d) workspace/activity.log

# By agent
grep "engineer" workspace/activity.log
```

### Check Notifications

```python
from notifications import check_notifications

# Check your notifications
notifs = check_notifications('human')
for n in notifs:
    print(f"{n['from']} ({n['task_id']}): {n['message']}")
```

---

## Dashboard View

```python
from task_manager import get_task_manager
from activity import today_activity

tm = get_task_manager('workspace')

print("""
╔══════════════════════════════════════════════════════════╗
║         AUTONOMOUS AGENT TEAM DASHBOARD                  ║
╚══════════════════════════════════════════════════════════╝
""")

# Task counts by status
statuses = {
    'Inbox': len(tm.list_tasks('inbox')),
    'Discovery': len(tm.list_tasks('in-discovery')),
    'Planning': len(tm.list_tasks('in-planning')),
    'Ready': len(tm.list_tasks('ready-to-build')),
    'In Progress': len(tm.list_tasks('in-progress')),
    'Testing': len(tm.list_tasks('ready-for-testing')) + len(tm.list_tasks('in-qa')),
    'Deploy': len(tm.list_tasks('ready-to-deploy')),
    'Deployed': len(tm.list_tasks('deployed')),
    'Blocked': len(tm.list_tasks('blocked'))
}

print("\n📊 Task Status:")
for status, count in statuses.items():
    if count > 0:
        print(f"  {status}: {count}")

# Today's activity
activities = today_activity()
print(f"\n📝 Today's Activity ({len(activities)} events):")
for activity in activities[-10:]:
    print(f"  {activity.strip()}")

print("\n💡 Commands:")
print("  /work status <task-id>  - Check task status")
print("  /work list              - List all active work")
print("  /work standup           - Daily summary")
print("  tail -f workspace/activity.log - Watch live")
```

---

## File Locations

```
workspace/
├── tasks/
│   ├── inbox/              # New tasks (PM picks up)
│   ├── in-discovery/       # PM researching
│   ├── in-planning/        # Architect designing
│   ├── ready-to-build/     # Engineers pick up
│   ├── in-progress/        # Being implemented
│   ├── ready-for-testing/  # QA picks up
│   ├── in-qa/              # Testing
│   ├── ready-to-deploy/    # DevOps picks up
│   ├── deployed/           # Done ✅
│   └── blocked/            # Stuck
│
├── docs/
│   ├── specs/              # PRDs
│   ├── plans/              # Technical plans
│   └── solutions/          # Learnings
│
├── agents/                 # Agent memory
│
├── activity.log            # All activity
├── notifications.md        # Notifications
└── standup.md             # Daily summary
```

---

## Quick Reference

```bash
# Create task
/work "feature description"

# Check status
/work status task-001

# List all work
/work list

# Daily summary
/work standup

# Watch activity
tail -f workspace/activity.log

# Find task
find workspace/tasks -name "task-*.md"

# Search mentions
grep -r "@human" workspace/tasks/

# View task
cat workspace/tasks/in-progress/task-001.md
```

---

## Success Metrics

After a week, you should see:
- Tasks flowing through pipeline automatically
- Agents coordinating via comments
- Clear audit trail in git
- Daily standups showing progress
- You only approving at 3 gates

---

## Remember

**You control what gets built** - via task creation and approvals

**Agents work autonomously** - between approval gates

**Everything is transparent** - just cat the files

**Git tracks everything** - full history always available

**Simple beats complex** - files > APIs

---

**Welcome to autonomous development with the file system.** 🚀
