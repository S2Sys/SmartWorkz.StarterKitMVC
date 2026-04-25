"""SmartWorkz SDK for Python - Official SDK for SmartWorkz APIs."""

from .client import SmartWorkzClient, AsyncSmartWorkzClient
from .config import SmartWorkzConfig
from .exceptions import (
    SmartWorkzError,
    SmartWorkzAuthenticationError,
    SmartWorkzAuthorizationError,
    SmartWorkzNotFoundError,
    SmartWorkzValidationError,
    SmartWorkzConflictError,
    SmartWorkzServerError,
    SmartWorkzRateLimitError,
    SmartWorkzConnectionError,
    SmartWorkzTimeoutError,
)
from .models import User, Transaction, Product, Report
from .endpoints import (
    UsersEndpoint,
    TransactionsEndpoint,
    ProductsEndpoint,
    ReportsEndpoint,
    WebhooksEndpoint,
)

__version__ = "1.0.0"
__author__ = "S2 Systems"
__email__ = "sdk@smartworkz.com"

__all__ = [
    # Client classes
    "SmartWorkzClient",
    "AsyncSmartWorkzClient",
    # Configuration
    "SmartWorkzConfig",
    # Exceptions
    "SmartWorkzError",
    "SmartWorkzAuthenticationError",
    "SmartWorkzAuthorizationError",
    "SmartWorkzNotFoundError",
    "SmartWorkzValidationError",
    "SmartWorkzConflictError",
    "SmartWorkzServerError",
    "SmartWorkzRateLimitError",
    "SmartWorkzConnectionError",
    "SmartWorkzTimeoutError",
    # Models
    "User",
    "Transaction",
    "Product",
    "Report",
    # Endpoints
    "UsersEndpoint",
    "TransactionsEndpoint",
    "ProductsEndpoint",
    "ReportsEndpoint",
    "WebhooksEndpoint",
]
