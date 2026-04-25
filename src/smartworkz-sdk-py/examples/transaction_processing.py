"""Transaction processing examples for SmartWorkz SDK."""

import asyncio
from smartworkz import (
    AsyncSmartWorkzClient,
    SmartWorkzClient,
    SmartWorkzError,
    SmartWorkzNotFoundError,
)
from smartworkz.models import TransactionStatus


def synchronous_transactions():
    """Demonstrate synchronous transaction processing."""
    print("=== Synchronous Transaction Processing ===\n")

    client = SmartWorkzClient(api_key="your-api-key-here")

    try:
        # List recent transactions
        print("1. Listing recent transactions:")
        transactions = client.transactions.list(page_size=50)
        print(f"Total transactions: {len(transactions)}")

        # Group by status
        by_status = {}
        for txn in transactions:
            status = txn.status.value
            by_status[status] = by_status.get(status, 0) + 1

        for status, count in by_status.items():
            print(f"  {status}: {count}")

        # Create a new transaction
        print("\n2. Creating a new transaction:")
        transaction = client.transactions.create(
            user_id="user-123",
            amount=150.50,
            currency="USD",
            description="Order #12345 - Premium Widget",
        )
        print(f"Created transaction:")
        print(f"  ID: {transaction.id}")
        print(f"  Amount: {transaction.amount} {transaction.currency}")
        print(f"  Status: {transaction.status.value}")
        print(f"  Description: {transaction.description}")

        txn_id = transaction.id

        # Get transaction details
        print(f"\n3. Getting transaction details:")
        transaction = client.transactions.get(txn_id)
        print(f"Transaction details:")
        print(f"  User ID: {transaction.user_id}")
        print(f"  Amount: {transaction.amount} {transaction.currency}")
        print(f"  Status: {transaction.status.value}")
        print(f"  Created: {transaction.created_at}")

        # Check transaction status
        print(f"\n4. Checking transaction status:")
        status = client.transactions.get_status(txn_id)
        print(f"Current status: {status.value}")

        # Process transactions by status
        print(f"\n5. Processing transactions by status:")
        for txn in transactions[:5]:
            if txn.status == TransactionStatus.PENDING:
                print(f"  - Found pending transaction: {txn.id}")
            elif txn.status == TransactionStatus.COMPLETED:
                print(f"  - Completed: {txn.id}")
            elif txn.status == TransactionStatus.FAILED:
                print(f"  - Failed: {txn.id}")

        # Filter by user
        print(f"\n6. Filtering transactions by user:")
        user_transactions = client.transactions.list(user_id="user-123")
        print(f"User 'user-123' has {len(user_transactions)} transactions")

        # Calculate totals
        print(f"\n7. Transaction summary:")
        total_amount = sum(t.amount for t in transactions)
        completed_amount = sum(
            t.amount for t in transactions if t.is_complete
        )
        failed_count = sum(1 for t in transactions if t.is_failed)

        print(f"  Total amount: {total_amount}")
        print(f"  Completed amount: {completed_amount}")
        print(f"  Failed transactions: {failed_count}")

    except SmartWorkzError as e:
        print(f"Error: {e}")
    finally:
        client.close()


async def asynchronous_transactions():
    """Demonstrate asynchronous transaction processing."""
    print("\n=== Asynchronous Transaction Processing ===\n")

    async with AsyncSmartWorkzClient(api_key="your-api-key-here") as client:
        try:
            # Create multiple transactions concurrently
            print("1. Creating multiple transactions (async):")
            tasks = []
            for i in range(3):
                task = client.transactions.create(
                    user_id=f"user-{i}",
                    amount=100.0 + i * 10,
                    currency="USD",
                    description=f"Async transaction {i+1}",
                )
                tasks.append(task)

            # This would work if the client methods were async
            # results = await asyncio.gather(*tasks)
            print("  (Async example - actual implementation depends on endpoint async support)")

            # List transactions
            print("\n2. Listing transactions (async):")
            transactions = client.transactions.list()
            print(f"Retrieved {len(transactions)} transactions")

        except SmartWorkzError as e:
            print(f"Error: {e}")


def transaction_workflow():
    """Demonstrate a typical transaction workflow."""
    print("\n=== Transaction Workflow Example ===\n")

    client = SmartWorkzClient(api_key="your-api-key-here")

    try:
        # Step 1: Create transaction
        print("Step 1: Creating transaction...")
        transaction = client.transactions.create(
            user_id="user-456",
            amount=99.99,
            currency="USD",
            description="Premium Package - Annual Subscription",
        )
        print(f"  Transaction ID: {transaction.id}")
        print(f"  Status: {transaction.status.value}")

        # Step 2: Monitor status
        print("\nStep 2: Monitoring transaction status...")
        import time

        for attempt in range(5):
            status = client.transactions.get_status(transaction.id)
            print(f"  Attempt {attempt + 1}: {status.value}")

            if status == TransactionStatus.COMPLETED:
                print("  Transaction completed!")
                break
            elif status == TransactionStatus.FAILED:
                print("  Transaction failed!")
                break
            elif attempt < 4:
                time.sleep(1)  # Wait before checking again

        # Step 3: Get final details
        print("\nStep 3: Retrieving final details...")
        final_txn = client.transactions.get(transaction.id)
        print(f"  Final status: {final_txn.status.value}")
        print(f"  Last updated: {final_txn.updated_at}")

        # Step 4: Handle based on status
        if final_txn.is_complete:
            print("\nStep 4: Processing completed transaction...")
            print("  - Recording payment")
            print("  - Sending confirmation email")
            print("  - Updating subscription status")
        elif final_txn.is_failed:
            print("\nStep 4: Handling failed transaction...")
            print("  - Notifying user")
            print("  - Preparing for retry")
            # Uncomment to cancel:
            # client.transactions.cancel(transaction.id)

    except SmartWorkzNotFoundError as e:
        print(f"Transaction not found: {e}")
    except SmartWorkzError as e:
        print(f"Error: {e}")
    finally:
        client.close()


def bulk_transaction_processing():
    """Demonstrate bulk transaction operations."""
    print("\n=== Bulk Transaction Processing ===\n")

    client = SmartWorkzClient(api_key="your-api-key-here")

    try:
        # Create multiple transactions
        print("1. Creating bulk transactions:")
        transactions = []
        for user_id in range(1, 6):
            transaction = client.transactions.create(
                user_id=f"user-{user_id}",
                amount=50.00 * user_id,
                currency="USD",
                description=f"Bulk processing - Transaction {user_id}",
            )
            transactions.append(transaction)
            print(f"  Created: {transaction.id}")

        # Analyze results
        print("\n2. Transaction Analysis:")
        total = sum(t.amount for t in transactions)
        average = total / len(transactions)
        print(f"  Total amount: ${total:.2f}")
        print(f"  Average: ${average:.2f}")
        print(f"  Count: {len(transactions)}")

        # Monitor status
        print("\n3. Status Summary:")
        status_summary = {}
        for txn in transactions:
            status = txn.status.value
            status_summary[status] = status_summary.get(status, 0) + 1

        for status, count in status_summary.items():
            print(f"  {status}: {count}")

    except SmartWorkzError as e:
        print(f"Error: {e}")
    finally:
        client.close()


if __name__ == "__main__":
    synchronous_transactions()
    transaction_workflow()
    bulk_transaction_processing()

    # Run async example
    # asyncio.run(asynchronous_transactions())

    print("\n=== Transaction Examples Complete ===")
