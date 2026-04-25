/**
 * Basic usage example for SmartWorkz SDK
 *
 * This example demonstrates common SDK operations:
 * - Creating a client
 * - Listing resources
 * - Creating new resources
 * - Updating resources
 * - Deleting resources
 */

const { SmartWorkzClient } = require('../dist');

async function main() {
  try {
    // Initialize client with API key
    const client = new SmartWorkzClient({
      apiKey: process.env.SMARTWORKZ_API_KEY || 'your-api-key'
    });

    console.log('SmartWorkz SDK - Basic Example\n');

    // Example 1: List users
    console.log('1. Listing users...');
    const users = await client.users.list({ pageSize: 50 });
    console.log(`   Found ${users.length} users\n`);

    // Example 2: Get specific user
    if (users.length > 0) {
      const userId = users[0].id;
      console.log(`2. Getting user: ${userId}`);
      const user = await client.users.get(userId);
      console.log(`   User: ${user.firstName} ${user.lastName} (${user.email})\n`);
    }

    // Example 3: Create user
    console.log('3. Creating new user...');
    const newUser = await client.users.create({
      email: `user-${Date.now()}@example.com`,
      firstName: 'John',
      lastName: 'Doe'
    });
    console.log(`   Created user: ${newUser.id}\n`);

    // Example 4: Update user
    console.log('4. Updating user...');
    const updated = await client.users.update(newUser.id, {
      firstName: 'Jane'
    });
    console.log(`   Updated user: ${updated.firstName} ${updated.lastName}\n`);

    // Example 5: List transactions
    console.log('5. Listing transactions...');
    const transactions = await client.transactions.list({ pageSize: 25 });
    console.log(`   Found ${transactions.length} transactions\n`);

    // Example 6: Create transaction
    console.log('6. Creating transaction...');
    const transaction = await client.transactions.create({
      userId: newUser.id,
      amount: 99.99,
      currency: 'USD',
      description: 'Test transaction'
    });
    console.log(`   Created transaction: ${transaction.id}\n`);

    // Example 7: List products
    console.log('7. Listing products...');
    const products = await client.products.list({ pageSize: 50 });
    console.log(`   Found ${products.length} products\n`);

    // Example 8: List reports
    console.log('8. Listing reports...');
    const reports = await client.reports.list({ pageSize: 10 });
    console.log(`   Found ${reports.length} reports\n`);

    // Example 9: List webhooks
    console.log('9. Listing webhooks...');
    const webhooks = await client.webhooks.list({ pageSize: 10 });
    console.log(`   Found ${webhooks.length} webhooks\n`);

    // Example 10: Delete user
    console.log('10. Deleting user...');
    await client.users.delete(newUser.id);
    console.log(`    Deleted user: ${newUser.id}\n`);

    console.log('All examples completed successfully!');
  } catch (error) {
    console.error('Error:', error.message);
    process.exit(1);
  }
}

main();
