# DataLoaders Usage Guide

## Overview

DataLoader for batching Product queries to prevent N+1 query problems.
            Groups product lookups by ID and loads them in a single batch operation.
            Note: Full DataLoader implementation requires HotChocolate.DataLoader package
            and would integrate with a repository pattern for actual data loading.

## API Reference

See [API Reference](./api-reference.md) for complete documentation.

