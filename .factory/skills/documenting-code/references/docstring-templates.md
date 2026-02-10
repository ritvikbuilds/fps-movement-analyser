# Documentation Templates

Ready-to-use templates for common documentation patterns.

## README Template

```markdown
# Project Name

Brief one-sentence description of what this project does.

## Features

- Key feature 1
- Key feature 2
- Key feature 3

## Installation

```bash
# Installation command
npm install package-name
# or
pip install package-name
```

## Quick Start

```language
// Minimal working example
const example = require('package-name');
example.doSomething();
```

## Usage

### Basic Usage

Explain the most common use case with example.

### Advanced Usage

More complex scenarios with examples.

## Configuration

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| option1 | string | 'default' | What this option does |
| option2 | number | 100 | What this option controls |

## API Reference

Link to detailed API documentation or include it here.

## Development

```bash
# Clone the repository
git clone https://github.com/username/repo.git

# Install dependencies
npm install

# Run tests
npm test
```

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.
```

## API Documentation Template

```markdown
# API Reference

## Module: `module_name`

Brief description of what this module provides.

### Functions

#### `functionName(param1, param2, options)`

Brief description of what the function does.

**Parameters:**

- `param1` (string, required): Description of param1
- `param2` (number, optional): Description of param2. Default: 10
- `options` (object, optional): Configuration options
  - `option1` (boolean): What this option does. Default: false
  - `option2` (string): What this option does. Default: 'auto'

**Returns:**

- (Promise<Result>): Description of what gets returned

**Throws:**

- `ValidationError`: When validation fails
- `NetworkError`: When network request fails

**Example:**

```javascript
const result = await functionName('value', 20, {
  option1: true
});
console.log(result);
```

---

#### `anotherFunction(param)`

[Same structure as above]

### Classes

#### `ClassName`

Brief description of the class purpose.

**Constructor:**

```javascript
new ClassName(config)
```

**Parameters:**

- `config` (object): Configuration object
  - `property1` (string): Description
  - `property2` (number): Description

**Properties:**

- `property1` (string): Description of this property
- `property2` (number): Description of this property

**Methods:**

##### `methodName(param)`

Description of what the method does.

**Parameters:**

- `param` (type): Description

**Returns:**

- (type): Description

**Example:**

```javascript
const instance = new ClassName({ property1: 'value' });
const result = instance.methodName('param');
```
```

## Architecture Documentation Template

```markdown
# Architecture Overview

## System Overview

Brief description of the overall system and its purpose.

## High-Level Architecture

```
[ASCII diagram or description]

┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│   Client    │─────▶│   API       │─────▶│  Database   │
│             │      │   Gateway   │      │             │
└─────────────┘      └─────────────┘      └─────────────┘
```

## Components

### Component 1: [Name]

**Purpose:** What this component does

**Responsibilities:**
- Responsibility 1
- Responsibility 2
- Responsibility 3

**Technology:** Languages/frameworks used

**Interactions:**
- Receives data from: Component X
- Sends data to: Component Y
- Dependencies: Library A, Service B

### Component 2: [Name]

[Same structure]

## Data Flow

1. User initiates action in Component A
2. Component A validates and forwards to Component B
3. Component B processes and queries Component C
4. Results flow back through the chain

## Key Design Decisions

### Decision 1: [Title]

**Context:** What problem needed solving

**Decision:** What was chosen

**Rationale:** Why this approach

**Consequences:** Trade-offs and implications

### Decision 2: [Title]

[Same structure]

## Technology Stack

- **Frontend:** React, TypeScript
- **Backend:** Node.js, Express
- **Database:** PostgreSQL
- **Caching:** Redis
- **Infrastructure:** Docker, Kubernetes

## Security Considerations

- Authentication approach
- Authorization model
- Data encryption
- Security measures

## Performance Considerations

- Scalability approach
- Caching strategy
- Optimization techniques
- Performance targets

## Known Limitations

- Limitation 1 and workaround
- Limitation 2 and future plans
```

## Developer Guide Template

```markdown
# Developer Guide

## Getting Started

### Prerequisites

- Requirement 1 (version X.Y)
- Requirement 2 (version X.Y)
- Requirement 3

### Environment Setup

1. Clone the repository:
```bash
git clone https://github.com/org/repo.git
cd repo
```

2. Install dependencies:
```bash
npm install
# or
pip install -r requirements.txt
```

3. Configure environment:
```bash
cp .env.example .env
# Edit .env with your settings
```

4. Initialize database:
```bash
npm run db:migrate
```

## Project Structure

```
project/
├── src/                  # Source code
│   ├── components/       # UI components
│   ├── services/         # Business logic
│   ├── utils/            # Helper functions
│   └── index.js          # Entry point
├── tests/                # Test files
├── docs/                 # Documentation
└── package.json          # Dependencies
```

## Development Workflow

### Running Locally

```bash
npm run dev  # Start development server
```

Visit http://localhost:3000

### Running Tests

```bash
npm test              # Run all tests
npm run test:watch    # Watch mode
npm run test:coverage # With coverage
```

### Code Style

We use [Tool] for linting and formatting:

```bash
npm run lint          # Check for issues
npm run format        # Auto-format code
```

**Conventions:**
- Use camelCase for variables and functions
- Use PascalCase for classes and components
- Maximum line length: 80 characters
- Always use semicolons

### Making Changes

1. Create a feature branch:
```bash
git checkout -b feature/your-feature-name
```

2. Make your changes and commit:
```bash
git add .
git commit -m "feat: add new feature"
```

We follow [Conventional Commits](https://www.conventionalcommits.org/).

3. Push and create PR:
```bash
git push origin feature/your-feature-name
```

## Building and Deployment

### Build for Production

```bash
npm run build
```

Output will be in `dist/` directory.

### Deployment

[Deployment instructions specific to your setup]

## Testing Guidelines

### Unit Tests

- Test individual functions and components
- Mock external dependencies
- Aim for >80% code coverage

Example:
```javascript
describe('functionName', () => {
  it('should handle valid input', () => {
    expect(functionName('input')).toBe('expected');
  });
  
  it('should throw on invalid input', () => {
    expect(() => functionName(null)).toThrow();
  });
});
```

### Integration Tests

- Test component interactions
- Use test database
- Clean up after each test

## Troubleshooting

### Common Issues

**Issue:** Error message here

**Solution:** How to fix it

**Issue:** Another error message

**Solution:** How to fix it

## Additional Resources

- [Link to API docs]
- [Link to style guide]
- [Link to architecture docs]
- [Link to deployment docs]

## Getting Help

- Check [Issues](link) for known problems
- Ask in [Slack channel / Discord]
- Contact: dev-team@example.com
```

