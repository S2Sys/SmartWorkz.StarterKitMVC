# Geo-Hierarchy Design Analysis: 3 Approaches

**Date:** 2026-03-31
**Question:** Should Countries/States/Cities be 3 separate tables or 1 HierarchyId table?

---

## Option A: Single GeoHierarchy Table (HierarchyId)

### Structure
```sql
GeoHierarchy
├─ GeoHierarchyId (GUID)
├─ NodePath (HierarchyId) -- /1/, /1/1/, /1/1/1/, /1/1/1/1/
├─ GeoType (VARCHAR 50) -- 'World', 'Continent', 'Country', 'State', 'City'
├─ Code (VARCHAR 10) -- 'US', 'CA', 'NY', etc.
├─ Name (NVARCHAR 100)
├─ Description (NVARCHAR MAX, nullable)
├─ Latitude (DECIMAL 10,8, nullable)
├─ Longitude (DECIMAL 11,8, nullable)
├─ CurrencyCode (VARCHAR 3, nullable) -- Only for country level
├─ PhoneCode (VARCHAR 20, nullable) -- Only for country level
├─ TimeZone (VARCHAR 50, nullable)
├─ Population (BIGINT, nullable)
├─ IsActive (BIT)
├─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
└─ Indexes: (NodePath), (GeoType, Code), (Name)

Example Hierarchy:
/1/                           World
├─ /1/1/                      North America (Continent)
│  ├─ /1/1/1/                 United States (Country)
│  │  ├─ /1/1/1/1/            California (State)
│  │  │  ├─ /1/1/1/1/1/       San Francisco (City)
│  │  │  └─ /1/1/1/1/2/       Los Angeles (City)
│  │  └─ /1/1/1/2/            New York (State)
│  │     └─ /1/1/1/2/1/       New York City (City)
│  └─ /1/1/2/                 Canada (Country)
│     └─ /1/1/2/1/            Ontario (State)
├─ /1/2/                      Europe (Continent)
│  └─ /1/2/1/                 United Kingdom (Country)
└─ /1/3/                      Asia (Continent)
```

### Queries
```sql
-- Get all cities in California
SELECT * FROM GeoHierarchy
WHERE GeoType = 'City'
  AND NodePath.IsDescendantOf('/1/1/1/1/')

-- Get all states in USA
SELECT * FROM GeoHierarchy
WHERE GeoType = 'State'
  AND NodePath.IsDescendantOf('/1/1/1/')

-- Get country for a city (ancestor query)
SELECT * FROM GeoHierarchy
WHERE GeoType = 'Country'
  AND NodePath = '/1/1/1/' -- Ancestor of city

-- Get path: Country → State → City
SELECT DISTINCT c.Name as Country, s.Name as State, cy.Name as City
FROM GeoHierarchy c
INNER JOIN GeoHierarchy s ON s.NodePath.IsDescendantOf(c.NodePath)
INNER JOIN GeoHierarchy cy ON cy.NodePath.IsDescendantOf(s.NodePath)
WHERE cy.GeoType = 'City'
  AND cy.Code = 'SF'
```

### Pros
✅ **Unlimited flexibility** — Add continents, regions, districts, neighborhoods without schema changes
✅ **Single table** — Easier to manage, no FKs between tables
✅ **Efficient tree queries** — HierarchyId is optimized for ancestor/descendant queries
✅ **Polymorphic-friendly** — One source of truth for Addresses FK
✅ **Future-proof** — Easy to add new geo levels
✅ **Simplified Address FK** — `GeoCity FK → GeoHierarchy.Id` (instead of FK to Cities)
✅ **No redundancy** — Country attributes (CurrencyCode, PhoneCode) stored once per country

### Cons
❌ **More complex queries** — Need to filter by GeoType in most queries
❌ **Larger table** — All levels in one table (might get large, e.g., 200k+ rows globally)
❌ **Less normalized** — Some columns null for non-country rows
❌ **Harder to index** — Single table makes it harder to optimize specific lookups
❌ **Performance risk** — Tree queries can be slower on large datasets

---

## Option B: Keep 3 Separate Tables (Current Design)

