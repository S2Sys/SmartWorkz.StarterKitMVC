# Clients Usage Guide

## Overview

Client for the Authentication API endpoint.
            
             Endpoints:
             • POST /api/authentication/login - User login
             • POST /api/authentication/register - User registration
             • POST /api/authentication/refresh - Refresh JWT token
             • POST /api/authentication/logout - User logout
             • POST /api/authentication/change-password - Change user password
             Usage Examples:
             
             // Login and get JWT token
             var response = await client.Authentication.LoginAsync(
                 new LoginRequest("user@example.com", "password123"));
             var token = response.Token;
            
             // Register new user
             var registerResponse = await client.Authentication.RegisterAsync(
                 new RegisterRequest("newuser@example.com", "John", "Doe", "password123"));
            
             // Refresh token
             var refreshResponse = await client.Authentication.RefreshTokenAsync(
                 new RefreshTokenRequest(currentRefreshToken));

## API Reference

See [API Reference](./api-reference.md) for complete documentation.

