// Form change detection and navigation warning
// Prevents accidental navigation away from unsaved forms

window.formProtection = {
    isDirty: false,
    
    // Initialize protection for the page
    init: function() {
        window.addEventListener('beforeunload', this.handleBeforeUnload.bind(this));
        return true;
    },
    
    // Mark form as having unsaved changes
    setDirty: function(dirty) {
        this.isDirty = dirty;
        return true;
    },
    
    // Check if form has unsaved changes
    getDirty: function() {
        return this.isDirty;
    },
    
    // Handle browser navigation/refresh
    handleBeforeUnload: function(e) {
        if (this.isDirty) {
            e.preventDefault();
            e.returnValue = 'You have unsaved changes. Are you sure you want to leave?';
            return e.returnValue;
        }
    },
    
    // Cleanup when component is disposed
    dispose: function() {
        this.isDirty = false;
        window.removeEventListener('beforeunload', this.handleBeforeUnload);
        return true;
    },
    
    // Show confirmation dialog for Blazor navigation
    confirmNavigation: function(message) {
        if (!this.isDirty) {
            return true;
        }
        return confirm(message || 'You have unsaved changes. Are you sure you want to leave this page?');
    }
};

// Currency formatting utilities
window.currencyFormat = {
    // Format a number as currency for display
    format: function(value, locale = 'en-US', currency = 'USD') {
        if (value === null || value === undefined || isNaN(value)) return '';
        return new Intl.NumberFormat(locale, {
            style: 'currency',
            currency: currency,
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }).format(value);
    },
    
    // Parse a currency string back to a number
    parse: function(value) {
        if (!value) return 0;
        // Remove currency symbols, commas, and spaces
        const cleaned = value.toString().replace(/[^0-9.-]/g, '');
        const parsed = parseFloat(cleaned);
        return isNaN(parsed) ? 0 : parsed;
    },
    
    // Format number with thousand separators (no currency symbol)
    formatNumber: function(value) {
        if (value === null || value === undefined || isNaN(value)) return '';
        return new Intl.NumberFormat('en-US', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }).format(value);
    }
};
