// Text-to-Speech functionality using browser's Speech Synthesis API
window.textToSpeech = {
    // Check if speech synthesis is supported
    isSupported: function() {
        return 'speechSynthesis' in window;
    },

    // Speak text with specified rate
    speak: function(text, rate = 1.0) {
        if (!this.isSupported()) {
            console.warn('Speech synthesis not supported in this browser');
            return;
        }

        // Cancel any ongoing speech
        window.speechSynthesis.cancel();

        const utterance = new SpeechSynthesisUtterance(text);
        utterance.rate = rate;
        utterance.pitch = 1.0;
        utterance.volume = 1.0;

        // Try to use a specific voice if available
        const voices = window.speechSynthesis.getVoices();
        if (voices.length > 0) {
            // Prefer English voices
            const englishVoice = voices.find(v => v.lang.startsWith('en-'));
            if (englishVoice) {
                utterance.voice = englishVoice;
            }
        }

        window.speechSynthesis.speak(utterance);
    },

    // Stop any ongoing speech
    stop: function() {
        if (this.isSupported()) {
            window.speechSynthesis.cancel();
        }
    },

    // Pause speech
    pause: function() {
        if (this.isSupported()) {
            window.speechSynthesis.pause();
        }
    },

    // Resume paused speech
    resume: function() {
        if (this.isSupported()) {
            window.speechSynthesis.resume();
        }
    },

    // Get available voices
    getVoices: function() {
        if (!this.isSupported()) {
            return [];
        }
        return window.speechSynthesis.getVoices().map(v => ({
            name: v.name,
            lang: v.lang,
            default: v.default
        }));
    }
};

// Load voices (some browsers require this)
if ('speechSynthesis' in window) {
    window.speechSynthesis.onvoiceschanged = function() {
        window.speechSynthesis.getVoices();
    };
}
