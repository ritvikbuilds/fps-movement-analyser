# Language-Specific Documentation Style Guides

Comprehensive conventions for documenting code in various programming languages.

## Python

### Docstring Format Options

**Google Style (Recommended):**
```python
def function_name(param1: str, param2: int = 10) -> bool:
    """Brief description of function.
    
    Longer description explaining behavior, algorithm, or context.
    Can span multiple lines.
    
    Args:
        param1: Description of param1
        param2: Description of param2 (default: 10)
    
    Returns:
        Description of return value
    
    Raises:
        ValueError: When condition occurs
        TypeError: When type is invalid
    
    Example:
        >>> result = function_name("test", 20)
        >>> print(result)
        True
    
    Note:
        Additional notes or warnings
    """
```

**NumPy Style:**
```python
def function_name(param1, param2=10):
    """Brief description of function.
    
    Longer description explaining behavior, algorithm, or context.
    Can span multiple lines.
    
    Parameters
    ----------
    param1 : str
        Description of param1
    param2 : int, optional
        Description of param2 (default is 10)
    
    Returns
    -------
    bool
        Description of return value
    
    Raises
    ------
    ValueError
        When condition occurs
    TypeError
        When type is invalid
    
    Examples
    --------
    >>> result = function_name("test", 20)
    >>> print(result)
    True
    
    Notes
    -----
    Additional notes or warnings
    """
```

### Class Documentation

```python
class ExampleClass:
    """Brief class description.
    
    Detailed explanation of the class purpose, responsibilities,
    and typical usage patterns.
    
    Attributes:
        attr1: Description of attribute 1
        attr2: Description of attribute 2
    
    Example:
        >>> obj = ExampleClass("value")
        >>> obj.method()
        'result'
    """
    
    def __init__(self, param: str):
        """Initialize the class.
        
        Args:
            param: Description of initialization parameter
        """
        self.attr1 = param
```

### Module Documentation

```python
"""Module for handling user authentication.

This module provides classes and functions for authenticating users,
managing sessions, and handling authorization checks.

Typical usage example:

    auth = Authenticator(config)
    user = auth.login(username, password)
    if auth.check_permission(user, 'admin'):
        # perform admin action
        pass
"""
```

## JavaScript/TypeScript

### JSDoc Format

**Function Documentation:**
```javascript
/**
 * Calculates the total price including tax and discounts
 * 
 * @param {number} basePrice - The base price before calculations
 * @param {number} taxRate - Tax rate as decimal (e.g., 0.08 for 8%)
 * @param {Object} [options] - Optional configuration
 * @param {number} [options.discount=0] - Discount percentage (0-100)
 * @param {boolean} [options.roundUp=true] - Whether to round up to nearest cent
 * @returns {number} The final calculated price
 * @throws {RangeError} If taxRate is negative or discount is invalid
 * 
 * @example
 * // Calculate with tax only
 * calculatePrice(100, 0.08);  // Returns 108
 * 
 * @example
 * // Calculate with tax and discount
 * calculatePrice(100, 0.08, { discount: 10 });  // Returns 97.2
 */
function calculatePrice(basePrice, taxRate, options = {}) {
    // implementation
}
```

**Class Documentation:**
```javascript
/**
 * Represents a shopping cart with item management
 * 
 * @class
 * @property {Array<CartItem>} items - Array of items in cart
 * @property {number} total - Current cart total
 * 
 * @example
 * const cart = new ShoppingCart();
 * cart.addItem({ id: 1, name: 'Product', price: 29.99 });
 */
class ShoppingCart {
    /**
     * Creates a new shopping cart
     * 
     * @param {Object} [config] - Configuration options
     * @param {string} [config.currency='USD'] - Currency code
     * @param {number} [config.taxRate=0] - Default tax rate
     */
    constructor(config = {}) {
        // implementation
    }
}
```

### TypeScript (TSDoc)

```typescript
/**
 * Fetches user data from the API
 * 
 * @param userId - The unique identifier for the user
 * @param options - Request configuration options
 * @returns Promise resolving to user data
 * 
 * @throws {@link NetworkError} When network request fails
 * @throws {@link AuthError} When authentication is required
 * 
 * @remarks
 * This method implements exponential backoff for retries.
 * Maximum retry attempts is configurable via options.
 * 
 * @example
 * ```typescript
 * const user = await fetchUser('user-123', { retries: 3 });
 * console.log(user.email);
 * ```
 * 
 * @public
 */
async function fetchUser(
    userId: string,
    options?: FetchOptions
): Promise<User> {
    // implementation
}
```

## Java

### Javadoc Format

