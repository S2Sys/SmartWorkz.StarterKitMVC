/**
 * TypeScript usage example for SmartWorkz SDK
 *
 * This example demonstrates TypeScript-specific features:
 * - Type-safe API calls
 * - Interface usage
 * - Custom configuration
 * - Bearer token authentication
 */

import { SmartWorkzClient, User, CreateUserRequest, Transaction } from '../dist';

async function main(): Promise<void> {
  try {
    // Initialize client with bearer token (JWT)
    const client = new SmartWorkzClient({
      bearerToken: process.env.SMARTWORKZ_TOKEN || 'your-jwt-token',
      baseUrl: process.env.SMARTWORKZ_API_URL || 'https://api.smartworkz.com',
      timeout: 30000
    });

    console.log('SmartWorkz SDK - TypeScript Example\n');

    // List users with type safety
    console.log('1. Listing users with type safety...');
    const users: User[] = await client.users.list({ pageSize: 50 });
    console.log(`   Found ${users.length} users`);
    users.forEach((user: User) => {
      console.log(`   - ${user.firstName} ${user.lastName} (${user.email})`);
    });
    console.log();

    // Create user with typed request
    console.log('2. Creating user with type-safe request...');
    const userRequest: CreateUserRequest = {
      email: `typescript-user-${Date.now()}@example.com`,
      firstName: 'TypeScript',
      lastName: 'User'
    };
    const newUser: User = await client.users.create(userRequest);
    console.log(`   Created user: ${newUser.id}`);
    console.log(`   Created at: ${newUser.createdAt}\n`);

    // Create transaction with typed request
    console.log('3. Creating transaction with type-safe request...');
    const transaction: Transaction = await client.transactions.create({
      userId: newUser.id,
      amount: 199.99,
      currency: 'USD',
      description: 'TypeScript example transaction'
    });
    console.log(`   Created transaction: ${transaction.id}`);
    console.log(`   Amount: ${transaction.amount} ${transaction.currency}`);
    console.log(`   Status: ${transaction.status}\n`);

    // Update user with partial request
    console.log('4. Updating user with partial request...');
    const updatedUser: User = await client.users.update(newUser.id, {
      firstName: 'TypeScriptUpdated'
    });
    console.log(`   Updated user: ${updatedUser.firstName} ${updatedUser.lastName}\n`);

    // Get specific user
    console.log('5. Getting specific user...');
    const user: User = await client.users.get(newUser.id);
    console.log(`   User ID: ${user.id}`);
    console.log(`   Name: ${user.firstName} ${user.lastName}`);
    console.log(`   Active: ${user.isActive}\n`);

    // List transactions with filtering
    console.log('6. Listing transactions with filtering...');
    const transactions: Transaction[] = await client.transactions.list({
      pageSize: 25,
      userId: newUser.id
    });
    console.log(`   Found ${transactions.length} transactions for user\n`);

    // List products
    console.log('7. Listing products...');
    const products = await client.products.list({
      pageSize: 50,
      isActive: true
    });
    console.log(`   Found ${products.length} active products\n`);

    // Create report
    console.log('8. Creating report...');
    const report = await client.reports.create({
      title: 'User Activity Report',
      description: 'Report of user activities',
      type: 'users',
      filters: {
        isActive: true
      }
    });
    console.log(`   Created report: ${report.id}`);
    console.log(`   Status: ${report.status}\n`);

    // Create webhook
    console.log('9. Creating webhook...');
    const webhook = await client.webhooks.create({
      url: 'https://example.com/webhooks/smartworkz',
      events: ['user.created', 'transaction.completed']
    });
    console.log(`   Created webhook: ${webhook.id}`);
    console.log(`   URL: ${webhook.url}`);
    console.log(`   Events: ${webhook.events.join(', ')}\n`);

    // Cleanup: Delete user
    console.log('10. Deleting user...');
    await client.users.delete(newUser.id);
    console.log(`    Deleted user: ${newUser.id}\n`);

    console.log('TypeScript example completed successfully!');
  } catch (error) {
    if (error instanceof Error) {
      console.error('Error:', error.message);
    } else {
      console.error('Unknown error:', error);
    }
    process.exit(1);
  }
}

main();
