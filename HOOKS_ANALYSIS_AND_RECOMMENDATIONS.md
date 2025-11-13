# Hooks Analysis & Recommendations

**Date:** 2025-01-12
**Status:** Complete Analysis with Corrected Implementation
**Goal:** Autonomous quality control without "going haywire"

---

## Executive Summary

### What Went Wrong

Your Claude Code instance went "haywire" due to **overly aggressive automation**:

1. **Stop hook** runs 2 scripts that automatically commit code (!)
2. **Autonomous-finish-work.sh** automatically stages, commits, and amends commits without user consent
3. **PreToolUse** runs `dotnet build` before EVERY file edit (slow on Windows)
4. **Hooks triggering in wrong contexts** (e.g., when Claude Code itself is making edits)

### About `$CLAUDE_PROJECT_DIR`

**Good news:** According to the official documentation, `$CLAUDE_PROJECT_DIR` **IS** available to all hooks. However:

- ✅ Available when Claude Code spawns the hook
- ❌ May not be set if you run scripts manually
- 💡 **Alternative on Windows:** Use `$(git rev-parse --show-toplevel)` for manual testing

The environment variable is fine - the issue is the **hook design**, not the variable.

---

## Root Cause Analysis

### Issue 1: Autonomous Commits in Stop Hook ❌

```json
"Stop": [
  {
    "hooks": [
      {
        "command": "bash $CLAUDE_PROJECT_DIR/scripts/hooks/auto-add-tests.sh"
      },
      {
        "command": "bash $CLAUDE_PROJECT_DIR/scripts/hooks/autonomous-finish-work.sh"
      }
    ]
  }
]
```

**Problem:**
- `Stop` event fires when **Claude finishes responding** to ANY prompt
- `autonomous-finish-work.sh` **automatically commits code** without asking
- This happens even if you just asked "what's in this file?"
- Creates unwanted commits constantly

**Why it goes haywire:**
- You ask Claude to refactor something
- Claude makes edits
- Stop event fires
- Script auto-commits intermediate state
- Claude responds to you
- You ask a follow-up question
- Claude makes more edits
- Stop event fires again
- Another auto-commit
- Result: 20 commits for a simple task!

### Issue 2: Build Check Before Every Edit (Performance) ⚠️

```json
"PreToolUse": [
  {
    "matcher": "Edit|Write",
    "hooks": [
      {
        "command": "dotnet build --no-restore --verbosity quiet"
      }
    ]
  }
]
```

**Problem:**
- Runs `dotnet build` before EVERY Edit or Write operation
- On Windows, builds can take 2-5 seconds
- If Claude makes 20 edits, that's 40-100 seconds of waiting
- The build will fail on intermediate states during refactoring
- `continueOnFailure: false` means failed builds **block edits**

**Scenario that breaks:**
1. You ask Claude to refactor 5 files
2. Claude tries to edit File1.cs
3. PreToolUse: Build check (passes)
4. File1 edited
5. Claude tries to edit File2.cs
6. PreToolUse: Build check (FAILS - File2 references new API from File1 not yet created)
7. Edit blocked
8. Refactoring fails

### Issue 3: Stdin Reading in UserPromptSubmit 🤔

```bash
# In enrich-context.sh
USER_INPUT=$(cat)
```

**Potential Problem:**
- UserPromptSubmit hook tries to read stdin
- If Claude Code doesn't provide proper stdin, the script hangs
- Could cause delays or timeouts

### Issue 4: Hook Timing & Context

**The fundamental issue:** Hooks fire during the **conversation**, not at the **end of work**.

```
User: "Refactor the combat system"
  ↓
UserPromptSubmit fires → enrich-context.sh runs
  ↓
Claude: "I'll refactor... <uses Edit tool>"
  ↓
PreToolUse fires → dotnet build runs (2s delay)
  ↓
Edit happens
  ↓
PostToolUse fires → validation runs
  ↓
Claude: "I'll make another change... <uses Edit tool>"
  ↓
PreToolUse fires → dotnet build runs (2s delay)
  ↓
... repeat 10 times ...
  ↓
Claude: "Done!"
  ↓
Stop fires → auto-add-tests.sh runs
          → autonomous-finish-work.sh runs
          → Automatically commits everything!!!
```

**You never got a chance to review the changes!**

---

