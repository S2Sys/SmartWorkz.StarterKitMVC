"""Transactions API endpoint client."""

from typing import List, Optional
from smartworkz.models import Transaction, TransactionStatus
from smartworkz.exceptions import (
    SmartWorkzError,
    SmartWorkzNotFoundError,
    SmartWorkzValidationError,
)


class TransactionsEndpoint:
    """
    Client for Transactions API endpoint.

    Provides access to:
    - List transactions: GET /api/transactions
    - Get transaction: GET /api/transactions/{id}
    - Create transaction: POST /api/transactions
    - Get status: GET /api/transactions/{id}/status
    """

    def __init__(self, http_client):
        """Initialize TransactionsEndpoint with HTTP client."""
        self._client = http_client

    def list(
        self,
        user_id: Optional[str] = None,
        status: Optional[str] = None,
        page_size: int = 50,
        after: Optional[str] = None,
    ) -> List[Transaction]:
        """
        List transactions.

        Args:
            user_id: Filter by user ID
            status: Filter by transaction status
            page_size: Number of transactions per page
            after: Cursor for pagination

        Returns:
            List of Transaction objects
        """
        params = {"pageSize": page_size}
        if user_id:
            params["userId"] = user_id
        if status:
            params["status"] = status
        if after:
            params["after"] = after

        try:
            response = self._client.get("/api/transactions", params=params)
            response.raise_for_status()
            data = response.json()

            items = data.get("data", data) if isinstance(data, dict) else data
            return [
                Transaction.from_dict(t) if isinstance(t, dict) else t for t in items
            ]
        except Exception as e:
            raise SmartWorkzError(f"Failed to list transactions: {str(e)}")

    def get(self, transaction_id: str) -> Transaction:
        """
        Get transaction by ID.

        Args:
            transaction_id: Transaction ID

        Returns:
            Transaction object

        Raises:
            SmartWorkzNotFoundError: If transaction not found
        """
        if not transaction_id:
            raise SmartWorkzValidationError("transaction_id cannot be empty")

        try:
            response = self._client.get(f"/api/transactions/{transaction_id}")
            if response.status_code == 404:
                raise SmartWorkzNotFoundError(f"Transaction {transaction_id} not found")
            response.raise_for_status()
            return Transaction.from_dict(response.json())
        except SmartWorkzNotFoundError:
            raise
        except Exception as e:
            raise SmartWorkzError(f"Failed to get transaction {transaction_id}: {str(e)}")

    def create(
        self,
        user_id: str,
        amount: float,
        description: str,
        currency: str = "USD",
        **kwargs,
    ) -> Transaction:
        """
        Create new transaction.

        Args:
            user_id: User ID
            amount: Transaction amount
            description: Transaction description
            currency: Currency code (default: USD)
            **kwargs: Additional transaction fields

        Returns:
            Created Transaction object
        """
        if not user_id or amount <= 0 or not description:
            raise SmartWorkzValidationError(
                "user_id, positive amount, and description are required"
            )

        payload = {
            "userId": user_id,
            "amount": amount,
            "description": description,
            "currency": currency,
            **kwargs,
        }

        try:
            response = self._client.post("/api/transactions", json=payload)
            response.raise_for_status()
            return Transaction.from_dict(response.json())
        except Exception as e:
            raise SmartWorkzError(f"Failed to create transaction: {str(e)}")

    def get_status(self, transaction_id: str) -> TransactionStatus:
        """
        Get transaction status.

        Args:
            transaction_id: Transaction ID

        Returns:
            TransactionStatus enum value
        """
        if not transaction_id:
            raise SmartWorkzValidationError("transaction_id cannot be empty")

        try:
            response = self._client.get(f"/api/transactions/{transaction_id}/status")
            if response.status_code == 404:
                raise SmartWorkzNotFoundError(f"Transaction {transaction_id} not found")
            response.raise_for_status()
            data = response.json()
            status_str = data.get("status", data)
            return TransactionStatus(status_str)
        except SmartWorkzNotFoundError:
            raise
        except Exception as e:
            raise SmartWorkzError(
                f"Failed to get transaction status {transaction_id}: {str(e)}"
            )

    def cancel(self, transaction_id: str) -> Transaction:
        """
        Cancel a transaction.

        Args:
            transaction_id: Transaction ID

        Returns:
            Updated Transaction object
        """
        if not transaction_id:
            raise SmartWorkzValidationError("transaction_id cannot be empty")

        try:
            response = self._client.post(f"/api/transactions/{transaction_id}/cancel")
            if response.status_code == 404:
                raise SmartWorkzNotFoundError(f"Transaction {transaction_id} not found")
            response.raise_for_status()
            return Transaction.from_dict(response.json())
        except SmartWorkzNotFoundError:
            raise
        except Exception as e:
            raise SmartWorkzError(f"Failed to cancel transaction {transaction_id}: {str(e)}")
