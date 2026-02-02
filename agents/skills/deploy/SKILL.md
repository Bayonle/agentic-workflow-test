# DevOps Engineer Agent

**Role**: Deployment and Infrastructure
**Trigger**: `/deploy` or automatic task pickup
**Purpose**: Deploy features to production safely with verification and monitoring

---

## When to Use This Skill

- Tasks in "Ready to Deploy" status
- Final deployment to production
- Infrastructure changes needed

## What This Agent Does

1. **Pre-Deploy Verification** - Ensure all quality gates passed
2. **Deployment Planning** - Create deployment plan with verification
3. **Approval Request** - Request human approval for production deploy
4. **Deployment Execution** - Execute deployment safely
5. **Verification** - Verify deployment successful
6. **Monitoring** - Monitor production metrics post-deploy
7. **Documentation** - Document deployment and any learnings

---

## Workflow

### Step 1: Pick Up Deployment Task

```python
from agents.src.clickup_client import get_client

client = get_client()

# Find tasks ready to deploy
tasks = client.get_tasks(
    list_id=CLICKUP_LIST_ID,
    status='Ready to Deploy'
)

task = tasks[0]

# Acknowledge
client.add_comment(
    task.id,
    '🚀 DevOps: Preparing deployment...'
)
```

### Step 2: Pre-Deploy Verification

```python
# Verify all quality gates passed
checks = {
    'tests_passing': check_ci_tests(),
    'security_approved': check_security_approval(task.id),
    'qa_approved': check_qa_approval(task.id),
    'pr_merged': check_pr_merged(task.id),
}

if not all(checks.values()):
    client.add_comment(
        task.id,
        f"""
⚠️ DEPLOYMENT BLOCKED - Quality gates not met

{format_failed_checks(checks)}

Cannot deploy until all gates pass.
"""
    )
    return

# Check for migration safety
has_migrations = check_for_migrations()
if has_migrations:
    review_migration_safety()
```

### Step 3: Create Deployment Plan

Use `/deployment-verification-agent`:

```python
deployment_plan = create_deployment_plan(task)

# Deployment plan includes:
# - Pre-deploy checklist
# - Deployment steps
# - Verification queries/tests
# - Rollback procedure
# - Monitoring plan
```

Example deployment plan:

```markdown
# Deployment Plan: User Notifications

## Pre-Deploy Checklist
- [x] All tests passing in CI
- [x] Security review approved
- [x] QA approved
- [x] PR merged to main
- [x] Database migration reviewed
- [x] Rollback plan ready
- [x] Monitoring alerts configured

## Deployment Steps

### 1. Database Migration (if needed)
```bash
# Run migration
npm run migrate:up

# Verify migration
npm run migrate:verify
```

### 2. Deploy Backend
```bash
# Deploy API server
kubectl apply -f k8s/api-deployment.yaml

# Wait for rollout
kubectl rollout status deployment/api

# Verify health
curl https://api.example.com/health
```

### 3. Deploy Frontend
```bash
# Build frontend
npm run build

# Deploy to CDN
aws s3 sync dist/ s3://app-bucket/ --delete
aws cloudfront create-invalidation --distribution-id XXX

# Verify
curl https://app.example.com
```

### 4. Deploy WebSocket Server (if needed)
```bash
kubectl apply -f k8s/websocket-deployment.yaml
kubectl rollout status deployment/websocket
```

## Verification Plan

### Database Verification
```sql
-- Verify migration applied
SELECT version FROM schema_migrations ORDER BY version DESC LIMIT 1;

-- Verify no data lost
SELECT COUNT(*) FROM notifications;

-- Verify indexes created
SHOW INDEX FROM notifications;
```

### API Verification
```bash
# Health check
curl https://api.example.com/health
# Expected: {"status": "healthy"}

# Test notification endpoint
curl -X GET https://api.example.com/api/notifications \
  -H "Authorization: Bearer $TEST_TOKEN"
# Expected: 200 OK with notifications array

# Test notification creation
curl -X POST https://api.example.com/api/notifications \
  -H "Authorization: Bearer $TEST_TOKEN" \
  -d '{"title":"Test","message":"Test notification"}'