## Official Documentation Review

### What Hooks Are Available

| Hook Event | When It Fires | Best Use Case |
|------------|---------------|---------------|
| **UserPromptSubmit** | User submits prompt | ✅ Context injection (passive) |
| **PreToolUse** | Before tool execution | ⚠️ Validation (must be FAST) |
| **PostToolUse** | After tool execution | ✅ Validation (passive warnings) |
| **Stop** | When main agent finishes | ✅ Reminders/summaries (NO automation!) |
| **SubagentStop** | When subagent finishes | ✅ Subagent-specific checks |
| **SessionStart** | Session begins/resumes | ✅ Environment setup |
| **SessionEnd** | Session ends | ✅ Cleanup, final reports |
| **PreCompact** | Before context compaction | ✅ Save important context |

### Hook Execution Flow

**Command-based hooks use exit codes:**
- **0** = Success (stdout shown to user if `transcript` mode)
- **2** = Blocking error (stderr shown to Claude, can prevent action)
- **Other** = Non-blocking error (stderr shown to user only)

**Prompt-based hooks use JSON responses:**
```json
{
  "decision": "approve" | "block",
  "reason": "Why blocking/approving",
  "systemMessage": "Warning to show user"
}
```

### What Changed from Your Understanding

| Aspect | Your Implementation | Reality |
|--------|-------------------|---------|
| **Stop hook purpose** | Automate commits | ❌ For reminders only! |
| **PreToolUse blocking** | Build before every edit | ❌ Too slow, breaks multi-file changes |
| **Auto-commit timing** | When Claude stops | ❌ That's mid-conversation! |
| **Autonomous workflow** | Fully automatic | ❌ User should control commits |

---

## Recommended Hooks Strategy

### Philosophy: **Guard Rails, Not Autopilot**

Hooks should:
- ✅ **Prevent mistakes** (e.g., block commits with Console I/O)
- ✅ **Provide context** (e.g., show relevant docs)
- ✅ **Give warnings** (e.g., "tests might be needed")
- ❌ **NOT make decisions for you** (e.g., auto-commit)
- ❌ **NOT block normal workflow** (e.g., build before every edit)

### Tier 1: Essential Quality Guards (Always On)

#### 1. Context Enrichment (UserPromptSubmit)
**Purpose:** Help Claude understand your codebase
**Risk:** Low
**Performance:** Fast (<1s)

```json
{
  "hooks": {
    "UserPromptSubmit": [
      {
        "matcher": "",
        "hooks": [
          {
            "type": "command",
            "command": "bash scripts/hooks/enrich-context-v2.sh",
            "timeout": 5000
          }
        ]
      }
    ]
  }
}
```

**Changes from your version:**
- ✅ Keep the keyword detection logic
- ✅ Remove stdin reading (use hook input JSON instead)
- ✅ Output to stdout (Claude sees it)
- ✅ Keep it fast (5s timeout)

#### 2. Post-Edit Architecture Validation (PostToolUse)
**Purpose:** Warn about architecture violations
**Risk:** Low (warnings only)
**Performance:** Fast (<0.5s)

```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [
          {
            "type": "command",
            "command": "bash scripts/hooks/architecture-guard.sh",
            "timeout": 3000
          }
        ]
      }
    ]
  }
}
```

**Checks:**
- Console.WriteLine/ReadLine in game logic
- JSON syntax errors
- Interface violations (e.g., calling Console from GameCore)

**Always returns exit 0** (warnings, never blocks)

#### 3. Session Start Environment Check (SessionStart)
**Purpose:** Verify environment is ready
**Risk:** Low
**Performance:** Fast (<2s)

```json
{
  "hooks": {
    "SessionStart": [
      {
        "matcher": "startup",
        "hooks": [
          {
            "type": "command",
            "command": "bash scripts/hooks/session-start.sh",
            "timeout": 5000
          }
        ]
      }
    ]
  }
}
```

**Checks:**
- .NET SDK available
- Build is passing (baseline)
- Tests are passing (baseline)
- Git status clean or reports uncommitted work

### Tier 2: Interactive Quality Control (User-Triggered)

These should **NOT** be hooks - they're **manual scripts** you run when ready.

#### 4. Pre-Commit Quality Gate (Manual)
**Run:** `bash scripts/hooks/pre-commit-gate.sh`
**Purpose:** Comprehensive checks before committing

