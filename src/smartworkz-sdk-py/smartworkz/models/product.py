"""Product model."""

from dataclasses import dataclass, field
from datetime import datetime
from typing import Optional


@dataclass
class Product:
    """
    Represents a SmartWorkz product.

    Attributes:
        id: Unique product identifier
        name: Product name
        description: Product description
        price: Product price
        currency: Currency code (e.g., USD, EUR)
        sku: Stock keeping unit
        created_at: Timestamp when product was created
        updated_at: Timestamp when product was last updated
        is_active: Whether the product is active
        metadata: Optional additional product metadata
    """

    id: str
    name: str
    description: str
    price: float
    currency: str
    sku: str
    created_at: datetime
    updated_at: Optional[datetime] = None
    is_active: bool = True
    metadata: Optional[dict] = field(default_factory=dict)

    @classmethod
    def from_dict(cls, data: dict) -> "Product":
        """Create Product from dictionary."""
        if isinstance(data.get("created_at"), str):
            data["created_at"] = datetime.fromisoformat(data["created_at"])
        if isinstance(data.get("updated_at"), str):
            data["updated_at"] = datetime.fromisoformat(data["updated_at"])
        return cls(**data)

    def __str__(self) -> str:
        return f"Product({self.id}, {self.name}, {self.price} {self.currency})"

    def __repr__(self) -> str:
        return f"Product(id={self.id!r}, name={self.name!r}, sku={self.sku!r})"

    @property
    def display_price(self) -> str:
        """Get formatted price for display."""
        return f"{self.currency} {self.price:.2f}"
