"""Users API endpoint client."""

from typing import List, Optional
from smartworkz.models import User
from smartworkz.exceptions import (
    SmartWorkzError,
    SmartWorkzNotFoundError,
    SmartWorkzValidationError,
)


class UsersEndpoint:
    """
    Client for Users API endpoint.

    Provides access to:
    - List users: GET /api/users
    - Get user: GET /api/users/{id}
    - Create user: POST /api/users
    - Update user: PUT /api/users/{id}
    - Delete user: DELETE /api/users/{id}

    Example usage:
        >>> client = SmartWorkzClient(api_key="key")
        >>> users = client.users.list(page_size=50)
        >>> user = client.users.get("user-123")
        >>> new_user = client.users.create(
        ...     email="test@example.com",
        ...     first_name="John",
        ...     last_name="Doe"
        ... )
    """

    def __init__(self, http_client):
        """Initialize UsersEndpoint with HTTP client."""
        self._client = http_client

    def list(self, page_size: int = 50, after: Optional[str] = None) -> List[User]:
        """
        List all users.

        Args:
            page_size: Number of users per page (default: 50, max: 100)
            after: Cursor for pagination

        Returns:
            List of User objects

        Raises:
            SmartWorkzError: If API request fails
        """
        if page_size <= 0 or page_size > 100:
            raise SmartWorkzValidationError("page_size must be between 1 and 100")

        params = {"pageSize": page_size}
        if after:
            params["after"] = after

        try:
            response = self._client.get("/api/users", params=params)
            response.raise_for_status()
            data = response.json()

            # Handle paginated response
            if isinstance(data, dict) and "data" in data:
                items = data["data"]
            elif isinstance(data, list):
                items = data
            else:
                items = data

            return [User.from_dict(u) if isinstance(u, dict) else u for u in items]
        except Exception as e:
            raise SmartWorkzError(f"Failed to list users: {str(e)}")

    def get(self, user_id: str) -> User:
        """
        Get user by ID.

        Args:
            user_id: User ID

        Returns:
            User object

        Raises:
            SmartWorkzNotFoundError: If user not found
            SmartWorkzError: If API request fails
        """
        if not user_id:
            raise SmartWorkzValidationError("user_id cannot be empty")

        try:
            response = self._client.get(f"/api/users/{user_id}")
            if response.status_code == 404:
                raise SmartWorkzNotFoundError(f"User {user_id} not found")
            response.raise_for_status()
            return User.from_dict(response.json())
        except SmartWorkzNotFoundError:
            raise
        except Exception as e:
            raise SmartWorkzError(f"Failed to get user {user_id}: {str(e)}")

    def create(self, email: str, first_name: str, last_name: str, **kwargs) -> User:
        """
        Create new user.

        Args:
            email: User email
            first_name: User first name
            last_name: User last name
            **kwargs: Additional user fields

        Returns:
            Created User object

        Raises:
            SmartWorkzValidationError: If validation fails
            SmartWorkzError: If API request fails
        """
        if not email or not first_name or not last_name:
            raise SmartWorkzValidationError(
                "email, first_name, and last_name are required"
            )

        payload = {
            "email": email,
            "firstName": first_name,
            "lastName": last_name,
            **kwargs,
        }

        try:
            response = self._client.post("/api/users", json=payload)
            response.raise_for_status()
            return User.from_dict(response.json())
        except Exception as e:
            raise SmartWorkzError(f"Failed to create user: {str(e)}")

    def update(self, user_id: str, **kwargs) -> User:
        """
        Update user by ID.

        Args:
            user_id: User ID
            **kwargs: Fields to update

        Returns:
            Updated User object

        Raises:
            SmartWorkzNotFoundError: If user not found
            SmartWorkzError: If API request fails
        """
        if not user_id:
            raise SmartWorkzValidationError("user_id cannot be empty")

        try:
            response = self._client.put(f"/api/users/{user_id}", json=kwargs)
            if response.status_code == 404:
                raise SmartWorkzNotFoundError(f"User {user_id} not found")
            response.raise_for_status()
            return User.from_dict(response.json())
        except SmartWorkzNotFoundError:
            raise
        except Exception as e:
            raise SmartWorkzError(f"Failed to update user {user_id}: {str(e)}")

    def delete(self, user_id: str) -> None:
        """
        Delete user by ID.

        Args:
            user_id: User ID

        Raises:
            SmartWorkzNotFoundError: If user not found
            SmartWorkzError: If API request fails
        """
        if not user_id:
            raise SmartWorkzValidationError("user_id cannot be empty")

        try:
            response = self._client.delete(f"/api/users/{user_id}")
            if response.status_code == 404:
                raise SmartWorkzNotFoundError(f"User {user_id} not found")
            response.raise_for_status()
        except SmartWorkzNotFoundError:
            raise
        except Exception as e:
            raise SmartWorkzError(f"Failed to delete user {user_id}: {str(e)}")
