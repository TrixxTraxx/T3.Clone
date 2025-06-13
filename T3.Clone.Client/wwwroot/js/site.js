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

// Set history state for navigation
window.setHistory = function(url) {
    if (typeof history.pushState === 'function') {
        history.pushState(null, '', url);
    } else {
        console.warn('History API not supported');
    }
}

// Download file from URL
window.downloadFile = function(url, filename) {
    try {
        const link = document.createElement('a');
        link.href = url;
        link.download = filename || 'download';
        link.style.display = 'none';
        
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        
        console.log('File download initiated:', filename);
    } catch (err) {
        console.error('Failed to download file:', err);
    }
}

// Trigger file input selection
window.triggerFileInput = function(supportedContentTypes) {
    try {
        const fileInput = document.getElementById('hiddenFileInput');
        fileInput.setAttribute('accept', supportedContentTypes || '*/*');
        if (fileInput && fileInput.click) {
            fileInput.click();
        }
    } catch (err) {
        console.error('Failed to trigger file input:', err);
    }
}

window.focusElement = function (element) {
    if (element) {
        element.focus();
    }
};

window.hightlightCodeBlocks = function() {
    if (typeof hljs !== 'undefined') {
        document.querySelectorAll('pre code').forEach((block) => {
            hljs.highlightElement(block);
        });
    } else {
        console.warn('Highlight.js is not loaded');
    }
    
    if(typeof marked !== 'undefined' && typeof DOMPurify !== 'undefined') {
        document.querySelectorAll('.markdown-content').forEach((el) => {
            el.innerHTML =
                DOMPurify.sanitize(
                    marked.parse(el.innerHTML)
                );
        });
    }
    else {
        console.warn('Marked.js or DOMPurify is not loaded');
    }
}

// Setup click outside handler for dropdowns
window.setupDropdownClickOutside = function(chatInputRef) {
    document.addEventListener('click', function(event) {
        // Check if click is outside thinking dropdown
        const thinkingSelector = event.target.closest('.thinking-selector-wrapper');
        const modelSelector = event.target.closest('.model-selector-wrapper');
        
        if (!thinkingSelector && !modelSelector) {
            chatInputRef.invokeMethodAsync('CloseDropdowns');
        }
    });
};

// Initialize dropdown close handler
window.initializeDropdownCloseHandler = function(dotNetRef) {
    // Remove existing listener if any
    if (window.dropdownCloseHandler) {
        document.removeEventListener('click', window.dropdownCloseHandler);
    }
    
    // Add new listener
    window.dropdownCloseHandler = function(event) {
        // Check if click is outside any dropdown
        const modelDropdown = event.target.closest('.model-selector-wrapper');
        const reasoningDropdown = event.target.closest('.reasoning-selector-wrapper');
        
        if (!modelDropdown && !reasoningDropdown) {
            dotNetRef.invokeMethodAsync('CloseDropdowns');
        }
    };
    
    document.addEventListener('click', window.dropdownCloseHandler);
};
