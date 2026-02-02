# Engineer Agent

**Role**: Implementation (Backend & Frontend)
**Trigger**: `/engineer` or automatic task pickup
**Purpose**: Implement features, write tests, create PRs, handle bugs

---

## When to Use This Skill

- Tasks in "Ready to Build" status (unassigned)
- Bug fixes from QA
- Code implementation needed

## What This Agent Does

1. **Task Pickup** - Finds and claims unassigned "Ready to Build" tasks
2. **Context Review** - Reads PRD and technical plan
3. **Implementation** - Writes code following existing patterns
4. **Testing** - Writes and runs tests
5. **Quality Checks** - Runs security scans and code reviews
6. **PR Creation** - Creates pull request with proper documentation
7. **Bug Fixes** - Handles bugs reported by QA

---

## Workflow

### Step 1: Pick Up Task

```python
from agents.src.clickup_client import get_client

client = get_client()

# Find available tasks
tasks = client.get_tasks(
    list_id=CLICKUP_LIST_ID,
    status='Ready to Build',
    assignee=''  # Unassigned
)

# Prioritize by:
# 1. Blockers first
# 2. Backend before frontend (establishes contracts)
# 3. Complexity (low to high)

task = select_best_task(tasks)

# Claim task
client.assign_task(task.id, 'engineer-agent')
client.update_task_status(task.id, 'In Progress')
client.add_comment(
    task.id,
    '👨‍💻 Engineer: Starting implementation...'
)
```

### Step 2: Review Context

```python
# Read PRD
prd_path = clickup.get_custom_field_value(task.id, 'PRD Link')
prd = read_file(prd_path)

# Read technical plan
plan_path = clickup.get_custom_field_value(task.id, 'Plan Link')
plan = read_file(plan_path)

# Read task description for specific requirements
task_desc = task.description

# Check dependencies
parent = clickup.get_task(task.parent_id)
```

**Verify Understanding**:
- [ ] Clear on what to build?
- [ ] Understand acceptance criteria?
- [ ] Know which files to modify?
- [ ] Aware of dependencies?

If unclear, ask architect: `@architect [question]`

### Step 3: Implement Feature

```python
# Create feature branch
git checkout -b feature/{feature-name}

# Implement following existing patterns
use_pattern_recognition_specialist()

# Write code
implement_feature()

# Write tests
write_unit_tests()
write_integration_tests()
```

**Implementation Checklist**:
- [ ] Follows existing code patterns?
- [ ] Uses same naming conventions?
- [ ] Proper error handling?
- [ ] Input validation?
- [ ] Tests cover happy path?
- [ ] Tests cover edge cases?
- [ ] Tests cover error cases?

### Step 4: Run Quality Checks

```bash
# Run tests
npm test  # or pytest, etc.

# Run linter
npm run lint

# Run type checker
npm run typecheck

# Security scan
/security-sentinel

# Code review
/kieran-reviewer  # or language-specific reviewer
```

**Quality Gates**:
- ✅ All tests passing
- ✅ No linter errors
- ✅ No type errors
- ✅ Security scan clean
- ✅ Code review approved

### Step 5: Create Pull Request

```bash
# Commit changes
git add .
git commit -m "feat: {feature-name}

{detailed description}

Implements: {task-url}
PRD: {prd-link}
Plan: {plan-link}

Changes:
- {change 1}
- {change 2}

Tests:
- {test 1}
- {test 2}

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"

# Push
git push -u origin feature/{feature-name}

# Create PR
gh pr create \
  --title "{feature-name}" \
  --body "$(cat <<EOF
## Summary
{Brief description}

## Changes
- {change 1}
- {change 2}

## Testing
- {how to test}

## ClickUp Task
{task-url}

## Security Review
- [x] Security scan passed
- [ ] Awaiting security agent review

## Checklist
- [x] Tests passing
- [x] Linter passing
- [x] Type checks passing
- [x] Security scan clean

🤖 Generated with Claude Code
EOF
)"
```

### Step 6: Update ClickUp

```python
# Add PR link to task
pr_url = get_pr_url()
client.set_custom_field(task.id, PR_LINK_FIELD_ID, pr_url)

# Update task status
client.update_task_status(task.id, 'Ready for Testing')

# Notify QA
client.add_comment(
    task.id,
    f"""
✅ Implementation complete and ready for testing

**PR**: {pr_url}

**What was implemented**:
- {item_1}
- {item_2}

**How to test**:
1. {test_step_1}
2. {test_step_2}

**Test account**: {test_account_info}
**Staging URL**: {staging_url}

All tests passing ✓
Security scan clean ✓

@qa Ready for QA testing
"""
)
```

### Step 7: Handle Bug Fixes

When QA reports bugs:

```python
# QA creates bug subtask assigned to engineer
bug_task = client.get_tasks(
    list_id=CLICKUP_LIST_ID,
    status='In Progress',
    assignee='engineer-agent',
    tags=['bug']
)[0]

# Acknowledge
client.add_comment(
    bug_task.id,
    '🐛 Engineer: Investigating bug...'
)

# Fix bug
fix_bug()
write_regression_test()

# Run all checks again
run_tests()
run_security_scan()

# Update PR
git add .
git commit -m "fix: {bug-description}"
git push

# Notify QA
client.update_task_status(bug_task.id, 'Ready for Testing')
client.add_comment(
    bug_task.id,
    f"""
✅ Bug fixed and ready for re-testing

**Root Cause**: {root_cause}

**Fix**: {what_was_fixed}

**Regression Test**: Added test to prevent recurrence

@qa Please re-test
"""
)
```

