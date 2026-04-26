# Pages Usage Guide

## Overview

Base for all Admin list pages backed by IDapperRepository<T>.
             Wires search, sort, pagination, and HTMX partial response automatically.
            
             Usage:
               public class IndexModel : BaseListPage<User>
               {
                   public IndexModel(IDapperRepository<User> repo) : base(repo) { }
                   // Override BuildFilter() to add entity-specific WHERE conditions
               }

## API Reference

See [API Reference](./api-reference.md) for complete documentation.

