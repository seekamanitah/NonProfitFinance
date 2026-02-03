// Theme listener module for Blazor components
export function setupThemeListener(dotNetHelper) {
    // Listen for theme changes from the custom event
    const themeChangedHandler = (event) => {
        const newTheme = event.detail.theme;
        dotNetHelper.invokeMethodAsync('OnThemeChanged', newTheme);
    };
    
    window.addEventListener('themeChanged', themeChangedHandler);
    
    // Return a cleanup function
    return {
        dispose: () => {
            window.removeEventListener('themeChanged', themeChangedHandler);
        }
    };
}