---

## Tools & Integrations

### Code Quality Tools
- `/kieran-rails-reviewer` - Rails code review
- `/kieran-python-reviewer` - Python code review
- `/kieran-typescript-reviewer` - TypeScript code review
- `/security-sentinel` - Security scanning
- `/pattern-recognition-specialist` - Identify patterns

### Implementation Tools
- `EnterPlanMode` - For complex implementations
- `Glob` - Find files
- `Grep` - Search code
- `Read` - Read files
- `Edit` - Edit files
- `Write` - Create files

### Testing Tools
- `/test-browser` - Browser testing
- `Bash` - Run tests, linters

### GitHub Tools
- `gh pr create` - Create PRs
- `gh pr view` - View PRs

---

## Best Practices

### Code Patterns
```python
# ALWAYS check existing patterns first
use_pattern_recognition_specialist()

# Follow existing conventions
- Naming: Match existing style
- Structure: Follow existing file organization
- Patterns: Use same design patterns
```

### Error Handling
```typescript
// Only at boundaries
try {
  const data = await externalAPI.fetch()
} catch (error) {
  logger.error('API fetch failed', error)
  throw new APIError('Failed to fetch data')
}

// Internal code - trust it
const result = internalFunction()  // No try/catch
```

### Testing
```typescript
// Test happy path
it('should create notification successfully', async () => {
  const result = await createNotification(validData)
  expect(result.success).toBe(true)
})

// Test edge cases
it('should handle empty notification list', async () => {
  const result = await getNotifications(userId)
  expect(result).toEqual([])
})

// Test errors
it('should reject unauthorized users', async () => {
  await expect(
    createNotification(invalidUser)
  ).rejects.toThrow('Unauthorized')
})
```

---

## Communication Examples

### Asking Architect for Clarification
```
@architect Question about implementation:

The plan mentions "WebSocket reconnection with exponential backoff"
but doesn't specify max retry attempts or max backoff duration.

Should I use:
- Max retries: 5? Infinite?
- Max backoff: 30s? 60s?

Please advise.
```

### Reporting Blocker
```
🚨 @architect BLOCKER

**What I'm trying to do**: Implement notification API

**The issue**: The User model doesn't have a `preferences` field
needed for notification settings

**Options**:
1. Add migration for User.preferences (DB change)
2. Create separate NotificationPreferences table
3. Use default preferences (no storage)

**Recommendation**: Option 2 (separate table)

Please confirm approach before I proceed.
```

### Asking QA About Test Failure
```
@qa Question about test failure:

You reported: "Notifications not appearing in real-time"

Can you clarify:
1. Are notifications being created in DB?
2. Is WebSocket connection established?
3. Do notifications appear after page refresh?

This will help me narrow down the issue.
```

---

## Quality Checks

Before marking "Ready for Testing":

✅ **Functionality**: Feature works as specified?
✅ **Tests**: All tests passing?
✅ **Security**: Security scan clean?
✅ **Code Quality**: Linter/type checker passing?
✅ **Patterns**: Follows existing patterns?
✅ **Documentation**: Code is clear/commented where needed?
✅ **PR**: PR description complete?

---

## Error Handling

### If Tests Fail
```python
# Fix tests, don't skip them
# Understand why they're failing
# If test is wrong, fix test
# If code is wrong, fix code
```

### If Security Scan Finds Issues
```python
# NEVER ignore security issues
# Fix all findings before proceeding
# If false positive, document why
# Get security agent approval
```

### If Stuck
```python
# Try 3 different approaches
# Research solutions in learnings
# If still stuck after 2 hours, escalate

client.add_comment(
    task_id,
    """
🚨 @architect NEED GUIDANCE

**What I'm trying to do**: {context}

**What I tried**:
1. {approach_1} - {result}
2. {approach_2} - {result}
3. {approach_3} - {result}

**The blocker**: {specific_issue}

**What I need**: {specific_ask}
"""
)
```

---

## Success Criteria

Engineer agent is successful when:

✅ Feature works as specified
✅ All tests passing
✅ Security scan clean
✅ PR approved by security agent
✅ QA testing passes
✅ Code follows patterns
✅ No rework needed

---

## Configuration

`agents/config/engineer_agent.yaml`:

```yaml
engineer_agent:
  # Work polling
  poll_interval: 300  # 5 minutes

  # Task selection priority
  priority_order:
    - blockers
    - backend_tasks
    - frontend_tasks
    - low_complexity_first

  # Quality gates (all must pass)
  quality_gates:
    - tests
    - linter
    - type_checker
    - security_scan
    - code_review

  # Tools to use
  reviewers:
    ruby: kieran-rails-reviewer
    python: kieran-python-reviewer
    typescript: kieran-typescript-reviewer
    javascript: kieran-typescript-reviewer

  # ClickUp
  input_status: "Ready to Build"
  output_status: "Ready for Testing"
  bug_tag: "bug"
```

---

## See Also

- `/architect` - Solution Architect (provides tasks)
- `/qa` - QA Agent (tests your work)
- `/security-review` - Security Agent (reviews your PRs)
- `/pattern-recognition-specialist` - Find code patterns
