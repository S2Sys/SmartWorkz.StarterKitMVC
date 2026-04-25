/**
 * Async/Await pattern example for SmartWorkz SDK
 *
 * This example demonstrates:
 * - Promise-based async/await patterns
 * - Error handling
 * - Batch operations
 * - Resource cleanup
 */

const { SmartWorkzClient } = require('../dist');

/**
 * Helper function to wait with timeout
 */
function delay(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

/**
 * Create multiple users concurrently
 */
async function createMultipleUsers(client, count) {
  const users = [];
  const promises = [];

  for (let i = 0; i < count; i++) {
    const promise = client.users.create({
      email: `batch-user-${i}-${Date.now()}@example.com`,
      firstName: `Batch`,
      lastName: `User${i}`
    });
    promises.push(promise);
  }

  try {
    const results = await Promise.all(promises);
    return results;
  } catch (error) {
    console.error('Error creating users:', error.message);
    return [];
  }
}

/**
 * Process users sequentially
 */
async function processUsersSequentially(client, users) {
  const results = [];

  for (const user of users) {
    try {
      // Update user
      const updated = await client.users.update(user.id, {
        firstName: `Updated${user.firstName}`
      });
      results.push(updated);

      // Create transaction
      await client.transactions.create({
        userId: user.id,
        amount: Math.random() * 1000,
        currency: 'USD',
        description: `Processing for ${user.firstName}`
      });

      console.log(`  Processed user: ${user.id}`);
    } catch (error) {
      console.error(`  Error processing user ${user.id}:`, error.message);
    }
  }

  return results;
}

/**
 * Main async function
 */
async function main() {
  const client = new SmartWorkzClient({
    apiKey: process.env.SMARTWORKZ_API_KEY || 'your-api-key'
  });

  console.log('SmartWorkz SDK - Async/Await Example\n');

  try {
    // 1. Create multiple users concurrently
    console.log('1. Creating 5 users concurrently...');
    const newUsers = await createMultipleUsers(client, 5);
    console.log(`   Created ${newUsers.length} users\n`);

    if (newUsers.length === 0) {
      console.log('   No users created, skipping further operations');
      return;
    }

    // 2. Process users sequentially
    console.log('2. Processing users sequentially...');
    const processedUsers = await processUsersSequentially(client, newUsers);
    console.log(`   Processed ${processedUsers.length} users\n`);

    // 3. Get all transactions for each user
    console.log('3. Fetching transactions for all users...');
    const allTransactions = [];
    for (const user of newUsers) {
      try {
        const transactions = await client.transactions.list({
          userId: user.id,
          pageSize: 100
        });
        allTransactions.push(...transactions);
        console.log(`   Found ${transactions.length} transactions for ${user.firstName}`);
      } catch (error) {
        console.error(`   Error fetching transactions for ${user.id}:`, error.message);
      }
    }
    console.log(`   Total transactions: ${allTransactions.length}\n`);

    // 4. Cleanup - delete all created users
    console.log('4. Cleaning up - deleting created users...');
    const deletePromises = newUsers.map(user =>
      client.users.delete(user.id)
        .then(() => console.log(`   Deleted user: ${user.id}`))
        .catch(error => console.error(`   Error deleting ${user.id}:`, error.message))
    );

    await Promise.all(deletePromises);
    console.log();

    // 5. Verify deletion
    console.log('5. Verifying deletion...');
    const remainingUsers = await client.users.list({ pageSize: 100 });
    const stillExists = remainingUsers.filter(u => newUsers.some(nu => nu.id === u.id));
    console.log(`   Users still remaining: ${stillExists.length}\n`);

    console.log('Async/Await example completed successfully!');
  } catch (error) {
    console.error('Fatal error:', error.message);
    process.exit(1);
  }
}

main();
