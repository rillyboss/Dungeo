# Hooks Setup Guide - Custom Agentic Workflow

This guide explains how to set up and use the custom hooks system for TestRPGGame.

---

## Overview

Four intelligent hooks have been created for your workflow:

1. **Context Enrichment** - Surfaces relevant docs when starting work
2. **Finish Work Verification** - Ensures quality before committing
3. **Git Automation** - Handles staging and committing
4. **Documentation Updates** - Prompts for doc updates

---

## Quick Setup (5 Minutes)

### Step 1: Verify Scripts Exist
```bash
ls -la scripts/hooks/
```

You should see:
- `enrich-context.sh` - Context enrichment
- `verify-finish-work.sh` - Quality checks
- `handle-git.sh` - Git automation
- `update-docs.sh` - Doc updates
- `post-edit-validation.sh` - Post-edit checks
- `finish-work-workflow.sh` - Complete workflow orchestrator

### Step 2: Test Scripts Manually
```bash
# Test context enrichment
bash scripts/hooks/enrich-context.sh "add new ability"

# Test verification (safe - read-only)
bash scripts/hooks/verify-finish-work.sh

# Test git handler (dry run)
bash scripts/hooks/handle-git.sh
```

### Step 3: Configure Claude Code Hooks

**Option A: Project-Level (Recommended)**

The hooks configuration is already in `.claude/hooks-config.json`. Claude Code should automatically detect and use it.

**Option B: Global Configuration**

Add to `~/.claude-code/config.json`:
```json
{
  "hooks": {
    "userPromptSubmit": {
      "command": "bash ./scripts/hooks/enrich-context.sh \"{{prompt}}\"",
      "description": "Enrich context with relevant documentation",
      "enabled": true,
      "frequency": "always",
      "continueOnFailure": true
    },

    "preToolCall": {
      "command": "dotnet build --no-restore --verbosity quiet",
      "description": "Quick build check before modifications",
      "enabled": true,
      "toolPatterns": ["Edit", "Write"],
      "continueOnFailure": false
    },

    "postToolCall": {
      "command": "bash ./scripts/hooks/post-edit-validation.sh \"{{filePath}}\"",
      "description": "Validate changes after editing",
      "enabled": true,
      "toolPatterns": ["Edit", "Write"],
      "continueOnFailure": true
    }
  }
}
```

---

## Hook Details

### 1. Context Enrichment Hook

**Trigger:** When you submit a prompt to Claude Code
**Purpose:** Automatically surfaces relevant documentation

**How it works:**
1. Analyzes your prompt for keywords
2. Identifies relevant documentation files
3. Displays first 50 lines of each relevant file
4. AI assistant uses this context during the session

**Example:**
```bash
You: "add new ability to warrior"
```

Hook automatically shows:
- `MULTIPLE_EFFECTS_GUIDE.md` (how to add abilities)
- `DATA_DRIVEN_SYSTEM.md` (where to edit JSON)

**Keyword Mappings:**
- "architecture/design/refactor" → ARCHITECTURE.md
- "ability/effect/skill" → MULTIPLE_EFFECTS_GUIDE.md
- "json/data/enemy/dungeon" → DATA_DRIVEN_SYSTEM.md
- "test/testing" → SESSION_GUIDE.md
- "bug/fix/error" → DEVELOPMENT_LOG.md

### 2. Finish Work Verification Hook

**Trigger:** Manual - run when finishing work
**Purpose:** Comprehensive quality checks

**Checks performed:**
1. ✅ Build succeeds
2. ✅ All tests passing (375/375)
3. ✅ Test coverage for new code (reminder)
4. ✅ No Console I/O in game logic
5. ✅ Valid JSON syntax

**Usage:**
```bash
# Run before committing
bash scripts/hooks/verify-finish-work.sh
```

**Output:**
```
🔍 Verifying work is ready to finish...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ Build successful
✅ All tests passing (375/375)
✅ No architecture violations detected
✅ ALL CHECKS PASSED - Ready to finish!
```

### 3. Git Automation Hook

**Trigger:** Manual - run when ready to commit
**Purpose:** Automates git workflow

**Features:**
1. Shows detailed change summary
2. Stages all changes (`git add -A`)
3. Analyzes changes to suggest commit type
4. Provides commit message template
5. Auto-commits if message provided

