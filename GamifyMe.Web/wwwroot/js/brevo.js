window.brevoInterop = {
    identify: function (email, attributes) {
        if (window.Brevo) {
            window.Brevo.push(function () {
                window.Brevo.identify({
                    identifiers: {
                        email_id: email
                    },
                    attributes: attributes
                });
            });
            console.log("[Brevo] Identify called for:", email);
        } else {
            console.warn("[Brevo] SDK not loaded.");
        }
    },
    trackEvent: function (eventName, eventData) {
        if (window.Brevo) {
            // Documented format: Brevo.push(["track", event_name, properties, event_data]);
            // We pass {} for properties to rely on cookie identification, and pass eventData for custom event info.
            window.Brevo.push(['track', eventName, {}, eventData]);
            console.log("[Brevo] Track event:", eventName, eventData);
        } else {
            console.warn("[Brevo] SDK not loaded.");
        }
    }
};
