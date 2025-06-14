window.getWindowWidth = function() {
    return window.innerWidth;
};

window.focusElement = function(element) {
    if (element) {
        element.focus();
    }
};
// Focus an element by reference and select text if it's an input
window.focusElementAndSelect = function(element) {
    if (element) {
        element.focus();
        // Also select all text if it's an input
        if (element.tagName === 'INPUT' && element.type === 'text') {
            element.select();
        }
    }
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

// Check if threads overflow and apply appropriate class to sidebar
window.checkThreadOverflow = function() {
    const threadsSection = document.querySelector('.threads-scroll-section');
    const drawerCustom = document.querySelector('.drawer-custom');
    
    if (!threadsSection || !drawerCustom) return;
    
    const isOverflowing = threadsSection.scrollHeight > threadsSection.clientHeight;
    
    if (isOverflowing) {
        drawerCustom.classList.add('has-overflow');
    } else {
        drawerCustom.classList.remove('has-overflow');
    }
};

// Set up ResizeObserver to check for overflow changes
window.setupOverflowObserver = function() {
    const threadsSection = document.querySelector('.threads-scroll-section');
    if (!threadsSection) return;
    
    if (window.threadsResizeObserver) {
        window.threadsResizeObserver.disconnect();
    }
    
    window.threadsResizeObserver = new ResizeObserver(() => {
        window.checkThreadOverflow();
    });
    
    window.threadsResizeObserver.observe(threadsSection);
};

window.isChatScrolledToBottom = function() {
    const chatContainer = document.querySelector('.chat-messages-container');
    if (!chatContainer) return true;
    return chatContainer.scrollHeight - chatContainer.scrollTop - chatContainer.clientHeight < 10;
};

window.scrollChatToBottom = function() {
    const chatContainer = document.querySelector('.chat-messages-container');
    if (chatContainer) {
        chatContainer.scrollTop = chatContainer.scrollHeight;
    }
};

window.scrollChatToBottomNoAnimation = function() {
    const chatContainer = document.querySelector('.chat-messages-container');
    if (chatContainer) {
        // Save current scroll-behavior
        const oldBehavior = chatContainer.style.scrollBehavior;
        // Force instant scroll
        chatContainer.style.scrollBehavior = 'auto';
        chatContainer.scrollTop = chatContainer.scrollHeight;
        // Restore old scroll-behavior
        chatContainer.style.scrollBehavior = oldBehavior;
    }
};

window.setupChatScrollHandler = function(dotnetRef) {
    const chatContainer = document.querySelector('.chat-messages-container');
    if (!chatContainer) return;
    if (chatContainer._scrollHandler) return; // Only attach once

    chatContainer._scrollHandler = function() {
        const atBottom = (chatContainer.scrollHeight - chatContainer.scrollTop - chatContainer.clientHeight < 10);
        dotnetRef.invokeMethodAsync('SetScrolledToBottom', atBottom);
    };
    chatContainer.addEventListener('scroll', chatContainer._scrollHandler);

    // Fire once on setup to sync initial state
    chatContainer._scrollHandler();
};

window.cleanupChatScrollHandler = function() {
    const chatContainer = document.querySelector('.chat-messages-container');
    if (chatContainer && chatContainer._scrollHandler) {
        chatContainer.removeEventListener('scroll', chatContainer._scrollHandler);
        delete chatContainer._scrollHandler;
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

window.setupChatInputAutoFocus = function (element) {
    if (!element) return;
    // Store handler on element so we can remove it later
    element._chatInputAutoFocusHandler = function (e) {
        const tag = document.activeElement.tagName;
        const editable = document.activeElement.isContentEditable;
        if (
            !e.ctrlKey && !e.metaKey && !e.altKey &&
            !editable &&
            tag !== 'INPUT' && tag !== 'TEXTAREA' && tag !== 'SELECT'
        ) {
            if (e.key.length === 1) {
                element.focus();
            }
        }
    };
    document.addEventListener('keydown', element._chatInputAutoFocusHandler);
};

// Call this to remove it:
window.removeChatInputAutoFocus = function (element) {
    if (!element || !element._chatInputAutoFocusHandler) return;
    document.removeEventListener('keydown', element._chatInputAutoFocusHandler);
    delete element._chatInputAutoFocusHandler;
};

window.getHighlightedHtml = function(code, language) {
    if (typeof hljs === 'undefined') {
        console.warn('Highlight.js is not loaded');
        return code;
    }
    
    try {
        // Auto-detect language
        const result = hljs.highlightAuto(code);
        return result.value;
    } catch (error) {
        console.warn('Error highlighting code:', error);
        // Return the original code if highlighting fails
        return code;
    }
}
