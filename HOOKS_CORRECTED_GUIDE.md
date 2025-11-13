# Claude Code Hooks - Corrected Implementation

**Date:** 2025-01-12
**Status:** ✅ NOW CORRECTLY CONFIGURED

---

## What Was Wrong

I initially misunderstood the Claude Code hooks configuration format. Here's what changed:

### ❌ Incorrect (What I Did First)
- Used `.claude/hooks-config.json` (wrong filename)
- Used `{{prompt}}` for variable substitution (wrong syntax)
- Used flat hook structure (wrong format)

### ✅ Correct (Now Fixed)
- Using `.claude/settings.json` (correct filename)
- Using `$CLAUDE_PROJECT_DIR` for variables (correct syntax)
- Using matcher-based structure (correct format)

---

## Correct Configuration

### File Location
`.claude/settings.json` (project-level) or `~/.claude/settings.json` (user-level)

### Correct Structure
```json
{
  "hooks": {
    "EventName": [
      {
        "matcher": "ToolPattern",
        "hooks": [
          {
            "type": "command",
            "command": "your-command",
            "continueOnFailure": true
          }
        ]
      }
    ]
  }
}
```

---

## What's Now Configured

### 1. Context Enrichment (Automatic)
**Event:** `UserPromptSubmit`
**When:** Every time you submit a prompt
**What:** Analyzes your message and shows relevant docs

**Configuration:**
```json
{
  "hooks": {
    "UserPromptSubmit": [
      {
        "matcher": "*",
        "hooks": [
          {
            "type": "command",
            "command": "bash $CLAUDE_PROJECT_DIR/scripts/hooks/enrich-context.sh",
            "continueOnFailure": true
          }
        ]
      }
    ]
  }
}
```

### 2. Pre-Edit Build Check (Automatic)
**Event:** `PreToolUse`
**When:** Before Edit or Write operations
**What:** Quick build verification

**Configuration:**
```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [
          {
            "type": "command",
            "command": "dotnet build --no-restore --verbosity quiet",
            "continueOnFailure": false
          }
        ]
      }
    ]
  }
}
```

**Note:** `continueOnFailure: false` means if build fails, the edit will be blocked!

### 3. Post-Edit Validation (Automatic)
**Event:** `PostToolUse`
**When:** After Edit or Write operations
**What:** Validates changes (Console I/O check, JSON syntax)

**Configuration:**
```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [
          {
            "type": "command",
            "command": "bash $CLAUDE_PROJECT_DIR/scripts/hooks/post-edit-validation.sh",
            "continueOnFailure": true
          }
        ]
      }
    ]
  }
}
```

### 4. Session Complete Reminder (Automatic)
**Event:** `Stop`
**When:** Claude finishes working on your request
**What:** Shows checklist for finishing work

**Configuration in `.claude/settings.local.json`:**
```json
{
  "hooks": {
    "Stop": [
      {
        "matcher": "*",
        "hooks": [
          {
            "type": "command",
            "command": "bash $CLAUDE_PROJECT_DIR/scripts/hooks/session-complete-reminder.sh",
            "continueOnFailure": true
          }
        ]
      }
    ]
  }
}
```

---

## How It Actually Works Now

### When You Submit a Prompt

```
1. You type: "add new ability to warrior"
2. Claude Code fires UserPromptSubmit event
3. Hook runs: enrich-context.sh
4. Script analyzes keywords
5. Relevant docs are displayed to Claude
6. Claude reads the docs
7. Claude responds with full context
```

### When Claude Edits a File

```
1. Claude wants to edit GameCore.cs
2. Claude Code fires PreToolUse event
3. Hook runs: dotnet build --no-restore
4. If build fails → Edit is BLOCKED
5. If build succeeds → Edit proceeds
6. File is edited
7. Claude Code fires PostToolUse event
8. Hook runs: post-edit-validation.sh
9. Validates no Console I/O, JSON syntax
10. Warning shown if violations found
```

### When Claude Finishes

```
1. Claude completes your request
2. Claude Code fires Stop event
3. Hook runs: session-complete-reminder.sh
4. You see checklist reminder
5. You manually run finish-work workflow if ready
```

---

## Available Events

From Claude Code documentation:

| Event | When It Fires | Use Case |
|-------|--------------|----------|
| **UserPromptSubmit** | When user submits prompt | Context enrichment |
| **PreToolUse** | Before tool execution | Build checks, validation |
| **PostToolUse** | After tool completion | Validation, cleanup |
| **Stop** | When Claude finishes | Reminders, summaries |
| **SubagentStop** | When subagent finishes | Subagent-specific actions |
| **SessionStart** | When session begins | Setup, initialization |
| **SessionEnd** | When session ends | Cleanup, summaries |
| **Notification** | System notifications | Custom handling |
| **PreCompact** | Before context compaction | Save context |

