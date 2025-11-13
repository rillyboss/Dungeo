# Hooks v3 - Simplified Context Enrichment

> Based on Reddit best practices: Keep it simple, use cat, always enrich.

---

## 🎯 What Changed

### Old Approach (v2) ❌
- Complex bash scripts with JSON parsing
- Conditional logic with grep patterns
- Dumped 50 lines from multiple docs
- Overwhelming context injection
- Fragile and hard to maintain

### New Approach (v3) ✅
- **Single command:** `cat .claude/context-enrichment.md`
- **Universal enrichment:** Loads on EVERY command
- **Short & focused:** ~160 lines of critical context
- **No bash scripts:** Direct cat command in settings.json
- **Always helpful:** Essential rules, quick refs, what NOT to do

---

## 📁 File Structure

```
.claude/
├── settings.json                   # Hooks configuration
├── settings.local.json             # Permissions
└── context-enrichment.md           # The enrichment content (cat'd on every command)
```

### No more scripts/hooks/ directory! 🎉

---

## ⚙️ Configuration

**`.claude/settings.json`:**
```json
{
  "hooks": {
    "user-prompt-submit": {
      "command": "cat .claude/context-enrichment.md",
      "description": "Enriches every command with essential context, testing rules, and quick references"
    }
  }
}
```

That's it! Just cat the markdown file.

---

## 📝 What Gets Enriched

**`.claude/context-enrichment.md` contains:**

1. **Testing Rules** - When to test, how to test, expectations
2. **Version Control Rules** - Commit format, when to commit, git workflow
3. **Quick Reference** - Where to find things (files, patterns, code)
4. **Critical Rules** - What NOT to do (assumptions, unnecessary files, etc.)
5. **Common Workflows** - Adding abilities, enemies, fixing bugs
6. **Success Checklist** - Before considering work "done"
7. **Current Game State** - Quick facts (classes, abilities, tests)

**Total:** ~160 lines of focused, actionable context

---

## 🧠 Design Philosophy

### Always Enrich (Universal Context)
Instead of conditionally loading docs based on keywords, we provide a **short, universal context** that's relevant for nearly all commands.

**Benefits:**
- ✅ Always know the testing rules
- ✅ Always know version control expectations
- ✅ Always have quick file references
- ✅ Always reminded of critical "don'ts"
- ✅ Consistent experience across all commands

### Keep It Short
- **~160 lines** vs old approach (200+ lines per doc × multiple docs)
- Quick to read, easy to scan
- Focused on rules and references, not full documentation

### Use Cat, Not Scripts
- No bash complexity
- No JSON parsing
- No conditional logic
- Just `cat` the file
- Always exits 0 (never blocks)

---

## 🔄 How It Works

1. **User submits a prompt** → Claude Code triggers `user-prompt-submit` hook
2. **Hook runs:** `cat .claude/context-enrichment.md`
3. **Output injected** into Claude's context automatically
4. **Claude sees:** Your prompt + enrichment context
5. **Result:** Claude follows testing rules, commit guidelines, knows where files are

---

## 🎛️ Customization

### Want to add more context?
Just edit `.claude/context-enrichment.md`

**Guidelines:**
- Keep total under 300 lines
- Focus on rules, expectations, quick refs
- Don't duplicate CLAUDE.md content
- State what NOT to do clearly
- Include file paths for quick navigation

### Want conditional enrichment?
Use **matchers** in settings.json (future enhancement):
```json
{
  "hooks": {
    "user-prompt-submit": [
      {
        "matcher": "test|testing",
        "command": "cat .claude/testing-guide.md"
      },
      {
        "matcher": ".*",
        "command": "cat .claude/context-enrichment.md"
      }
    ]
  }
}
```

---

## ✅ Benefits Over v2

| Aspect | v2 (Old) | v3 (New) |
|--------|----------|----------|
| **Complexity** | 121 lines of bash | 1 line: `cat file.md` |
| **Context Size** | 50 lines × multiple docs | ~160 lines total |
| **Conditional Logic** | Bash grep patterns | None (universal) |
| **Maintenance** | Complex, fragile | Simple, robust |
| **Execution** | JSON parsing, head, grep | Just cat |
| **Failure Mode** | Could break on bad JSON | Can't fail (cat always works) |

---

## 🧪 Testing

All 375 tests pass:
```bash
dotnet test
# Passed!  - Failed: 0, Passed: 375
```

Hook executes on every command and provides consistent context enrichment.

---

## 📖 See Also

- **CLAUDE.md** - Full project documentation
- **.claude/context-enrichment.md** - The enrichment content
- **.claude/settings.json** - Hooks configuration

---

## 💡 Pro Tips

1. **Keep enrichment focused** - Rules, expectations, quick refs only
2. **Update when patterns change** - New workflows? Add to Common Workflows
3. **State what NOT to do** - Claude needs clear boundaries
4. **Include file paths** - Help Claude navigate quickly
5. **Keep it under 300 lines** - Longer = less effective

---

**Result:** Simple, effective, universal context enrichment that works every time. 🎉