# Expected: 201 Created
```

### Frontend Verification
```
1. Open https://app.example.com
2. Verify notification bell appears
3. Trigger test notification
4. Verify notification appears in real-time
5. Mark notification as read
6. Verify read state persists
```

### WebSocket Verification
```bash
# Check connection count
kubectl logs deployment/websocket | grep "connections: "
# Expected: Active connections > 0

# Test real-time delivery
# (Use test script)
node scripts/test-notification-delivery.js
```

## Monitoring Plan

Monitor these metrics for 1 hour post-deploy:

### Application Metrics
- API response time (target: <100ms p99)
- Error rate (target: <0.1%)
- WebSocket connections (target: stable)
- Notification delivery time (target: <1s)

### Infrastructure Metrics
- CPU usage (target: <70%)
- Memory usage (target: <80%)
- Database connections (target: <max_connections)
- Database query time (target: <50ms)

### Alerts
- API error rate > 1%
- API response time > 500ms
- WebSocket disconnection rate > 5%
- Database CPU > 90%

## Rollback Plan

If issues detected within 1 hour:

### Rollback Steps
1. **Revert frontend**
   ```bash
   # Restore previous version
   aws s3 sync s3://app-bucket-backup/ s3://app-bucket/ --delete
   aws cloudfront create-invalidation --distribution-id XXX
   ```

2. **Revert backend**
   ```bash
   kubectl rollout undo deployment/api
   kubectl rollout undo deployment/websocket
   ```

3. **Keep database**
   ```
   Migration is additive (only adds notifications table)
   Safe to keep - old code doesn't use it
   ```

### Rollback Verification
```bash
# Verify old version running
curl https://api.example.com/version
# Expected: Previous version number

# Verify app functional
# Test core user flows
```

## Success Criteria

Deployment is successful if:
- All verification queries return expected results
- All metrics within target ranges
- No alerts triggered for 1 hour
- Sample user flows work correctly
- No spike in error rates
```

### Step 4: Request Human Approval

```python
client.add_comment(
    task.id,
    """
🚀 @human READY TO DEPLOY TO PRODUCTION

I've prepared the deployment plan and verified all quality gates.

**Pre-Deploy Status**:
✓ All tests passing
✓ Security approved
✓ QA approved
✓ PR merged

**Deployment Plan**: See detailed plan above

**What will be deployed**:
- Backend API with notification endpoints
- Frontend notification UI
- WebSocket server for real-time delivery
- Database migration (adds notifications table)

**Risk Assessment**: Low
- Migration is additive (no data loss risk)
- Rollback plan ready
- Can revert in <5 minutes if issues

**Monitoring Plan**:
- 1 hour active monitoring
- Alerts configured
- Rollback automatic if error rate spikes

**Deployment Window**: Now (low traffic period)

**Approve to proceed with production deployment**

Reply "Approved" to deploy or "Hold" to wait.
"""
)

# Wait for human approval
# (In practice, check comments for approval)
```

### Step 5: Execute Deployment

After approval:

```python
client.add_comment(
    task.id,
    '🚀 Deployment started...'
)

deployment_log = []

# Execute each step
for step in deployment_plan.steps:
    try:
        result = execute_step(step)
        deployment_log.append(f"✓ {step.name}: {result}")

        # Update progress
        client.add_comment(
            task.id,
            f"✓ {step.name} complete"
        )
    except Exception as e:
        deployment_log.append(f"✗ {step.name}: {str(e)}")

        # Deployment failed - rollback
        client.add_comment(
            task.id,
            f"""
❌ Deployment failed at step: {step.name}
Error: {str(e)}

Starting automatic rollback...
"""
        )
        execute_rollback()
        return
```

### Step 6: Verify Deployment

```python
# Run verification queries
verification_results = []

for query in deployment_plan.verification:
    result = run_verification(query)
    verification_results.append(result)

# Check metrics
metrics = check_production_metrics()

# Verify health
health_check = verify_application_health()
```

### Step 7: Monitor Post-Deploy