```bash
#!/bin/bash
# Pre-Commit Quality Gate
# Run manually before committing: bash scripts/hooks/pre-commit-gate.sh

echo "🔍 Running pre-commit quality gate..."
echo ""

EXIT_CODE=0

# Check 1: Build
echo "🔨 Building project..."
if ! dotnet build --no-restore --verbosity quiet; then
    echo "❌ Build failed"
    EXIT_CODE=1
else
    echo "✅ Build passed"
fi

# Check 2: Tests
echo "🧪 Running tests..."
if ! dotnet test --no-build --verbosity quiet; then
    echo "❌ Tests failed"
    EXIT_CODE=1
else
    TEST_COUNT=$(dotnet test --no-build --verbosity quiet | grep -oP "Passed:\s+\K\d+" || echo "?")
    echo "✅ Tests passed ($TEST_COUNT tests)"
fi

# Check 3: Architecture violations
echo "🏛️ Checking architecture..."
VIOLATIONS=$(grep -r "Console\.\(WriteLine\|ReadLine\|Write\|Read\)" TestRPGGame/{Systems,Combat,Entities}/**/*.cs 2>/dev/null | grep -v "ConsoleInterface.cs" || echo "")
if [ -n "$VIOLATIONS" ]; then
    echo "❌ Architecture violations found:"
    echo "$VIOLATIONS"
    EXIT_CODE=1
else
    echo "✅ No architecture violations"
fi

# Check 4: JSON validity
echo "📄 Validating JSON files..."
for json_file in TestRPGGame/Data/**/*.json; do
    if ! python -m json.tool "$json_file" > /dev/null 2>&1; then
        echo "❌ Invalid JSON: $json_file"
        EXIT_CODE=1
    fi
done
echo "✅ All JSON files valid"

# Summary
echo ""
if [ $EXIT_CODE -eq 0 ]; then
    echo "✅ ALL CHECKS PASSED - Ready to commit!"
else
    echo "❌ CHECKS FAILED - Please fix issues before committing"
fi

exit $EXIT_CODE
```

#### 5. Smart Commit Helper (Manual)
**Run:** `bash scripts/hooks/smart-commit.sh "feat: your message"`

#### 6. Documentation Sync (Manual)
**Run:** `bash scripts/hooks/doc-sync.sh`

### Tier 3: Advanced Guardrails (Optional)

#### 7. Prompt-Based Pre-Commit Hook
**Purpose:** LLM analyzes changes before allowing commit
**When:** Only on git commit (using git hooks, not Claude Code hooks)

**Example `.git/hooks/pre-commit`:**
```bash
#!/bin/bash
# Git pre-commit hook (NOT Claude Code hook)
# Runs when you do `git commit`

echo "🤖 Analyzing commit for quality..."

# Get staged changes
DIFF=$(git diff --cached)

# Run LLM analysis (requires Claude API)
ANALYSIS=$(echo "$DIFF" | claude-cli analyze-commit)

# Check for issues
if echo "$ANALYSIS" | grep -q "BLOCK"; then
    echo "❌ Commit blocked by LLM analysis:"
    echo "$ANALYSIS"
    exit 1
fi

echo "✅ Commit approved"
exit 0
```

---

## Corrected Hooks Configuration

### For `.claude/settings.json` (Project-Level)

```json
{
  "hooks": {
    "UserPromptSubmit": [
      {
        "matcher": "",
        "hooks": [
          {
            "type": "command",
            "command": "bash scripts/hooks/context-enrichment.sh",
            "timeout": 5000
          }
        ]
      }
    ],
    "PostToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [
          {
            "type": "command",
            "command": "bash scripts/hooks/architecture-guard.sh",
            "timeout": 3000
          }
        ]
      }
    ],
    "SessionStart": [
      {
        "matcher": "startup",
        "hooks": [
          {
            "type": "command",
            "command": "bash scripts/hooks/session-start-check.sh",
            "timeout": 10000
          }
        ]
      }
    ]
  }
}
```

### For `.claude/settings.local.json` (Personal)

