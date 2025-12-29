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

window.quillInterop = {
    init: function(elementId, content, placeholder, dotNetRef) {
        var options = {
            theme: 'snow',
            placeholder: placeholder || 'Rédigez quelque chose de génial...',
            modules: {
                toolbar: [
                    [{ 'header': [2, 3, 4, false] }],
                    ['bold', 'italic', 'underline', 'strike', 'blockquote'],
                    [{ 'list': 'ordered'}, { 'list': 'bullet' }],
                    [{ 'color': [] }, { 'background': [] }],
                    ['link', 'image', 'video'],
                    ['clean']
                ]
            }
        };

        var elem = document.getElementById(elementId);
        if (!elem) return;

        // Prevent double init
        if (elem.classList.contains('ql-container')) return;

        var quill = new Quill('#' + elementId, options);
        
        if (content) {
            quill.root.innerHTML = content;
        }

        quill.on('text-change', function() {
            var html = quill.root.innerHTML;
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('OnContentChanged', html);
            }
        });
        
        // Store instance
        elem.__quill = quill;
    },
    setContent: function(elementId, content) {
        var elem = document.getElementById(elementId);
        if (elem && elem.__quill) {
            if (elem.__quill.root.innerHTML !== content) {
                elem.__quill.root.innerHTML = content;
            }
        }
    }
};