### Structure
```sql
Countries
├─ CountryId (GUID)
├─ CountryCode2 (CHAR 2, unique) -- 'US', 'CA', 'GB'
├─ CountryCode3 (CHAR 3, unique) -- 'USA', 'CAN', 'GBR'
├─ CountryName (NVARCHAR 100)
├─ PhoneCode (VARCHAR 20)
├─ CurrencyCode (CHAR 3)
├─ TimeZone (VARCHAR 50, nullable)
├─ Population (BIGINT, nullable)
├─ Audit columns
└─ Indexes: (CountryCode2), (CountryCode3)

States
├─ StateId (GUID)
├─ FK → Countries (CountryId)
├─ StateCode (VARCHAR 10) -- 'CA', 'NY', 'ON'
├─ StateName (NVARCHAR 100)
├─ Latitude (DECIMAL 10,8)
├─ Longitude (DECIMAL 11,8)
├─ TimeZone (VARCHAR 50, nullable)
├─ Population (BIGINT, nullable)
├─ Audit columns
└─ Indexes: (CountryId), (StateCode, CountryId)

Cities
├─ CityId (GUID)
├─ FK → Countries (CountryId)
├─ FK → States (StateId, nullable)
├─ CityName (NVARCHAR 100)
├─ Latitude (DECIMAL 10,8)
├─ Longitude (DECIMAL 11,8)
├─ TimeZone (VARCHAR 50, nullable)
├─ Population (BIGINT, nullable)
├─ Audit columns
└─ Indexes: (CountryId), (StateId), (CityName)
```

### Queries
```sql
-- Get all cities in California
SELECT c.* FROM Cities c
INNER JOIN States s ON c.StateId = s.StateId
WHERE s.CountryId = @countryId
  AND s.StateCode = 'CA'

-- Get country for a city
SELECT c.* FROM Countries c
WHERE c.CountryId = @countryId

-- Get path: Country → State → City
SELECT c.CountryName, s.StateName, cy.CityName
FROM Cities cy
INNER JOIN States s ON cy.StateId = s.StateId
INNER JOIN Countries c ON s.CountryId = c.CountryId
WHERE cy.CityName = 'San Francisco'
```

### Pros
✅ **Normalized design** — Minimal redundancy, efficient storage
✅ **Clear semantics** — Each table has specific purpose
✅ **Fast lookups** — Optimized indexes for each level
✅ **Simple queries** — No GeoType filtering needed
✅ **Industry standard** — Most apps use this approach
✅ **Proven performance** — Used by Google Maps, Mapbox, etc.
✅ **Easy to understand** — Developers immediately know what's what

### Cons
❌ **Fixed structure** — Hard to add new levels (districts, neighborhoods, etc.)
❌ **Multiple FKs** — Address table needs FK to Cities (not States or Countries)
❌ **Redundancy** — Country attributes repeated if accessed via State or City
❌ **Schema change required** — To add new geo level, must alter tables
❌ **Less flexible** — Can't handle varying depth per country (e.g., some countries have 2 levels, some have 4)

---

## Option C: Hybrid (RECOMMENDED)

### Structure
```sql
-- Keep for reference, currency, phone code
Countries
├─ CountryId (GUID)
├─ CountryCode2 (CHAR 2, unique)
├─ CountryCode3 (CHAR 3, unique)
├─ CountryName (NVARCHAR 100)
├─ PhoneCode (VARCHAR 20)
├─ CurrencyCode (CHAR 3)
├─ TimeZone (VARCHAR 50, nullable)
├─ Population (BIGINT, nullable)
├─ Audit columns
└─ Indexes: (CountryCode2), (CountryCode3)

-- HierarchyId for flexible State/City/District structure
GeoHierarchy (for State/City and beyond)
├─ GeoHierarchyId (GUID)
├─ FK → Countries (CountryId) -- Link back to Countries
├─ NodePath (HierarchyId) -- /1/, /1/1/, /1/1/1/ (State, City, District)
├─ GeoType (VARCHAR 50) -- 'State', 'City', 'District', 'Neighborhood'
├─ Code (VARCHAR 20) -- 'CA', 'NY', etc.
├─ Name (NVARCHAR 100)
├─ Latitude (DECIMAL 10,8, nullable)
├─ Longitude (DECIMAL 11,8, nullable)
├─ TimeZone (VARCHAR 50, nullable)
├─ Population (BIGINT, nullable)
├─ IsActive (BIT)
├─ Audit columns
└─ Indexes: (CountryId, GeoType), (NodePath)

Example:
Countries:
├─ US (countryId=1)
├─ CA (countryId=2)
├─ UK (countryId=3)

GeoHierarchy (for US):
/1/                           States
├─ /1/1/                      California
│  ├─ /1/1/1/                 San Francisco
│  ├─ /1/1/2/                 Los Angeles
│  └─ /1/1/3/                 Bay Area (region/district)
├─ /1/2/                      New York
│  └─ /1/2/1/                 New York City
└─ /1/3/                      Texas

GeoHierarchy (for UK):
/1/                           England
├─ /1/1/                      Greater London
│  ├─ /1/1/1/                 Westminster
│  ├─ /1/1/2/                 Kensington & Chelsea
│  └─ /1/1/3/                 Tower Hamlets
├─ /1/2/                      Greater Manchester
│  └─ /1/2/1/                 Manchester
└─ /1/3/                      West Midlands
```

