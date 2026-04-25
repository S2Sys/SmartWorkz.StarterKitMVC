"""Transaction model."""

from dataclasses import dataclass, field
from datetime import datetime
from typing import Optional
from enum import Enum


class TransactionStatus(str, Enum):
    """Transaction status enumeration."""

    PENDING = "PENDING"
    PROCESSING = "PROCESSING"
    COMPLETED = "COMPLETED"
    FAILED = "FAILED"
    CANCELLED = "CANCELLED"


@dataclass
class Transaction:
    """
    Represents a SmartWorkz transaction.

    Attributes:
        id: Unique transaction identifier
        user_id: User who initiated the transaction
        amount: Transaction amount
        currency: Currency code (e.g., USD, EUR)
        status: Current transaction status
        description: Transaction description
        created_at: Timestamp when transaction was created
        updated_at: Timestamp when transaction was last updated
        metadata: Optional additional transaction metadata
    """

    id: str
    user_id: str
    amount: float
    currency: str
    status: TransactionStatus
    description: str
    created_at: datetime
    updated_at: Optional[datetime] = None
    metadata: Optional[dict] = field(default_factory=dict)

    @classmethod
    def from_dict(cls, data: dict) -> "Transaction":
        """Create Transaction from dictionary."""
        if isinstance(data.get("created_at"), str):
            data["created_at"] = datetime.fromisoformat(data["created_at"])
        if isinstance(data.get("updated_at"), str):
            data["updated_at"] = datetime.fromisoformat(data["updated_at"])
        if isinstance(data.get("status"), str):
            data["status"] = TransactionStatus(data["status"])
        return cls(**data)

    def __str__(self) -> str:
        return f"Transaction({self.id}, {self.amount} {self.currency}, {self.status.value})"

    def __repr__(self) -> str:
        return (
            f"Transaction(id={self.id!r}, user_id={self.user_id!r}, "
            f"amount={self.amount}, status={self.status.value})"
        )

    @property
    def is_complete(self) -> bool:
        """Check if transaction is complete."""
        return self.status == TransactionStatus.COMPLETED

    @property
    def is_failed(self) -> bool:
        """Check if transaction failed."""
        return self.status == TransactionStatus.FAILED
