// Theme Manager for Dark/Light Mode
const themeManager = {
    currentTheme: 'light',

    init: function() {
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
    },

    getTheme: function() {
        return this.currentTheme;
    },

    setTheme: function(theme) {
        this.currentTheme = theme;
        localStorage.setItem('theme', theme);
        this.applyTheme(theme);
    },

    applyTheme: function(theme) {
        if (theme === 'dark') {
            document.documentElement.setAttribute('data-theme', 'dark');
        } else {
            document.documentElement.removeAttribute('data-theme');
        }
    },

    toggle: function() {
        const newTheme = this.currentTheme === 'light' ? 'dark' : 'light';
        this.setTheme(newTheme);
        return newTheme;
    }
};

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    themeManager.init();
});