```json
{
  "hooks": {
    "Stop": [
      {
        "matcher": "*",
        "hooks": [
          {
            "type": "command",
            "command": "bash scripts/hooks/stop-reminder.sh",
            "timeout": 2000
          }
        ]
      }
    ]
  },
  "permissions": {
    "allow": [
      "Bash(dotnet build:*)",
      "Bash(dotnet test:*)"
    ]
  }
}
```

**Key Changes:**
- ❌ Removed PreToolUse build check (too slow, breaks multi-file edits)
- ❌ Removed auto-commit hooks from Stop event
- ❌ Removed auto-add-tests from Stop event
- ✅ Added simple reminder in Stop event
- ✅ Kept context enrichment (passive, helpful)
- ✅ Kept architecture validation (passive warnings)
- ✅ Added session start checks (baseline verification)

---

## Revised Script Implementations

### 1. Context Enrichment v2 (Fixed)

**File:** `scripts/hooks/context-enrichment.sh`

```bash
#!/bin/bash
# Context Enrichment v2 - Fixed for Claude Code hooks
# Reads hook input from stdin (JSON), extracts prompt, surfaces relevant docs

# Get hook input (JSON from stdin)
HOOK_INPUT=$(cat)

# Extract user prompt from JSON
USER_PROMPT=$(echo "$HOOK_INPUT" | python -c "import sys, json; print(json.load(sys.stdin).get('prompt', ''))" 2>/dev/null || echo "")

if [ -z "$USER_PROMPT" ]; then
    # Fallback: maybe the input is just the prompt directly
    USER_PROMPT="$HOOK_INPUT"
fi

PROMPT_LOWER=$(echo "$USER_PROMPT" | tr '[:upper:]' '[:lower:]')

# Use git to find project root (works even if CLAUDE_PROJECT_DIR not set)
PROJECT_ROOT=$(git rev-parse --show-toplevel 2>/dev/null || pwd)
CONTEXT_DIR="$PROJECT_ROOT/AgenticContext"

DOCS_TO_SURFACE=()

# === Keyword matching logic (same as before) ===
echo "$PROMPT_LOWER" | grep -qE "architecture|design|pattern|refactor" && DOCS_TO_SURFACE+=("ARCHITECTURE.md")
echo "$PROMPT_LOWER" | grep -qE "ability|effect|skill" && DOCS_TO_SURFACE+=("MULTIPLE_EFFECTS_GUIDE.md")
echo "$PROMPT_LOWER" | grep -qE "test|testing" && DOCS_TO_SURFACE+=("SESSION_GUIDE.md")
echo "$PROMPT_LOWER" | grep -qE "json|data|enemy|dungeon" && DOCS_TO_SURFACE+=("DATA_DRIVEN_SYSTEM.md")

# Remove duplicates
UNIQUE_DOCS=($(printf '%s\n' "${DOCS_TO_SURFACE[@]}" | sort -u))

if [ ${#UNIQUE_DOCS[@]} -eq 0 ]; then
    # No specific docs matched, exit silently
    exit 0
fi

# Output context
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "📖 RELEVANT CONTEXT LOADED"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

for DOC in "${UNIQUE_DOCS[@]}"; do
    DOC_PATH="$CONTEXT_DIR/$DOC"
    if [ -f "$DOC_PATH" ]; then
        echo "📄 $DOC"
        echo ""
        head -n 50 "$DOC_PATH"
        echo ""
        echo "... (see $DOC for complete information)"
        echo ""
    fi
done

exit 0
```

### 2. Architecture Guard (Passive Warnings)

**File:** `scripts/hooks/architecture-guard.sh`

