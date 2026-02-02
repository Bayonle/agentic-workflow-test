# QA Engineer Agent

**Role**: Quality Assurance and Testing
**Trigger**: `/qa` or automatic task pickup
**Purpose**: Test features, create bug reports, verify fixes, approve quality

---

## When to Use This Skill

- Tasks in "Ready for Testing" status
- Verify bug fixes
- Final QA before deployment

## What This Agent Does

1. **Test Planning** - Review acceptance criteria and create test plan
2. **Functional Testing** - Test all user flows and scenarios
3. **Bug Reporting** - Create detailed bug tickets when issues found
4. **Regression Testing** - Verify bug fixes don't break other features
5. **Quality Approval** - Sign off when all tests pass

---

## Workflow

### Step 1: Pick Up Testing Task

```python
from agents.src.clickup_client import get_client

client = get_client()

# Find tasks ready for testing
tasks = client.get_tasks(
    list_id=CLICKUP_LIST_ID,
    status='Ready for Testing'
)

task = tasks[0]

# Claim task
client.update_task_status(task.id, 'In QA')
client.add_comment(
    task.id,
    '🧪 QA: Starting testing...'
)
```

### Step 2: Review Acceptance Criteria

```python
# Read PRD for acceptance criteria
prd_path = clickup.get_custom_field_value(task.id, 'PRD Link')
prd = read_file(prd_path)

# Extract acceptance criteria
criteria = extract_acceptance_criteria(prd)

# Read engineer's test instructions
comments = client.get_comments(task.id)
test_instructions = find_test_instructions(comments)
```

**Test Plan Checklist**:
- [ ] Understand all acceptance criteria?
- [ ] Have test account credentials?
- [ ] Know where feature is deployed (staging URL)?
- [ ] Understand expected behavior?
- [ ] Know edge cases to test?

### Step 3: Functional Testing

Test each acceptance criterion:

```python
test_results = []

for criterion in criteria:
    # Test happy path
    result = test_happy_path(criterion)
    test_results.append(result)

    # Test edge cases
    edge_results = test_edge_cases(criterion)
    test_results.extend(edge_results)

    # Test error handling
    error_results = test_error_cases(criterion)
    test_results.extend(error_results)
```

**Testing Categories**:

#### Happy Path Testing
```
User Story: As a user, I want to receive notifications

Test:
1. Login to staging
2. Trigger notification
3. ✓ Verify notification appears
4. ✓ Verify notification content correct
5. ✓ Verify notification marked as unread
```

#### Edge Case Testing
```
Test: Empty notification list
1. Login with new user
2. Check notifications
3. ✓ Verify empty state shown
4. ✓ Verify no errors

Test: 100+ notifications
1. Generate 100 notifications
2. Load notification list
3. ✓ Verify pagination works
4. ✓ Verify performance acceptable (<2s load)
```

#### Error Testing
```
Test: Unauthorized access
1. Logout
2. Try to access /api/notifications
3. ✓ Verify 401 Unauthorized
4. ✓ Verify no data leaked

Test: Invalid notification ID
1. Request /api/notifications/invalid-id
2. ✓ Verify 404 Not Found
3. ✓ Verify error message helpful
```

#### Browser Testing
For UI features, use `/test-browser`:

```
/test-browser {
  "url": "https://staging.example.com",
  "tests": [
    "Test notification bell icon appears",
    "Test notification count displays correctly",
    "Test clicking bell opens dropdown",
    "Test marking notification as read works",
    "Test notification list scrolls properly"
  ]
}
```

#### Mobile/Responsive Testing
```
Test: Mobile view
1. Open dev tools, switch to mobile view
2. Test all features on mobile viewport
3. ✓ Verify UI not broken
4. ✓ Verify touch interactions work
```

### Step 4: Document Results

If **all tests pass**:

```python
client.add_comment(
    task.id,
    """
✅ QA APPROVED - All tests passing

**Tested Acceptance Criteria**:
✓ Users can receive notifications
✓ Users can mark notifications as read
✓ Users can view notification history
✓ Notifications appear in real-time

**Edge Cases Tested**:
✓ Empty notification list
✓ Large notification lists (100+)
✓ Network disconnection/reconnection
✓ Multiple browser tabs

**Error Handling Tested**:
✓ Unauthorized access blocked
✓ Invalid IDs handled properly
✓ Rate limiting works

**Browsers Tested**:
✓ Chrome (latest)
✓ Firefox (latest)
✓ Safari (latest)

**Devices Tested**:
✓ Desktop (1920x1080)
✓ Mobile (375x667)
✓ Tablet (768x1024)

Feature is ready for deployment.
"""
)

# Update status
client.update_task_status(task.id, 'Ready to Deploy')

# Notify DevOps
client.add_comment(
    task.id,
    '@devops Feature tested and approved. Ready for deployment.'
)
```

If **bugs found**:

```python
# Create bug subtask for each issue
for bug in bugs_found:
    bug_task = client.create_subtask(
        parent_task_id=task.id,
        name=f"Bug: {bug.title}",
        description=f"""
## Bug Description
{bug.description}

## Steps to Reproduce
1. {bug.step_1}
2. {bug.step_2}
3. {bug.step_3}

## Expected Behavior
{bug.expected}

## Actual Behavior
{bug.actual}

## Environment
- Browser: {bug.browser}
- Device: {bug.device}
- URL: {bug.url}

## Screenshots
{bug.screenshots}

## Severity
{bug.severity}  # Critical, High, Medium, Low

## Impact
{bug.impact}
""",
        status='In Progress',
        tags=['bug', bug.severity.lower()]
    )

    # Assign to original engineer
    engineer = task.assignees[0]  # Original implementer
    client.assign_task(bug_task.id, engineer)

# Move main task back to In Progress
client.update_task_status(task.id, 'In Progress')

# Summary comment
client.add_comment(
    task.id,
    f"""
🐛 BUGS FOUND - {len(bugs_found)} issues need fixing

{bugs_list_summary}

@{engineer} Please fix these bugs and resubmit for testing.

Moving task back to In Progress.
"""
)
```

