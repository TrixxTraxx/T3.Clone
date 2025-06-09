window.getWindowWidth = function() {
    return window.innerWidth;
};

// Copy text to clipboard
window.copyToClipboard = async function(text) {
    try {
        await navigator.clipboard.writeText(text);
        console.log('Text copied to clipboard');
    } catch (err) {
        console.error('Failed to copy text: ', err);
        // Fallback for older browsers
        const textArea = document.createElement("textarea");
        textArea.value = text;
        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();
        try {
            document.execCommand('copy');
            console.log('Text copied to clipboard (fallback)');
        } catch (err) {
            console.error('Fallback copy failed: ', err);
        }
        document.body.removeChild(textArea);
    }
};

// Auto-scroll to bottom of chat
window.scrollChatToBottom = function() {
    const chatContainer = document.querySelector('.chat-messages-container');
    if (chatContainer) {
        chatContainer.scrollTop = chatContainer.scrollHeight;
    }
};

// Focus on input when page loads
window.focusChatInput = function() {
    const input = document.querySelector('.message-input input');
    if (input) {
        input.focus();
    }
};

// Textarea auto-resize functions for ChatInput
window.initializeTextareaAutoResize = function(textarea) {
    if (!textarea) return;
    
    // Set initial height
    textarea.style.height = 'auto';
    textarea.style.height = Math.max(textarea.scrollHeight, 52) + 'px';
};

window.autoResizeTextarea = function(textarea) {
    if (!textarea) return;
    
    // Reset height to auto to get the correct scrollHeight
    textarea.style.height = 'auto';
    
    // Calculate new height (min 52px, max 200px)
    const newHeight = Math.min(Math.max(textarea.scrollHeight, 52), 200);
    textarea.style.height = newHeight + 'px';
    
    // If content exceeds max height, show scrollbar
    if (textarea.scrollHeight > 200) {
        textarea.style.overflowY = 'auto';
    } else {
        textarea.style.overflowY = 'hidden';
    }
};

window.resetTextareaHeight = function(textarea) {
    if (!textarea) return;
    
    textarea.style.height = '52px';
    textarea.style.overflowY = 'hidden';
};