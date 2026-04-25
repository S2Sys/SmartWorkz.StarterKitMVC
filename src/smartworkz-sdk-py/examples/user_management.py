"""User management examples for SmartWorkz SDK."""

from smartworkz import SmartWorkzClient, SmartWorkzError

# Initialize client
client = SmartWorkzClient(api_key="your-api-key-here")

def manage_users():
    """Demonstrate user management operations."""
    print("=== User Management Examples ===\n")

    # 1. List all users
    print("1. Listing all users:")
    try:
        users = client.users.list(page_size=100)
        print(f"Total users: {len(users)}")
        for user in users[:5]:  # Show first 5
            print(f"  - {user.full_name} ({user.email}), ID: {user.id}")
        if len(users) > 5:
            print(f"  ... and {len(users) - 5} more")
    except SmartWorkzError as e:
        print(f"Error: {e}")

    # 2. Create a new user
    print("\n2. Creating a new user:")
    try:
        new_user = client.users.create(
            email="alice.johnson@example.com",
            first_name="Alice",
            last_name="Johnson",
        )
        print(f"Created user:")
        print(f"  ID: {new_user.id}")
        print(f"  Name: {new_user.full_name}")
        print(f"  Email: {new_user.email}")
        print(f"  Active: {new_user.is_active}")
        user_id = new_user.id
    except SmartWorkzError as e:
        print(f"Error: {e}")
        user_id = "example-user-id"

    # 3. Get specific user
    print(f"\n3. Getting user {user_id}:")
    try:
        user = client.users.get(user_id)
        print(f"User details:")
        print(f"  Name: {user.full_name}")
        print(f"  Email: {user.email}")
        print(f"  Created: {user.created_at}")
        print(f"  Updated: {user.updated_at}")
        print(f"  Active: {user.is_active}")
    except SmartWorkzError as e:
        print(f"Error: {e}")

    # 4. Update user
    print(f"\n4. Updating user {user_id}:")
    try:
        updated_user = client.users.update(
            user_id,
            first_name="Alicia",
            is_active=True,
        )
        print(f"Updated user:")
        print(f"  Name: {updated_user.full_name}")
        print(f"  Email: {updated_user.email}")
    except SmartWorkzError as e:
        print(f"Error: {e}")

    # 5. Bulk create multiple users
    print("\n5. Creating multiple users:")
    user_data = [
        ("bob.smith@example.com", "Bob", "Smith"),
        ("carol.white@example.com", "Carol", "White"),
        ("david.brown@example.com", "David", "Brown"),
    ]
    created_users = []
    for email, first_name, last_name in user_data:
        try:
            user = client.users.create(
                email=email,
                first_name=first_name,
                last_name=last_name,
            )
            created_users.append(user)
            print(f"  Created: {user.full_name}")
        except SmartWorkzError as e:
            print(f"  Error creating {first_name} {last_name}: {e}")

    # 6. List with pagination
    print("\n6. Paginated listing:")
    try:
        page1 = client.users.list(page_size=10)
        print(f"Page 1: {len(page1)} users")
        if len(page1) > 0:
            # Get cursor for next page (if available)
            last_user = page1[-1]
            print(f"Last user on page 1: {last_user.full_name}")
    except SmartWorkzError as e:
        print(f"Error: {e}")

    # 7. Search/filter users
    print("\n7. Filtering users by status:")
    try:
        active_users = client.users.list(page_size=50)
        inactive_count = sum(1 for u in active_users if not u.is_active)
        active_count = sum(1 for u in active_users if u.is_active)
        print(f"Active users: {active_count}")
        print(f"Inactive users: {inactive_count}")
    except SmartWorkzError as e:
        print(f"Error: {e}")

    # 8. Delete user (example, not executing)
    print(f"\n8. Deleting user {user_id} (example only):")
    print("To delete, call: client.users.delete(user_id)")
    try:
        # Uncomment to actually delete:
        # client.users.delete(user_id)
        # print("User deleted successfully")
        print("Skipped actual deletion")
    except SmartWorkzError as e:
        print(f"Error: {e}")


def user_details_example(user_id: str):
    """Get detailed information about a user."""
    print(f"\n=== User Details for {user_id} ===\n")

    try:
        user = client.users.get(user_id)

        print(f"Basic Information:")
        print(f"  ID: {user.id}")
        print(f"  Full Name: {user.full_name}")
        print(f"  Email: {user.email}")

        print(f"\nStatus Information:")
        print(f"  Active: {user.is_active}")
        print(f"  Created: {user.created_at}")
        print(f"  Last Updated: {user.updated_at}")

        if user.metadata:
            print(f"\nMetadata:")
            for key, value in user.metadata.items():
                print(f"  {key}: {value}")

    except SmartWorkzError as e:
        print(f"Error getting user details: {e}")


def batch_update_users(user_ids: list):
    """Update multiple users."""
    print(f"\n=== Batch Update Users ===\n")

    for user_id in user_ids:
        try:
            updated_user = client.users.update(user_id, is_active=True)
            print(f"Updated {updated_user.full_name}")
        except SmartWorkzError as e:
            print(f"Error updating {user_id}: {e}")


if __name__ == "__main__":
    try:
        manage_users()
        # user_details_example("user-123")
        # batch_update_users(["user-1", "user-2", "user-3"])
    finally:
        client.close()