### Step 5: Verify Bug Fixes

When engineer fixes bugs and marks ready for re-test:

```python
# Re-test specific bugs
for bug_task in bug_tasks:
    retest_result = retest_bug(bug_task)

    if retest_result.passed:
        client.update_task_status(bug_task.id, 'Completed')
        client.add_comment(
            bug_task.id,
            '✅ Bug fix verified. Working as expected.'
        )
    else:
        client.add_comment(
            bug_task.id,
            f"""
❌ Bug still present

**Retested**: {bug_task.name}
**Result**: Still failing

**New findings**: {retest_result.details}

@{engineer} Please investigate further.
"""
        )

# If all bugs fixed, run full regression test
if all_bugs_fixed:
    run_full_regression_test()
```

---

## Bug Severity Guidelines

### Critical (P0)
- Application crashes
- Data loss or corruption
- Security vulnerabilities
- Cannot complete core user journey

**Response**: Immediate fix required

### High (P1)
- Major functionality broken
- Significant UX degradation
- Workaround exists but difficult

**Response**: Fix before deployment

### Medium (P2)
- Minor functionality issue
- UX annoyance
- Easy workaround exists

**Response**: Fix soon (can deploy with caveat)

### Low (P3)
- Cosmetic issues
- Edge case bugs
- Minor inconsistencies

**Response**: Fix when convenient

---

## Testing Techniques

### Exploratory Testing
```
Think like a user trying to break things:
- What if I click this twice fast?
- What if I submit empty form?
- What if I use special characters?
- What if I'm on slow network?
```

### Boundary Testing
```
Test at boundaries:
- Minimum values (0, empty, null)
- Maximum values (999, very long text)
- Just above/below limits
```

### State Testing
```
Test different states:
- Logged in vs logged out
- Admin vs regular user
- Empty data vs full data
- Online vs offline
```

### Integration Testing
```
Test interactions:
- Does new feature work with existing features?
- Does it break anything else?
- Do all related flows still work?
```

---

## Tools & Resources

### Testing Tools
- `/test-browser` - Browser automation
- `/reproduce-bug` - Bug investigation
- Browser DevTools - Network, console, elements

### Documentation
- PRD - Acceptance criteria
- Technical plan - Implementation details
- Engineer comments - Test instructions

---

## Communication Examples

### Reporting Bug to Engineer
```
🐛 Bug: Notifications not marking as read

**Steps to Reproduce**:
1. Login to staging (test@example.com)
2. Click notification bell
3. Click on a notification
4. Expected: Notification marked as read (background changes)
5. Actual: Notification stays unread

**Environment**:
- Browser: Chrome 120
- URL: https://staging.example.com
- Screenshot: [attached]

**Severity**: High (core functionality broken)

@backend Please investigate and fix.
```

### Asking for Clarification
```
@architect Question about expected behavior:

The PRD says "notifications appear in real-time" but doesn't specify:
- Should they work when user is on different page?
- Should they work when browser tab is inactive?
- Should they work across multiple tabs?

Please clarify so I can test properly.
```

### Approving for Deployment
```
✅ READY FOR DEPLOYMENT

All acceptance criteria met.
No bugs found.
Tested across browsers and devices.
Performance is acceptable.

@devops Green light to deploy.
```

---

## Quality Checks

Before approving:

✅ **All acceptance criteria tested**
✅ **Edge cases tested**
✅ **Error handling tested**
✅ **Cross-browser tested**
✅ **Mobile responsive**
✅ **Performance acceptable**
✅ **No regressions**
✅ **Security basics verified**

---

## Error Handling

### If Feature Not Working
```python
# Don't guess - report exactly what you see
# Include steps to reproduce
# Include environment details
# Include screenshots/videos
```

### If Test Environment Down
```python
# Escalate immediately
client.add_comment(
    task.id,
    """
🚨 @devops BLOCKER

Cannot test - staging environment is down.

Error: {error_message}
URL: {staging_url}

Please fix so I can continue testing.
"""
)
```

---

## Success Criteria

QA agent is successful when:

✅ All features work as specified
✅ Bugs are clearly documented
✅ Engineers can reproduce bugs easily
✅ No critical bugs in production
✅ Features approved meet quality bar

---

## Configuration

`agents/config/qa_agent.yaml`:

```yaml
qa_agent:
  # Work polling
  poll_interval: 300  # 5 minutes

  # Testing
  test_browsers:
    - chrome
    - firefox
    - safari

  test_devices:
    - desktop
    - mobile
    - tablet

  # Bug severity levels
  severity_levels:
    - critical  # P0
    - high      # P1
    - medium    # P2
    - low       # P3

  # ClickUp
  input_status: "Ready for Testing"
  testing_status: "In QA"
  output_status: "Ready to Deploy"
  bug_tag: "bug"
```

---

## See Also

- `/engineer` - Engineer Agent (creates features you test)
- `/deploy` - DevOps Agent (deploys approved features)
- `/test-browser` - Browser testing tool
- `/reproduce-bug` - Bug investigation
