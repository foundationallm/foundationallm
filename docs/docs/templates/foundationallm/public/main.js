export default {
    defaultTheme: 'light',
    // Ensure the left-hand (sidebar) TOC is enabled in the modern theme.
    // (DocFX will ignore unknown keys; keeping these explicit prevents
    // accidental disablement when overriding theme defaults.)
    disableToc: false,
    disableSideToc: false,
    disableSidebar: false,
    disableSideFilter: false,
    iconLinks: [
      {
        icon: 'github',
        href: 'https://github.com/foundationallm/foundationallm',
        title: 'GitHub'
      }
    ]
  }