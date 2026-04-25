"""
SmartWorkz API Client for Python.

Example usage:

    Example 1: Initialize with API key
    >>> from smartworkz import SmartWorkzClient
    >>> client = SmartWorkzClient(api_key="your-api-key")
    >>> users = client.users.list()

    Example 2: Initialize with bearer token
    >>> client = SmartWorkzClient(bearer_token="your-jwt-token")
    >>> user = client.users.get("user-123")

    Example 3: Custom configuration
    >>> from smartworkz import SmartWorkzClient, SmartWorkzConfig
    >>> config = SmartWorkzConfig(
    ...     base_url="https://api.smartworkz.com",
    ...     timeout=30,
    ...     api_key="your-api-key"
    ... )
    >>> client = SmartWorkzClient(config=config)
    >>> products = client.products.list(page_size=100)

    Example 4: Async usage
    >>> import asyncio
    >>> from smartworkz import AsyncSmartWorkzClient
    >>> async def main():
    ...     async with AsyncSmartWorkzClient(api_key="key") as client:
    ...         users = await client.users.list()
    >>> asyncio.run(main())
"""

import httpx
from typing import Optional
from smartworkz.config import SmartWorkzConfig
from smartworkz.endpoints import (
    UsersEndpoint,
    TransactionsEndpoint,
    ProductsEndpoint,
    ReportsEndpoint,
    WebhooksEndpoint,
)
from smartworkz.auth import Authenticator
from smartworkz.exceptions import SmartWorkzError


class SmartWorkzClient:
    """
    Main client for interacting with SmartWorkz APIs.

    Provides strongly-typed access to:
    - Users API (list, get, create, update, delete)
    - Transactions API (list, get, create, status, cancel)
    - Products API (list, get, create, update, delete)
    - Reports API (list, get, generate, status, download)
    - Webhooks API (register, list, delete, test)

    Attributes:
        users: UsersEndpoint for user operations
        transactions: TransactionsEndpoint for transaction operations
        products: ProductsEndpoint for product operations
        reports: ReportsEndpoint for report operations
        webhooks: WebhooksEndpoint for webhook operations
    """

    def __init__(
        self,
        api_key: Optional[str] = None,
        bearer_token: Optional[str] = None,
        base_url: str = "https://api.smartworkz.com",
        timeout: int = 30,
        config: Optional[SmartWorkzConfig] = None,
    ):
        """
        Initialize SmartWorkz client.

        Args:
            api_key: API key for authentication
            bearer_token: JWT bearer token for authentication
            base_url: Base URL of SmartWorkz API
            timeout: Request timeout in seconds
            config: SmartWorkzConfig object (overrides other params)

        Raises:
            ValueError: If neither api_key nor bearer_token provided
        """
        if config:
            self.config = config
        else:
            self.config = SmartWorkzConfig(
                base_url=base_url,
                api_key=api_key,
                bearer_token=bearer_token,
                timeout=timeout,
            )

        try:
            self.config.validate()
        except ValueError as e:
            raise SmartWorkzError(str(e))

        # Initialize HTTP client with authentication
        self._http_client = httpx.Client(
            base_url=self.config.base_url,
            timeout=self.config.timeout,
            headers=Authenticator.get_headers(self.config),
        )

        # Initialize endpoint clients
        self.users = UsersEndpoint(self._http_client)
        self.transactions = TransactionsEndpoint(self._http_client)
        self.products = ProductsEndpoint(self._http_client)
        self.reports = ReportsEndpoint(self._http_client)
        self.webhooks = WebhooksEndpoint(self._http_client)

    def close(self):
        """Close HTTP client connection."""
        if self._http_client:
            self._http_client.close()

    def __enter__(self):
        """Context manager entry."""
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        """Context manager exit."""
        self.close()

    def __del__(self):
        """Cleanup on deletion."""
        try:
            self.close()
        except Exception:
            pass


class AsyncSmartWorkzClient:
    """
    Async version of SmartWorkzClient using httpx AsyncClient.

    Provides the same API as SmartWorkzClient but with async/await support.

    Example:
        >>> import asyncio
        >>> from smartworkz import AsyncSmartWorkzClient
        >>> async def main():
        ...     async with AsyncSmartWorkzClient(api_key="key") as client:
        ...         users = await client.users.list()
        ...         print(len(users))
        >>> asyncio.run(main())
    """

    def __init__(
        self,
        api_key: Optional[str] = None,
        bearer_token: Optional[str] = None,
        base_url: str = "https://api.smartworkz.com",
        timeout: int = 30,
        config: Optional[SmartWorkzConfig] = None,
    ):
        """
        Initialize async SmartWorkz client.

        Args:
            api_key: API key for authentication
            bearer_token: JWT bearer token for authentication
            base_url: Base URL of SmartWorkz API
            timeout: Request timeout in seconds
            config: SmartWorkzConfig object (overrides other params)

        Raises:
            ValueError: If neither api_key nor bearer_token provided
        """
        if config:
            self.config = config
        else:
            self.config = SmartWorkzConfig(
                base_url=base_url,
                api_key=api_key,
                bearer_token=bearer_token,
                timeout=timeout,
            )

        try:
            self.config.validate()
        except ValueError as e:
            raise SmartWorkzError(str(e))

        self._http_client = httpx.AsyncClient(
            base_url=self.config.base_url,
            timeout=self.config.timeout,
            headers=Authenticator.get_headers(self.config),
        )

        # Initialize endpoint clients
        self.users = UsersEndpoint(self._http_client)
        self.transactions = TransactionsEndpoint(self._http_client)
        self.products = ProductsEndpoint(self._http_client)
        self.reports = ReportsEndpoint(self._http_client)
        self.webhooks = WebhooksEndpoint(self._http_client)

    async def close(self):
        """Close HTTP client connection."""
        if self._http_client:
            await self._http_client.aclose()

    async def __aenter__(self):
        """Async context manager entry."""
        return self

    async def __aexit__(self, exc_type, exc_val, exc_tb):
        """Async context manager exit."""
        await self.close()

    def __del__(self):
        """Cleanup on deletion."""
        try:
            # Note: Can't await in __del__, so just close synchronously
            if hasattr(self, "_http_client") and self._http_client:
                self._http_client.close()
        except Exception:
            pass
