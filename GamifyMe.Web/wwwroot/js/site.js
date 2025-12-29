window.siteUtils = {
    isStandalone: function () {
        return (window.matchMedia('(display-mode: standalone)').matches) || (window.navigator.standalone) || document.referrer.includes('android-app://');
    },
    insertTextAtCursor: function(elementId, openTag, closeTag) {
        const el = document.getElementById(elementId);
        if (!el) return;

        const start = el.selectionStart;
        const end = el.selectionEnd;
        const text = el.value;
        const selectedText = text.substring(start, end);
        const replacement = openTag + selectedText + closeTag;

        el.value = text.substring(0, start) + replacement + text.substring(end);
        
        // Dispatch input event so Blazor knows value changed
        el.dispatchEvent(new Event('change', { bubbles: true }));
        el.dispatchEvent(new Event('input', { bubbles: true }));
        
        el.focus();
        el.setSelectionRange(start + openTag.length, start + openTag.length + selectedText.length);
    }
};
