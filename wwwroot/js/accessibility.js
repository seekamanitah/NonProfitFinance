// Accessibility Helpers for NonProfit Finance
// Ensure we always have both a global and local reference.
window.accessibilityHelpers = window.accessibilityHelpers || {
    
    /**
     * Set UI scale for the entire application
     * @param {number} scale - Scale factor (0.8 to 1.5)
     */
    setUiScale: function(scale) {
        // Clamp scale between 0.8 and 1.5
        scale = Math.max(0.8, Math.min(1.5, scale));
        
        // Apply zoom to root element
        document.documentElement.style.fontSize = `${scale * 16}px`; // Base is 16px
        
        // Save to localStorage
        localStorage.setItem('uiScale', scale.toString());
        
        console.log('UI scale set to:', scale);
    },
    
    /**
     * Load and apply saved UI scale
     */
    loadUiScale: function() {
        try {
            const savedScale = localStorage.getItem('uiScale');
            if (savedScale) {
                const scale = parseFloat(savedScale);
                if (scale >= 0.8 && scale <= 1.5) {
                    this.setUiScale(scale);
                }
            }
        } catch (e) {
            console.error('Error loading UI scale:', e);
        }
    },
    
    /**
     * Check if high contrast mode is enabled (OS level)
     */
    isHighContrast: function () {
        return window.matchMedia('(prefers-contrast: high)').matches;
    },

    /**
     * Check if reduced motion is preferred (OS level)
     */
    isReducedMotion: function () {
        return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    },

    /**
     * Apply high contrast mode class to body
     */
    applyHighContrast: function (enable) {
        if (enable) {
            document.body.classList.add('high-contrast');
        } else {
            document.body.classList.remove('high-contrast');
        }
    },

    /**
     * Apply reduced motion class to body
     */
    applyReducedMotion: function (enable) {
        if (enable) {
            document.body.classList.add('reduced-motion');
        } else {
            document.body.classList.remove('reduced-motion');
        }
    },
    
    /**
     * Focus trap for modal dialogs
     */
    trapFocus: function (containerSelector) {
        const container = document.querySelector(containerSelector);
        if (!container) return;

        const focusableElements = container.querySelectorAll(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );
        
        if (focusableElements.length === 0) return;

        const firstElement = focusableElements[0];
        const lastElement = focusableElements[focusableElements.length - 1];

        container.addEventListener('keydown', function(e) {
            if (e.key !== 'Tab') return;

            if (e.shiftKey) {
                if (document.activeElement === firstElement) {
                    e.preventDefault();
                    lastElement.focus();
                }
            } else {
                if (document.activeElement === lastElement) {
                    e.preventDefault();
                    firstElement.focus();
                }
            }
        });

        firstElement.focus();
    },
    
    /**
     * Announce message to screen readers
     */
    announce: function (message, priority = 'polite') {
        const announcement = document.createElement('div');
        announcement.setAttribute('role', 'status');
        announcement.setAttribute('aria-live', priority);
        announcement.setAttribute('aria-atomic', 'true');
        announcement.className = 'sr-only visually-hidden';
        announcement.textContent = message;
        
        document.body.appendChild(announcement);
        
        setTimeout(() => {
            document.body.removeChild(announcement);
        }, 1000);
    }
};

// Local alias used by code below (some pages may reference accessibilityHelpers directly)
const accessibilityHelpers = window.accessibilityHelpers;

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    // Ensure we have the helpers object
    const helpers = window.accessibilityHelpers;
    if (!helpers) return;
    
    // Load saved UI scale
    if (typeof helpers.loadUiScale === 'function') {
        helpers.loadUiScale();
    }
    
    // Check OS-level preferences
    if (typeof helpers.isHighContrast === 'function' && helpers.isHighContrast()) {
        helpers.applyHighContrast(true);
    }
    
    if (typeof helpers.isReducedMotion === 'function' && helpers.isReducedMotion()) {
        helpers.applyReducedMotion(true);
    }
    
    // Listen for preference changes
    window.matchMedia('(prefers-contrast: high)').addEventListener('change', (e) => {
        if (typeof accessibilityHelpers.applyHighContrast === 'function') {
            accessibilityHelpers.applyHighContrast(e.matches);
        }
    });
    
    window.matchMedia('(prefers-reduced-motion: reduce)').addEventListener('change', (e) => {
        if (typeof accessibilityHelpers.applyReducedMotion === 'function') {
            accessibilityHelpers.applyReducedMotion(e.matches);
        }
    });
});