## Inline Documentation Templates

### Python Function Template

```python
def function_name(param1: Type1, param2: Type2 = default) -> ReturnType:
    """Brief one-line description.
    
    More detailed explanation if needed. Can be multiple lines.
    Explain the purpose, algorithm, or any important context.
    
    Args:
        param1: Description of param1
        param2: Description of param2 (default: default_value)
    
    Returns:
        Description of what gets returned
    
    Raises:
        ErrorType1: When this error occurs
        ErrorType2: When this error occurs
    
    Example:
        >>> result = function_name("input", 42)
        >>> print(result)
        expected_output
    
    Note:
        Any important notes or warnings
    """
```

### JavaScript Function Template

```javascript
/**
 * Brief one-line description
 * 
 * More detailed explanation if needed. Can be multiple lines.
 * Explain the purpose, algorithm, or any important context.
 * 
 * @param {Type1} param1 - Description of param1
 * @param {Type2} [param2=default] - Description of param2
 * @returns {ReturnType} Description of what gets returned
 * @throws {ErrorType1} When this error occurs
 * @throws {ErrorType2} When this error occurs
 * 
 * @example
 * // Example usage
 * const result = functionName("input", 42);
 * console.log(result);  // expected_output
 * 
 * @example
 * // Another example
 * const result = functionName("other");
 */
function functionName(param1, param2 = default) {
    // implementation
}
```

### Java Method Template

```java
/**
 * Brief one-line description.
 * 
 * <p>More detailed explanation if needed. Can be multiple paragraphs.
 * Explain the purpose, algorithm, or any important context.
 * 
 * <p><b>Thread Safety:</b> Describe thread safety guarantees
 * 
 * @param param1 description of param1
 * @param param2 description of param2
 * @return description of what gets returned
 * @throws ErrorType1 when this error occurs
 * @throws ErrorType2 when this error occurs
 * @see RelatedClass
 * @since 1.0
 */
public ReturnType methodName(Type1 param1, Type2 param2) 
        throws ErrorType1, ErrorType2 {
    // implementation
}
```

### TypeScript Function Template

```typescript
/**
 * Brief one-line description
 * 
 * More detailed explanation if needed. Can be multiple lines.
 * Explain the purpose, algorithm, or any important context.
 * 
 * @param param1 - Description of param1
 * @param param2 - Description of param2
 * @returns Description of what gets returned
 * 
 * @throws {@link ErrorType1} When this error occurs
 * @throws {@link ErrorType2} When this error occurs
 * 
 * @example
 * ```typescript
 * const result = functionName("input", 42);
 * console.log(result);  // expected_output
 * ```
 * 
 * @remarks
 * Any important notes, caveats, or performance considerations
 * 
 * @public
 */
function functionName(param1: Type1, param2: Type2 = default): ReturnType {
    // implementation
}
```

### Go Function Template

```go
// FunctionName does a brief one-line description.
//
// More detailed explanation if needed. Can be multiple lines.
// Explain the purpose, algorithm, or any important context.
//
// The param1 parameter describes first parameter.
// The param2 parameter describes second parameter.
//
// Returns description of what gets returned.
//
// Example:
//
//	result := FunctionName("input", 42)
//	fmt.Println(result)  // expected output
//
// Returns error if condition occurs.
func FunctionName(param1 Type1, param2 Type2) (ReturnType, error) {
    // implementation
}
```

### Rust Function Template

```rust
/// Brief one-line description.
///
/// More detailed explanation if needed. Can be multiple lines.
/// Explain the purpose, algorithm, or any important context.
///
/// # Arguments
///
/// * `param1` - Description of param1
/// * `param2` - Description of param2
///
/// # Returns
///
/// Description of what gets returned
///
/// # Errors
///
/// Returns `ErrorType1` when this error occurs.
/// Returns `ErrorType2` when this error occurs.
///
/// # Examples
///
/// ```
/// let result = function_name("input", 42)?;
/// assert_eq!(result, expected);
/// ```
///
/// # Panics
///
/// This function panics if condition occurs.
pub fn function_name(param1: Type1, param2: Type2) -> Result<ReturnType, Error> {
    // implementation
}
```


