---
name: documenting-code
description: Generate comprehensive, professional documentation for codebases including API references, architecture overviews, inline documentation, and developer guides. Use when Claude needs to document code, create README files, generate API documentation, write docstrings, explain code architecture, or create developer onboarding materials for any programming language or framework.
---

# Code Documenter

Generate high-quality technical documentation for code projects.

## Core Approach

1. **Analyze before documenting** - Read and understand code structure, dependencies, and patterns
2. **Match the language's conventions** - Use appropriate docstring formats (JSDoc, Python docstrings, Javadoc, etc.)
3. **Focus on the "why"** - Code shows "what", documentation explains "why" and "how to use"
4. **Progressive detail** - High-level overview first, then detailed references
5. **Include examples** - Concrete usage examples are more valuable than abstract descriptions

## Documentation Types

### README Files

Create comprehensive README.md files:

**Structure:**
- Project title and brief description (1-2 sentences)
- Key features/capabilities (bullet points)
- Installation/setup instructions
- Quick start example
- Configuration options
- API/Usage documentation (or link to detailed docs)
- Development setup (if applicable)
- Contributing guidelines (if applicable)
- License

**Tone:** Clear, concise, welcoming to new users

### API Documentation

For libraries, modules, or services:

**Include:**
- Purpose and scope of the API
- Authentication/initialization requirements
- Available endpoints/functions with:
  - Parameters (types, required/optional, defaults)
  - Return values (types, possible values)
  - Error conditions and handling
  - Usage examples for each major function
- Rate limits or usage constraints
- Version compatibility notes

### Inline Documentation

Add docstrings/comments to existing code:

**Python (Google/NumPy style):**
```python
def process_data(input_data: list[dict], threshold: float = 0.5) -> dict:
    """Process input data and filter by threshold.
    
    Args:
        input_data: List of data dictionaries with 'value' and 'label' keys
        threshold: Minimum value to include in results (default: 0.5)
    
    Returns:
        Dictionary with 'filtered' list and 'count' of filtered items
    
    Raises:
        ValueError: If input_data is empty or malformed
    
    Example:
        >>> data = [{'value': 0.7, 'label': 'A'}, {'value': 0.3, 'label': 'B'}]
        >>> result = process_data(data)
        >>> result['count']
        1
    """
```

**JavaScript (JSDoc):**
```javascript
/**
 * Fetches user data from the API with retry logic
 * 
 * @param {string} userId - The unique identifier for the user
 * @param {Object} options - Configuration options
 * @param {number} [options.retries=3] - Number of retry attempts
 * @param {number} [options.timeout=5000] - Request timeout in milliseconds
 * @returns {Promise<User>} Promise resolving to user object
 * @throws {NetworkError} When all retry attempts fail
 * 
 * @example
 * const user = await fetchUser('user-123', { retries: 5 });
 * console.log(user.name);
 */
```

**Java (Javadoc):**
```java
/**
 * Validates and processes payment transactions.
 * 
 * <p>This method performs validation checks on the payment amount,
 * currency, and merchant information before processing the transaction
 * through the payment gateway.
 * 
 * @param payment the payment transaction to process
 * @param merchantId the unique identifier for the merchant
 * @return a PaymentResult containing transaction status and reference
 * @throws InvalidPaymentException if payment validation fails
 * @throws PaymentGatewayException if the gateway is unavailable
 * @since 2.0
 */
```

### Architecture Documentation

Explain system design and structure:

**Include:**
- High-level architecture diagram (describe or create)
- Component responsibilities and interactions
- Data flow through the system
- Key design decisions and rationale
- Technology stack choices
- Scalability considerations
- Security considerations
- Known limitations or technical debt

**Format:** Markdown with diagrams (Mermaid, ASCII art, or descriptions)

### Developer Guides

Help developers understand and contribute:

**Topics:**
- Development environment setup
- Code organization and structure
- Coding standards and conventions
- Testing approach and how to run tests
- Build and deployment process
- Debugging tips
- Common pitfalls and solutions
- Where to find additional resources

## Language-Specific Conventions

**Python:**
- Use Google or NumPy docstring format
- Type hints in function signatures
- Module-level docstrings
- Examples in doctest format

**JavaScript/TypeScript:**
- JSDoc comments for functions and classes
- TSDoc for TypeScript
- Examples in code comments
- README for package-level documentation

**Java:**
- Javadoc for all public methods and classes
- `@param`, `@return`, `@throws` tags
- Package documentation in package-info.java
- Version and author tags where appropriate

**Go:**
- Comments above declarations
- Package comment in doc.go
- Examples as test functions (Example*)
- Concise, imperative style

**Rust:**
- Triple-slash comments (`///`) for items
- Doc comments support Markdown
- Code examples in comments run as tests
- Module-level documentation with `//!`

## Documentation Quality Checklist

Before finalizing documentation, verify:

- [ ] **Accuracy** - All information is correct and up-to-date
- [ ] **Completeness** - All public APIs/features are documented
- [ ] **Clarity** - Technical terms are explained or linked
- [ ] **Examples** - Concrete usage examples are provided
- [ ] **Organization** - Logical flow from overview to details
- [ ] **Consistency** - Formatting and style are uniform
- [ ] **Searchability** - Key terms and concepts are easy to find
- [ ] **Maintainability** - Clear structure for future updates

## Common Documentation Patterns

### Pattern 1: Reference-First Documentation
For established libraries where users need quick API lookups.

**Structure:**
1. Brief project description
2. Installation/setup
3. Complete API reference (all functions/classes)
4. Configuration options
5. Advanced usage examples

### Pattern 2: Tutorial-First Documentation
For new tools or complex systems where users need guidance.

**Structure:**
1. What problem does this solve?
2. Quick start tutorial
3. Core concepts explained
4. Common use cases with examples
5. API reference (detailed)
6. Advanced topics

### Pattern 3: Architecture-First Documentation
For large systems or frameworks where understanding structure is critical.

**Structure:**
1. System overview and goals
2. Architecture diagram and components
3. How components interact
4. Getting started for developers
5. API reference per component
6. Contributing guide

## Tips for Effective Documentation

**Do:**
- Start with a clear purpose statement
- Use active voice and present tense
- Include runnable code examples
- Link to external resources
- Keep examples self-contained
- Update docs when code changes
- Consider your audience's expertise level

**Avoid:**
- Stating the obvious (code already shows "what")
- Using jargon without explanation
- Creating orphaned documentation
- Long paragraphs without structure
- Examples that don't run
- Outdated version information

## Additional Resources

For detailed documentation style guides by language:
- View `references/style-guides.md` for comprehensive language-specific conventions
- View `references/docstring-templates.md` for ready-to-use documentation templates


