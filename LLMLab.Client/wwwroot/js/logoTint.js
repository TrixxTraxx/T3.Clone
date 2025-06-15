// Helper function to convert hex to RGB
function hexToRgb(hex) {
    // Handle both 6-character (#RRGGBB) and 8-character (#RRGGBBAA) hex codes
    const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})?$/i.exec(hex);
    return result ? {
        r: parseInt(result[1], 16),
        g: parseInt(result[2], 16),
        b: parseInt(result[3], 16)
        // Ignore alpha channel (result[4]) if present
    } : null;
}

// Helper function to parse RGBA color
function parseRgbaColor(color) {
    // Handle hex colors
    if (color.startsWith('#')) {
        return hexToRgb(color);
    }

    // Handle rgb() and rgba() colors
    const rgbaMatch = color.match(/rgba?\((\d+),\s*(\d+),\s*(\d+)(?:,\s*[\d.]+)?\)/);
    if (rgbaMatch) {
        return {
            r: parseInt(rgbaMatch[1], 10),
            g: parseInt(rgbaMatch[2], 10),
            b: parseInt(rgbaMatch[3], 10)
        };
    }

    return null;
}

// Helper function to convert RGB to HSL - simple working version
function convertRgbToHsl(r, g, b) {
    // Normalize RGB values to 0-1 range
    r = r / 255;
    g = g / 255;
    b = b / 255;

    const max = Math.max(r, g, b);
    const min = Math.min(r, g, b);
    const diff = max - min;

    // Calculate lightness
    const l = (max + min) / 2;

    let h = 0;
    let s = 0;

    if (diff !== 0) {
        // Calculate saturation
        s = l > 0.5 ? diff / (2 - max - min) : diff / (max + min);

        // Calculate hue
        if (max === r) {
            h = (g - b) / diff + (g < b ? 6 : 0);
        } else if (max === g) {
            h = (b - r) / diff + 2;
        } else if (max === b) {
            h = (r - g) / diff + 4;
        }
        h = h / 6;
    }

    return {
        h: h * 360,
        s: s * 100,
        l: l * 100
    };
}

// Function to calculate CSS filter to transform source color to target color
function calculateLogoFilter(targetColor) {
    const sourceColor = '#6366f1'; // Original logo color

    const sourceRgb = hexToRgb(sourceColor);
    const targetRgb = parseRgbaColor(targetColor.trim());

    if (!sourceRgb || !targetRgb) {
        return 'none';
    }
    
    const sourceHsl = convertRgbToHsl(sourceRgb.r, sourceRgb.g, sourceRgb.b);
    const targetHsl = convertRgbToHsl(targetRgb.r, targetRgb.g, targetRgb.b);

    // Calculate adjustments
    let hueRotate = targetHsl.h - sourceHsl.h;
    if (hueRotate < 0) hueRotate += 360;
    if (hueRotate > 180) hueRotate -= 360;

    // Avoid division by zero
    const saturateAmount = sourceHsl.s > 0 ? targetHsl.s / sourceHsl.s : 1;
    const brightnessAmount = sourceHsl.l > 0 ? targetHsl.l / sourceHsl.l : 1;
    // Build filter string
    const filters = [];

    filters.push(`hue-rotate(${Math.round(hueRotate)}deg)`);
    filters.push(`saturate(${saturateAmount.toFixed(2)})`);
    //always apply brightness to ensure visibility
    filters.push(`brightness(1)`);

    return filters.length > 0 ? filters.join(' ') : 'none';
}

// Function to update logo color based on primary color
window.updateLogoColor = function(primaryColor) {
    const filter = calculateLogoFilter(primaryColor);

    // Set CSS custom property for the filter
    document.documentElement.style.setProperty('--logo-color-filter', filter);

    console.log(`Logo filter updated for color ${primaryColor}: ${filter}`);
};