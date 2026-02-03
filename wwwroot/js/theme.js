// Theme Manager for Dark/Light Mode
const themeManager = {
    currentTheme: 'light',
    initialized: false,
    listeners: [],

    init: function() {
        if (this.initialized) {
            return this.currentTheme;
        }
        
        // Check saved theme or system preference
        const savedTheme = localStorage.getItem('theme');
        if (savedTheme) {
            this.currentTheme = savedTheme;
        } else {
            // Check system preference
            if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
                this.currentTheme = 'dark';
            }
        }
        
        this.applyTheme(this.currentTheme);
        this.initialized = true;
        
        // Dispatch custom event for components to listen to
        window.dispatchEvent(new CustomEvent('themeInitialized', { detail: { theme: this.currentTheme } }));
        
        return this.currentTheme;
    },

    getTheme: function() {
        // Ensure initialized before returning
        if (!this.initialized) {
            this.init();
        }
        return this.currentTheme;
    },

    setTheme: function(theme) {
        if (!['light', 'dark'].includes(theme)) {
            console.warn(`Invalid theme: ${theme}. Using 'light'.`);
            theme = 'light';
        }
        
        this.currentTheme = theme;
        localStorage.setItem('theme', theme);
        this.applyTheme(theme);
        
        // Dispatch custom event so Blazor components can listen
        window.dispatchEvent(new CustomEvent('themeChanged', { detail: { theme: theme } }));
        
        // Call registered listeners (for Blazor interop)
        this.listeners.forEach(callback => {
            try {
                callback(theme);
            } catch (e) {
                console.error('Error in theme change listener:', e);
            }
        });
        
        return theme;
    },

    applyTheme: function(theme) {
        const html = document.documentElement;
        if (theme === 'dark') {
            html.setAttribute('data-theme', 'dark');
            document.body.classList.add('dark-mode');
            document.body.classList.remove('light-mode');
        } else {
            html.removeAttribute('data-theme');
            document.body.classList.add('light-mode');
            document.body.classList.remove('dark-mode');
        }
        
        // Force CSS recalculation
        void html.offsetHeight; // Trigger reflow
        
        // Trigger resize to recalculate layouts if needed
        window.dispatchEvent(new Event('resize'));
    },

    toggle: function() {
        const newTheme = this.currentTheme === 'light' ? 'dark' : 'light';
        this.setTheme(newTheme);
        return newTheme;
    },

    // Register a callback to be called when theme changes (for Blazor interop)
    onThemeChange: function(callback) {
        if (typeof callback === 'function') {
            this.listeners.push(callback);
        }
    }
};

// Export to window for access from other scripts
window.themeManager = themeManager;

// Initialize immediately if DOM is ready, otherwise wait for DOMContentLoaded
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        themeManager.init();
    });
} else {
    // DOM is already loaded (can happen in Blazor)
    themeManager.init();
}