**Method Documentation:**
```java
/**
 * Processes a payment transaction with fraud detection.
 * 
 * <p>This method validates the payment details, performs fraud
 * detection checks, and processes the transaction through the
 * payment gateway. The operation is idempotent and can be safely
 * retried with the same transaction ID.
 * 
 * <p><b>Thread Safety:</b> This method is thread-safe and can be
 * called concurrently from multiple threads.
 * 
 * @param transaction the payment transaction to process, must not be null
 * @param merchantId the merchant identifier, must not be empty
 * @param options optional processing configuration, can be null for defaults
 * @return a {@link PaymentResult} containing transaction status and reference
 * @throws InvalidTransactionException if transaction validation fails
 * @throws PaymentGatewayException if the gateway is unavailable
 * @throws IllegalArgumentException if merchantId is null or empty
 * 
 * @see PaymentResult
 * @see FraudDetectionService
 * @since 2.0
 */
public PaymentResult processPayment(
    Transaction transaction,
    String merchantId,
    ProcessingOptions options
) throws InvalidTransactionException, PaymentGatewayException {
    // implementation
}
```

**Class Documentation:**
```java
/**
 * Manages user authentication and session handling.
 * 
 * <p>This class provides methods for authenticating users via multiple
 * strategies (password, OAuth, SSO) and managing their session lifecycle.
 * 
 * <p><b>Example Usage:</b>
 * <pre>{@code
 * AuthenticationManager auth = new AuthenticationManager(config);
 * User user = auth.authenticate(credentials);
 * Session session = auth.createSession(user);
 * }</pre>
 * 
 * @author Development Team
 * @version 2.1.0
 * @since 1.0
 */
public class AuthenticationManager {
    // implementation
}
```

**Package Documentation (package-info.java):**
```java
/**
 * Provides classes for user authentication and authorization.
 * 
 * <p>This package contains the core authentication components including:
 * <ul>
 *   <li>{@link AuthenticationManager} - Main authentication interface</li>
 *   <li>{@link SessionManager} - Session lifecycle management</li>
 *   <li>{@link PermissionChecker} - Authorization logic</li>
 * </ul>
 * 
 * <h2>Getting Started</h2>
 * <p>To use authentication in your application:
 * <pre>{@code
 * AuthConfig config = new AuthConfig.Builder()
 *     .sessionTimeout(Duration.ofMinutes(30))
 *     .build();
 * AuthenticationManager auth = new AuthenticationManager(config);
 * }</pre>
 * 
 * @since 1.0
 */
package com.example.auth;
```

## Go

### Go Documentation Conventions

**Package Documentation (doc.go):**
```go
// Package auth provides authentication and authorization primitives.
//
// This package offers a simple API for authenticating users and managing
// sessions. It supports multiple authentication strategies and includes
// built-in rate limiting.
//
// Basic usage:
//
//	auth := auth.NewAuthenticator(config)
//	user, err := auth.Authenticate(ctx, credentials)
//	if err != nil {
//	    log.Fatal(err)
//	}
//
// For more examples, see the examples directory.
package auth
```

**Function Documentation:**
```go
// Authenticate verifies user credentials and returns an authenticated user.
//
// The method supports multiple authentication strategies configured via
// the Authenticator's config. If authentication fails, an error describing
// the failure reason is returned.
//
// Example:
//
//	creds := Credentials{Username: "user", Password: "pass"}
//	user, err := auth.Authenticate(ctx, creds)
//	if err != nil {
//	    return fmt.Errorf("auth failed: %w", err)
//	}
//
// Returns ErrInvalidCredentials if credentials are invalid.
// Returns ErrRateLimited if too many attempts were made.
func (a *Authenticator) Authenticate(ctx context.Context, creds Credentials) (*User, error) {
    // implementation
}
```

**Type Documentation:**
```go
// User represents an authenticated user in the system.
//
// Users are created by the Authenticator upon successful authentication.
// Each user has a unique ID and associated permissions.
type User struct {
    // ID is the unique identifier for this user
    ID string
    
    // Username is the user's login name
    Username string
    
    // Permissions contains the user's authorization permissions
    Permissions []string
}
```

## Rust

### Rust Documentation Format