**Usage:**

**Interactive mode:**
```bash
bash scripts/hooks/handle-git.sh
```

**Auto-commit mode:**
```bash
bash scripts/hooks/handle-git.sh --auto-commit "feat: add new warrior ability"
```

**Commit type detection:**
- Detects `feat`, `fix`, `refactor`, `test`, `docs`, `chore`
- Suggests appropriate type based on files changed
- Automatically adds Claude Code signature

### 4. Documentation Update Hook

**Trigger:** Manual - run after committing
**Purpose:** Ensures documentation stays current

**Features:**
1. Analyzes committed changes
2. Identifies which docs need updates
3. Generates update templates
4. Auto-updates ACTIVE_CONTEXT.md timestamp

**Usage:**
```bash
bash scripts/hooks/update-docs.sh
```

**Recommendations based on changes:**
- Changed `Interfaces/` → Update ARCHITECTURE.md
- Changed `Data/*.json` → Update CONTENT_EXPANSION_ROADMAP.md
- Changed `*.cs` → Update DEVELOPMENT_LOG.md
- New feature detected → Update DEVELOPMENT_LOG.md

---

## Complete Workflow

### Option 1: Manual Step-by-Step

```bash
# 1. Make your changes with AI assistance
# (Context enrichment hook runs automatically)

# 2. Verify work is complete
bash scripts/hooks/verify-finish-work.sh

# 3. Handle git
bash scripts/hooks/handle-git.sh --auto-commit "feat: your message"

# 4. Update documentation
bash scripts/hooks/update-docs.sh
```

### Option 2: Orchestrated Workflow (Recommended)

```bash
# Single command to handle everything
bash scripts/hooks/finish-work-workflow.sh "feat: add new warrior ability"
```

This runs all steps in sequence:
1. ✅ Verification
2. ✅ Git handling
3. ✅ Documentation updates
4. ✅ Final summary

**Interactive variant (prompts for commit):**
```bash
bash scripts/hooks/finish-work-workflow.sh
```

---

## Real-World Examples

### Example 1: Adding New Ability

**Start work:**
```
You: "I want to add a new ability called Whirlwind to the warrior"
```

**What happens:**
- Context enrichment hook runs automatically
- Shows `MULTIPLE_EFFECTS_GUIDE.md` and `DATA_DRIVEN_SYSTEM.md`
- AI has full context on how to add abilities

**After making changes:**
```bash
# Finish work
bash scripts/hooks/finish-work-workflow.sh "feat: add Whirlwind ability to warrior"
```

**Output:**
1. ✅ Verifies build and tests pass
2. ✅ Validates JSON changes
3. ✅ Stages and commits changes
4. ✅ Prompts for documentation update
5. ✅ Shows summary with commit hash

### Example 2: Fixing Bug

**Start work:**
```
You: "fix the bug where poison damage doesn't apply"
```

**What happens:**
- Shows `DEVELOPMENT_LOG.md` (historical context)
- Shows `ARCHITECTURE.md` (system overview)

**After fixing:**
```bash
bash scripts/hooks/finish-work-workflow.sh "fix: poison damage now applies correctly"
```

### Example 3: Refactoring

**Start work:**
```
You: "refactor the combat system to use strategy pattern"
```

**What happens:**
- Shows `ARCHITECTURE.md` (design patterns)
- Shows `FUTURE_REFACTORS.md` (planned improvements)

**After refactoring:**
```bash
# More thorough check since refactoring is risky
bash scripts/hooks/verify-finish-work.sh

# Review changes carefully
git diff

# Commit
bash scripts/hooks/handle-git.sh --auto-commit "refactor: combat system uses strategy pattern"

# Update architecture docs
bash scripts/hooks/update-docs.sh
```

---

## Customization

### Adjust Context Enrichment Keywords

Edit `scripts/hooks/enrich-context.sh`:

```bash
# Add new keyword mapping
if echo "$PROMPT_LOWER" | grep -qE "your|keywords|here"; then
    DOCS_TO_SURFACE+=("$CONTEXT_DIR/YOUR_DOC.md")
fi
```

### Change Verification Checks

Edit `scripts/hooks/verify-finish-work.sh`:

```bash
# Add custom check
echo "🔍 Step X: Your custom check..."
if your_condition; then
    echo "✅ Check passed"
else
    echo "❌ Check failed"
    EXIT_CODE=1
fi
```

### Modify Git Commit Format

Edit `scripts/hooks/handle-git.sh`:

```bash
# Change commit message template
FULL_MESSAGE="$COMMIT_MESSAGE

Your custom footer here"
```

---

## Troubleshooting

### Hook Not Running

**Check if scripts are executable:**
```bash
ls -l scripts/hooks/*.sh
# Should show -rwxr-xr-x permissions
```

**Make executable if needed:**
```bash
chmod +x scripts/hooks/*.sh
```

**Test manually:**
```bash
bash scripts/hooks/enrich-context.sh "test prompt"
```

### Python JSON Validation Fails

**Install Python (if missing):**
```bash
python --version
# or
python3 --version
```

**Alternative: Use jq instead:**

Edit validation scripts to replace:
```bash
python -m json.tool "$FILE"
```

With:
```bash
jq empty "$FILE"
```

### Build Check Too Slow

**Disable pre-tool-call hook temporarily:**

Edit `.claude/hooks-config.json`:
```json
{
  "hooks": {
    "preToolCall": {
      "enabled": false
    }
  }
}
```

### False Positives (Console I/O Check)

**Exclude specific files:**

Edit `scripts/hooks/post-edit-validation.sh`:
```bash
# Add exclusion
if [[ "$FILE" =~ TestRPGGame/(Systems|Combat|Entities) ]] && [[ "$FILE" != *"YourException.cs" ]]; then
    # Check for Console I/O
fi
```

---

## Performance Impact

| Hook | Typical Duration | When It Runs |
|------|-----------------|--------------|
| Context Enrichment | <1s | On prompt submit |
| Pre-Tool-Call Build | 1-2s | Before each Edit/Write |
| Post-Tool-Call Validation | <0.5s | After each Edit/Write |
| Finish Work Verification | 3-5s | Manual (when finishing) |
| Git Automation | <1s | Manual (when committing) |
| Documentation Update | <1s | Manual (after commit) |

**Total overhead during active coding:** ~2-3 seconds per edit

---

## Advanced Usage

### Disable Hooks Temporarily

```bash
# Run Claude Code without hooks
claude-code --no-hooks

# Or set environment variable
export CLAUDE_CODE_HOOKS_DISABLED=1
```

### Run Hooks on Existing Commits

```bash
# Check last commit
bash scripts/hooks/verify-finish-work.sh

# Generate doc update for last commit
bash scripts/hooks/update-docs.sh --auto
```

### Batch Testing Hook Scripts

```bash
# Test all hooks
for hook in scripts/hooks/*.sh; do
    echo "Testing $hook..."
    bash "$hook" "test" || echo "Failed: $hook"
done
```

---

## Best Practices

### ✅ Do This

1. **Let context enrichment run automatically** - It's fast and helpful
2. **Run verification before finishing** - Catches issues early
3. **Use orchestrated workflow** - `finish-work-workflow.sh` does everything
4. **Review documentation prompts** - Keep docs current
5. **Commit frequently** - Small, atomic commits

### ❌ Avoid This

1. **Don't skip verification** - Tests might fail later
2. **Don't ignore doc updates** - Context degrades over time
3. **Don't disable all hooks** - They save more time than they cost
4. **Don't commit without running hooks** - Quality might suffer

---

## Next Steps

1. ✅ **Test hooks manually** - Verify they work
2. ✅ **Try context enrichment** - Submit a prompt and see results
3. ✅ **Run finish workflow** - Try the complete workflow
4. ✅ **Adjust as needed** - Customize for your preferences
5. ✅ **Monitor performance** - Disable if too slow

---

## Summary

Your workflow now has intelligent automation:

1. **Starting work:** Context automatically enriched ✨
2. **During work:** Build checks before edits ✅
3. **Finishing work:** Complete quality verification 🔍
4. **Committing:** Automated git workflow 📦
5. **After commit:** Documentation update prompts 📚

**Result:** Higher quality code, better documentation, less manual work.

---

**Questions?** Check `AgenticContext/HOOKS_RECOMMENDATIONS.md` for detailed technical documentation.

**Ready to start?** Just use Claude Code as normal - hooks will run automatically!
