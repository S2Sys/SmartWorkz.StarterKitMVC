"""Products API endpoint client."""

from typing import List, Optional
from smartworkz.models import Product
from smartworkz.exceptions import (
    SmartWorkzError,
    SmartWorkzNotFoundError,
    SmartWorkzValidationError,
)


class ProductsEndpoint:
    """
    Client for Products API endpoint.

    Provides access to:
    - List products: GET /api/products
    - Get product: GET /api/products/{id}
    - Create product: POST /api/products
    - Update product: PUT /api/products/{id}
    - Delete product: DELETE /api/products/{id}
    """

    def __init__(self, http_client):
        """Initialize ProductsEndpoint with HTTP client."""
        self._client = http_client

    def list(self, page_size: int = 50, after: Optional[str] = None) -> List[Product]:
        """
        List all products.

        Args:
            page_size: Number of products per page (default: 50, max: 100)
            after: Cursor for pagination

        Returns:
            List of Product objects
        """
        if page_size <= 0 or page_size > 100:
            raise SmartWorkzValidationError("page_size must be between 1 and 100")

        params = {"pageSize": page_size}
        if after:
            params["after"] = after

        try:
            response = self._client.get("/api/products", params=params)
            response.raise_for_status()
            data = response.json()

            items = data.get("data", data) if isinstance(data, dict) else data
            return [
                Product.from_dict(p) if isinstance(p, dict) else p for p in items
            ]
        except Exception as e:
            raise SmartWorkzError(f"Failed to list products: {str(e)}")

    def get(self, product_id: str) -> Product:
        """
        Get product by ID.

        Args:
            product_id: Product ID

        Returns:
            Product object

        Raises:
            SmartWorkzNotFoundError: If product not found
        """
        if not product_id:
            raise SmartWorkzValidationError("product_id cannot be empty")

        try:
            response = self._client.get(f"/api/products/{product_id}")
            if response.status_code == 404:
                raise SmartWorkzNotFoundError(f"Product {product_id} not found")
            response.raise_for_status()
            return Product.from_dict(response.json())
        except SmartWorkzNotFoundError:
            raise
        except Exception as e:
            raise SmartWorkzError(f"Failed to get product {product_id}: {str(e)}")

    def create(
        self,
        name: str,
        description: str,
        price: float,
        sku: str,
        currency: str = "USD",
        **kwargs,
    ) -> Product:
        """
        Create new product.

        Args:
            name: Product name
            description: Product description
            price: Product price
            sku: Stock keeping unit
            currency: Currency code (default: USD)
            **kwargs: Additional product fields

        Returns:
            Created Product object
        """
        if not name or not description or price < 0 or not sku:
            raise SmartWorkzValidationError(
                "name, description, price, and sku are required"
            )

        payload = {
            "name": name,
            "description": description,
            "price": price,
            "sku": sku,
            "currency": currency,
            **kwargs,
        }

        try:
            response = self._client.post("/api/products", json=payload)
            response.raise_for_status()
            return Product.from_dict(response.json())
        except Exception as e:
            raise SmartWorkzError(f"Failed to create product: {str(e)}")

    def update(self, product_id: str, **kwargs) -> Product:
        """
        Update product by ID.

        Args:
            product_id: Product ID
            **kwargs: Fields to update

        Returns:
            Updated Product object
        """
        if not product_id:
            raise SmartWorkzValidationError("product_id cannot be empty")

        try:
            response = self._client.put(f"/api/products/{product_id}", json=kwargs)
            if response.status_code == 404:
                raise SmartWorkzNotFoundError(f"Product {product_id} not found")
            response.raise_for_status()
            return Product.from_dict(response.json())
        except SmartWorkzNotFoundError:
            raise
        except Exception as e:
            raise SmartWorkzError(f"Failed to update product {product_id}: {str(e)}")

    def delete(self, product_id: str) -> None:
        """
        Delete product by ID.

        Args:
            product_id: Product ID

        Raises:
            SmartWorkzNotFoundError: If product not found
        """
        if not product_id:
            raise SmartWorkzValidationError("product_id cannot be empty")

        try:
            response = self._client.delete(f"/api/products/{product_id}")
            if response.status_code == 404:
                raise SmartWorkzNotFoundError(f"Product {product_id} not found")
            response.raise_for_status()
        except SmartWorkzNotFoundError:
            raise
        except Exception as e:
            raise SmartWorkzError(f"Failed to delete product {product_id}: {str(e)}")