**Module Documentation:**
```rust
//! Authentication and authorization primitives.
//!
//! This module provides types and functions for authenticating users
//! and managing authorization. It includes:
//!
//! - Password-based authentication
//! - Token-based sessions
//! - Permission checking
//!
//! # Examples
//!
//! ```
//! use myapp::auth::Authenticator;
//!
//! let auth = Authenticator::new(config);
//! let user = auth.authenticate(credentials)?;
//! ```
//!
//! # Security Considerations
//!
//! All authentication methods use constant-time comparison to prevent
//! timing attacks.
```

**Function Documentation:**
```rust
/// Authenticates a user with the provided credentials.
///
/// This function verifies the credentials against the configured
/// authentication backend and returns a `User` on success.
///
/// # Arguments
///
/// * `credentials` - The user credentials to verify
/// * `options` - Optional authentication configuration
///
/// # Returns
///
/// Returns `Ok(User)` if authentication succeeds, or an error if:
/// - Credentials are invalid
/// - Backend is unavailable
/// - Rate limit is exceeded
///
/// # Examples
///
/// ```
/// # use myapp::auth::{authenticate, Credentials};
/// let creds = Credentials::new("user", "password");
/// let user = authenticate(creds, None)?;
/// println!("Authenticated: {}", user.username);
/// # Ok::<(), Box<dyn std::error::Error>>(())
/// ```
///
/// # Errors
///
/// Returns `AuthError::InvalidCredentials` if credentials are invalid.
/// Returns `AuthError::BackendError` if the backend is unavailable.
///
/// # Panics
///
/// This function will panic if the credentials contain null bytes.
pub fn authenticate(
    credentials: Credentials,
    options: Option<AuthOptions>
) -> Result<User, AuthError> {
    // implementation
}
```

**Type Documentation:**
```rust
/// Represents an authenticated user.
///
/// Users are created by successful authentication and contain
/// identity information and permissions.
///
/// # Examples
///
/// ```
/// # use myapp::auth::User;
/// let user = User {
///     id: "user-123".to_string(),
///     username: "alice".to_string(),
///     permissions: vec!["read".to_string(), "write".to_string()],
/// };
/// ```
pub struct User {
    /// Unique identifier for the user
    pub id: String,
    
    /// User's login name
    pub username: String,
    
    /// List of permission strings
    pub permissions: Vec<String>,
}
```

## C#

### XML Documentation Comments

```csharp
/// <summary>
/// Processes payment transactions with fraud detection.
/// </summary>
/// <remarks>
/// This method validates payment details, performs fraud detection,
/// and processes transactions through the payment gateway.
/// The operation is idempotent and can be safely retried.
/// </remarks>
/// <param name="transaction">The payment transaction to process</param>
/// <param name="merchantId">The merchant identifier</param>
/// <param name="options">Optional processing configuration</param>
/// <returns>
/// A <see cref="PaymentResult"/> containing the transaction status
/// and reference number.
/// </returns>
/// <exception cref="InvalidTransactionException">
/// Thrown when transaction validation fails
/// </exception>
/// <exception cref="PaymentGatewayException">
/// Thrown when the payment gateway is unavailable
/// </exception>
/// <example>
/// <code>
/// var result = await processor.ProcessPayment(
///     transaction,
///     "merchant-123",
///     new ProcessingOptions { Retry = true }
/// );
/// Console.WriteLine($"Status: {result.Status}");
/// </code>
/// </example>
public async Task<PaymentResult> ProcessPayment(
    Transaction transaction,
    string merchantId,
    ProcessingOptions options = null)
{
    // implementation
}
```

## Ruby

### RDoc/YARD Format

```ruby
# Authenticates a user with provided credentials.
#
# This method verifies credentials against the authentication backend
# and returns a User object on success.
#
# @param credentials [Hash] the user credentials
# @option credentials [String] :username the username
# @option credentials [String] :password the password
# @param options [Hash] optional authentication settings
# @option options [Integer] :timeout (30) timeout in seconds
#
# @return [User] authenticated user object
#
# @raise [InvalidCredentialsError] if credentials are invalid
# @raise [AuthBackendError] if backend is unavailable
#
# @example Authenticate with username and password
#   user = authenticate(
#     username: 'alice',
#     password: 'secret123'
#   )
#   puts user.email
#
# @example With custom timeout
#   user = authenticate(
#     { username: 'alice', password: 'secret123' },
#     { timeout: 60 }
#   )
#
# @note This method uses constant-time comparison for passwords
# @see User
def authenticate(credentials, options = {})
  # implementation
end
```

## PHP

### PHPDoc Format

```php
/**
 * Processes a payment transaction with fraud detection.
 *
 * This method validates the payment details, performs fraud detection
 * checks, and processes the transaction through the payment gateway.
 *
 * @param Transaction $transaction The payment transaction to process
 * @param string $merchantId The merchant identifier
 * @param ProcessingOptions|null $options Optional processing configuration
 * 
 * @return PaymentResult The transaction result with status and reference
 * 
 * @throws InvalidTransactionException If transaction validation fails
 * @throws PaymentGatewayException If the gateway is unavailable
 * @throws \InvalidArgumentException If merchantId is empty
 *
 * @example
 * $result = $processor->processPayment(
 *     $transaction,
 *     'merchant-123',
 *     new ProcessingOptions(['retry' => true])
 * );
 *
 * @see PaymentResult
 * @see FraudDetectionService
 * @since 2.0.0
 */
public function processPayment(
    Transaction $transaction,
    string $merchantId,
    ?ProcessingOptions $options = null
): PaymentResult {
    // implementation
}
```