```bash
#!/bin/bash
# Architecture Guard - Post-edit validation
# Warns about architecture violations but never blocks

# Get hook input
HOOK_INPUT=$(cat)

# Extract tool name and file path
TOOL_NAME=$(echo "$HOOK_INPUT" | python -c "import sys, json; print(json.load(sys.stdin).get('tool_name', ''))" 2>/dev/null || echo "")
FILE_PATH=$(echo "$HOOK_INPUT" | python -c "import sys, json; d=json.load(sys.stdin); print(d.get('tool_input', {}).get('file_path', ''))" 2>/dev/null || echo "")

if [ -z "$FILE_PATH" ]; then
    exit 0
fi

WARNINGS=()

# Check 1: Console I/O in game logic
if echo "$FILE_PATH" | grep -qE "TestRPGGame/(Systems|Combat|Entities|Equipment|Abilities)"; then
    if grep -qE "Console\.(WriteLine|ReadLine|Write|Read)" "$FILE_PATH" 2>/dev/null; then
        WARNINGS+=("⚠️  Console I/O detected in game logic: $FILE_PATH")
        WARNINGS+=("   Game logic should use IGameInterface, not Console directly")
    fi
fi

# Check 2: JSON syntax (if JSON file)
if echo "$FILE_PATH" | grep -q "\.json$"; then
    if ! python -m json.tool "$FILE_PATH" > /dev/null 2>&1; then
        WARNINGS+=("⚠️  Invalid JSON syntax: $FILE_PATH")
    fi
fi

# Output warnings if any
if [ ${#WARNINGS[@]} -gt 0 ]; then
    echo ""
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "⚠️  ARCHITECTURE WARNINGS"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    for WARNING in "${WARNINGS[@]}"; do
        echo "$WARNING"
    done
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo ""
fi

# Always exit 0 (warnings, never block)
exit 0
```

### 3. Session Start Check

**File:** `scripts/hooks/session-start-check.sh`

```bash
#!/bin/bash
# Session Start Check - Verify environment is ready

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "🚀 SESSION START - Environment Check"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# Check .NET SDK
if command -v dotnet &> /dev/null; then
    echo "✅ .NET SDK available: $(dotnet --version)"
else
    echo "⚠️  .NET SDK not found"
fi

# Check git status
if git rev-parse --git-dir > /dev/null 2>&1; then
    echo "✅ Git repository"

    # Check for uncommitted changes
    if ! git diff-index --quiet HEAD -- 2>/dev/null; then
        CHANGED=$(git diff --name-only | wc -l)
        echo "ℹ️  $CHANGED uncommitted file(s)"
    else
        echo "✅ Working directory clean"
    fi
fi

# Quick build check (with timeout)
echo ""
echo "🔨 Running baseline build check..."
if timeout 30s dotnet build --no-restore --verbosity quiet > /dev/null 2>&1; then
    echo "✅ Build passing"
else
    echo "⚠️  Build may have issues (or timed out)"
fi

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✅ Session ready"
echo ""

exit 0
```

### 4. Stop Reminder (Passive)

**File:** `scripts/hooks/stop-reminder.sh`

```bash
#!/bin/bash
# Stop Reminder - Show next steps (NO automation!)

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✅ Task complete! Next steps:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "1. Review changes:    git status && git diff"
echo "2. Run quality gate:  bash scripts/hooks/pre-commit-gate.sh"
echo "3. Commit:            bash scripts/hooks/smart-commit.sh \"type: message\""
echo "4. Run tests:         dotnet test"
echo ""
echo "💡 Tip: Use pre-commit-gate.sh before every commit"
echo ""

exit 0
```

---

## Migration Plan

### Step 1: Backup Current Setup ✅
```bash
# Already done - you renamed to old.settings.json
cp .claude/old.settings.json .claude/backup.settings.json
```

### Step 2: Create New Hooks
```bash
# Create new scripts with corrected implementations
mv scripts/hooks/enrich-context.sh scripts/hooks/old.enrich-context.sh
# Create new files using code above
```

### Step 3: Update Configuration
```bash
# Copy new configs
# .claude/settings.json - Project-level (commit this)
# .claude/settings.local.json - Local only (don't commit)
```

### Step 4: Test Incrementally
```bash
# Test context enrichment
echo '{"prompt": "add new ability"}' | bash scripts/hooks/context-enrichment.sh

# Test architecture guard
echo '{"tool_input": {"file_path": "TestRPGGame/GameCore.cs"}}' | bash scripts/hooks/architecture-guard.sh

# Test session start
bash scripts/hooks/session-start-check.sh
```

### Step 5: Enable Gradually
1. Start with only SessionStart hook (low risk)
2. Add UserPromptSubmit hook (context enrichment)
3. Add PostToolUse hook (architecture guard)
4. Add Stop hook last (reminder only)

---

## Testing Your Hooks

### Test UserPromptSubmit
```bash
# Simulate hook input
cat > /tmp/test-input.json << 'EOF'
{
  "session_id": "test",
  "prompt": "add new ability to warrior",
  "cwd": "/path/to/project"
}
EOF

cat /tmp/test-input.json | bash scripts/hooks/context-enrichment.sh
```

