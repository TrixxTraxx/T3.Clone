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

// Copy code from code blocks
window.copyCode = function(button) {
    const codeBlock = button.closest('.code-block');
    const code = codeBlock.querySelector('code');
    if (code) {
        const text = code.textContent;
        copyToClipboard(text).then(() => {
            const originalText = button.textContent;
            button.textContent = 'Copied!';
            button.style.backgroundColor = 'var(--mud-palette-success)';
            setTimeout(() => {
                button.textContent = originalText;
                button.style.backgroundColor = '';
            }, 2000);
        });
    }
};

// Download file from data URL
window.downloadFile = function(dataUrl, filename) {
    const link = document.createElement('a');
    link.href = dataUrl;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

// Initialize syntax highlighting (can be extended with libraries like Prism.js)
window.highlightCodeBlocks = function() {
    // Basic syntax highlighting - can be replaced with a library
    const codeBlocks = document.querySelectorAll('code[class*="language-"]');
    codeBlocks.forEach(block => {
        const language = block.className.match(/language-(\w+)/)?.[1];
        if (language) {
            block.setAttribute('data-language', language);
            // Add basic highlighting for common languages
            highlightBasicSyntax(block, language);
        }
    });
};

// Basic syntax highlighting for common languages
function highlightBasicSyntax(codeElement, language) {
    let code = codeElement.innerHTML;
    
    switch (language.toLowerCase()) {
        case 'javascript':
        case 'js':
            code = highlightJavaScript(code);
            break;
        case 'csharp':
        case 'cs':
            code = highlightCSharp(code);
            break;
        case 'html':
            code = highlightHTML(code);
            break;
        case 'css':
            code = highlightCSS(code);
            break;
        case 'sql':
            code = highlightSQL(code);
            break;
        case 'json':
            code = highlightJSON(code);
            break;
        default:
            // Generic highlighting for keywords
            code = highlightGeneric(code);
            break;
    }
    
    codeElement.innerHTML = code;
}

function highlightJavaScript(code) {
    const keywords = ['function', 'var', 'let', 'const', 'if', 'else', 'for', 'while', 'return', 'class', 'extends', 'import', 'export', 'async', 'await', 'try', 'catch', 'throw', 'new'];
    const types = ['string', 'number', 'boolean', 'object', 'undefined', 'null'];
    
    code = highlightKeywords(code, keywords, 'keyword');
    code = highlightKeywords(code, types, 'type');
    code = highlightStrings(code);
    code = highlightComments(code);
    
    return code;
}

function highlightCSharp(code) {
    const keywords = ['public', 'private', 'protected', 'internal', 'static', 'readonly', 'const', 'void', 'string', 'int', 'bool', 'var', 'class', 'interface', 'namespace', 'using', 'if', 'else', 'for', 'foreach', 'while', 'return', 'new', 'this', 'base', 'async', 'await', 'try', 'catch', 'throw'];
    const types = ['string', 'int', 'bool', 'double', 'float', 'decimal', 'char', 'byte', 'long', 'short', 'object', 'DateTime', 'List', 'Dictionary', 'IEnumerable'];
    
    code = highlightKeywords(code, keywords, 'keyword');
    code = highlightKeywords(code, types, 'type');
    code = highlightStrings(code);
    code = highlightComments(code);
    
    return code;
}

function highlightHTML(code) {
    // Highlight HTML tags
    code = code.replace(/(&lt;[^&gt;]+&gt;)/g, '<span class="html-tag">$1</span>');
    code = highlightStrings(code);
    
    return code;
}

function highlightCSS(code) {
    const properties = ['color', 'background', 'margin', 'padding', 'border', 'font', 'display', 'position', 'width', 'height', 'flex', 'grid'];
    
    code = highlightKeywords(code, properties, 'css-property');
    code = code.replace(/(#[0-9a-fA-F]{3,6})/g, '<span class="css-color">$1</span>');
    code = highlightStrings(code);
    code = highlightComments(code);
    
    return code;
}

function highlightSQL(code) {
    const keywords = ['SELECT', 'FROM', 'WHERE', 'INSERT', 'UPDATE', 'DELETE', 'CREATE', 'ALTER', 'DROP', 'TABLE', 'INDEX', 'JOIN', 'LEFT', 'RIGHT', 'INNER', 'OUTER', 'ON', 'GROUP', 'BY', 'ORDER', 'HAVING', 'AND', 'OR', 'NOT', 'NULL', 'IS'];
    
    code = highlightKeywords(code, keywords, 'keyword');
    code = highlightStrings(code);
    code = highlightComments(code);
    
    return code;
}

function highlightJSON(code) {
    // Highlight JSON keys and values
    code = code.replace(/("[\w\s-]+")\s*:/g, '<span class="json-key">$1</span>:');
    code = code.replace(/:\s*(".*?")/g, ': <span class="json-string">$1</span>');
    code = code.replace(/:\s*(true|false|null|\d+)/g, ': <span class="json-value">$1</span>');
    
    return code;
}

function highlightGeneric(code) {
    code = highlightStrings(code);
    code = highlightComments(code);
    
    return code;
}

function highlightKeywords(code, keywords, className) {
    keywords.forEach(keyword => {
        const regex = new RegExp(`\\b${keyword}\\b`, 'g');
        code = code.replace(regex, `<span class="${className}">${keyword}</span>`);
    });
    return code;
}

function highlightStrings(code) {
    // Single quotes
    code = code.replace(/'([^'\\]|\\.)*'/g, '<span class="string">$&</span>');
    // Double quotes
    code = code.replace(/"([^"\\]|\\.)*"/g, '<span class="string">$&</span>');
    // Template literals
    code = code.replace(/`([^`\\]|\\.)*`/g, '<span class="string">$&</span>');
    
    return code;
}

function highlightComments(code) {
    // Single line comments
    code = code.replace(/\/\/.*$/gm, '<span class="comment">$&</span>');
    // Multi-line comments
    code = code.replace(/\/\*[\s\S]*?\*\//g, '<span class="comment">$&</span>');
    
    return code;
}

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