### Queries
```sql
-- Get all cities in California (USA)
SELECT gh.* FROM GeoHierarchy gh
WHERE gh.CountryId = (SELECT CountryId FROM Countries WHERE CountryCode2 = 'US')
  AND gh.GeoType = 'City'
  AND gh.NodePath.IsDescendantOf(
      (SELECT NodePath FROM GeoHierarchy WHERE CountryId = 1 AND Code = 'CA')
  )

-- Get country + full path for a city
SELECT c.CountryName, c.CurrencyCode, c.PhoneCode,
       (SELECT Name FROM GeoHierarchy WHERE NodePath = gh.NodePath.GetAncestor(1)) as State,
       gh.Name as City
FROM GeoHierarchy gh
INNER JOIN Countries c ON gh.CountryId = c.CountryId
WHERE gh.Name = 'San Francisco'
  AND gh.GeoType = 'City'

-- Get country reference data
SELECT * FROM Countries WHERE CountryCode2 = 'US'

-- Create Address FK: Can point to Cities (GeoHierarchy with GeoType='City')
-- Or allow FK to any GeoHierarchy level depending on needs
```

### Pros
✅ **Best of both** — Countries as reference + HierarchyId for flexibility
✅ **Clear semantics** — Countries table is obvious, hierarchy handles details
✅ **Fast country lookups** — Indexed Countries table for currency, phone code
✅ **Flexible geo hierarchy** — Add states, cities, districts per country without schema changes
✅ **Reduced redundancy** — Country data (currency, phone) stored once, referenced from GeoHierarchy
✅ **Address-friendly** — FK to GeoHierarchy (with GeoType='City' or any level)
✅ **Scalable** — Can handle varying depths per country
✅ **Future-proof** — Easy to add neighborhoods, districts, regions

### Cons
⚠️ **Slightly more complex** — Two tables instead of one, but simpler than Option B
⚠️ **Queries require join** — Need to join Countries + GeoHierarchy for full path

---

## Performance Comparison

| Operation | Option A (HierarchyId) | Option B (3 Tables) | Option C (Hybrid) |
|-----------|------------------------|-------------------|------------------|
| Get all cities in state | HierarchyId.IsDescendantOf() | JOIN query | JOIN + IsDescendantOf() |
| Get country reference data | Filter on GeoType | Direct lookup | Direct lookup (faster) |
| Get full path (Country → State → City) | Multiple GeoType filters | Multiple JOINs | Smart JOIN |
| Add new geo level | No schema change | ALTER TABLE | No schema change |
| Query complexity | Medium | High | Low-Medium |
| Storage size | Large (all levels in 1 table) | Small (normalized) | Medium |
| Index efficiency | Good for trees | Excellent for each level | Good overall |
| Future flexibility | Excellent | Poor | Excellent |

---

## Recommendation: **OPTION C (HYBRID)** ⭐

### Why Option C is Best for v4:

1. **Keep Countries Table** ✅
   - Fast lookups for currency, phone code (most common)
   - Clear reference data
   - Used by Address FK when you need country-level access
   - Indexed for speed

2. **Use GeoHierarchy for State/City/Beyond** ✅
   - Flexible for different depths per country
   - No schema changes as you add levels
   - HierarchyId efficient for tree queries
   - Can add districts, neighborhoods, regions without altering schema