```python
# Monitor for 1 hour
monitoring_period = 60  # minutes
start_time = datetime.now()

while (datetime.now() - start_time).minutes < monitoring_period:
    metrics = get_production_metrics()

    # Check for issues
    if metrics.error_rate > threshold:
        trigger_rollback('Error rate exceeded threshold')
        break

    if metrics.response_time_p99 > threshold:
        alert_human('Response time degradation detected')

    # Report status every 15 minutes
    if (datetime.now() - start_time).minutes % 15 == 0:
        client.add_comment(
            task.id,
            f"""
📊 Deployment Status Update ({elapsed} minutes)

**Metrics**:
- Error rate: {metrics.error_rate}% ✓
- Response time (p99): {metrics.response_time_p99}ms ✓
- Active connections: {metrics.connections} ✓

All metrics within normal range.
"""
        )

    time.sleep(60)  # Check every minute
```

### Step 8: Mark Complete

If successful:

```python
client.update_task_status(task.id, 'Deployed')

client.add_comment(
    task.id,
    """
✅ DEPLOYMENT SUCCESSFUL

**Deployment Time**: {start_time} - {end_time} ({duration})

**What Was Deployed**:
- Backend: {backend_version}
- Frontend: {frontend_version}
- Database: Migration {migration_version}

**Verification Results**:
✓ All health checks passing
✓ All verification queries successful
✓ All metrics within target range

**Post-Deploy Monitoring** (1 hour):
✓ Error rate: 0.02% (target: <0.1%)
✓ Response time p99: 87ms (target: <100ms)
✓ WebSocket connections: 1,247 active
✓ No alerts triggered

**Production Links**:
- App: https://app.example.com
- API: https://api.example.com
- Monitoring: https://grafana.example.com/dashboard/notifications

Feature is LIVE in production. 🎉
"""
)

# Document learnings
document_deployment_learnings(task.id, deployment_log)
```

---

## Rollback Procedures

### Automatic Rollback Triggers

```python
# Trigger rollback if:
- Error rate > 1% for 5 minutes
- Response time p99 > 1000ms for 5 minutes
- Health check fails 3 times
- Database errors spike
```

### Manual Rollback

```bash
/deploy rollback [task-id]

# Executes:
# 1. Revert frontend to previous version
# 2. Revert backend to previous version
# 3. Verify rollback successful
# 4. Notify team
```

---

## Tools & Resources

### Deployment Tools
- `/deployment-verification-agent` - Create deployment plans
- `kubectl` - Kubernetes deployments
- `gh` - GitHub CLI
- Cloud CLI (AWS/GCP/Azure)

### Monitoring
- Grafana - Metrics dashboard
- Sentry - Error tracking
- CloudWatch/Datadog - Infrastructure monitoring

---

## Communication Examples

### Requesting Deploy Approval
```
🚀 @human PRODUCTION DEPLOY READY

All gates passed. Deployment plan ready.
Risk: Low | Window: Now | Duration: ~15 min

Approve to deploy User Notifications to production.
```

### Reporting Deployment Success
```
✅ DEPLOYED TO PRODUCTION

User Notifications is now live.
All metrics nominal after 1hr monitoring.

Users can now receive real-time notifications! 🎉
```

### Reporting Deployment Failure
```
❌ DEPLOYMENT FAILED

Failed at: Database migration step
Error: Connection timeout

Automatic rollback completed successfully.
Application restored to previous state.

Root cause: Database connection pool exhausted
Action: Increase pool size, retry deployment

@architect Please review infrastructure capacity.
```

---

## Success Criteria

DevOps agent is successful when:

✅ Zero-downtime deployments
✅ All deployments verified before completion
✅ Rollback plan ready and tested
✅ Clear deployment documentation
✅ No production incidents from deployments

---

## Configuration

`agents/config/devops_agent.yaml`:

```yaml
devops_agent:
  # Work polling
  poll_interval: 300  # 5 minutes

  # Deployment
  require_human_approval: true
  monitoring_period: 60  # minutes

  # Rollback
  auto_rollback_enabled: true
  rollback_triggers:
    error_rate: 1.0  # percent
    response_time_p99: 1000  # ms
    health_check_failures: 3

  # Verification
  verification_timeout: 300  # seconds

  # ClickUp
  input_status: "Ready to Deploy"
  output_status: "Deployed"
```

---

## See Also

- `/deployment-verification-agent` - Deployment planning tool
- `/qa` - QA Agent (approves before deploy)
- `/security-review` - Security Agent (approves before deploy)
- `docs/runbooks/deployment.md` - Deployment runbook
