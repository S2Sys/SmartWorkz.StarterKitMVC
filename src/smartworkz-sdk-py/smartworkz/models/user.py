"""User model."""

from dataclasses import dataclass, field
from datetime import datetime
from typing import Optional


@dataclass
class User:
    """
    Represents a SmartWorkz user.

    Attributes:
        id: Unique user identifier
        email: User email address
        first_name: User first name
        last_name: User last name
        created_at: Timestamp when user was created
        updated_at: Timestamp when user was last updated
        is_active: Whether the user account is active
        metadata: Optional additional user metadata
    """

    id: str
    email: str
    first_name: str
    last_name: str
    created_at: datetime
    updated_at: Optional[datetime] = None
    is_active: bool = True
    metadata: Optional[dict] = field(default_factory=dict)

    @classmethod
    def from_dict(cls, data: dict) -> "User":
        """Create User from dictionary."""
        if isinstance(data.get("created_at"), str):
            data["created_at"] = datetime.fromisoformat(data["created_at"])
        if isinstance(data.get("updated_at"), str):
            data["updated_at"] = datetime.fromisoformat(data["updated_at"])
        return cls(**data)

    def __str__(self) -> str:
        return f"User({self.id}, {self.email})"

    def __repr__(self) -> str:
        return f"User(id={self.id!r}, email={self.email!r}, name={self.first_name} {self.last_name})"

    @property
    def full_name(self) -> str:
        """Get user's full name."""
        return f"{self.first_name} {self.last_name}".strip()
