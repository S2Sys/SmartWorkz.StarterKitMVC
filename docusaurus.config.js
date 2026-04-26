const fs = require('fs');
const path = require('path');

module.exports = {
  title: 'SmartWorkz Documentation',
  tagline: 'Comprehensive framework and platform documentation',
  favicon: 'img/favicon.ico',
  url: 'https://docs.smartworkz.dev',
  baseUrl: '/',
  organizationName: 'S2Sys',
  projectName: 'SmartWorkz.StarterKitMVC',
  onBrokenLinks: 'warn',
  onBrokenMarkdownLinks: 'warn',
  i18n: { defaultLocale: 'en', locales: ['en'] },
  presets: [
    [
      '@docusaurus/preset-classic',
      {
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          editUrl: 'https://github.com/S2Sys/SmartWorkz.StarterKitMVC/edit/main/',
        },
        blog: false,
        theme: { customCss: require.resolve('./src/css/custom.css') },
      },
    ],
  ],
  themeConfig: {
    navbar: {
      title: 'SmartWorkz',
      items: [
        { to: '/docs/plan', label: '📋 Planning', position: 'left' },
        { to: '/docs/framework', label: '🏗️ Framework', position: 'left' },
        { to: '/docs/guides', label: '📖 Guides', position: 'left' },
        { href: 'https://github.com/S2Sys/SmartWorkz.StarterKitMVC', label: 'GitHub', position: 'right' },
      ],
    },
    footer: {
      style: 'dark',
      copyright: `© ${new Date().getFullYear()} S2Sys. Built with Docusaurus.`,
    },
    prism: {
      additionalLanguages: ['csharp', 'xml'],
    },
  },
};