3. **Minimal Complexity** ✅
   - Only 2 tables (instead of 1 or 3)
   - Clear separation: reference data (Countries) vs hierarchical data (GeoHierarchy)
   - Queries are straightforward

4. **Address Linking** ✅
   - FK → GeoHierarchy (for City-level addresses)
   - Option to FK → Countries (for country-level addresses in some cases)
   - Flexible and correct

5. **Real-World Flexibility** ✅
   - Some countries have States (USA, Canada, India)
   - Some have Provinces (Spain)
   - Some have Districts (UK, Germany)
   - Some have varying depths (Australia has States + Territories)
   - Option C handles all without schema changes

---

## Implementation - Option C

### SQL (Simplified)
```sql
-- Master.Countries (reference data)
CREATE TABLE Master.Countries (
    CountryId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    CountryCode2 CHAR(2) UNIQUE NOT NULL,
    CountryCode3 CHAR(3) UNIQUE NOT NULL,
    CountryName NVARCHAR(100) NOT NULL,
    PhoneCode VARCHAR(20),
    CurrencyCode CHAR(3),
    TimeZone VARCHAR(50),
    Population BIGINT,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    CreatedBy UNIQUEIDENTIFIER,
    UpdatedBy UNIQUEIDENTIFIER,
    IsDeleted BIT DEFAULT 0,
    INDEX idx_CountryCode2 (CountryCode2),
    INDEX idx_CountryCode3 (CountryCode3)
)

-- Master.GeoHierarchy (for State/City/District/Neighborhood)
CREATE TABLE Master.GeoHierarchy (
    GeoHierarchyId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    CountryId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Countries(CountryId),
    NodePath HierarchyId NOT NULL,
    GeoType VARCHAR(50) NOT NULL, -- 'State', 'City', 'District', 'Neighborhood'
    Code VARCHAR(20) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Latitude DECIMAL(10,8),
    Longitude DECIMAL(11,8),
    TimeZone VARCHAR(50),
    Population BIGINT,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    CreatedBy UNIQUEIDENTIFIER,
    UpdatedBy UNIQUEIDENTIFIER,
    IsDeleted BIT DEFAULT 0,
    INDEX idx_NodePath (NodePath),
    INDEX idx_CountryId_GeoType (CountryId, GeoType),
    INDEX idx_Code (Code)
)

-- Core.Addresses (polymorphic)
CREATE TABLE Core.Addresses (
    AddressId UNIQUEIDENTIFIER PRIMARY KEY,
    EntityType VARCHAR(50) NOT NULL,
    EntityId UNIQUEIDENTIFIER NOT NULL,
    AddressType VARCHAR(50),
    Street1 NVARCHAR(255),
    Street2 NVARCHAR(255),
    City NVARCHAR(100),
    FK_GeoHierarchyId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES Master.GeoHierarchy(GeoHierarchyId), -- Link to City-level geo
    FK_CountryId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES Master.Countries(CountryId), -- Direct country link
    PostalCode VARCHAR(20),
    Latitude DECIMAL(10,8),
    Longitude DECIMAL(11,8),
    ...
)
```

### Benefits Over Current Design

| Current (Option B) | Option C (Proposed) |
|---|---|
| Countries + States + Cities (3 tables) | Countries + GeoHierarchy (2 tables) |
| Hard to add districts/neighborhoods | Easy to add any level |
| Schema change required for new level | No schema changes needed |
| Address FK to Cities only | Address FK to any geo level |
| Less flexible for real-world data | Handles all geo structures globally |

---

## Summary

| Approach | Best For | Trade-off |
|----------|----------|-----------|
| **Option A** (HierarchyId only) | Ultra-flexible, unlimited nesting | More complex queries, larger table |
| **Option B** (3 Tables) | Simple, normalized, fast lookups | Hard to extend, schema-locked |
| **Option C** (Hybrid) ⭐ | Balance of simplicity + flexibility | Slight join complexity, but minimal |

**RECOMMENDATION: Go with Option C (Hybrid)**
- Keep Countries for fast reference data lookups
- Use GeoHierarchy for flexible State/City/District structure
- 2 tables total
- Best balance of performance, flexibility, and maintainability

---

**Ready to update schema design with Option C?** Let me know and I'll update SCHEMA-REVIEW-v2.md immediately!