**Expected:** Shows relevant documentation

### Test PostToolUse
```bash
# Simulate editing a file with Console I/O
cat > /tmp/test-input.json << 'EOF'
{
  "tool_name": "Edit",
  "tool_input": {
    "file_path": "TestRPGGame/Systems/SaveManager.cs"
  }
}
EOF

cat /tmp/test-input.json | bash scripts/hooks/architecture-guard.sh
```

**Expected:** Warns if Console I/O found, silent otherwise

### Test SessionStart
```bash
bash scripts/hooks/session-start-check.sh
```

**Expected:** Shows environment status

---

## Answers to Your Specific Questions

### Q: Can we use `$CLAUDE_PROJECT_DIR`?
**A: YES**, according to the official docs it's available to all hooks when Claude Code spawns them.

**However:**
- ✅ Use it in hook scripts run by Claude Code
- ❌ Won't be set when you test scripts manually
- 💡 Fallback: `$(git rev-parse --show-toplevel)`

**Recommended pattern:**
```bash
PROJECT_ROOT="${CLAUDE_PROJECT_DIR:-$(git rev-parse --show-toplevel)}"
```

This uses `$CLAUDE_PROJECT_DIR` if set, otherwise falls back to git.

### Q: How to prevent "cheating" (skipping tests, guidelines)?
**A: Use validation hooks (PostToolUse) + manual quality gates**

**DON'T:**
- ❌ Auto-commit (Claude might commit broken code)
- ❌ Block edits with slow checks (breaks workflow)
- ❌ Run tests after every edit (too slow)

**DO:**
- ✅ Warn about violations immediately (PostToolUse)
- ✅ Require manual quality gate before commit
- ✅ Use git pre-commit hooks to enforce rules
- ✅ Educate Claude with context enrichment

### Q: How to ensure tests are written?
**A: Multi-layered approach**

1. **During development** (PostToolUse):
   - Warn if editing production code without corresponding test file
   - Don't block, just remind

2. **Before commit** (Manual pre-commit-gate.sh):
   - Check test coverage
   - Warn if coverage decreased
   - Show which files lack tests

3. **On commit** (Git pre-commit hook):
   - Analyze diff
   - If new public methods added, require tests
   - Block commit if no tests for new code

4. **Context enrichment**:
   - When user says "add feature", show testing guidelines
   - Remind about TDD approach

### Q: How to ensure architectural guidelines are followed?
**A: Validation hooks + prevention + education**

1. **PostToolUse hook** (architecture-guard.sh):
   - Check for Console I/O in game logic
   - Check for interface violations
   - Warn immediately

2. **UserPromptSubmit hook**:
   - Surface ARCHITECTURE.md when relevant
   - Claude sees guidelines before writing code

3. **Git pre-commit hook**:
   - Block commits with violations
   - Run full architecture validation

4. **SessionStart hook**:
   - Remind Claude about key principles
   - Show recent violations (if any)

### Q: What's the best autonomous workflow?
**A: Semi-autonomous with approval gates**

```
┌─────────────────────────────────────────┐
│ USER: "Add warrior ability"             │
└────────────┬────────────────────────────┘
             ↓
   [UserPromptSubmit Hook]
   - Load ARCHITECTURE.md
   - Load MULTIPLE_EFFECTS_GUIDE.md
             ↓
┌─────────────────────────────────────────┐
│ CLAUDE: Edits files autonomously        │
└────────────┬────────────────────────────┘
             ↓
   [PostToolUse Hook - after each edit]
   - Validate architecture
   - Warn if issues
   - Never block
             ↓
┌─────────────────────────────────────────┐
│ CLAUDE: "Done! Made X changes"          │
└────────────┬────────────────────────────┘
             ↓
   [Stop Hook]
   - Show reminder about next steps
   - DO NOT auto-commit
             ↓
┌─────────────────────────────────────────┐
│ USER: Reviews changes                   │
│  git diff                               │
│  dotnet test                            │
└────────────┬────────────────────────────┘
             ↓
┌─────────────────────────────────────────┐
│ USER: Runs quality gate                 │
│  bash scripts/hooks/pre-commit-gate.sh  │
└────────────┬────────────────────────────┘
             ↓
        ✅ Passes?
             ↓ No → Fix issues → Repeat
             ↓ Yes
┌─────────────────────────────────────────┐
│ USER: Commits                           │
│  bash scripts/hooks/smart-commit.sh     │
│  "feat: add warrior ability"            │
└────────────┬────────────────────────────┘
             ↓
   [Git pre-commit hook]
   - Final validation
   - Block if violations
             ↓
┌─────────────────────────────────────────┐
│ COMMITTED ✅                             │
└─────────────────────────────────────────┘
```

