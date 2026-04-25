"""Authentication handling for SmartWorkz SDK."""

from typing import Dict
from smartworkz.config import SmartWorkzConfig


class Authenticator:
    """Handles authentication headers for API requests."""

    @staticmethod
    def get_headers(config: SmartWorkzConfig) -> Dict[str, str]:
        """
        Get authentication headers based on configuration.

        Args:
            config: SmartWorkzConfig instance

        Returns:
            Dictionary of headers including authentication

        Raises:
            ValueError: If configuration is invalid
        """
        config.validate()

        headers = {
            "Content-Type": "application/json",
            "User-Agent": "smartworkz-sdk-py/1.0.0",
        }

        if config.api_key:
            headers["X-API-Key"] = config.api_key
        elif config.bearer_token:
            headers["Authorization"] = f"Bearer {config.bearer_token}"

        return headers
