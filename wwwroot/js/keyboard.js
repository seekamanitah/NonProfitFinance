// Keyboard Shortcuts Manager
window.keyboardShortcuts = {
    dotNetRef: null,
    enabled: true,

    initialize: function (dotNetReference) {
        this.dotNetRef = dotNetReference;
        this.attachListeners();
        console.log('Keyboard shortcuts initialized');
    },

    attachListeners: function () {
        document.addEventListener('keydown', (e) => {
            if (!this.enabled || !this.dotNetRef) return;

            // Don't trigger shortcuts when typing in inputs (except Escape)
            const isInput = ['INPUT', 'TEXTAREA', 'SELECT'].includes(e.target.tagName);
            const isContentEditable = e.target.isContentEditable;
            
            // Allow Escape to work everywhere
            if (e.key === 'Escape') {
                e.preventDefault();
                e.stopPropagation();
                this.invokeShortcut('escape', e.ctrlKey, e.altKey, e.shiftKey);
                return;
            }
            
            // Don't trigger other shortcuts when typing
            if (isInput || isContentEditable) return;

            const key = e.key.toLowerCase();
            const ctrl = e.ctrlKey || e.metaKey; // metaKey for Mac Command
            const alt = e.altKey;
            const shift = e.shiftKey;

            // Track if we handled the shortcut
            let handled = false;

            // Global shortcuts - Prevent default BEFORE invoking
            if (ctrl && key === 'n') {
                e.preventDefault();
                e.stopPropagation();
                this.invokeShortcut('ctrl+n', ctrl, alt, shift);
                handled = true;
            }
            else if (ctrl && key === 'f') {
                e.preventDefault();
                e.stopPropagation();
                this.invokeShortcut('ctrl+f', ctrl, alt, shift);
                handled = true;
            }
            else if (ctrl && key === 's') {
                e.preventDefault();
                e.stopPropagation();
                this.invokeShortcut('ctrl+s', ctrl, alt, shift);
                handled = true;
            }
            else if (ctrl && key === 'p') {
                // Print
                e.preventDefault();
                e.stopPropagation();
                this.invokeShortcut('ctrl+p', ctrl, alt, shift);
                handled = true;
            }
            else if (ctrl && key === 'k') {
                // Quick search
                e.preventDefault();
                e.stopPropagation();
                this.invokeShortcut('ctrl+k', ctrl, alt, shift);
                handled = true;
            }
            else if (key === '?' && shift) {
                // Help (Shift + /)
                e.preventDefault();
                e.stopPropagation();
                this.invokeShortcut('shift+?', ctrl, alt, shift);
                handled = true;
            }
            else if (ctrl && shift && key === 's') {
                // TTS Toggle
                e.preventDefault();
                e.stopPropagation();
                this.invokeShortcut('ctrl+shift+s', ctrl, alt, shift);
                handled = true;
            }
            else if (ctrl && shift && key === 'x') {
                // Stop TTS
                e.preventDefault();
                e.stopPropagation();
                this.invokeShortcut('ctrl+shift+x', ctrl, alt, shift);
                handled = true;
            }
            
            // Return whether we handled the event
            return handled ? false : true;
        }, true); // Use capture phase to catch before other handlers
    },

    invokeShortcut: function (key, ctrl, alt, shift) {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('HandleKeyPress', key, ctrl, alt, shift);
        }
    },

    enable: function () {
        this.enabled = true;
    },

    disable: function () {
        this.enabled = false;
    },

    // Focus management
    focusFirst: function (selector) {
        const element = document.querySelector(selector);
        if (element) {
            setTimeout(() => element.focus(), 100);
        }
    },

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

    // Announce to screen readers
    announce: function (message, priority = 'polite') {
        const announcement = document.createElement('div');
        announcement.setAttribute('role', 'status');
        announcement.setAttribute('aria-live', priority);
        announcement.setAttribute('aria-atomic', 'true');
        announcement.className = 'sr-only';
        announcement.textContent = message;
        
        document.body.appendChild(announcement);
        
        setTimeout(() => {
            document.body.removeChild(announcement);
        }, 1000);
    }
};

// High contrast mode detection
window.accessibilityHelpers = {
    isHighContrast: function () {
        return window.matchMedia('(prefers-contrast: high)').matches;
    },

    isReducedMotion: function () {
        return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    },

    applyHighContrast: function (enable) {
        if (enable) {
            document.body.classList.add('high-contrast');
        } else {
            document.body.classList.remove('high-contrast');
        }
    },

    applyReducedMotion: function (enable) {
        if (enable) {
            document.body.classList.add('reduced-motion');
        } else {
            document.body.classList.remove('reduced-motion');
        }
    }
};
