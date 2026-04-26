module.exports = {
  sidebar: [
    {
      label: 'Planning',
      collapsed: false,
      items: [
        { type: 'doc', id: 'plan/index' },
        {
          label: 'Design Specs',
          collapsed: true,
          items: [
            { type: 'doc', id: 'plan/specs/2026-03-31-v4-phase1-implementation-plan' },
            { type: 'doc', id: 'plan/specs/2026-04-01-razor-pages-htmx-interactive-pattern' },
            { type: 'doc', id: 'plan/specs/2026-04-01-seo-admin-portal-decision' },
            { type: 'doc', id: 'plan/specs/2026-04-17-admin-auth-optional-design' },
            { type: 'doc', id: 'plan/specs/2026-04-17-lov-consolidation-design' },
            { type: 'doc', id: 'plan/specs/2026-04-18-shared-components-design' },
            { type: 'doc', id: 'plan/specs/2026-04-18-smartworkz-core-ecosystem-design' },
            { type: 'doc', id: 'plan/specs/2026-04-19-grid-component-design' },
            { type: 'doc', id: 'plan/specs/2026-04-20-multi-view-data-components-design' },
            { type: 'doc', id: 'plan/specs/2026-04-23-core-web-framework-design' },
            { type: 'doc', id: 'plan/specs/2026-04-24-core-web-phase2-blazor-design' },
            { type: 'doc', id: 'plan/specs/2026-04-25-docusaurus-wiki-generator-design' },
          ]
        },
        {
          label: 'Implementation Plans',
          collapsed: true,
          items: [
            { type: 'doc', id: 'plan/implementations/2026-04-17-admin-auth-optional' },
            { type: 'doc', id: 'plan/implementations/2026-04-17-lov-v2-backend' },
            { type: 'doc', id: 'plan/implementations/2026-04-18-smartworkz-core-phase1' },
            { type: 'doc', id: 'plan/implementations/2026-04-19-grid-component-implementation' },
            { type: 'doc', id: 'plan/implementations/2026-04-20-multi-view-data-components' },
            { type: 'doc', id: 'plan/implementations/2026-04-21-complete-architecture-roadmap' },
            { type: 'doc', id: 'plan/implementations/2026-04-22-phase4-refinements' },
            { type: 'doc', id: 'plan/implementations/2026-04-22-phase5-extended-capabilities' },
            { type: 'doc', id: 'plan/implementations/2026-04-23-core-web-phase1-implementation' },
            { type: 'doc', id: 'plan/implementations/2026-04-24-core-web-phase2-blazor-implementation' },
            { type: 'doc', id: 'plan/implementations/2026-04-24-phase2-week1-infrastructure' },
            { type: 'doc', id: 'plan/implementations/2026-04-24-smartworkz-macos-phase5-3' },
            { type: 'doc', id: 'plan/implementations/2026-04-24-smartworkz-windows-phase5-4' },
            { type: 'doc', id: 'plan/implementations/2026-04-25-docusaurus-wiki-generator-implementation' },
          ]
        }
      ]
    },
    {
      label: 'Framework',
      collapsed: false,
      items: [
        { type: 'doc', id: 'framework/index', label: 'Overview' },
      ]
    },
    {
      label: 'Guides',
      collapsed: false,
      items: [
        { type: 'doc', id: 'DEVELOPER' },
        { type: 'doc', id: 'QUICK_REFERENCE' },
      ]
    }
  ]
};
