// Modal Focus Management for Accessibility (WCAG 2.1 AA)
window.modalFocus = {
    previouslyFocused: null,
    focusableSelector: 'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])',

    // Trap focus within modal
    trapFocus: function (modalElement) {
        if (!modalElement) return;

        // Store previously focused element
        this.previouslyFocused = document.activeElement;

        // Get all focusable elements
        const focusables = modalElement.querySelectorAll(this.focusableSelector);
        if (focusables.length === 0) return;

        const firstFocusable = focusables[0];
        const lastFocusable = focusables[focusables.length - 1];

        // Focus first element
        setTimeout(() => firstFocusable.focus(), 50);

        // Handle Tab key to trap focus
        modalElement.addEventListener('keydown', function trapHandler(e) {
            if (e.key !== 'Tab') return;

            if (e.shiftKey) {
                // Shift+Tab
                if (document.activeElement === firstFocusable) {
                    e.preventDefault();
                    lastFocusable.focus();
                }
            } else {
                // Tab
                if (document.activeElement === lastFocusable) {
                    e.preventDefault();
                    firstFocusable.focus();
                }
            }
        });
    },

    // Restore focus when modal closes
    restoreFocus: function () {
        if (this.previouslyFocused && typeof this.previouslyFocused.focus === 'function') {
            this.previouslyFocused.focus();
            this.previouslyFocused = null;
        }
    },

    // Initialize modal (call from Blazor)
    init: function (modalSelector) {
        const modal = document.querySelector(modalSelector);
        if (modal) {
            this.trapFocus(modal);
        }
    },

    // Close modal and restore focus
    close: function () {
        this.restoreFocus();
    }
};

// Auto-initialize modals when they appear
const observer = new MutationObserver((mutations) => {
    mutations.forEach((mutation) => {
        mutation.addedNodes.forEach((node) => {
            if (node.nodeType === 1) {
                // Check if it's a modal backdrop
                if (node.classList && node.classList.contains('modal-backdrop')) {
                    const modal = node.querySelector('.modal');
                    if (modal) {
                        window.modalFocus.trapFocus(modal);
                    }
                }
            }
        });

        mutation.removedNodes.forEach((node) => {
            if (node.nodeType === 1) {
                if (node.classList && node.classList.contains('modal-backdrop')) {
                    window.modalFocus.restoreFocus();
                }
            }
        });
    });
});

// Start observing
document.addEventListener('DOMContentLoaded', () => {
    observer.observe(document.body, { childList: true, subtree: true });
});
