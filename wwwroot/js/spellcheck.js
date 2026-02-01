// Spell Check Manager for NonProfit Finance
window.spellCheck = {
    enabled: true,
    customDictionary: [],
    
    /**
     * Initialize spell checker
     * @param {boolean} enabled - Whether spell check is enabled
     * @param {string[]} customWords - Custom dictionary words
     */
    initialize: function(enabled, customWords) {
        this.enabled = enabled;
        this.customDictionary = customWords || [];
        
        if (enabled) {
            this.applyToAllInputs();
            this.setupMutationObserver();
        }
        
        console.log('Spell check initialized:', enabled ? 'enabled' : 'disabled');
    },
    
    /**
     * Enable or disable spell checking
     * @param {boolean} enabled
     */
    setEnabled: function(enabled) {
        this.enabled = enabled;
        
        if (enabled) {
            this.applyToAllInputs();
        } else {
            this.removeFromAllInputs();
        }
    },
    
    /**
     * Apply spell check to all text inputs and textareas
     */
    applyToAllInputs: function() {
        if (!this.enabled) return;
        
        // Select all text inputs, textareas, and contenteditable elements
        const elements = document.querySelectorAll(
            'input[type="text"], input[type="search"], textarea, [contenteditable="true"]'
        );
        
        elements.forEach(element => {
            // Don't enable on password, email, number inputs
            const type = element.getAttribute('type');
            if (type === 'password' || type === 'email' || type === 'number' || 
                type === 'tel' || type === 'url') {
                return;
            }
            
            // Don't enable on elements with data-nospellcheck attribute
            if (element.hasAttribute('data-nospellcheck')) {
                element.setAttribute('spellcheck', 'false');
                return;
            }
            
            // Enable spellcheck
            element.setAttribute('spellcheck', 'true');
            element.setAttribute('lang', 'en-US');
        });
    },
    
    /**
     * Remove spell check from all inputs
     */
    removeFromAllInputs: function() {
        const elements = document.querySelectorAll('[spellcheck="true"]');
        elements.forEach(element => {
            element.setAttribute('spellcheck', 'false');
        });
    },
    
    /**
     * Setup mutation observer to apply spell check to dynamically added elements
     */
    setupMutationObserver: function() {
        if (this.observer) {
            this.observer.disconnect();
        }
        
        this.observer = new MutationObserver((mutations) => {
            if (this.enabled) {
                this.applyToAllInputs();
            }
        });
        
        this.observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    },
    
    /**
     * Add word to custom dictionary
     * @param {string} word
     */
    addToCustomDictionary: function(word) {
        if (word && !this.customDictionary.includes(word)) {
            this.customDictionary.push(word);
            
            // Store in localStorage for persistence
            try {
                localStorage.setItem('spellcheck_custom_dictionary', 
                    JSON.stringify(this.customDictionary));
            } catch (e) {
                console.warn('Could not save custom dictionary:', e);
            }
        }
    },
    
    /**
     * Load custom dictionary from localStorage
     */
    loadCustomDictionary: function() {
        try {
            const stored = localStorage.getItem('spellcheck_custom_dictionary');
            if (stored) {
                const words = JSON.parse(stored);
                this.customDictionary = [...this.customDictionary, ...words];
            }
        } catch (e) {
            console.warn('Could not load custom dictionary:', e);
        }
    },
    
    /**
     * Enable spell check on specific element
     * @param {string} selector - CSS selector
     */
    enableOnElement: function(selector) {
        const element = document.querySelector(selector);
        if (element) {
            element.setAttribute('spellcheck', 'true');
            element.setAttribute('lang', 'en-US');
        }
    },
    
    /**
     * Disable spell check on specific element
     * @param {string} selector - CSS selector
     */
    disableOnElement: function(selector) {
        const element = document.querySelector(selector);
        if (element) {
            element.setAttribute('spellcheck', 'false');
        }
    },
    
    /**
     * Context menu handler for "Add to Dictionary"
     */
    setupContextMenu: function() {
        document.addEventListener('contextmenu', (e) => {
            const target = e.target;
            
            // Check if target is a text input with spell check enabled
            if ((target.tagName === 'INPUT' || target.tagName === 'TEXTAREA') &&
                target.getAttribute('spellcheck') === 'true') {
                
                // Get selected text
                const selectedText = window.getSelection().toString().trim();
                
                if (selectedText) {
                    // You could show a custom context menu here
                    // For now, browser's built-in context menu will handle "Add to Dictionary"
                }
            }
        });
    }
};

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        spellCheck.loadCustomDictionary();
        spellCheck.setupContextMenu();
    });
} else {
    spellCheck.loadCustomDictionary();
    spellCheck.setupContextMenu();
}

// Helper: Create custom attribute directive for Vue/React-like spell check control
// Usage in HTML: <input type="text" data-spellcheck="true" />
document.addEventListener('DOMContentLoaded', () => {
    const observer = new MutationObserver((mutations) => {
        mutations.forEach((mutation) => {
            mutation.addedNodes.forEach((node) => {
                if (node.nodeType === 1) { // Element node
                    // Check for data-spellcheck attribute
                    if (node.hasAttribute && node.hasAttribute('data-spellcheck')) {
                        const enabled = node.getAttribute('data-spellcheck') === 'true';
                        node.setAttribute('spellcheck', enabled ? 'true' : 'false');
                    }
                }
            });
        });
    });
    
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
});
