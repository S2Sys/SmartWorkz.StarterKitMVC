# SmartWorkz Usage Guide

## Overview

Main client for interacting with SmartWorkz APIs.
            
             Purpose: Provides strongly-typed access to SmartWorkz REST API endpoints
             including Authentication, Users, Transactions, Products, Reports, and Webhooks.Key Features:
             • Async/await APIs with CancellationToken support
             • Automatic API key or bearer token authentication
             • Built-in request/response logging
             • Typed DTOs for all endpoints
             • Exception handling with meaningful error messages
             Dependency Injection Usage:
             
             services.AddSmartWorkzClient(options =>
             {
                 options.BaseUrl = "https://api.smartworkz.com";
                 options.BearerToken = "your-jwt-token";  // or use ApiKey instead
                 options.Timeout = TimeSpan.FromSeconds(30);
             });
             Injected Service Usage:
             
             public class UserService
             {
                 private readonly ISmartWorkzClient _client;
            
                 public UserService(ISmartWorkzClient client) => _client = client;
            
                 public async Task<GetUserResponse> GetUserAsync(string userId)
                 {
                     return await _client.Users.GetAsync(userId);
                 }
             }
             Direct Instantiation:
             
             var httpClient = new HttpClient();
             var options = new SmartWorkzClientOptions
             {
                 BaseUrl = "https://api.smartworkz.com",
                 BearerToken = "your-jwt-token"
             };
             var client = new SmartWorkzClient(httpClient, options);
             var user = await client.Users.GetAsync("user-123");

## API Reference

See [API Reference](./api-reference.md) for complete documentation.

