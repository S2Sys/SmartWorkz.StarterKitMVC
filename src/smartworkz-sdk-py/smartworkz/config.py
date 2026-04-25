"""Configuration for SmartWorkz client."""

from dataclasses import dataclass
from typing import Optional


@dataclass
class SmartWorkzConfig:
    """
    Configuration options for SmartWorkz client.

    Attributes:
        base_url: Base URL of SmartWorkz API
        api_key: API key for authentication
        bearer_token: JWT bearer token for authentication
        timeout: Request timeout in seconds
    """

    base_url: str = "https://api.smartworkz.com"
    api_key: Optional[str] = None
    bearer_token: Optional[str] = None
    timeout: int = 30

    def validate(self):
        """
        Validate configuration.

        Raises:
            ValueError: If neither api_key nor bearer_token is provided
        """
        if not self.api_key and not self.bearer_token:
            raise ValueError("Either api_key or bearer_token must be provided")

        if self.api_key and self.bearer_token:
            raise ValueError("Provide either api_key or bearer_token, not both")

        if self.timeout <= 0:
            raise ValueError("timeout must be positive")

        return True