**Key Principles:**
1. ✅ Claude autonomously writes code
2. ✅ Hooks provide real-time feedback
3. ✅ User reviews before committing
4. ✅ Quality gate prevents bad commits
5. ❌ No automatic commits

---

## Advanced: Prompt-Based Hooks (Future)

The documentation mentions **prompt-based hooks** that use an LLM instead of bash commands.

**Example: LLM-Based Pre-Commit Review**

```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Bash(git commit:*)",
        "hooks": [
          {
            "type": "prompt",
            "prompt": "Analyze the git diff and check for:\n1. Console I/O in game logic\n2. Missing tests for new features\n3. Architecture violations\n4. Hardcoded data (should be in JSON)\n\nRespond with:\n{\"decision\": \"approve\"|\"block\", \"reason\": \"...\", \"systemMessage\": \"...\"}"
          }
        ]
      }
    ]
  }
}
```

**This could enable:**
- Intelligent code review before commits
- Context-aware validation
- Natural language explanations

**Note:** Currently only supported for UserPromptSubmit, Stop, SubagentStop, and PreToolUse according to docs.

---

## Summary: What To Do Now

### Immediate Actions

1. **✅ Use the corrected configurations above**
2. **✅ Create the new script implementations**
3. **✅ Test each hook individually**
4. **✅ Enable hooks gradually**

### Hooks to Enable

#### In `.claude/settings.json` (Project)
- ✅ UserPromptSubmit → Context enrichment
- ✅ PostToolUse → Architecture guard (warnings only)
- ✅ SessionStart → Environment check

#### In `.claude/settings.local.json` (Local)
- ✅ Stop → Reminder (NO automation!)

### Hooks to Remove
- ❌ PreToolUse build check (too slow)
- ❌ Stop → auto-add-tests.sh (wrong time)
- ❌ Stop → autonomous-finish-work.sh (too aggressive!)

### Manual Scripts to Use
- ✅ `pre-commit-gate.sh` - Run before every commit
- ✅ `smart-commit.sh` - Helper for commits
- ✅ `doc-sync.sh` - Keep docs updated

### Git Hooks (Optional)
- ✅ `.git/hooks/pre-commit` - Final validation gate

---

## Why This Approach Works

### Problem with Original Approach
- Tried to automate the **entire workflow**
- Hooks fired at **wrong times** (during conversation, not at end)
- Auto-commits without review = dangerous
- Build checks before every edit = slow + breaks multi-file changes

### Why New Approach is Better
- Hooks provide **guard rails**, not automation
- User stays **in control** of commits
- **Fast** validation (warnings, not blocking builds)
- **Context enrichment** helps Claude write better code upfront
- **Manual quality gates** prevent bad commits
- **Clear separation**: Hooks = real-time feedback, Scripts = commit-time validation

### Expected Results
- ✅ Claude writes better code (has architecture context)
- ✅ You catch violations immediately (PostToolUse warnings)
- ✅ No surprise commits (you control when to commit)
- ✅ Fast workflow (no slow build checks between edits)
- ✅ High quality (pre-commit gate catches issues)
- ✅ Claude Code doesn't go "haywire" (no aggressive automation)

---

## Conclusion

Your original intuition was correct - hooks are powerful for ensuring quality. The issue was:
1. **Timing:** Running automation in Stop hook (mid-conversation)
2. **Aggressiveness:** Auto-committing without user review
3. **Performance:** Build checks before every edit

The corrected approach:
1. ✅ Uses hooks for **passive monitoring** (warnings, context)
2. ✅ Uses **manual scripts** for **active enforcement** (quality gates)
3. ✅ Keeps user **in control** of commits
4. ✅ Provides **real-time feedback** without blocking workflow

**You'll get autonomous coding with quality guarantees, without the "haywire" behavior.**

---

Ready to implement? Let's test the new hooks incrementally!
