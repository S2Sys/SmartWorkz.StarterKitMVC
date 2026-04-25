"""Webhooks API endpoint client."""

from typing import List, Optional, Dict, Any
from smartworkz.exceptions import (
    SmartWorkzError,
    SmartWorkzNotFoundError,
    SmartWorkzValidationError,
)


class WebhookConfig:
    """Represents a webhook configuration."""

    def __init__(
        self,
        id: str,
        url: str,
        events: List[str],
        active: bool = True,
        headers: Optional[Dict[str, str]] = None,
    ):
        self.id = id
        self.url = url
        self.events = events
        self.active = active
        self.headers = headers or {}

    @classmethod
    def from_dict(cls, data: dict) -> "WebhookConfig":
        """Create WebhookConfig from dictionary."""
        return cls(**data)

    def to_dict(self) -> dict:
        """Convert to dictionary."""
        return {
            "id": self.id,
            "url": self.url,
            "events": self.events,
            "active": self.active,
            "headers": self.headers,
        }

    def __str__(self) -> str:
        return f"WebhookConfig({self.id}, {self.url})"


class WebhooksEndpoint:
    """
    Client for Webhooks API endpoint.

    Provides access to:
    - List webhooks: GET /api/webhooks
    - Get webhook: GET /api/webhooks/{id}
    - Register webhook: POST /api/webhooks
    - Update webhook: PUT /api/webhooks/{id}
    - Delete webhook: DELETE /api/webhooks/{id}
    - Test webhook: POST /api/webhooks/{id}/test
    """

    def __init__(self, http_client):
        """Initialize WebhooksEndpoint with HTTP client."""
        self._client = http_client

    def list(self, page_size: int = 50, after: Optional[str] = None) -> List[WebhookConfig]:
        """
        List all webhooks.

        Args:
            page_size: Number of webhooks per page
            after: Cursor for pagination

        Returns:
            List of WebhookConfig objects
        """
        params = {"pageSize": page_size}
        if after:
            params["after"] = after

        try:
            response = self._client.get("/api/webhooks", params=params)
            response.raise_for_status()
            data = response.json()

            items = data.get("data", data) if isinstance(data, dict) else data
            return [
                WebhookConfig.from_dict(w) if isinstance(w, dict) else w for w in items
            ]
        except Exception as e:
            raise SmartWorkzError(f"Failed to list webhooks: {str(e)}")

    def get(self, webhook_id: str) -> WebhookConfig:
        """
        Get webhook by ID.

        Args:
            webhook_id: Webhook ID

        Returns:
            WebhookConfig object

        Raises:
            SmartWorkzNotFoundError: If webhook not found
        """
        if not webhook_id:
            raise SmartWorkzValidationError("webhook_id cannot be empty")

        try:
            response = self._client.get(f"/api/webhooks/{webhook_id}")
            if response.status_code == 404:
                raise SmartWorkzNotFoundError(f"Webhook {webhook_id} not found")
            response.raise_for_status()
            return WebhookConfig.from_dict(response.json())
        except SmartWorkzNotFoundError:
            raise
        except Exception as e:
            raise SmartWorkzError(f"Failed to get webhook {webhook_id}: {str(e)}")

    def register(
        self,
        url: str,
        events: List[str],
        active: bool = True,
        headers: Optional[Dict[str, str]] = None,
    ) -> WebhookConfig:
        """
        Register new webhook.

        Args:
            url: Webhook URL to receive events
            events: List of event types to subscribe to
            active: Whether webhook is active
            headers: Optional custom headers to send with events

        Returns:
            Created WebhookConfig object

        Raises:
            SmartWorkzValidationError: If validation fails
        """
        if not url or not events:
            raise SmartWorkzValidationError("url and events are required")

        payload = {
            "url": url,
            "events": events,
            "active": active,
        }
        if headers:
            payload["headers"] = headers

        try:
            response = self._client.post("/api/webhooks", json=payload)
            response.raise_for_status()
            return WebhookConfig.from_dict(response.json())
        except Exception as e:
            raise SmartWorkzError(f"Failed to register webhook: {str(e)}")

    def update(
        self,
        webhook_id: str,
        url: Optional[str] = None,
        events: Optional[List[str]] = None,
        active: Optional[bool] = None,
        headers: Optional[Dict[str, str]] = None,
    ) -> WebhookConfig:
        """
        Update webhook configuration.

        Args:
            webhook_id: Webhook ID
            url: New webhook URL
            events: New event list
            active: New active status
            headers: New custom headers

        Returns:
            Updated WebhookConfig object
        """
        if not webhook_id:
            raise SmartWorkzValidationError("webhook_id cannot be empty")

        payload = {}
        if url is not None:
            payload["url"] = url
        if events is not None:
            payload["events"] = events
        if active is not None:
            payload["active"] = active
        if headers is not None:
            payload["headers"] = headers

        try:
            response = self._client.put(f"/api/webhooks/{webhook_id}", json=payload)
            if response.status_code == 404:
                raise SmartWorkzNotFoundError(f"Webhook {webhook_id} not found")
            response.raise_for_status()
            return WebhookConfig.from_dict(response.json())
        except SmartWorkzNotFoundError:
            raise
        except Exception as e:
            raise SmartWorkzError(f"Failed to update webhook {webhook_id}: {str(e)}")

    def delete(self, webhook_id: str) -> None:
        """
        Delete webhook.

        Args:
            webhook_id: Webhook ID

        Raises:
            SmartWorkzNotFoundError: If webhook not found
        """
        if not webhook_id:
            raise SmartWorkzValidationError("webhook_id cannot be empty")

        try:
            response = self._client.delete(f"/api/webhooks/{webhook_id}")
            if response.status_code == 404:
                raise SmartWorkzNotFoundError(f"Webhook {webhook_id} not found")
            response.raise_for_status()
        except SmartWorkzNotFoundError:
            raise
        except Exception as e:
            raise SmartWorkzError(f"Failed to delete webhook {webhook_id}: {str(e)}")

    def test(self, webhook_id: str) -> Dict[str, Any]:
        """
        Send test event to webhook.

        Args:
            webhook_id: Webhook ID

        Returns:
            Test result dictionary

        Raises:
            SmartWorkzNotFoundError: If webhook not found
        """
        if not webhook_id:
            raise SmartWorkzValidationError("webhook_id cannot be empty")

        try:
            response = self._client.post(f"/api/webhooks/{webhook_id}/test")
            if response.status_code == 404:
                raise SmartWorkzNotFoundError(f"Webhook {webhook_id} not found")
            response.raise_for_status()
            return response.json()
        except SmartWorkzNotFoundError:
            raise
        except Exception as e:
            raise SmartWorkzError(f"Failed to test webhook {webhook_id}: {str(e)}")
