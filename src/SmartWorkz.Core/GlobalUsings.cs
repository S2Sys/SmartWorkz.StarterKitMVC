global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;

// Re-export SmartWorkz.Core.Shared namespaces for flattened namespace access
global using SmartWorkz.Core.Shared.Results;
global using SmartWorkz.Core.Shared.Pagination;
global using SmartWorkz.Core.Shared.Guards;
global using SmartWorkz.Core.Shared.Extensions;
global using SmartWorkz.Core.Shared.Primitives;
global using SmartWorkz.Core.Shared.Base_Classes;
global using SmartWorkz.Core.Shared.Response;
global using SmartWorkz.Core.Shared.Mapping;
global using SmartWorkz.Core.Shared.Constants;
global using SmartWorkz.Core.Shared.Helpers;
global using SmartWorkz.Core.Shared.Data;
global using SmartWorkz.Core.Shared.Email;
global using SmartWorkz.Core.Shared.File;
global using SmartWorkz.Core.Shared.Validation;
global using SmartWorkz.Core.Shared.Security;
global using SmartWorkz.Core.Shared.Http;
global using SmartWorkz.Core.Shared.Diagnostics;
global using SmartWorkz.Core.Shared.Configuration;
global using SmartWorkz.Core.Shared.Caching;
global using SmartWorkz.Core.Shared.MultiTenancy;
global using SmartWorkz.Core.Shared.Resilience;
global using SmartWorkz.Core.Shared.Features;
global using SmartWorkz.Core.Shared.Events;
global using SmartWorkz.Core.Shared.Communications;
global using SmartWorkz.Core.Shared.Templates;
global using SmartWorkz.Core.Shared.BackgroundJobs;
global using SmartWorkz.Core.Shared.Notifications;
global using SmartWorkz.Core.Shared.FileStorage;
global using SmartWorkz.Core.Shared.CQRS;
global using SmartWorkz.Core.Shared.Sagas;
global using SmartWorkz.Core.Shared.Specifications;
global using SmartWorkz.Core.Shared.EventSourcing;
global using SmartWorkz.Core.Shared.Tracing;
global using SmartWorkz.Core.Shared.Utilities;

// Re-export SmartWorkz.Core.Services namespaces
global using SmartWorkz.Core.Services;
global using SmartWorkz.Core.Services.Notifications;
global using SmartWorkz.Core.Services.BackgroundJobs;
global using SmartWorkz.Core.Services.FileStorage;

// Re-export SmartWorkz.Core.External namespaces
global using SmartWorkz.Core.External.Export;
