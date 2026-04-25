"""Custom exceptions for SmartWorkz SDK."""


class SmartWorkzError(Exception):
    """Base exception for SmartWorkz SDK."""

    def __init__(self, message: str, status_code: int = None, response_body: str = None):
        """
        Initialize SmartWorkzError.

        Args:
            message: Error message
            status_code: HTTP status code if applicable
            response_body: Response body if applicable
        """
        self.message = message
        self.status_code = status_code
        self.response_body = response_body
        super().__init__(self.message)


class SmartWorkzAuthenticationError(SmartWorkzError):
    """Authentication failed (401)."""

    pass


class SmartWorkzAuthorizationError(SmartWorkzError):
    """Authorization failed (403)."""

    pass


class SmartWorkzNotFoundError(SmartWorkzError):
    """Resource not found (404)."""

    pass


class SmartWorkzValidationError(SmartWorkzError):
    """Validation error (400)."""

    pass


class SmartWorkzConflictError(SmartWorkzError):
    """Resource conflict (409)."""

    pass


class SmartWorkzServerError(SmartWorkzError):
    """Server error (5xx)."""

    pass


class SmartWorkzRateLimitError(SmartWorkzError):
    """Rate limit exceeded (429)."""

    pass


class SmartWorkzConnectionError(SmartWorkzError):
    """Connection error."""

    pass


class SmartWorkzTimeoutError(SmartWorkzError):
    """Request timeout."""

    pass