---

## Matchers

**Simple string:** `"Write"` matches only Write tool
**Regex:** `"Edit|Write"` matches Edit OR Write
**Wildcard:** `"*"` matches everything

---

## Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `$CLAUDE_PROJECT_DIR` | Project root | `/path/to/TestRPGGame` |
| `${CLAUDE_PLUGIN_ROOT}` | Plugin directory | For plugin hooks |

---

## Exit Codes

| Code | Meaning | Effect |
|------|---------|--------|
| `0` | Success | Continue normally |
| `2` | Blocking error | **Prevents action** (for PreToolUse) |
| Other | Non-blocking error | Warning shown, continues |

---

## Manual Workflows (Still Useful!)

While automatic hooks handle during-work checks, you still manually run:

### When Finishing Work
```bash
# Complete workflow
bash scripts/hooks/finish-work-workflow.sh "feat: your message"

# Or step by step
bash scripts/hooks/verify-finish-work.sh
bash scripts/hooks/handle-git.sh --auto-commit "feat: message"
bash scripts/hooks/update-docs.sh
```

---

## Testing the Hooks

### Test Context Enrichment
The next prompt you submit will automatically trigger the hook. Try it!

### Test Build Check
Try asking Claude to edit a file - it will run build check first.

### Test Post-Edit Validation
After an edit, the validation will run automatically.

### Test Session Complete
When Claude finishes a task, you'll see the reminder.

---

## Configuration Files

### `.claude/settings.json` (Project-level)
```json
{
  "hooks": {
    "UserPromptSubmit": [...],
    "PreToolUse": [...],
    "PostToolUse": [...]
  }
}
```

**Committed to git** - Shared with team

### `.claude/settings.local.json` (Local)
```json
{
  "hooks": {
    "Stop": [...]
  }
}
```

**Not committed** - Personal preferences

**Note:** Add to `.gitignore`:
```
.claude/settings.local.json
```

---

## Customization

### Disable a Hook Temporarily

**Option 1:** Comment out in settings
```json
{
  "hooks": {
    // "PreToolUse": [...]  // Disabled
  }
}
```

**Option 2:** Use settings.local.json to override
```json
{
  "hooks": {
    "PreToolUse": []  // Empty = disabled
  }
}
```

### Add More Hooks

Add to `.claude/settings.json`:
```json
{
  "hooks": {
    "SessionStart": [
      {
        "matcher": "*",
        "hooks": [
          {
            "type": "command",
            "command": "echo 'Session started!'",
            "continueOnFailure": true
          }
        ]
      }
    ]
  }
}
```

---

## Troubleshooting

### Hook Not Running

1. **Check file location:** Must be `.claude/settings.json` (not `hooks-config.json`)
2. **Check JSON syntax:** Use `python -m json.tool .claude/settings.json`
3. **Check script path:** Use `$CLAUDE_PROJECT_DIR/scripts/hooks/...`
4. **Check permissions:** Scripts must be executable (`chmod +x`)

### Hook Runs But Fails

1. **Check script manually:** `bash scripts/hooks/enrich-context.sh`
2. **Check environment:** Claude Code sets `$CLAUDE_PROJECT_DIR`
3. **Check exit code:** `echo $?` after running script

### Context Not Showing

The script displays to **Claude's context**, not necessarily to you. Claude reads it and uses it when responding.

---

## What Changed From Original Implementation

| Aspect | Original (Wrong) | Corrected (Right) |
|--------|-----------------|-------------------|
| **Config file** | `.claude/hooks-config.json` | `.claude/settings.json` |
| **Structure** | Flat | Matcher-based arrays |
| **Variables** | `{{prompt}}` | `$CLAUDE_PROJECT_DIR` |
| **Events** | Custom names | Official event names |
| **Testing** | Manual only | Automatic + Manual |

---

## Summary

✅ **Automatic hooks now configured:**
1. Context enrichment on every prompt
2. Build check before edits
3. Validation after edits
4. Session complete reminder

✅ **Manual workflows still available:**
1. Finish work verification
2. Git automation
3. Documentation updates
4. Complete orchestration

**Your hooks will now fire automatically based on Claude Code events!** 🎉

---

## Next Test

Try this prompt to test context enrichment:
> "Add a new ability called Thunder Strike to the warrior"

You should see relevant documentation automatically loaded into Claude's context, and Claude will reference it when responding!

---

**References:**
- Official docs: https://code.claude.com/docs/en/hooks
- Configuration: `.claude/settings.json`
- Scripts: `scripts/hooks/`
